namespace FamilyLibrary.Plugin.Core.Models;

/// <summary>
/// Snapshot of a family's state for comparison purposes.
/// </summary>
public class FamilySnapshot
{
    public int Version { get; set; }
    public string FamilyName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> Types { get; set; } = new List<string>();
    public List<ParameterSnapshot> Parameters { get; set; } = new List<ParameterSnapshot>();
    public bool HasGeometryChanges { get; set; }
    public string? TxtHash { get; set; }
}
