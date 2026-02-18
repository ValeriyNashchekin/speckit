using Newtonsoft.Json;

#nullable enable

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;

/// <summary>
/// JSON model for StackedWallType serialization (Group D).
/// Stores wall layer references by TypeName with height.
/// </summary>
public class StackedWallJson
{
    [JsonProperty("typeName")]
    public string TypeName { get; set; } = string.Empty;

    [JsonProperty("kind")]
    public string Kind { get; set; } = "Stacked";

    [JsonProperty("category")]
    public string Category { get; set; } = "Walls";

    [JsonProperty("systemFamily")]
    public string SystemFamily { get; set; } = "Basic Wall";

    [JsonProperty("parameters")]
    public StackedWallParametersJson Parameters { get; set; } = new StackedWallParametersJson();

    [JsonProperty("stackedLayers")]
    public List<StackedLayerJson> StackedLayers { get; set; } = new List<StackedLayerJson>();

    [JsonProperty("dependencies")]
    public List<StackedWallDependencyJson> Dependencies { get; set; } = new List<StackedWallDependencyJson>();
}

/// <summary>
/// Basic stacked wall parameters.
/// </summary>
public class StackedWallParametersJson
{
    [JsonProperty("width")]
    public double Width { get; set; }

    [JsonProperty("function")]
    public string? Function { get; set; }
}

/// <summary>
/// Single layer in a stacked wall.
/// </summary>
public class StackedLayerJson
{
    [JsonProperty("wallTypeName")]
    public string? WallTypeName { get; set; }

    [JsonProperty("height")]
    public double Height { get; set; }

    /// <summary>
    /// Height=0 means "extend to top".
    /// </summary>
    [JsonProperty("extendsToTop")]
    public bool ExtendsToTop { get; set; }
}

/// <summary>
/// Dependency on a child WallType.
/// </summary>
public class StackedWallDependencyJson
{
    [JsonProperty("wallTypeName")]
    public string? WallTypeName { get; set; }

    [JsonProperty("inLibrary")]
    public bool InLibrary { get; set; }

    [JsonProperty("libraryVersion")]
    public int? LibraryVersion { get; set; }

    [JsonProperty("groupId")]
    public string? GroupId { get; set; }
}
