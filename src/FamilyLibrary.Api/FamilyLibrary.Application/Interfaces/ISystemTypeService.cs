using FamilyLibrary.Application.Common;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Application.Interfaces;

/// <summary>
/// Service interface for SystemType operations.
/// </summary>
public interface ISystemTypeService
{
    /// <summary>
    /// Gets all system types with pagination and optional filtering.
    /// </summary>
    Task<PagedResult<SystemTypeDto>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        Guid? roleId = null,
        SystemFamilyGroup? group = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a system type by ID.
    /// </summary>
    Task<SystemTypeDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new system type.
    /// </summary>
    Task<Guid> CreateAsync(CreateSystemTypeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing system type.
    /// </summary>
    Task UpdateAsync(Guid id, UpdateSystemTypeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a system type.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all system types for a specific role.
    /// </summary>
    Task<IReadOnlyList<SystemTypeDto>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
}
