using Autodesk.Revit.DB;
using FamilyLibrary.Plugin.Core.Enums;
using FamilyLibrary.Plugin.Infrastructure.ExtensibleStorage;

namespace FamilyLibrary.Plugin.Core.Entities;

/// <summary>
/// Information about a system family type (WallType, FloorType, etc.).
/// No Revit API dependencies except ElementId for reference.
/// </summary>
public class SystemTypeInfo
{
    public string UniqueId { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SystemFamily { get; set; } = string.Empty;
    public SystemFamilyGroup Group { get; set; }
    public ElementId ElementId { get; set; } = ElementId.InvalidElementId;
    public bool HasStamp { get; set; }
    public EsStampData? StampData { get; set; }
}
