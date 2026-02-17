namespace FamilyLibrary.Plugin.Core.Entities;

/// <summary>
/// Information about a family in the Revit document.
/// No Revit API dependencies - pure domain model.
/// </summary>
public class FamilyInfo
{
    public string UniqueId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? SourcePath { get; set; }
    public bool IsSystemFamily { get; set; }
    public bool HasStamp { get; set; }
    public EsStampData? StampData { get; set; }
}
