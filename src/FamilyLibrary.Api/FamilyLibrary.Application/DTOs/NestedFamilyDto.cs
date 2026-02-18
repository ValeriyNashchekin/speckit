namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// DTO for saving a nested family dependency detected by the Revit plugin.
/// </summary>
public record SaveDependencyDto
{
    /// <summary>
    /// Name of the nested family as it appears in the RFA file.
    /// </summary>
    public required string NestedFamilyName { get; init; }

    /// <summary>
    /// Role name if the nested family is Shared and has been stamped.
    /// </summary>
    public string? NestedRoleName { get; init; }

    /// <summary>
    /// Indicates whether the nested family is a Shared family.
    /// </summary>
    public bool IsShared { get; init; }

    /// <summary>
    /// Indicates whether the nested family exists in the library.
    /// </summary>
    public bool InLibrary { get; init; }

    /// <summary>
    /// Current version number in the library, if InLibrary is true.
    /// </summary>
    public int? LibraryVersion { get; init; }
}

/// <summary>
/// Request DTO for saving multiple nested family dependencies at once.
/// </summary>
public record SaveDependenciesRequest
{
    /// <summary>
    /// List of nested family dependencies to save.
    /// </summary>
    public required List<SaveDependencyDto> Dependencies { get; init; }
}

/// <summary>
/// DTO representing a nested family within a parent family RFA file.
/// Contains information about the family and its version status across library, RFA file, and project.
/// </summary>
public record NestedFamilyDto
{
    /// <summary>
    /// Name of the nested family.
    /// </summary>
    public required string FamilyName { get; init; }

    /// <summary>
    /// Role name of the nested family, if assigned.
    /// </summary>
    public string? RoleName { get; init; }

    /// <summary>
    /// Indicates whether the nested family is a shared family.
    /// </summary>
    public bool IsShared { get; init; }

    /// <summary>
    /// Indicates whether the nested family exists in the family library.
    /// </summary>
    public bool InLibrary { get; init; }

    /// <summary>
    /// Version number of the family in the library, if exists.
    /// </summary>
    public int? LibraryVersion { get; init; }

    /// <summary>
    /// Version of the nested family embedded in the parent RFA file.
    /// </summary>
    public int? RfaVersion { get; init; }

    /// <summary>
    /// Version of the nested family currently loaded in the Revit project.
    /// </summary>
    public int? ProjectVersion { get; init; }
}
