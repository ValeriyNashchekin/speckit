using Autodesk.Revit.DB;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;
using Newtonsoft.Json;

#nullable enable

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;

/// <summary>
/// Serializes CurtainWallType (WallKind.Curtain) to JSON format.
/// Extracts panel dependencies for Pull Update validation.
/// Supports multi-version Revit (2020-2026).
/// </summary>
public class CurtainSerializer
{
    /// <summary>
    /// Serializes CurtainWallType to structured JSON model.
    /// </summary>
    public CurtainWallTypeJson Serialize(WallType wallType, Document document)
    {
        if (wallType.Kind != WallKind.Curtain)
            throw new System.ArgumentException("WallType is not a curtain wall");

        var result = new CurtainWallTypeJson
        {
            TypeName = wallType.Name,
            Kind = "Curtain",
            Category = "Walls",
            SystemFamily = "Curtain Wall"
        };

        SerializeParameters(wallType, result);
        SerializeGridSettings(wallType, result);
        SerializePanelSettings(wallType, document, result);
        SerializeMullionSettings(wallType, document, result);
        ExtractDependencies(wallType, document, result);

        return result;
    }

    /// <summary>
    /// Serializes CurtainWallType to JSON string.
    /// </summary>
    public string SerializeToJson(WallType wallType, Document document)
    {
        var model = Serialize(wallType, document);
        return JsonConvert.SerializeObject(model, Formatting.Indented);
    }

    private void SerializeParameters(WallType wallType, CurtainWallTypeJson result)
    {
        var unconnectedHeightParam = wallType.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM);
        if (unconnectedHeightParam != null && unconnectedHeightParam.StorageType == StorageType.Double)
        {
            result.Parameters["UnconnectedHeight"] = unconnectedHeightParam.AsDouble();
        }

        var locationLineParam = wallType.get_Parameter(BuiltInParameter.WALL_KEY_REF_PARAM);
        if (locationLineParam != null)
        {
            result.Parameters["WallLocationLine"] = locationLineParam.AsInteger();
        }
    }

    private void SerializeGridSettings(WallType wallType, CurtainWallTypeJson result)
    {
        var horizSpacingParam = wallType.get_Parameter(BuiltInParameter.CURTAIN_GRID_HORIZ_SPACING);
        if (horizSpacingParam != null && horizSpacingParam.StorageType == StorageType.Double)
        {
            result.Grid.HorizontalSpacing = horizSpacingParam.AsDouble();
        }

        var vertSpacingParam = wallType.get_Parameter(BuiltInParameter.CURTAIN_GRID_VERT_SPACING);
        if (vertSpacingParam != null && vertSpacingParam.StorageType == StorageType.Double)
        {
            result.Grid.VerticalSpacing = vertSpacingParam.AsDouble();
        }

        var horizAdjustParam = wallType.get_Parameter(BuiltInParameter.CURTAIN_GRID_HORIZ_ADJUST);
        if (horizAdjustParam != null && horizAdjustParam.StorageType == StorageType.Double)
        {
            result.Grid.HorizontalAdjust = horizAdjustParam.AsDouble();
        }

        var vertAdjustParam = wallType.get_Parameter(BuiltInParameter.CURTAIN_GRID_VERT_ADJUST);
        if (vertAdjustParam != null && vertAdjustParam.StorageType == StorageType.Double)
        {
            result.Grid.VerticalAdjust = vertAdjustParam.AsDouble();
        }
    }

    private void SerializePanelSettings(WallType wallType, Document document, CurtainWallTypeJson result)
    {
        var panelTypeParam = wallType.get_Parameter(BuiltInParameter.CURTAIN_WALL_PANEL_TYPE);
        if (panelTypeParam != null)
        {
            var panelTypeId = panelTypeParam.AsElementId();
            if (panelTypeId != ElementId.InvalidElementId)
            {
                var panelType = document.GetElement(panelTypeId);
                if (panelType != null)
                {
                    result.Panels.DefaultPanelTypeName = panelType.Name;
                    result.Panels.IsLoadable = panelType is FamilySymbol;
                }
            }
        }
    }

    private void SerializeMullionSettings(WallType wallType, Document document, CurtainWallTypeJson result)
    {
        var horizMullionParam = wallType.get_Parameter(BuiltInParameter.CURTAIN_WALL_MULLION_HORIZ_TYPE);
        if (horizMullionParam != null)
        {
            var mullionTypeId = horizMullionParam.AsElementId();
            if (mullionTypeId != ElementId.InvalidElementId)
            {
                var mullionType = document.GetElement(mullionTypeId);
                result.Mullions.HorizontalMullionTypeName = mullionType?.Name;
            }
        }

        var vertMullionParam = wallType.get_Parameter(BuiltInParameter.CURTAIN_WALL_MULLION_VERT_TYPE);
        if (vertMullionParam != null)
        {
            var mullionTypeId = vertMullionParam.AsElementId();
            if (mullionTypeId != ElementId.InvalidElementId)
            {
                var mullionType = document.GetElement(mullionTypeId);
                result.Mullions.VerticalMullionTypeName = mullionType?.Name;
            }
        }
    }

    private void ExtractDependencies(WallType wallType, Document document, CurtainWallTypeJson result)
    {
        var panelTypeParam = wallType.get_Parameter(BuiltInParameter.CURTAIN_WALL_PANEL_TYPE);
        if (panelTypeParam != null)
        {
            var panelTypeId = panelTypeParam.AsElementId();
            if (panelTypeId != ElementId.InvalidElementId)
            {
                var panelType = document.GetElement(panelTypeId) as FamilySymbol;
                if (panelType != null)
                {
                    result.Dependencies.Add(new CurtainDependencyJson
                    {
                        FamilyName = panelType.FamilyName,
                        TypeName = panelType.Name,
                        Category = panelType.Category?.Name,
                        InLibrary = false
                    });
                }
            }
        }
    }
}