using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Domain.Interfaces;

/// <summary>
/// Repository interface for Category entities.
/// </summary>
public interface ICategoryRepository : IRepository<CategoryEntity>
{
    Task<CategoryEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CategoryEntity>> GetOrderedAsync(CancellationToken cancellationToken = default);
}
