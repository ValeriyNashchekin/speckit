using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// DTO for FamilyId entity.
/// </summary>
public record FamilyIdDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public RoleType Type { get; init; }
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
    public string? CategoryName { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// DTO for creating a new FamilyId.
/// </summary>
public record CreateFamilyIdDto
{
    public required string Name { get; init; }
    public RoleType Type { get; init; }
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
}

/// <summary>
/// DTO for updating a FamilyId.
/// </summary>
public record UpdateFamilyIdDto
{
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
}
