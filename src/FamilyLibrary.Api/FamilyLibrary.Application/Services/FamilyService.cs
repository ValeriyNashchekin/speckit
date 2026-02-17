using System.Security.Cryptography;
using System.Text;
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
        var allFamilies = await _familyRepository.GetAllWithRolesAsync(cancellationToken);

        // Apply filtering
        var filteredFamilies = allFamilies.AsEnumerable();

        // Filter by roleId
        if (roleId.HasValue)
        {
            filteredFamilies = filteredFamilies.Where(f => f.RoleId == roleId.Value);
        }

        // Filter by searchTerm (family name search, case-insensitive)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            filteredFamilies = filteredFamilies.Where(f =>
                f.FamilyName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by categoryId (check role's category)
        if (categoryId.HasValue)
        {
            filteredFamilies = filteredFamilies.Where(f =>
                f.Role?.CategoryId == categoryId.Value);
        }

        // Filter by tagIds (check if role has ANY of the specified tags)
        if (tagIds is not null && tagIds.Count > 0)
        {
            var tagIdSet = new HashSet<Guid>(tagIds);
            filteredFamilies = filteredFamilies.Where(f =>
                f.Role?.Tags.Any(t => tagIdSet.Contains(t.Id)) == true);
        }

        var filteredList = filteredFamilies.ToList();
        var totalCount = filteredList.Count;

        // Apply pagination
        var pagedItems = filteredList
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var dtos = pagedItems.Adapt<List<FamilyDto>>();

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
        // Validate role exists
        var roleExists = await _roleRepository.ExistsAsync(dto.RoleId, cancellationToken);
        if (!roleExists)
        {
            throw new NotFoundException(nameof(FamilyRoleEntity), dto.RoleId);
        }

        // Calculate content hash
        var hash = await CalculateHashAsync(fileStream, cancellationToken);
        
        // Reset stream position after hash calculation
        fileStream.Position = 0;

        // Check for duplicate
        if (await _familyRepository.HashExistsAsync(hash, cancellationToken))
        {
            throw new ValidationException(nameof(fileName), $"A family with this content already exists (hash: {hash}).");
        }

        // Check if family with same name already exists for this role
        var existingFamilies = await _familyRepository.GetByRoleIdAsync(dto.RoleId, cancellationToken);
        var existingFamily = existingFamilies.FirstOrDefault(f => 
            f.FamilyName.Equals(dto.FamilyName, StringComparison.OrdinalIgnoreCase));

        FamilyEntity family;
        string? previousHash = null;

        if (existingFamily != null)
        {
            // Update existing family - increment version
            family = existingFamily;
            var latestVersion = await _versionRepository.GetLatestVersionAsync(family.Id, cancellationToken);
            previousHash = latestVersion?.Hash;
            
            // Note: We need to get a tracked entity for update
            // The repository will handle incrementing version
        }
        else
        {
            // Create new family
            family = new FamilyEntity(dto.RoleId, dto.FamilyName);
            await _familyRepository.AddAsync(family, cancellationToken);
        }

        // Generate blob path
        var blobName = $"{family.Id}/v{(existingFamily != null ? family.CurrentVersion + 1 : 1)}/{fileName}";
        
        // Upload to blob storage
        var blobPath = await _blobStorageService.UploadAsync(
            FamiliesContainer,
            blobName,
            fileStream,
            cancellationToken);

        // Handle type catalog (TXT file) if provided
        string? catalogBlobPath = null;
        string? catalogHash = null;
        if (typeCatalogStream != null && !string.IsNullOrEmpty(typeCatalogFileName))
        {
            // Calculate hash for type catalog
            catalogHash = await CalculateHashAsync(typeCatalogStream, cancellationToken);
            typeCatalogStream.Position = 0;

            var catalogBlobName = $"{family.Id}/v{(existingFamily != null ? family.CurrentVersion + 1 : 1)}/{typeCatalogFileName}";
            catalogBlobPath = await _blobStorageService.UploadAsync(
                FamiliesContainer,
                catalogBlobName,
                typeCatalogStream,
                cancellationToken);
        }

        // Create version entity
        var version = new FamilyVersionEntity(
            familyId: family.Id,
            version: existingFamily != null ? family.CurrentVersion + 1 : 1,
            hash: hash,
            blobPath: blobPath,
            originalFileName: fileName,
            snapshotJson: "{}", // TODO: Extract actual family snapshot from .rfa file
            publishedBy: "system", // TODO: Use actual user context
            previousHash: previousHash,
            catalogBlobPath: catalogBlobPath,
            catalogHash: catalogHash,
            originalCatalogName: typeCatalogFileName);

        await _versionRepository.AddAsync(version, cancellationToken);

        // Increment family version if updating existing
        if (existingFamily != null)
        {
            family.IncrementVersion();
            await _familyRepository.UpdateAsync(family, cancellationToken);
        }

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
        var results = new List<FamilyStatusDto>();

        foreach (var hash in hashes)
        {
            var family = await _familyRepository.GetByHashAsync(hash, cancellationToken);

            results.Add(new FamilyStatusDto
            {
                Hash = hash,
                Exists = family != null,
                FamilyId = family?.Id,
                FamilyName = family?.FamilyName,
                CurrentVersion = family?.CurrentVersion
            });
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
