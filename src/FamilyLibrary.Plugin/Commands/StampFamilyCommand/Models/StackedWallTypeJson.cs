using Newtonsoft.Json;
using System.Collections.Generic;

#nullable enable

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;

/// <summary>
/// JSON model for StackedWallType serialization.
/// </summary>
public class StackedWallTypeJson
{
    [JsonProperty("version")]
    public int Version { get; set; } = 1;

    [JsonProperty("typeName")]
    public string TypeName { get; set; } = string.Empty;

    [JsonProperty("kind")]
    public string Kind { get; set; } = "Stacked";

    [JsonProperty("category")]
    public string Category { get; set; } = "Walls";

    [JsonProperty("systemFamily")]
    public string SystemFamily { get; set; } = "Basic Wall";

    [JsonProperty("parameters")]
    public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

    [JsonProperty("stackedLayers")]
    public List<StackedLayerJson> StackedLayers { get; set; } = new List<StackedLayerJson>();

    [JsonProperty("dependencies")]
    public List<StackedWallDependencyJson> Dependencies { get; set; } = new List<StackedWallDependencyJson>();
}

public class StackedLayerJson
{
    [JsonProperty("wallTypeName")]
    public string WallTypeName { get; set; } = string.Empty;

    [JsonProperty("height")]
    public double Height { get; set; }

    [JsonProperty("heightIsVariable")]
    public bool HeightIsVariable { get; set; }
}

public class StackedWallDependencyJson
{
    [JsonProperty("wallTypeName")]
    public string WallTypeName { get; set; } = string.Empty;

    [JsonProperty("inLibrary")]
    public bool InLibrary { get; set; }

    [JsonProperty("groupId")]
    public string GroupId { get; set; } = "A";
}