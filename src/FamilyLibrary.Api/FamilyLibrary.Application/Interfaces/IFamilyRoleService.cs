using FamilyLibrary.Application.Common;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Application.Interfaces;

/// <summary>
/// Service interface for FamilyRole operations.
/// </summary>
public interface IFamilyRoleService
{
    /// <summary>
    /// Gets all family roles with pagination and optional filtering.
    /// </summary>
    Task<PagedResult<FamilyRoleDto>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        RoleType? type = null,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a family role by ID.
    /// </summary>
    Task<FamilyRoleDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new family role.
    /// </summary>
    Task<Guid> CreateAsync(CreateFamilyRoleDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing family role.
    /// </summary>
    Task UpdateAsync(Guid id, UpdateFamilyRoleDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a family role. Throws if role has associated families.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports multiple family roles, skipping duplicates.
    /// </summary>
    Task<BatchCreateResult> ImportAsync(
        IReadOnlyList<CreateFamilyRoleDto> dtos,
        CancellationToken cancellationToken = default);
}
