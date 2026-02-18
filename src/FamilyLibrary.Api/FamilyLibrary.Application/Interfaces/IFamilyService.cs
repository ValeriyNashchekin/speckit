using FamilyLibrary.Application.Common;
using FamilyLibrary.Application.DTOs;

namespace FamilyLibrary.Application.Interfaces;

/// <summary>
/// Service interface for Family operations.
/// </summary>
public interface IFamilyService
{
    /// <summary>
    /// Gets all families with pagination and optional filtering.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <param name="roleId">Filter by role ID.</param>
    /// <param name="searchTerm">Search term for family name (case-insensitive).</param>
    /// <param name="categoryId">Filter by category ID.</param>
    /// <param name="tagIds">Filter by tag IDs (families with roles having ANY of these tags).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paged result of families.</returns>
    Task<PagedResult<FamilyDto>> GetAllAsync(
        int page,
        int pageSize,
        Guid? roleId,
        string? searchTerm,
        Guid? categoryId,
        List<Guid>? tagIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a family by ID with full details including versions.
    /// </summary>
    Task<FamilyDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets version history for a family.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of family versions.</returns>
    Task<List<FamilyVersionDto>> GetVersionsAsync(Guid familyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publishes a new family file.
    /// </summary>
    Task<FamilyDto> PublishAsync(
        CreateFamilyDto dto,
        Stream fileStream,
        string fileName,
        Stream? typeCatalogStream = null,
        string? typeCatalogFileName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a content hash already exists.
    /// </summary>
    Task<bool> ValidateHashAsync(string hash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch checks multiple families against the library by role name and hash.
    /// Determines if families are up-to-date, need update, or are unmatched.
    /// </summary>
    /// <param name="request">The batch check request with family items.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Batch check response with status for each family.</returns>
    Task<BatchCheckResponse> BatchCheckAsync(
        BatchCheckRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets download URL for a family version.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="version">Optional specific version number. If null, returns the latest version.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Download URL and metadata for the family file.</returns>
    Task<FamilyDownloadDto> GetDownloadUrlAsync(
        Guid familyId,
        int? version,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the changes between two versions of a family.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="fromVersion">The source version number.</param>
    /// <param name="toVersion">The target version number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Change set describing the differences between versions.</returns>
    Task<ChangeSetDto> GetChangesAsync(
        Guid familyId,
        int fromVersion,
        int toVersion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a preview of changes that will occur when updating a family.
    /// US4: Pre-Update Preview - designers see what will change before confirming update.
    /// </summary>
    /// <param name="familyId">The family ID.</param>
    /// <param name="currentVersion">The current local version of the family.</param>
    /// <param name="targetVersion">The target library version to update to.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Change set describing what will change after the update.</returns>
    Task<ChangeSetDto> GetUpdatePreviewAsync(
        Guid familyId,
        int currentVersion,
        int targetVersion,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Detects local changes by comparing a local snapshot with the latest library version.
    /// </summary>
    /// <param name="familyId">The family ID to compare against.</param>
    /// <param name="localSnapshotJson">JSON-serialized local snapshot of the family.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Change set describing the differences between local and library versions.</returns>
    Task<ChangeSetDto> DetectLocalChangesAsync(
        Guid familyId,
        string localSnapshotJson,
        CancellationToken cancellationToken = default);
}
