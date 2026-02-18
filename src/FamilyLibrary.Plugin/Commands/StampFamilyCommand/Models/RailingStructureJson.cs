using Newtonsoft.Json;

#nullable enable

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;

/// <summary>
/// JSON model for RailingType serialization.
/// Includes baluster dependencies for Pull Update validation.
/// </summary>
public class RailingStructureJson
{
    [JsonProperty("typeName")]
    public string TypeName { get; set; } = string.Empty;

    [JsonProperty("category")]
    public string Category { get; set; } = "Railings";

    [JsonProperty("systemFamily")]
    public string SystemFamily { get; set; } = "Railing";

    [JsonProperty("parameters")]
    public RailingParametersJson Parameters { get; set; } = new RailingParametersJson();

    [JsonProperty("railingStructure")]
    public RailingStructureDataJson RailingStructure { get; set; } = new RailingStructureDataJson();

    [JsonProperty("dependencies")]
    public List<RailingDependencyJson> Dependencies { get; set; } = new List<RailingDependencyJson>();
}

/// <summary>
/// Basic railing parameters.
/// </summary>
public class RailingParametersJson
{
    [JsonProperty("height")]
    public double Height { get; set; }

    [JsonProperty("offset")]
    public double Offset { get; set; }

    [JsonProperty("primaryHandrailHeight")]
    public double PrimaryHandrailHeight { get; set; }

    [JsonProperty("secondaryHandrailHeight")]
    public double SecondaryHandrailHeight { get; set; }
}

/// <summary>
/// Railing structure data including top rail, handrails, and balusters.
/// </summary>
public class RailingStructureDataJson
{
    [JsonProperty("topRailTypeName")]
    public string? TopRailTypeName { get; set; }

    [JsonProperty("primaryHandrailTypeName")]
    public string? PrimaryHandrailTypeName { get; set; }

    [JsonProperty("secondaryHandrailTypeName")]
    public string? SecondaryHandrailTypeName { get; set; }

    [JsonProperty("balusterPlacement")]
    public BalusterPlacementJson BalusterPlacement { get; set; } = new BalusterPlacementJson();
}

/// <summary>
/// Baluster placement settings and pattern.
/// </summary>
public class BalusterPlacementJson
{
    [JsonProperty("useBalusterPerTreadOnStairs")]
    public bool UseBalusterPerTreadOnStairs { get; set; }

    [JsonProperty("balusterPerTreadNumber")]
    public int BalusterPerTreadNumber { get; set; }

    [JsonProperty("pattern")]
    public List<BalusterPatternItemJson> Pattern { get; set; } = new List<BalusterPatternItemJson>();
}

/// <summary>
/// Single baluster in the pattern.
/// </summary>
public class BalusterPatternItemJson
{
    [JsonProperty("balusterFamilyName")]
    public string? BalusterFamilyName { get; set; }

    [JsonProperty("balusterTypeName")]
    public string? BalusterTypeName { get; set; }

    [JsonProperty("baseOffset")]
    public double BaseOffset { get; set; }

    [JsonProperty("topOffset")]
    public double TopOffset { get; set; }

    [JsonProperty("spacing")]
    public double Spacing { get; set; }
}

/// <summary>
/// Dependency on a baluster family.
/// </summary>
public class RailingDependencyJson
{
    [JsonProperty("familyName")]
    public string? FamilyName { get; set; }

    [JsonProperty("typeName")]
    public string? TypeName { get; set; }

    [JsonProperty("category")]
    public string? Category { get; set; }

    [JsonProperty("inLibrary")]
    public bool InLibrary { get; set; }

    [JsonProperty("libraryVersion")]
    public int? LibraryVersion { get; set; }
}
