using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models
{
    /// <summary>
    /// T064: Event payload for revit:material:fallback event.
    /// Emitted when material is not found during Pull Update and no mapping exists.
    /// </summary>
    public class MaterialFallbackEvent
    {
        /// <summary>
        /// Event type identifier.
        /// </summary>
        [JsonProperty("type")]
        public string Type => "revit:material:fallback";

        /// <summary>
        /// ID of the system type being updated.
        /// </summary>
        [JsonProperty("systemTypeId")]
        public string SystemTypeId { get; set; } = string.Empty;

        /// <summary>
        /// Name of the system type being updated.
        /// </summary>
        [JsonProperty("systemTypeName")]
        public string SystemTypeName { get; set; } = string.Empty;

        /// <summary>
        /// Information about the missing material.
        /// </summary>
        [JsonProperty("missingMaterial")]
        public MissingMaterialInfo MissingMaterial { get; set; } = new MissingMaterialInfo();

        /// <summary>
        /// Available material options in the project.
        /// </summary>
        [JsonProperty("availableOptions")]
        public List<MaterialOption> AvailableOptions { get; set; } = new List<MaterialOption>();
    }

    /// <summary>
    /// Information about a missing material from template.
    /// </summary>
    public class MissingMaterialInfo
    {
        /// <summary>
        /// Material name from the template/library.
        /// </summary>
        [JsonProperty("templateMaterialName")]
        public string TemplateMaterialName { get; set; } = string.Empty;

        /// <summary>
        /// Material category if available.
        /// </summary>
        [JsonProperty("category")]
        public string? Category { get; set; }

        /// <summary>
        /// Layer index in compound structure (for walls/floors).
        /// </summary>
        [JsonProperty("layerIndex")]
        public int? LayerIndex { get; set; }
    }

    /// <summary>
    /// Available option for material selection.
    /// </summary>
    public class MaterialOption
    {
        /// <summary>
        /// Option identifier (material ID or action type).
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Display name for the option.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Type of option: existing, create, default, skip.
        /// </summary>
        [JsonProperty("type")]
        public string OptionType { get; set; } = "existing";
    }

    /// <summary>
    /// T065: Event payload for ui:material-mapping:save event.
    /// Sent when user saves a material mapping decision.
    /// </summary>
    public class MaterialMappingSaveEvent
    {
        /// <summary>
        /// Event type identifier.
        /// </summary>
        [JsonProperty("type")]
        public string Type => "ui:material-mapping:save";

        /// <summary>
        /// Project identifier.
        /// </summary>
        [JsonProperty("projectId")]
        public string ProjectId { get; set; } = string.Empty;

        /// <summary>
        /// Template material name.
        /// </summary>
        [JsonProperty("templateMaterialName")]
        public string TemplateMaterialName { get; set; } = string.Empty;

        /// <summary>
        /// Selected project material name.
        /// </summary>
        [JsonProperty("projectMaterialName")]
        public string ProjectMaterialName { get; set; } = string.Empty;

        /// <summary>
        /// Whether to apply to current update operation.
        /// </summary>
        [JsonProperty("applyToCurrent")]
        public bool ApplyToCurrent { get; set; }
    }
}
