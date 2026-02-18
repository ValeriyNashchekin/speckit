#nullable enable

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;

/// <summary>
/// Represents information about a nested family within a parent family.
/// Used by NestedDetectionService to return detected nested families.
/// Pure domain model without Revit API dependencies.
/// </summary>
public class NestedFamilyInfo
{
    /// <summary>
    /// Gets or sets the name of the nested family.
    /// </summary>
    public string FamilyName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the role name assigned to this nested family.
    /// Null if no role is assigned.
    /// </summary>
    public string? RoleName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the nested family is shared.
    /// Shared families can exist independently in the project.
    /// </summary>
    public bool IsShared { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the nested family has an assigned role.
    /// </summary>
    public bool HasRole { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the nested family exists in the library.
    /// </summary>
    public bool InLibrary { get; set; }

    /// <summary>
    /// Gets or sets the library version of the nested family.
    /// Null if not in library or version is unknown.
    /// </summary>
    public int? LibraryVersion { get; set; }

    /// <summary>
    /// Gets or sets the status of the nested family.
    /// Possible values: "ready", "not_published", "no_role".
    /// </summary>
    public string Status { get; set; } = "ready";
}
