using FamilyLibrary.Application.Common;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Application.Interfaces;

/// <summary>
/// Service interface for FamilyId operations.
/// </summary>
public interface IFamilyIdService
{
    /// <summary>
    /// Gets all family ids with pagination and optional filtering.
    /// </summary>
    Task<PagedResult<FamilyIdDto>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        RoleType? type = null,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a family id by ID.
    /// </summary>
    Task<FamilyIdDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new family id.
    /// </summary>
    Task<Guid> CreateAsync(CreateFamilyIdDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing family id.
    /// </summary>
    Task UpdateAsync(Guid id, UpdateFamilyIdDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a family id. Throws if role has associated families.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports multiple family ids, skipping duplicates.
    /// </summary>
    Task<BatchCreateResult> ImportAsync(
        IReadOnlyList<CreateFamilyIdDto> dtos,
        CancellationToken cancellationToken = default);
}
