using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Newtonsoft.Json;

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services
{
    /// <summary>
    /// Deserializes CompoundStructure from JSON format for pull updates.
    /// Reconstructs layers with materials from target document.
    /// </summary>
    public class CompoundStructureDeserializer
    {
        /// <summary>
        /// Deserializes CompoundStructure from JSON string.
        /// Finds materials in target document by name.
        /// </summary>
        /// <param name="json">JSON representation of compound structure.</param>
        /// <param name="document">Target document to find materials in.</param>
        /// <returns>CompoundStructure or null if deserialization fails.</returns>
        public CompoundStructure Deserialize(string json, Document document)
        {
            if (string.IsNullOrEmpty(json) || document == null)
                return null;

            try
            {
                var data = JsonConvert.DeserializeObject<CompoundStructureData>(json);
                if (data == null || data.Layers == null)
                    return null;

                return BuildCompoundStructure(data, document);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Builds CompoundStructure from deserialized data.
        /// </summary>
        private CompoundStructure BuildCompoundStructure(CompoundStructureData data, Document document)
        {
            var layers = new List<CompoundStructureLayer>();
            var materialCache = BuildMaterialCache(document);

            foreach (var layerData in data.Layers)
            {
                var layer = CreateLayer(layerData, materialCache);
                if (layer != null)
                    layers.Add(layer);
            }

            if (layers.Count == 0)
                return null;

            // Create compound structure with layers
            var structure = CompoundStructure.CreateSimpleCompoundStructure(layers);

            // Set variable layer index if specified
            if (data.VariableLayerIndex >= 0 && data.VariableLayerIndex < layers.Count)
            {
                structure.VariableLayerIndex = data.VariableLayerIndex;
            }

            return structure;
        }

        /// <summary>
        /// Creates a single compound structure layer from layer data.
        /// </summary>
        private CompoundStructureLayer CreateLayer(CompoundLayerData layerData, Dictionary<string, Material> materialCache)
        {
            var function = ParseMaterialFunction(layerData.Function);
            var materialId = FindMaterialId(layerData.MaterialName, materialCache);

            // Create layer with width, function, and material
            var layer = new CompoundStructureLayer(
                layerData.Thickness,
                function,
                materialId);

            return layer;
        }

        /// <summary>
        /// Parses material function from string.
        /// </summary>
        private MaterialFunctionAssignment ParseMaterialFunction(string functionName)
        {
            if (string.IsNullOrEmpty(functionName))
                return MaterialFunctionAssignment.Structure;

            // Try to parse enum value
            if (Enum.TryParse(functionName, out MaterialFunctionAssignment result))
                return result;

            // Fallback mapping
            switch (functionName.ToLowerInvariant())
            {
                case "structure":
                    return MaterialFunctionAssignment.Structure;
                case "substrate":
                    return MaterialFunctionAssignment.Substrate;
                case "insulation":
                    return MaterialFunctionAssignment.Insulation;
                case "finish1":
                case "finish":
                    return MaterialFunctionAssignment.Finish1;
                case "finish2":
                    return MaterialFunctionAssignment.Finish2;
                case "membrane":
                    return MaterialFunctionAssignment.Membrane;
                case "structuraldeck":
                    return MaterialFunctionAssignment.StructuralDeck;
                default:
                    return MaterialFunctionAssignment.Structure;
            }
        }

        /// <summary>
        /// Finds material ElementId by name in the cache.
        /// Returns InvalidElementId if not found.
        /// </summary>
        private ElementId FindMaterialId(string materialName, Dictionary<string, Material> materialCache)
        {
            if (string.IsNullOrEmpty(materialName))
                return ElementId.InvalidElementId;

            if (materialCache.TryGetValue(materialName, out var material))
                return material.Id;

            return ElementId.InvalidElementId;
        }

        /// <summary>
        /// Builds a cache of materials by name for fast lookup.
        /// </summary>
        private Dictionary<string, Material> BuildMaterialCache(Document document)
        {
            var cache = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);

            var materials = new FilteredElementCollector(document)
                .OfClass(typeof(Material))
                .Cast<Material>();

            foreach (var material in materials)
            {
                if (!string.IsNullOrEmpty(material.Name) && !cache.ContainsKey(material.Name))
                {
                    cache[material.Name] = material;
                }
            }

            return cache;
        }
    }
}