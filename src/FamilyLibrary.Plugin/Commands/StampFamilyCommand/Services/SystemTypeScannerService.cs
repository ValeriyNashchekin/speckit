using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using FamilyLibrary.Plugin.Core.Entities;
using FamilyLibrary.Plugin.Core.Enums;
using FamilyLibrary.Plugin.Core.Interfaces;
using FamilyLibrary.Plugin.Infrastructure.ExtensibleStorage;

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;

/// <summary>
/// Service for scanning system family types (WallType, FloorType, PipeType, DuctType, etc.).
/// Uses FilteredElementCollector with optimized filters.
/// </summary>
public class SystemTypeScannerService
{
    private readonly IEsService _esService;
    private readonly CompoundStructureSerializer _compoundStructureSerializer;
    private readonly RoutingPreferencesSerializer _routingPreferencesSerializer;

    // GroupA categories: CompoundStructure support
    private static readonly BuiltInCategory[] GroupACategories =
    {
        BuiltInCategory.OST_Walls,
        BuiltInCategory.OST_Floors,
        BuiltInCategory.OST_Roofs,
        BuiltInCategory.OST_Ceilings,
        BuiltInCategory.OST_StructuralFoundation
    };

    // GroupB categories: MEP with RoutingPreferences (Pipes, Ducts)
    private static readonly BuiltInCategory[] GroupBCategories =
    {
        BuiltInCategory.OST_PipeSegments,
        BuiltInCategory.OST_DuctCurves
    };

    // GroupE categories: Simple parameters only
    private static readonly BuiltInCategory[] GroupECategories =
    {
        BuiltInCategory.OST_Levels,
        BuiltInCategory.OST_Grids,
        BuiltInCategory.OST_Ramps,
        BuiltInCategory.OST_BuildingPad
    };

    public SystemTypeScannerService()
        : this(new EsService(), new CompoundStructureSerializer(), new RoutingPreferencesSerializer()) { }

    public SystemTypeScannerService(
        IEsService esService,
        CompoundStructureSerializer compoundStructureSerializer,
        RoutingPreferencesSerializer routingPreferencesSerializer)
    {
        _esService = esService;
        _compoundStructureSerializer = compoundStructureSerializer;
        _routingPreferencesSerializer = routingPreferencesSerializer;
    }

    /// <summary>
    /// Scans document for system family types of specified categories.
    /// </summary>
    public List<SystemTypeInfo> ScanSystemTypes(Document document, params BuiltInCategory[] categories)
    {
        if (document == null || categories == null || categories.Length == 0)
            return new List<SystemTypeInfo>();

        // Collect ElementTypes ONCE - avoid collector in loop
        var elementTypes = new FilteredElementCollector(document)
            .WhereElementIsElementType()
            .ToElements();

        var result = new List<SystemTypeInfo>();
        var categorySet = new HashSet<BuiltInCategory>(categories);

        foreach (var elementType in elementTypes)
        {
            var category = elementType.Category;
            if (category == null) continue;

            var bic = (BuiltInCategory)GetElementIdValue(category.Id);
            if (!categorySet.Contains(bic)) continue;

            var info = CreateSystemTypeInfo(document, elementType, bic);
            if (info != null)
                result.Add(info);
        }

        return result;
    }

    /// <summary>
    /// Gets the system family group for a category.
    /// </summary>
    public SystemFamilyGroup GetGroupForCategory(BuiltInCategory category)
    {
        if (Array.IndexOf(GroupACategories, category) >= 0)
            return SystemFamilyGroup.GroupA;

        if (Array.IndexOf(GroupBCategories, category) >= 0)
            return SystemFamilyGroup.GroupB;

        if (Array.IndexOf(GroupECategories, category) >= 0)
            return SystemFamilyGroup.GroupE;

        return SystemFamilyGroup.GroupA; // Default
    }

    /// <summary>
    /// Gets all WallTypes from document.
    /// </summary>
    public List<SystemTypeInfo> GetWallTypes(Document document)
    {
        return ScanSystemTypes(document, BuiltInCategory.OST_Walls);
    }

    /// <summary>
    /// Gets all FloorTypes from document.
    /// </summary>
    public List<SystemTypeInfo> GetFloorTypes(Document document)
    {
        return ScanSystemTypes(document, BuiltInCategory.OST_Floors);
    }

    /// <summary>
    /// Gets all RoofTypes from document.
    /// </summary>
    public List<SystemTypeInfo> GetRoofTypes(Document document)
    {
        return ScanSystemTypes(document, BuiltInCategory.OST_Roofs);
    }

