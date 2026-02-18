using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// Set of changes detected between family versions.
/// </summary>
public record ChangeSetDto
{
    public List<ChangeItemDto> Items { get; init; } = [];
    public bool HasChanges => Items.Count > 0;
}

/// <summary>
/// Individual change item within a change set.
/// </summary>
public record ChangeItemDto
{
    public ChangeCategory Category { get; init; }
    public string? PreviousValue { get; init; }
    public string? CurrentValue { get; init; }
    public List<string>? AddedItems { get; init; }
    public List<string>? RemovedItems { get; init; }
    public List<ParameterChangeDto>? ParameterChanges { get; init; }
}

/// <summary>
/// Change in a single parameter value.
/// </summary>
public record ParameterChangeDto
{
    public string Name { get; init; } = string.Empty;
    public string? PreviousValue { get; init; }
    public string? CurrentValue { get; init; }
}
