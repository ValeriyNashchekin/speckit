namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// DTO for RecognitionRule entity.
/// </summary>
public record RecognitionRuleDto
{
    public Guid Id { get; init; }
    public Guid RoleId { get; init; }
    public required string RootNode { get; init; }
    public required string Formula { get; init; }
}

/// <summary>
/// DTO for creating a RecognitionRule.
/// </summary>
public record CreateRecognitionRuleDto
{
    public Guid RoleId { get; init; }
    public required string RootNode { get; init; }
    public required string Formula { get; init; }
}

/// <summary>
/// DTO for updating a RecognitionRule.
/// </summary>
public record UpdateRecognitionRuleDto
{
    public Guid RoleId { get; init; }
    public string? RootNode { get; init; }
    public string? Formula { get; init; }
}
