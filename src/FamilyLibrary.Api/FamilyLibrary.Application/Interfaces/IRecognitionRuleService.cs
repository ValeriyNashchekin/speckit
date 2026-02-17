using FamilyLibrary.Application.Common;
using FamilyLibrary.Application.DTOs;

namespace FamilyLibrary.Application.Interfaces;

/// <summary>
/// Service interface for RecognitionRule operations.
/// </summary>
public interface IRecognitionRuleService
{
    /// <summary>
    /// Gets all recognition rules with pagination.
    /// </summary>
    Task<PagedResult<RecognitionRuleDto>> GetAllAsync(
        int pageNumber = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a recognition rule by ID.
    /// </summary>
    Task<RecognitionRuleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new recognition rule.
    /// </summary>
    Task<Guid> CreateAsync(CreateRecognitionRuleDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing recognition rule.
    /// </summary>
    Task UpdateAsync(Guid id, UpdateRecognitionRuleDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a recognition rule.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the formula syntax.
    /// </summary>
    Task<bool> ValidateFormulaAsync(string formula, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests a rule against a family name.
    /// </summary>
    Task<bool> TestRuleAsync(Guid id, string familyName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks for conflicts with other rules.
    /// </summary>
    Task<List<ConflictDto>> CheckConflictsAsync(Guid? excludeId = null, CancellationToken cancellationToken = default);
}
