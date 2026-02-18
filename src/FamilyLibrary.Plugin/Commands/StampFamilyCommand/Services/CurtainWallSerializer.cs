using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;
using Newtonsoft.Json;

#nullable enable

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;

/// <summary>
/// Serializes CurtainWallType (Group D) to JSON format.
/// Stores grid settings, panel, and mullion assignments.
/// Supports multi-version Revit (2020-2026).
/// </summary>
public class CurtainSerializer
{
    public CurtainWallJson Serialize(WallType wallType, Document document)
    {
        var result = new CurtainWallJson
        {
            TypeName = wallType.Name,
            Kind = "Curtain",
            Category = "Walls",
            SystemFamily = "Curtain Wall"
        };

        if (document == null || wallType == null)
            return result;

        if (wallType.Kind != WallKind.Curtain)
        {
            result.Kind = wallType.Kind.ToString();
            return result;
        }

        SerializeParameters(wallType, result.Parameters);
        SerializeGridSettings(wallType, result.Grid);
        SerializePanelSettings(wallType, document, result.Panels, result.Dependencies);
        SerializeMullionSettings(wallType, document, result.Mullions, result.Dependencies);

        return result;
    }

    public string SerializeToJson(WallType wallType, Document document)
    {
        var model = Serialize(wallType, document);
        return JsonConvert.SerializeObject(model, Formatting.Indented);
    }

    public List<string> ValidateDependencies(CurtainWallJson curtainJson, Document document)
    {
        var missing = new List<string>();

        if (curtainJson?.Dependencies == null || document == null)
            return missing;

        var panelsCache = GetCurtainPanels(document);
        var mullionsCache = GetMullions(document);

        foreach (var dep in curtainJson.Dependencies)
        {
            if (string.IsNullOrEmpty(dep.TypeName))
                continue;

            bool exists;
            if ("Panel".Equals(dep.DependencyType, StringComparison.OrdinalIgnoreCase))
            {
                exists = panelsCache.Contains(dep.TypeName);
            }
            else if ("Mullion".Equals(dep.DependencyType, StringComparison.OrdinalIgnoreCase))
            {
                exists = mullionsCache.Contains(dep.TypeName);
            }
            else
            {
                exists = panelsCache.Contains(dep.TypeName) || mullionsCache.Contains(dep.TypeName);
            }

            if (!exists)
            {
                missing.Add(dep.TypeName);
            }
        }

        return missing;
    }

    private void SerializeParameters(WallType wallType, CurtainParametersJson parameters)
    {
        try
        {
            parameters.Width = wallType.Width;
            parameters.Function = wallType.Function.ToString();
        }
        catch
        {
        }
    }

    private void SerializeGridSettings(WallType wallType, CurtainGridSettingsJson grid)
    {
        try
        {
            var gridParams = GetCurtainGridParameters(wallType);
            if (gridParams != null)
            {
                grid.HorizontalSpacing = gridParams.HorizontalSpacing;
                grid.VerticalSpacing = gridParams.VerticalSpacing;
                grid.HorizontalGridJustification = gridParams.HorizontalJustification;
                grid.VerticalGridJustification = gridParams.VerticalJustification;
            }
        }
        catch
        {
        }
    }

    private void SerializePanelSettings(
        WallType wallType,
        Document document,
        CurtainPanelSettingsJson panels,
        List<CurtainDependencyJson> dependencies)
    {
        try
        {
            var defaultPanelParam = wallType.get_Parameter(BuiltInParameter.CURTAIN_WALL_PANEL);
            if (defaultPanelParam != null)
            {
                var panelId = defaultPanelParam.AsElementId();
                if (panelId != null && panelId != ElementId.InvalidElementId)
                {
                    var panelElement = document.GetElement(panelId);
                    if (panelElement != null)
                    {
                        panels.DefaultPanelTypeName = panelElement.Name;
                        panels.IsLoadable = panelElement is FamilySymbol;

                        dependencies.Add(new CurtainDependencyJson
                        {
                            TypeName = panelElement.Name,
                            Category = "Curtain Panels",
                            DependencyType = "Panel",
                            InLibrary = false
                        });
                    }
                }
            }
        }
        catch
        {
        }
    }

