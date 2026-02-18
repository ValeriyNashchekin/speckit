using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Domain.Interfaces;

/// <summary>
/// Repository interface for FamilyNameMapping entities.
/// </summary>
public interface IFamilyNameMappingRepository : IRepository<FamilyNameMappingEntity>
{
    Task<FamilyNameMappingEntity?> GetByFamilyNameAndProjectAsync(string familyName, Guid projectId, CancellationToken cancellationToken = default);
}
