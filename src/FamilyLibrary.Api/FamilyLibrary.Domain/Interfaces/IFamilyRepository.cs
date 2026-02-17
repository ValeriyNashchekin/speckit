using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Domain.Interfaces;

/// <summary>
/// Repository interface for Family entities.
/// </summary>
public interface IFamilyRepository : IRepository<FamilyEntity>
{
    Task<FamilyEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<FamilyEntity?> GetWithVersionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FamilyEntity>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> HashExistsAsync(string hash, CancellationToken cancellationToken = default);
    Task<FamilyEntity?> GetByHashAsync(string hash, CancellationToken cancellationToken = default);
}