    private void SerializeMullionSettings(
        WallType wallType,
        Document document,
        CurtainMullionSettingsJson mullions,
        List<CurtainDependencyJson> dependencies)
    {
        try
        {
            SerializeMullionByParameter(
                wallType, document,
                BuiltInParameter.CURTAIN_WALL_MULLION_HORIZ,
                mullions, dependencies,
                ref mullions.HorizontalMullionTypeName);

            SerializeMullionByParameter(
                wallType, document,
                BuiltInParameter.CURTAIN_WALL_MULLION_VERT,
                mullions, dependencies,
                ref mullions.VerticalMullionTypeName);
        }
        catch
        {
        }
    }

    private void SerializeMullionByParameter(
        WallType wallType,
        Document document,
        BuiltInParameter paramId,
        CurtainMullionSettingsJson mullions,
        List<CurtainDependencyJson> dependencies,
        ref string? typeNameField)
    {
        var mullionParam = wallType.get_Parameter(paramId);
        if (mullionParam == null)
            return;

        var mullionId = mullionParam.AsElementId();
        if (mullionId == null || mullionId == ElementId.InvalidElementId)
            return;

        var mullionElement = document.GetElement(mullionId);
        if (mullionElement == null)
            return;

        typeNameField = mullionElement.Name;

        dependencies.Add(new CurtainDependencyJson
        {
            TypeName = mullionElement.Name,
            Category = "Curtain Mullions",
            DependencyType = "Mullion",
            InLibrary = false
        });
    }

    private CurtainGridParams? GetCurtainGridParameters(WallType wallType)
    {
        var result = new CurtainGridParams();

        try
        {
            var spacingH = wallType.get_Parameter(BuiltInParameter.CURTAIN_GRID_HORIZ_SPACING);
            if (spacingH != null)
                result.HorizontalSpacing = spacingH.AsDouble();

            var spacingV = wallType.get_Parameter(BuiltInParameter.CURTAIN_GRID_VERT_SPACING);
            if (spacingV != null)
                result.VerticalSpacing = spacingV.AsDouble();

            var justH = wallType.get_Parameter(BuiltInParameter.CURTAIN_GRID_HORIZ_JUSTIFICATION);
            if (justH != null)
                result.HorizontalJustification = justH.AsValueString();

            var justV = wallType.get_Parameter(BuiltInParameter.CURTAIN_GRID_VERT_JUSTIFICATION);
            if (justV != null)
                result.VerticalJustification = justV.AsValueString();

            return result;
        }
        catch
        {
            return null;
        }
    }

    private HashSet<string> GetCurtainPanels(Document document)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var panels = new FilteredElementCollector(document)
            .OfCategory(BuiltInCategory.OST_CurtainWallPanels)
            .WhereElementIsElementType()
            .ToElements();

        foreach (var panel in panels)
        {
            if (!string.IsNullOrEmpty(panel.Name))
                result.Add(panel.Name);
        }

        return result;
    }

    private HashSet<string> GetMullions(Document document)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var mullions = new FilteredElementCollector(document)
            .OfCategory(BuiltInCategory.OST_CurtainWallMullions)
            .WhereElementIsElementType()
            .ToElements();

        foreach (var mullion in mullions)
        {
            if (!string.IsNullOrEmpty(mullion.Name))
                result.Add(mullion.Name);
        }

        return result;
    }

    private class CurtainGridParams
    {
        public double HorizontalSpacing { get; set; }
        public double VerticalSpacing { get; set; }
        public string? HorizontalJustification { get; set; }
        public string? VerticalJustification { get; set; }
    }
}