    /// <summary>
    /// Gets all CeilingTypes from document.
    /// </summary>
    public List<SystemTypeInfo> GetCeilingTypes(Document document)
    {
        return ScanSystemTypes(document, BuiltInCategory.OST_Ceilings);
    }

    /// <summary>
    /// Gets all FoundationTypes from document.
    /// </summary>
    public List<SystemTypeInfo> GetFoundationTypes(Document document)
    {
        return ScanSystemTypes(document, BuiltInCategory.OST_StructuralFoundation);
    }

    /// <summary>
    /// Gets all GroupA types (Walls, Floors, Roofs, Ceilings, Foundations).
    /// </summary>
    public List<SystemTypeInfo> GetGroupATypes(Document document)
    {
        return ScanSystemTypes(document, GroupACategories);
    }

    /// <summary>
    /// Gets all PipeTypes from document.
    /// </summary>
    public List<SystemTypeInfo> GetPipeTypes(Document document)
    {
        if (document == null) return new List<SystemTypeInfo>();

        var result = new List<SystemTypeInfo>();
        var pipeTypes = new FilteredElementCollector(document)
            .OfClass(typeof(PipeType))
            .WhereElementIsElementType()
            .Cast<PipeType>();

        foreach (var pipeType in pipeTypes)
        {
            var info = CreateSystemTypeInfoFromMepCurveType(document, pipeType, "Pipe");
            if (info != null) result.Add(info);
        }

        return result;
    }

    /// <summary>
    /// Gets all DuctTypes from document.
    /// </summary>
    public List<SystemTypeInfo> GetDuctTypes(Document document)
    {
        if (document == null) return new List<SystemTypeInfo>();

        var result = new List<SystemTypeInfo>();
        var ductTypes = new FilteredElementCollector(document)
            .OfClass(typeof(DuctType))
            .WhereElementIsElementType()
            .Cast<DuctType>();

        foreach (var ductType in ductTypes)
        {
            var info = CreateSystemTypeInfoFromMepCurveType(document, ductType, "Duct");
            if (info != null) result.Add(info);
        }

        return result;
    }

    /// <summary>
    /// Gets all GroupB types (Pipes, Ducts).
    /// </summary>
    public List<SystemTypeInfo> GetGroupBTypes(Document document)
    {
        var result = new List<SystemTypeInfo>();
        result.AddRange(GetPipeTypes(document));
        result.AddRange(GetDuctTypes(document));
        return result;
    }

    /// <summary>
    /// Gets levels and grids (GroupE).
    /// </summary>
    public List<SystemTypeInfo> GetSimpleSystemTypes(Document document)
    {
        if (document == null) return new List<SystemTypeInfo>();

        var result = new List<SystemTypeInfo>();

        // Collect Levels
        var levels = new FilteredElementCollector(document)
            .OfClass(typeof(Level))
            .WhereElementIsNotElementType()
            .Cast<Level>();

        foreach (var level in levels)
        {
            var info = CreateSystemTypeInfoFromElement(level, BuiltInCategory.OST_Levels, "Level");
            if (info != null) result.Add(info);
        }

        // Collect Grids
        var grids = new FilteredElementCollector(document)
            .OfClass(typeof(Grid))
            .WhereElementIsNotElementType()
            .Cast<Grid>();

        foreach (var grid in grids)
        {
            var info = CreateSystemTypeInfoFromElement(grid, BuiltInCategory.OST_Grids, "Grid");
            if (info != null) result.Add(info);
        }

        return result;
    }

    /// <summary>
    /// Gets all system types from all supported categories.
    /// </summary>
    public List<SystemTypeInfo> GetAllSystemTypes(Document document)
    {
        var result = new List<SystemTypeInfo>();

        // Group A: CompoundStructure types
        result.AddRange(GetGroupATypes(document));

        // Group B: MEP types with RoutingPreferences
        result.AddRange(GetGroupBTypes(document));

        // Group E: Simple types
        result.AddRange(GetSimpleSystemTypes(document));

        return result;
    }

