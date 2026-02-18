using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// Summary information displayed before loading a parent family into a Revit project.
/// Contains the parent family details and all nested families with their version status.
/// </summary>
public record PreLoadSummaryDto
{
    /// <summary>
    /// Name of the parent family being loaded.
    /// </summary>
    public required string ParentFamilyName { get; init; }

    /// <summary>
    /// Version of the parent family being loaded.
    /// </summary>
    public int ParentVersion { get; init; }

    /// <summary>
    /// List of nested families found within the parent family RFA file.
    /// </summary>
    public List<NestedFamilySummaryDto> NestedFamilies { get; init; } = [];
}

/// <summary>
/// Summary information for a nested family within a parent family.
/// Includes version comparison across RFA file, library, and project,
/// along with the recommended action for loading.
/// </summary>
public record NestedFamilySummaryDto
{
    /// <summary>
    /// Name of the nested family.
    /// </summary>
    public required string FamilyName { get; init; }

    /// <summary>
    /// Role name assigned to the nested family, if any.
    /// </summary>
    public string? RoleName { get; init; }

    /// <summary>
    /// Version of the nested family embedded in the parent RFA file.
    /// Null if the nested family is non-shared (embedded).
    /// </summary>
    public int? RfaVersion { get; init; }

    /// <summary>
    /// Latest version of the family in the library.
    /// Null if the family does not exist in the library.
    /// </summary>
    public int? LibraryVersion { get; init; }

    /// <summary>
    /// Version of the family currently loaded in the Revit project.
    /// Null if the family is not loaded in the project.
    /// </summary>
    public int? ProjectVersion { get; init; }

    /// <summary>
    /// Recommended action for loading this nested family.
    /// </summary>
    public NestedLoadAction RecommendedAction { get; init; }
}
