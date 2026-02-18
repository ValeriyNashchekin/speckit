using FamilyLibrary.Application.DTOs;

namespace FamilyLibrary.Application.Interfaces;

/// <summary>
/// Service interface for nested family dependency operations.
/// Provides functionality for analyzing and managing nested family dependencies
/// within parent family RFA files.
/// </summary>
public interface INestedFamilyService
{
    /// <summary>
    /// Gets all nested family dependencies for a given parent family.
    /// </summary>
    /// <param name="familyId">The unique identifier of the parent family.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of nested family dependencies with their version status.</returns>
    Task<List<NestedFamilyDto>> GetDependenciesAsync(
        Guid familyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a pre-load summary for a family, including all nested families
    /// and recommended load actions.
    /// </summary>
    /// <param name="familyId">The unique identifier of the parent family.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Pre-load summary with nested families and recommended actions.</returns>
    Task<PreLoadSummaryDto> GetPreLoadSummaryAsync(
        Guid familyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets information about where a nested family (by role name) is used.
    /// Returns all parent families that reference the specified nested family role.
    /// </summary>
    /// <param name="roleName">The role name of the nested family to search for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Information about where the nested family is used.</returns>
    Task<UsedInDto> GetUsedInAsync(
        string roleName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves nested family dependencies detected by the Revit plugin during publishing.
    /// Replaces any existing dependencies for the parent family.
    /// </summary>
    /// <param name="familyId">The unique identifier of the parent family.</param>
    /// <param name="dependencies">List of nested family dependencies to save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Count of dependencies saved.</returns>
    Task<int> SaveDependenciesAsync(
        Guid familyId,
        List<SaveDependencyDto> dependencies,
        CancellationToken cancellationToken = default);
}
