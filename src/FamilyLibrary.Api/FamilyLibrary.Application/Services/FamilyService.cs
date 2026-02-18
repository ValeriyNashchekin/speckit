using System.Security.Cryptography;
using System.Text.Json;
using FamilyLibrary.Application.Common;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Application.Interfaces;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Enums;
using FamilyLibrary.Domain.Exceptions;
using FamilyLibrary.Domain.Interfaces;
using Mapster;

namespace FamilyLibrary.Application.Services;

/// <summary>
/// Service for Family operations.
/// </summary>
public class FamilyService : IFamilyService
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IFamilyVersionRepository _versionRepository;
    private readonly IFamilyRoleRepository _roleRepository;
    private readonly IBlobStorageService _blobStorageService;
    private readonly IChangeDetectionService _changeDetectionService;
    private readonly IUnitOfWork _unitOfWork;

    private const string FamiliesContainer = "families";

    public FamilyService(
        IFamilyRepository familyRepository,
        IFamilyVersionRepository versionRepository,
        IFamilyRoleRepository roleRepository,
        IBlobStorageService blobStorageService,
        IChangeDetectionService changeDetectionService,
        IUnitOfWork unitOfWork)
    {
        _familyRepository = familyRepository;
        _versionRepository = versionRepository;
        _roleRepository = roleRepository;
        _blobStorageService = blobStorageService;
        _changeDetectionService = changeDetectionService;
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<FamilyDto>> GetAllAsync(
        int page,
        int pageSize,
        Guid? roleId,
        string? searchTerm,
        Guid? categoryId,
        List<Guid>? tagIds,
        CancellationToken cancellationToken = default)
    {
        // Filtering and pagination at database level
        var (items, totalCount) = await _familyRepository.GetFilteredAsync(
            roleId, searchTerm, categoryId, tagIds, page, pageSize, cancellationToken);

        var dtos = items.Adapt<List<FamilyDto>>();
        return new PagedResult<FamilyDto>(dtos, totalCount, page, pageSize);
    }

    public async Task<FamilyDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _familyRepository.GetWithVersionsAsync(id, cancellationToken);
        return entity?.Adapt<FamilyDetailDto>();
    }

    public async Task<List<FamilyVersionDto>> GetVersionsAsync(Guid familyId, CancellationToken cancellationToken = default)
    {
        var versions = await _versionRepository.GetByFamilyIdAsync(familyId, cancellationToken);
        return versions.Adapt<List<FamilyVersionDto>>();
    }

    public async Task<FamilyDto> PublishAsync(
        CreateFamilyDto dto,
        Stream fileStream,
        string fileName,
        Stream? typeCatalogStream = null,
        string? typeCatalogFileName = null,
        CancellationToken cancellationToken = default)
    {
        // ============================================================
        // PHASE 1: Validation (DB reads only - no transaction yet)
        // ============================================================

        var roleExists = await _roleRepository.ExistsAsync(dto.RoleId, cancellationToken);
        if (!roleExists)
        {
            throw new NotFoundException(nameof(FamilyRoleEntity), dto.RoleId);
        }

        var hash = await CalculateHashAsync(fileStream, cancellationToken);
        fileStream.Position = 0;

        if (await _familyRepository.HashExistsAsync(hash, cancellationToken))
        {
            throw new ValidationException(nameof(fileName), $"A family with this content already exists (hash: {hash}).");
        }

        var existingFamily = await _familyRepository.GetByRoleAndNameAsync(dto.RoleId, dto.FamilyName, cancellationToken);

        // ============================================================
        // PHASE 2: Prepare entity & Upload blobs (I/O - OUTSIDE transaction)
        // IMPORTANT: Blob upload can take seconds for large files.
        // Do NOT hold DB connection during this operation!
        // ============================================================

        FamilyEntity family;
        string? previousHash = null;
        int newVersion;

        if (existingFamily != null)
        {
            family = existingFamily;
            var latestVersion = await _versionRepository.GetLatestVersionAsync(family.Id, cancellationToken);
            previousHash = latestVersion?.Hash;
            newVersion = family.CurrentVersion + 1;
        }
        else
        {
            // Create entity in memory (ID is auto-generated)
            // NOT saved to DB yet - will be saved after blob upload
            family = new FamilyEntity(dto.RoleId, dto.FamilyName);
            newVersion = 1;
        }

        // Upload main family file to blob storage
        var blobName = $"{family.Id}/v{newVersion}/{fileName}";
        var blobPath = await _blobStorageService.UploadAsync(
            FamiliesContainer,
            blobName,
            fileStream,
            cancellationToken);

        // Upload type catalog if provided
        string? catalogBlobName = null;
        string? catalogBlobPath = null;
        string? catalogHash = null;
        if (typeCatalogStream != null && !string.IsNullOrEmpty(typeCatalogFileName))
        {
            catalogHash = await CalculateHashAsync(typeCatalogStream, cancellationToken);
            typeCatalogStream.Position = 0;

            catalogBlobName = $"{family.Id}/v{newVersion}/{typeCatalogFileName}";
            catalogBlobPath = await _blobStorageService.UploadAsync(
                FamiliesContainer,
                catalogBlobName,
                typeCatalogStream,
                cancellationToken);
        }

        // ============================================================
        // PHASE 3: Database writes (atomic transaction with compensating action)
        // ============================================================

        var version = new FamilyVersionEntity(
            familyId: family.Id,
            version: newVersion,
            hash: hash,
            blobPath: blobPath,
            originalFileName: fileName,
            snapshotJson: "{}",
            publishedBy: "system",
            previousHash: previousHash,
            catalogBlobPath: catalogBlobPath,
            catalogHash: catalogHash,
            originalCatalogName: typeCatalogFileName);

        try
        {
            if (existingFamily == null)
            {
                await _familyRepository.AddAsync(family, cancellationToken);
            }
            else
            {
                family.IncrementVersion();
                await _familyRepository.UpdateAsync(family, cancellationToken);
            }

            await _versionRepository.AddAsync(version, cancellationToken);

            // SINGLE atomic commit for all changes
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // COMPENSATING ACTION: Delete uploaded blobs if DB transaction failed
            // This prevents orphaned files in blob storage
            await SafeDeleteBlobAsync(blobName, cancellationToken);
            if (catalogBlobName != null)
            {
                await SafeDeleteBlobAsync(catalogBlobName, cancellationToken);
            }
            throw;
        }

        // Fetch updated family with role
        var updatedFamily = await _familyRepository.GetByIdAsync(family.Id, cancellationToken);
        return updatedFamily!.Adapt<FamilyDto>();
    }

    public async Task<bool> ValidateHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        return await _familyRepository.HashExistsAsync(hash, cancellationToken);
    }

    public async Task<BatchCheckResponse> BatchCheckAsync(
        BatchCheckRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Families == null || request.Families.Count == 0)
            return new BatchCheckResponse { Results = [] };

        var roleNames = request.Families.Select(f => f.RoleName).Distinct().ToList();
        var requestItems = request.Families.ToList();

        // Query families with their roles and latest versions by role names
        var familiesWithRoles = await _familyRepository.GetByRoleNamesWithLatestVersionsAsync(
            roleNames, cancellationToken);

        // Build lookup by role name
        var roleToFamilies = familiesWithRoles
            .GroupBy(f => f.Role.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        // Build results
        var results = new List<FamilyCheckResult>(requestItems.Count);
        foreach (var item in requestItems)
        {
            if (!roleToFamilies.TryGetValue(item.RoleName, out var families) || families.Count == 0)
            {
                results.Add(new FamilyCheckResult
                {
                    RoleName = item.RoleName,
                    Status = FamilyScanStatus.Unmatched
                });
                continue;
            }

            // Find the family with matching hash in its versions
            FamilyEntity? matchedFamily = null;
            FamilyVersionEntity? latestVersion = null;

            foreach (var family in families)
            {
                latestVersion = family.Versions.OrderByDescending(v => v.Version).FirstOrDefault();
                if (latestVersion != null)
                {
                    // Check if any version matches the hash
                    var matchingVersion = family.Versions.FirstOrDefault(v =>
                        string.Equals(v.Hash, item.Hash, StringComparison.OrdinalIgnoreCase));

                    if (matchingVersion != null)
                    {
                        matchedFamily = family;
                        break;
                    }
                    else if (matchedFamily == null)
                    {
                        // Keep first family as candidate for update
                        matchedFamily = family;
                    }
                }
            }

            if (matchedFamily == null || latestVersion == null)
            {
                results.Add(new FamilyCheckResult
                {
                    RoleName = item.RoleName,
                    Status = FamilyScanStatus.Unmatched
                });
                continue;
            }

            // Determine status based on hash comparison
            var latestHash = latestVersion.Hash;
            var status = string.Equals(latestHash, item.Hash, StringComparison.OrdinalIgnoreCase)
                ? FamilyScanStatus.UpToDate
                : FamilyScanStatus.UpdateAvailable;

            results.Add(new FamilyCheckResult
            {
                RoleName = item.RoleName,
                Status = status,
                LibraryVersion = matchedFamily.CurrentVersion,
                CurrentVersion = matchedFamily.CurrentVersion,
                LibraryHash = latestHash
            });
        }

        return new BatchCheckResponse { Results = results };
    }

    public async Task<FamilyDownloadDto> GetDownloadUrlAsync(
        Guid familyId,
        int? version,
        CancellationToken cancellationToken = default)
    {
        var family = await _familyRepository.GetWithVersionsAsync(familyId, cancellationToken)
            ?? throw new NotFoundException(nameof(FamilyEntity), familyId);

        FamilyVersionEntity versionEntity;
        if (version.HasValue)
        {
            versionEntity = family.Versions.FirstOrDefault(v => v.Version == version.Value)
                ?? throw new NotFoundException("FamilyVersion", version.Value);
        }
        else
        {
            versionEntity = family.Versions.OrderByDescending(v => v.Version).First();
        }

        // Extract blob name from the full blob path
        var blobUri = new Uri(versionEntity.BlobPath);
        var blobName = blobUri.AbsolutePath.TrimStart('/');

        // Remove container prefix from blob name if present
        var containerPrefix = $"{FamiliesContainer}/";
        if (blobName.StartsWith(containerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            blobName = blobName.Substring(containerPrefix.Length);
        }

        // Generate SAS token for download
        var downloadUrl = await _blobStorageService.GetSasUrlAsync(
            FamiliesContainer,
            blobName,
            TimeSpan.FromMinutes(30),
            cancellationToken);

        return new FamilyDownloadDto
        {
            DownloadUrl = downloadUrl,
            OriginalFileName = versionEntity.OriginalFileName,
            Hash = versionEntity.Hash,
            Version = versionEntity.Version
        };
    }

    public async Task<ChangeSetDto> GetChangesAsync(
        Guid familyId,
        int fromVersion,
        int toVersion,
        CancellationToken cancellationToken = default)
    {
        // Verify family exists
        var familyExists = await _familyRepository.ExistsAsync(familyId, cancellationToken);
        if (!familyExists)
        {
            throw new NotFoundException(nameof(FamilyEntity), familyId);
        }

        // Get both versions in parallel
        var fromVersionTask = _versionRepository.GetByVersionAsync(familyId, fromVersion, cancellationToken);
        var toVersionTask = _versionRepository.GetByVersionAsync(familyId, toVersion, cancellationToken);

        await Task.WhenAll(fromVersionTask, toVersionTask);

        var fromVersionEntity = await fromVersionTask;
        var toVersionEntity = await toVersionTask;

        if (fromVersionEntity is null)
        {
            throw new NotFoundException("FamilyVersion", $"Version {fromVersion} not found for family {familyId}");
        }

        if (toVersionEntity is null)
        {
            throw new NotFoundException("FamilyVersion", $"Version {toVersion} not found for family {familyId}");
        }

        // Deserialize snapshots
        var fromSnapshot = JsonSerializer.Deserialize<FamilySnapshot>(fromVersionEntity.SnapshotJson);
        var toSnapshot = JsonSerializer.Deserialize<FamilySnapshot>(toVersionEntity.SnapshotJson);

        if (fromSnapshot is null)
        {
            throw new ValidationException("SnapshotJson", $"Failed to deserialize snapshot for version {fromVersion}");
        }

        if (toSnapshot is null)
        {
            throw new ValidationException("SnapshotJson", $"Failed to deserialize snapshot for version {toVersion}");
        }

        // Detect changes using the change detection service
        return _changeDetectionService.DetectChanges(fromSnapshot, toSnapshot);
    }

    private static async Task<string> CalculateHashAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Safely deletes a blob, ignoring any errors (best-effort cleanup).
    /// </summary>
    private async Task SafeDeleteBlobAsync(string blobName, CancellationToken cancellationToken)
    {
        try
        {
            await _blobStorageService.DeleteAsync(FamiliesContainer, blobName, cancellationToken);
        }
        catch
        {
            // Ignore deletion errors during cleanup
            // The blob may be cleaned up later by a background process
        }
    }
}
