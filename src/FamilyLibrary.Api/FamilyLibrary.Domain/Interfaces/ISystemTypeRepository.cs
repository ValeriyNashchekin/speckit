using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Domain.Interfaces;

/// <summary>
/// Repository interface for SystemType entities.
/// </summary>
public interface ISystemTypeRepository : IRepository<SystemTypeEntity>
{
    Task<SystemTypeEntity?> GetByTypeNameAsync(string typeName, string category, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SystemTypeEntity>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SystemTypeEntity>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
}
