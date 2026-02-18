namespace FamilyLibrary.Domain.Entities;

/// <summary>
/// Snapshot of family data for comparison and version tracking.
/// </summary>
public class FamilySnapshot
{
    public int Version { get; init; }
    public string FamilyName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public List<string> Types { get; init; } = [];
    public List<ParameterSnapshot> Parameters { get; init; } = [];
    public bool HasGeometryChanges { get; init; }
    public string? TxtHash { get; init; }
}

/// <summary>
/// Snapshot of a single parameter within a family.
/// </summary>
public class ParameterSnapshot
{
    public string Name { get; init; } = string.Empty;
    public string? Value { get; init; }
    public string? Group { get; init; }
}
