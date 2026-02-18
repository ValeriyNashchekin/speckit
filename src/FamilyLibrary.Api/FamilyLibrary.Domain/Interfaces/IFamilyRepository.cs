using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Domain.Interfaces;

/// <summary>
/// Repository interface for Family entities.
/// </summary>
public interface IFamilyRepository : IRepository<FamilyEntity>
{
    /// <summary>
    /// Gets all families with their roles (including category and tags) loaded.
    /// </summary>
    Task<IReadOnlyList<FamilyEntity>> GetAllWithRolesAsync(CancellationToken cancellationToken = default);

    Task<FamilyEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<FamilyEntity?> GetWithVersionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FamilyEntity>> GetWithVersionsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FamilyEntity>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> HashExistsAsync(string hash, CancellationToken cancellationToken = default);
    Task<FamilyEntity?> GetByHashAsync(string hash, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets families with filtering and pagination at database level.
    /// </summary>
    Task<(IReadOnlyList<FamilyEntity> Items, int TotalCount)> GetFilteredAsync(
        Guid? roleId,
        string? searchTerm,
        Guid? categoryId,
        List<Guid>? tagIds,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a family by role ID and name (case-insensitive).
    /// </summary>
    Task<FamilyEntity?> GetByRoleAndNameAsync(Guid roleId, string familyName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets families by multiple hashes (batch lookup).
    /// </summary>
    Task<IReadOnlyList<FamilyEntity>> GetByHashesAsync(IEnumerable<string> hashes, CancellationToken cancellationToken = default);
}
