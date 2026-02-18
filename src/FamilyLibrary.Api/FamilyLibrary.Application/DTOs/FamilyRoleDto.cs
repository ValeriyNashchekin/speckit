using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// DTO for FamilyRole entity.
/// </summary>
public record FamilyRoleDto
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
/// DTO for creating a new FamilyRole.
/// </summary>
public record CreateFamilyRoleDto
{
    public required string Name { get; init; }
    public RoleType Type { get; init; }
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
}

/// <summary>
/// DTO for updating a FamilyRole.
/// </summary>
public record UpdateFamilyRoleDto
{
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
}