    private SystemTypeInfo? CreateSystemTypeInfo(Document document, Element elementType, BuiltInCategory category)
    {
        var stampData = _esService.ReadStamp(elementType);
        var group = GetGroupForCategory(category);
        string? compoundStructureSnapshot = null;
        string? routingPreferencesSnapshot = null;

        // Serialize CompoundStructure for Group A types
        if (group == SystemFamilyGroup.GroupA)
        {
            compoundStructureSnapshot = TrySerializeCompoundStructure(document, elementType);
        }

        // Serialize RoutingPreferences for Group B types
        if (group == SystemFamilyGroup.GroupB)
        {
            routingPreferencesSnapshot = TrySerializeRoutingPreferences(document, elementType);
        }

        return new SystemTypeInfo
        {
            UniqueId = elementType.UniqueId,
            TypeName = elementType.Name,
            Category = category.ToString().Replace("OST_", ""),
            SystemFamily = GetSystemFamilyName(elementType),
            Group = group,
            ElementId = elementType.Id,
            HasStamp = stampData?.IsValid == true,
            StampData = stampData,
            CompoundStructureSnapshot = compoundStructureSnapshot,
            RoutingPreferencesSnapshot = routingPreferencesSnapshot
        };
    }

    /// <summary>
    /// Tries to serialize CompoundStructure from a HostObjAttributes element.
    /// Returns null if the element does not support CompoundStructure.
    /// </summary>
    private string? TrySerializeCompoundStructure(Document document, Element elementType)
    {
        if (!(elementType is HostObjAttributes hostAttributes))
            return null;

        try
        {
            var structure = hostAttributes.GetCompoundStructure();
            if (structure == null)
                return null;

            return _compoundStructureSerializer.Serialize(structure, document);
        }
        catch
        {
            // Some types may not support CompoundStructure
            return null;
        }
    }

    /// <summary>
    /// Tries to serialize RoutingPreferences from a PipeType or DuctType.
    /// Returns null if the element does not support RoutingPreferences.
    /// </summary>
    private string? TrySerializeRoutingPreferences(Document document, Element elementType)
    {
        try
        {
            if (elementType is PipeType pipeType)
            {
                return _routingPreferencesSerializer.SerializeToJson(pipeType, document);
            }

            if (elementType is DuctType ductType)
            {
                return _routingPreferencesSerializer.SerializeToJson(ductType, document);
            }
        }
        catch
        {
            // Some types may not support RoutingPreferences
        }

        return null;
    }

    /// <summary>
    /// Creates SystemTypeInfo from MEPCurveType (PipeType or DuctType).
    /// Includes RoutingPreferences serialization for Group B.
    /// </summary>
    private SystemTypeInfo? CreateSystemTypeInfoFromMepCurveType(
        Document document,
        MEPCurveType mepCurveType,
        string systemFamily)
    {
        if (mepCurveType == null) return null;

        var stampData = _esService.ReadStamp(mepCurveType);
        var routingPreferencesSnapshot = TrySerializeRoutingPreferences(document, mepCurveType);
        var category = mepCurveType.Category;

        return new SystemTypeInfo
        {
            UniqueId = mepCurveType.UniqueId,
            TypeName = mepCurveType.Name,
            Category = category?.Name?.Replace("OST_", "") ?? systemFamily,
            SystemFamily = systemFamily,
            Group = SystemFamilyGroup.GroupB,
            ElementId = mepCurveType.Id,
            HasStamp = stampData?.IsValid == true,
            StampData = stampData,
            RoutingPreferencesSnapshot = routingPreferencesSnapshot
        };
    }

    private SystemTypeInfo? CreateSystemTypeInfoFromElement(
        Element element, 
        BuiltInCategory category, 
        string systemFamily)
    {
        var stampData = _esService.ReadStamp(element);

        return new SystemTypeInfo
        {
            UniqueId = element.UniqueId,
            TypeName = element.Name,
            Category = category.ToString().Replace("OST_", ""),
            SystemFamily = systemFamily,
            Group = SystemFamilyGroup.GroupE,
            ElementId = element.Id,
            HasStamp = stampData?.IsValid == true,
            StampData = stampData
        };
    }

    private static string GetSystemFamilyName(Element elementType)
    {
        return elementType switch
        {
            WallType _ => "Wall",
            FloorType _ => "Floor",
            RoofType _ => "Roof",
            CeilingType _ => "Ceiling",
            WallFoundationType _ => "Foundation",
            PipeType _ => "Pipe",
            DuctType _ => "Duct",
            _ => elementType.Category?.Name ?? "Unknown"
        };
    }

    /// <summary>
    /// Gets the integer value of an ElementId in a version-compatible way.
    /// </summary>
    private static int GetElementIdValue(ElementId elementId)
    {
#if REVIT2024 || REVIT2025 || REVIT2026
        return (int)elementId.Value;
#else
        return elementId.IntegerValue;
#endif
    }
}
