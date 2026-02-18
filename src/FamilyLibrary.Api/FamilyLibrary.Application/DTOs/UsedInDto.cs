namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// DTO representing where a nested family is used within parent families.
/// </summary>
public record UsedInDto
{
    /// <summary>
    /// Name of the nested family.
    /// </summary>
    public required string NestedFamilyName { get; init; }

    /// <summary>
    /// List of parent families that reference this nested family.
    /// </summary>
    public List<ParentReferenceDto> ParentFamilies { get; init; } = [];
}

/// <summary>
/// DTO representing a parent family that references a nested family.
/// </summary>
public record ParentReferenceDto
{
    /// <summary>
    /// Unique identifier of the parent family.
    /// </summary>
    public Guid FamilyId { get; init; }

    /// <summary>
    /// Name of the parent family.
    /// </summary>
    public required string FamilyName { get; init; }

    /// <summary>
    /// Optional role name of the parent family.
    /// </summary>
    public string? RoleName { get; init; }

    /// <summary>
    /// Version of the nested family embedded in the parent.
    /// </summary>
    public int NestedVersionInParent { get; init; }

    /// <summary>
    /// Latest version of the parent family.
    /// </summary>
    public int ParentLatestVersion { get; init; }
}
