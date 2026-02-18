namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// DTO for Category entity.
/// </summary>
public record CategoryDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public int SortOrder { get; init; }
}

/// <summary>
/// DTO for creating a new Category.
/// </summary>
public record CreateCategoryDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public int SortOrder { get; init; }
}

/// <summary>
/// DTO for updating a Category.
/// </summary>
public record UpdateCategoryDto
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public int SortOrder { get; init; }
}
