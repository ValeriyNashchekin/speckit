using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// DTO for SystemType entity.
/// </summary>
public record SystemTypeDto
{
    public Guid Id { get; init; }
    public Guid RoleId { get; init; }
    public required string RoleName { get; init; }
    public required string TypeName { get; init; }
    public required string Category { get; init; }
    public required string SystemFamily { get; init; }
    public SystemFamilyGroup Group { get; init; }
    public required string Json { get; init; }
    public int CurrentVersion { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}

/// <summary>
/// DTO for creating a new SystemType.
/// </summary>
public record CreateSystemTypeDto
{
    public Guid RoleId { get; init; }
    public required string TypeName { get; init; }
    public required string Category { get; init; }
    public required string SystemFamily { get; init; }
    public SystemFamilyGroup Group { get; init; }
    public required string Json { get; init; }
    public required string Hash { get; init; }
}

/// <summary>
/// DTO for updating an existing SystemType.
/// </summary>
public record UpdateSystemTypeDto
{
    public required string Json { get; init; }
    public required string Hash { get; init; }
}
