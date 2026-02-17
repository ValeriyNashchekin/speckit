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
    Task<PagedResult<FamilyDto>> GetAllAsync(
        int page,
        int pageSize,
        Guid? roleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a family by ID with full details including versions.
    /// </summary>
    Task<FamilyDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

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
