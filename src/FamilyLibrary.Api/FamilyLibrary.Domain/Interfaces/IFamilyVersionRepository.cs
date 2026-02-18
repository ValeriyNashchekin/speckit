using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Domain.Interfaces;

/// <summary>
/// Repository interface for FamilyVersion entities.
/// </summary>
public interface IFamilyVersionRepository : IRepository<FamilyVersionEntity>
{
    Task<IReadOnlyList<FamilyVersionEntity>> GetByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken = default);
    Task<FamilyVersionEntity?> GetByVersionAsync(Guid familyId, int version, CancellationToken cancellationToken = default);
    Task<FamilyVersionEntity?> GetLatestVersionAsync(Guid familyId, CancellationToken cancellationToken = default);
}
