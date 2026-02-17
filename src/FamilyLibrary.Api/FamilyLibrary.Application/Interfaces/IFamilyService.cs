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
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates if a content hash already exists.
    /// </summary>
    Task<bool> ValidateHashAsync(string hash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Batch checks multiple hashes for existence.
    /// </summary>
    Task<List<FamilyStatusDto>> BatchCheckAsync(
        List<string> hashes,
        CancellationToken cancellationToken = default);
}
