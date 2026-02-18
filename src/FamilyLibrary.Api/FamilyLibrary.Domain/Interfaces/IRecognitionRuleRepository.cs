using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Domain.Interfaces;

/// <summary>
/// Repository interface for RecognitionRule entities.
/// </summary>
public interface IRecognitionRuleRepository : IRepository<RecognitionRuleEntity>
{
    Task<RecognitionRuleEntity?> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
}
