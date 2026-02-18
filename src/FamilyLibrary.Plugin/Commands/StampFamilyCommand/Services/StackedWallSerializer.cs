using Autodesk.Revit.DB;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;
using Newtonsoft.Json;

#nullable enable

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;

/// <summary>
/// Serializes StackedWallType (WallKind.Stacked) to JSON format.
/// Extracts child wall type dependencies for Pull Update validation.
/// Supports multi-version Revit (2020-2026).
/// </summary>
public class StackedWallSerializer
{
    /// <summary>
    /// Serializes StackedWallType to structured JSON model.
    /// </summary>
    public StackedWallTypeJson Serialize(WallType wallType, Document document)
    {
        if (wallType.Kind != WallKind.Stacked)
            throw new System.ArgumentException("WallType is not a stacked wall");

        var result = new StackedWallTypeJson
        {
            TypeName = wallType.Name,
            Kind = "Stacked",
            Category = "Walls",
            SystemFamily = "Basic Wall"
        };

        SerializeParameters(wallType, result);
        SerializeStackedLayers(wallType, document, result);
        ExtractDependencies(wallType, document, result);

        return result;
    }

    /// <summary>
    /// Serializes StackedWallType to JSON string.
    /// </summary>
    public string SerializeToJson(WallType wallType, Document document)
    {
        var model = Serialize(wallType, document);
        return JsonConvert.SerializeObject(model, Formatting.Indented);
    }

    private void SerializeParameters(WallType wallType, StackedWallTypeJson result)
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

    private void SerializeStackedLayers(WallType wallType, Document document, StackedWallTypeJson result)
    {
        var layers = new System.Collections.Generic.List<StackedLayerJson>();

        // Stacked wall types reference multiple wall types via parameters
        // WALL_ATTR_TYPE_PARAM_1, WALL_ATTR_TYPE_PARAM_2, etc.
        for (int i = 1; i <= 20; i++)
        {
            var wallTypeParamId = GetBuiltInParameterForLayer(i, false);
            var heightParamId = GetBuiltInParameterForLayer(i, true);

            var wallTypeParam = wallType.get_Parameter(wallTypeParamId);
            if (wallTypeParam == null)
                break;

            var childWallTypeId = wallTypeParam.AsElementId();
            if (childWallTypeId == ElementId.InvalidElementId)
                break;

            var childWallType = document.GetElement(childWallTypeId) as WallType;
            if (childWallType == null)
                break;

            var heightParam = wallType.get_Parameter(heightParamId);
            var height = heightParam?.AsDouble() ?? 0;
            var isVariable = System.Math.Abs(height) < 0.0001;

            layers.Add(new StackedLayerJson
            {
                WallTypeName = childWallType.Name,
                Height = height,
                HeightIsVariable = isVariable
            });
        }

        result.StackedLayers = layers;
    }

    private BuiltInParameter GetBuiltInParameterForLayer(int layerIndex, bool isHeight)
    {
        if (isHeight)
        {
            return layerIndex switch
            {
                1 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_1,
                2 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_2,
                3 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_3,
                4 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_4,
                5 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_5,
                6 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_6,
                7 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_7,
                8 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_8,
                9 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_9,
                10 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_10,
                11 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_11,
                12 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_12,
                13 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_13,
                14 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_14,
                15 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_15,
                16 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_16,
                17 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_17,
                18 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_18,
                19 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_19,
                20 => BuiltInParameter.WALL_ATTR_HEIGHT_PARAM_20,
                _ => BuiltInParameter.INVALID
            };
        }

        return layerIndex switch
        {
            1 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_1,
            2 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_2,
            3 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_3,
            4 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_4,
            5 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_5,
            6 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_6,
            7 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_7,
            8 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_8,
            9 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_9,
            10 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_10,
            11 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_11,
            12 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_12,
            13 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_13,
            14 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_14,
            15 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_15,
            16 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_16,
            17 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_17,
            18 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_18,
            19 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_19,
            20 => BuiltInParameter.WALL_ATTR_TYPE_PARAM_20,
            _ => BuiltInParameter.INVALID
        };
    }

    private void ExtractDependencies(WallType wallType, Document document, StackedWallTypeJson result)
    {
        foreach (var layer in result.StackedLayers)
        {
            result.Dependencies.Add(new StackedWallDependencyJson
            {
                WallTypeName = layer.WallTypeName,
                InLibrary = false,
                GroupId = "A"
            });
        }
    }
}