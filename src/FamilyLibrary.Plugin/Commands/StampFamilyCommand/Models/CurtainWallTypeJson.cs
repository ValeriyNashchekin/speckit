using Newtonsoft.Json;
using System.Collections.Generic;

#nullable enable

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;

/// <summary>
/// JSON model for CurtainWallType serialization.
/// </summary>
public class CurtainWallTypeJson
{
    [JsonProperty("version")]
    public int Version { get; set; } = 1;

    [JsonProperty("typeName")]
    public string TypeName { get; set; } = string.Empty;

    [JsonProperty("kind")]
    public string Kind { get; set; } = "Curtain";

    [JsonProperty("category")]
    public string Category { get; set; } = "Walls";

    [JsonProperty("systemFamily")]
    public string SystemFamily { get; set; } = "Curtain Wall";

    [JsonProperty("parameters")]
    public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

    [JsonProperty("grid")]
    public CurtainGridJson Grid { get; set; } = new CurtainGridJson();

    [JsonProperty("panels")]
    public CurtainPanelsJson Panels { get; set; } = new CurtainPanelsJson();

    [JsonProperty("mullions")]
    public CurtainMullionsJson Mullions { get; set; } = new CurtainMullionsJson();

    [JsonProperty("dependencies")]
    public List<CurtainDependencyJson> Dependencies { get; set; } = new List<CurtainDependencyJson>();
}

public class CurtainGridJson
{
    [JsonProperty("layout")]
    public string? Layout { get; set; }

    [JsonProperty("horizontalSpacing")]
    public double HorizontalSpacing { get; set; }

    [JsonProperty("verticalSpacing")]
    public double VerticalSpacing { get; set; }

    [JsonProperty("horizontalAdjust")]
    public double HorizontalAdjust { get; set; }

    [JsonProperty("verticalAdjust")]
    public double VerticalAdjust { get; set; }
}

public class CurtainPanelsJson
{
    [JsonProperty("defaultPanelTypeName")]
    public string? DefaultPanelTypeName { get; set; }

    [JsonProperty("isLoadable")]
    public bool IsLoadable { get; set; }
}

public class CurtainMullionsJson
{
    [JsonProperty("horizontalMullionTypeName")]
    public string? HorizontalMullionTypeName { get; set; }

    [JsonProperty("verticalMullionTypeName")]
    public string? VerticalMullionTypeName { get; set; }

    [JsonProperty("cornerMullionTypeName")]
    public string? CornerMullionTypeName { get; set; }
}

public class CurtainDependencyJson
{
    [JsonProperty("familyName")]
    public string FamilyName { get; set; } = string.Empty;

    [JsonProperty("typeName")]
    public string TypeName { get; set; } = string.Empty;

    [JsonProperty("category")]
    public string? Category { get; set; }

    [JsonProperty("inLibrary")]
    public bool InLibrary { get; set; }
}