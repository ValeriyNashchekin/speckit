using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Domain.Interfaces;

/// <summary>
/// Repository interface for Draft entities.
/// </summary>
public interface IDraftRepository : IRepository<DraftEntity>
{
    Task<DraftEntity?> GetByFamilyUniqueIdAsync(string familyUniqueId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DraftEntity>> GetByStatusAsync(DraftStatus status, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DraftEntity>> GetByTemplateIdAsync(Guid templateId, CancellationToken cancellationToken = default);
}
