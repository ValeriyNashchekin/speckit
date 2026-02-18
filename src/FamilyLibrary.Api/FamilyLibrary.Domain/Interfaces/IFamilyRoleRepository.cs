using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Domain.Interfaces;

/// <summary>
/// Repository interface for FamilyRole entities.
/// </summary>
public interface IFamilyRoleRepository : IRepository<FamilyRoleEntity>
{
    Task<FamilyRoleEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FamilyRoleEntity>> GetByCategoryIdAsync(Guid categoryId, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
    Task<bool> HasFamiliesAsync(Guid id, CancellationToken cancellationToken = default);
}
