using FamilyLibrary.Plugin.Core.Enums;

namespace FamilyLibrary.Plugin.Core.Models;

/// <summary>
/// Snapshot of a parameter's value.
/// </summary>
public class ParameterSnapshot
{
    public string Name { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string? Group { get; set; }
}

/// <summary>
/// Set of changes detected between two family snapshots.
/// </summary>
public class ChangeSet
{
    public List<ChangeItem> Items { get; set; } = new List<ChangeItem>();
    public bool HasChanges => Items.Count > 0;
}

/// <summary>
/// Individual change item in a changeset.
/// </summary>
public class ChangeItem
{
    public ChangeCategory Category { get; set; }
    public string? PreviousValue { get; set; }
    public string? CurrentValue { get; set; }
    public List<string>? AddedItems { get; set; }
    public List<string>? RemovedItems { get; set; }
    public List<ParameterChange>? ParameterChanges { get; set; }
}

/// <summary>
/// Change to a specific parameter.
/// </summary>
public class ParameterChange
{
    public string Name { get; set; } = string.Empty;
    public string? PreviousValue { get; set; }
    public string? CurrentValue { get; set; }
}
