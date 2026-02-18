namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// DTO for Tag entity.
/// </summary>
public record TagDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Color { get; init; }
}

/// <summary>
/// DTO for creating a new Tag.
/// </summary>
public record CreateTagDto
{
    public required string Name { get; init; }
    public string? Color { get; init; }
}

/// <summary>
/// DTO for updating a Tag.
/// </summary>
public record UpdateTagDto
{
    public required string Name { get; init; }
    public string? Color { get; init; }
}
