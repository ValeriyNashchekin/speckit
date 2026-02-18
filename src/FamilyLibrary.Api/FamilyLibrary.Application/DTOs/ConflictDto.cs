namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// DTO representing a conflict between recognition rules.
/// </summary>
public record ConflictDto
{
    /// <summary>
    /// The ID of the first conflicting rule.
    /// </summary>
    public Guid RuleId1 { get; init; }

    /// <summary>
    /// The ID of the second conflicting rule.
    /// </summary>
    public Guid RuleId2 { get; init; }

    /// <summary>
    /// The name of the first conflicting role.
    /// </summary>
    public required string RoleName1 { get; init; }

    /// <summary>
    /// The name of the second conflicting role.
    /// </summary>
    public required string RoleName2 { get; init; }

    /// <summary>
    /// Description of the conflict.
    /// </summary>
    public required string Description { get; init; }
}
