namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// DTO for Family entity.
/// </summary>
public record FamilyDto
{
    public Guid Id { get; init; }
    public Guid RoleId { get; init; }
    public required string RoleName { get; init; }
    public required string FamilyName { get; init; }
    public int CurrentVersion { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// DTO for Family with versions.
/// </summary>
public record FamilyDetailDto : FamilyDto
{
    public required List<FamilyVersionDto> Versions { get; init; }
}

/// <summary>
/// DTO for creating a new Family.
/// </summary>
public record CreateFamilyDto
{
    public Guid RoleId { get; init; }
    public required string FamilyName { get; init; }
}
