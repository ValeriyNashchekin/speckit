using Autodesk.Revit.DB;
using Newtonsoft.Json;

#nullable enable

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;

/// <summary>
/// Serializes CompoundStructure from Revit to JSON format.
/// Supports multi-version Revit (2020-2026).
/// </summary>
public class CompoundStructureSerializer
{
    /// <summary>
    /// Serializes CompoundStructure to JSON string.
    /// </summary>
    /// <param name="structure">The compound structure to serialize.</param>
    /// <param name="document">The document containing the structure.</param>
    /// <returns>JSON representation of the compound structure.</returns>
    public string Serialize(CompoundStructure structure, Document document)
    {
        if (structure == null)
            return "{}";

        var layers = new List<CompoundLayerData>();
        var layerList = structure.GetLayers();

        for (int i = 0; i < structure.LayerCount; i++)
        {
            var layer = layerList[i];
            var materialId = layer.MaterialId;
            Material? material = null;

            if (materialId != ElementId.InvalidElementId)
            {
                material = document.GetElement(materialId) as Material;
            }

            layers.Add(new CompoundLayerData
            {
                Function = layer.Function.ToString(),
                Thickness = layer.Width,
                MaterialName = material?.Name,
                MaterialId = GetElementIdValue(materialId),
                IsStructural = IsStructuralLayer(layer.Function),
                Priority = GetLayerPriority(layer)
            });
        }

        var result = new CompoundStructureData
        {
            Layers = layers,
            TotalThickness = structure.GetWidth(),
            VariableLayerIndex = structure.VariableLayerIndex
        };

        return JsonConvert.SerializeObject(result, Formatting.Indented);
    }

    /// <summary>
    /// Extracts materials from compound structure.
    /// </summary>
    /// <param name="structure">The compound structure.</param>
    /// <param name="document">The document containing the structure.</param>
    /// <returns>List of material information.</returns>
    public List<MaterialInfo> ExtractMaterials(CompoundStructure structure, Document document)
    {
        var materials = new List<MaterialInfo>();

        if (structure == null || document == null)
            return materials;

        for (int i = 0; i < structure.LayerCount; i++)
        {
            var layer = structure.GetLayers()[i];
            var materialId = layer.MaterialId;

            if (materialId == ElementId.InvalidElementId)
                continue;

            var material = document.GetElement(materialId) as Material;
            if (material == null)
                continue;

            materials.Add(new MaterialInfo
            {
                Name = material.Name,
                MaterialId = GetElementIdValue(materialId),
                LayerFunction = layer.Function.ToString()
            });
        }

        return materials;
    }

    /// <summary>
    /// Checks if materials exist in target document.
    /// </summary>
    /// <param name="structure">The compound structure.</param>
    /// <param name="sourceDoc">Source document.</param>
    /// <param name="targetDoc">Target document to check.</param>
    /// <returns>List of missing material names.</returns>
    public List<string> GetMissingMaterials(
        CompoundStructure structure,
        Document sourceDoc,
        Document targetDoc)
    {
        var missingMaterials = new List<string>();

        if (structure == null || sourceDoc == null || targetDoc == null)
            return missingMaterials;

        var materials = ExtractMaterials(structure, sourceDoc);
        var targetMaterialNames = GetMaterialNames(targetDoc);

        foreach (var material in materials)
        {
            if (!string.IsNullOrEmpty(material.Name) &&
                !targetMaterialNames.Contains(material.Name))
            {
                missingMaterials.Add(material.Name);
            }
        }

        return missingMaterials.Distinct().ToList();
    }

    /// <summary>
    /// Gets all material names from a document.
    /// </summary>
    private HashSet<string> GetMaterialNames(Document document)
    {
        var names = new HashSet<string>();

        var materials = new FilteredElementCollector(document)
            .OfClass(typeof(Material))
            .Cast<Material>();

        foreach (var material in materials)
        {
            if (!string.IsNullOrEmpty(material.Name))
            {
                names.Add(material.Name);
            }
        }

        return names;
    }

    /// <summary>
    /// Determines if a layer function represents structural material.
    /// </summary>
    private static bool IsStructuralLayer(MaterialFunctionAssignment function)
    {
        return function == MaterialFunctionAssignment.Structure ||
               function == MaterialFunctionAssignment.StructuralDeck;
    }

    /// <summary>
    /// Gets the priority of a layer based on its function.
    /// Uses default priorities: Structure=1, Substrate=2, Insulation=3,
    /// Finish1=4, Finish2=5, Membrane=999.
    /// </summary>
    private static int GetLayerPriority(CompoundStructureLayer layer)
    {
#if REVIT2026
        // In Revit 2026+, LayerPriority is available directly
        return layer.LayerPriority;
#else
        // For older versions, use default priority based on function
        return layer.Function switch
        {
            MaterialFunctionAssignment.Structure => 1,
            MaterialFunctionAssignment.Substrate => 2,
            MaterialFunctionAssignment.Insulation => 3,
            MaterialFunctionAssignment.Finish1 => 4,
            MaterialFunctionAssignment.Finish2 => 5,
            MaterialFunctionAssignment.Membrane => 999,
            MaterialFunctionAssignment.StructuralDeck => 1,
            _ => 0
        };
#endif
    }

    /// <summary>
    /// Gets the integer value of an ElementId in a version-compatible way.
    /// </summary>
    private static int GetElementIdValue(ElementId elementId)
    {
        if (elementId == ElementId.InvalidElementId)
            return -1;

#if REVIT2024 || REVIT2025 || REVIT2026
        return (int)elementId.Value;
#else
        return elementId.IntegerValue;
#endif
    }
}

/// <summary>
/// Data transfer object for compound structure serialization.
/// </summary>
public class CompoundStructureData
{
    /// <summary>
    /// List of layers in the compound structure.
    /// </summary>
    public List<CompoundLayerData> Layers { get; set; } = new List<CompoundLayerData>();

    /// <summary>
    /// Total thickness in feet.
    /// </summary>
    public double TotalThickness { get; set; }

    /// <summary>
    /// Index of the variable thickness layer (-1 if none).
    /// </summary>
    public int VariableLayerIndex { get; set; } = -1;
}

/// <summary>
/// Data transfer object for a single compound structure layer.
/// </summary>
public class CompoundLayerData
{
    /// <summary>
    /// Layer function (Structure, Finish1, Insulation, etc.).
    /// </summary>
    public string Function { get; set; } = string.Empty;

    /// <summary>
    /// Layer thickness in feet.
    /// </summary>
    public double Thickness { get; set; }

    /// <summary>
    /// Material name (may be null for empty layers).
    /// </summary>
    public string? MaterialName { get; set; }

    /// <summary>
    /// Material element ID (-1 if invalid).
    /// </summary>
    public int MaterialId { get; set; } = -1;

    /// <summary>
    /// Whether this layer is structural.
    /// </summary>
    public bool IsStructural { get; set; }

    /// <summary>
    /// Layer priority (1-5 for regular layers, 999 for membrane).
    /// </summary>
    public int Priority { get; set; }
}

/// <summary>
/// Information about a material in a compound structure.
/// </summary>
public class MaterialInfo
{
    /// <summary>
    /// Material name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Material element ID.
    /// </summary>
    public int MaterialId { get; set; }

    /// <summary>
    /// Function of the layer containing this material.
    /// </summary>
    public string LayerFunction { get; set; } = string.Empty;
}
