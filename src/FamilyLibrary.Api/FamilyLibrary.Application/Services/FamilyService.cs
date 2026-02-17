using System.Security.Cryptography;
using FamilyLibrary.Application.Common;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Application.Interfaces;
using FamilyLibrary.Domain.Entities;
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

    private const string FamiliesContainer = "families";

    public FamilyService(
        IFamilyRepository familyRepository,
        IFamilyVersionRepository versionRepository,
        IFamilyRoleRepository roleRepository,
        IBlobStorageService blobStorageService)
    {
        _familyRepository = familyRepository;
        _versionRepository = versionRepository;
        _roleRepository = roleRepository;
        _blobStorageService = blobStorageService;
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
        string? catalogBlobPath = null;
        string? catalogHash = null;
        if (typeCatalogStream != null && !string.IsNullOrEmpty(typeCatalogFileName))
        {
            catalogHash = await CalculateHashAsync(typeCatalogStream, cancellationToken);
            typeCatalogStream.Position = 0;

            var catalogBlobName = $"{family.Id}/v{newVersion}/{typeCatalogFileName}";
            catalogBlobPath = await _blobStorageService.UploadAsync(
                FamiliesContainer,
                catalogBlobName,
                typeCatalogStream,
                cancellationToken);
        }

        // ============================================================
        // PHASE 3: Database writes (MINIMAL time in transaction)
        // All I/O is done, now persist to DB atomically
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

        if (existingFamily == null)
        {
            // New family - save entity first
            await _familyRepository.AddAsync(family, cancellationToken);
        }
        else
        {
            // Existing family - increment version
            family.IncrementVersion();
            await _familyRepository.UpdateAsync(family, cancellationToken);
        }

        await _versionRepository.AddAsync(version, cancellationToken);

        // Fetch updated family with role
        var updatedFamily = await _familyRepository.GetByIdAsync(family.Id, cancellationToken);
        return updatedFamily!.Adapt<FamilyDto>();
    }

    public async Task<bool> ValidateHashAsync(string hash, CancellationToken cancellationToken = default)
    {
        return await _familyRepository.HashExistsAsync(hash, cancellationToken);
    }

    public async Task<List<FamilyStatusDto>> BatchCheckAsync(
        List<string> hashes,
        CancellationToken cancellationToken = default)
    {
        if (hashes == null || hashes.Count == 0)
            return [];

        // Single query to get families by hashes at database level
        var families = await _familyRepository.GetByHashesAsync(hashes, cancellationToken);

        // Build a lookup dictionary from hash to family
        var hashToFamily = new Dictionary<string, FamilyEntity>(StringComparer.OrdinalIgnoreCase);
        foreach (var family in families)
        {
            var latestVersion = family.Versions.OrderByDescending(v => v.Version).FirstOrDefault();
            if (latestVersion != null && !string.IsNullOrEmpty(latestVersion.Hash))
            {
                hashToFamily[latestVersion.Hash] = family;
            }
        }

        // Build results preserving original order
        var results = new List<FamilyStatusDto>(hashes.Count);
        foreach (var hash in hashes)
        {
            if (hashToFamily.TryGetValue(hash, out var family))
            {
                results.Add(new FamilyStatusDto
                {
                    Hash = hash,
                    Exists = true,
                    FamilyId = family.Id,
                    FamilyName = family.FamilyName,
                    CurrentVersion = family.CurrentVersion
                });
            }
            else
            {
                results.Add(new FamilyStatusDto
                {
                    Hash = hash,
                    Exists = false,
                    FamilyId = null,
                    FamilyName = null,
                    CurrentVersion = null
                });
            }
        }

        return results;
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

    private static async Task<string> CalculateHashAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
