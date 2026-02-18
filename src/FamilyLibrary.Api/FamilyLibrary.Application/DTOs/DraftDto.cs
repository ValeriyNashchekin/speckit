using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// DTO for Draft entity.
/// </summary>
public record DraftDto
{
    public Guid Id { get; init; }
    public required string FamilyName { get; init; }
    public required string FamilyUniqueId { get; init; }
    public Guid? SelectedRoleId { get; init; }
    public string? SelectedRoleName { get; init; }
    public Guid? TemplateId { get; init; }
    public DraftStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime LastSeen { get; init; }
}

/// <summary>
/// DTO for creating a new Draft.
/// </summary>
public record CreateDraftDto
{
    public required string FamilyName { get; init; }
    public required string FamilyUniqueId { get; init; }
    public Guid? TemplateId { get; init; }
}

/// <summary>
/// DTO for updating a Draft.
/// </summary>
public record UpdateDraftDto
{
    public Guid? SelectedRoleId { get; init; }
}
