using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Domain.Interfaces;

/// <summary>
/// Repository interface for Tag entities.
/// </summary>
public interface ITagRepository : IRepository<TagEntity>
{
    Task<TagEntity?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
