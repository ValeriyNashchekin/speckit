using Newtonsoft.Json;

#nullable enable

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;

/// <summary>
/// JSON model for CurtainWallType serialization (Group D).
/// Stores grid settings, panel, and mullion assignments.
/// </summary>
public class CurtainWallJson
{
    [JsonProperty("typeName")]
    public string TypeName { get; set; } = string.Empty;

    [JsonProperty("kind")]
    public string Kind { get; set; } = "Curtain";

    [JsonProperty("category")]
    public string Category { get; set; } = "Walls";

    [JsonProperty("systemFamily")]
    public string SystemFamily { get; set; } = "Curtain Wall";

    [JsonProperty("parameters")]
    public CurtainParametersJson Parameters { get; set; } = new CurtainParametersJson();

    [JsonProperty("grid")]
    public CurtainGridSettingsJson Grid { get; set; } = new CurtainGridSettingsJson();

    [JsonProperty("panels")]
    public CurtainPanelSettingsJson Panels { get; set; } = new CurtainPanelSettingsJson();

    [JsonProperty("mullions")]
    public CurtainMullionSettingsJson Mullions { get; set; } = new CurtainMullionSettingsJson();

    [JsonProperty("dependencies")]
    public List<CurtainDependencyJson> Dependencies { get; set; } = new List<CurtainDependencyJson>();
}

/// <summary>
/// Basic curtain wall parameters.
/// </summary>
public class CurtainParametersJson
{
    [JsonProperty("width")]
    public double Width { get; set; }

    [JsonProperty("function")]
    public string? Function { get; set; }
}

/// <summary>
/// Curtain grid layout settings.
/// </summary>
public class CurtainGridSettingsJson
{
    [JsonProperty("horizontalSpacing")]
    public double HorizontalSpacing { get; set; }

    [JsonProperty("verticalSpacing")]
    public double VerticalSpacing { get; set; }

    [JsonProperty("horizontalGridJustification")]
    public string? HorizontalGridJustification { get; set; }

    [JsonProperty("verticalGridJustification")]
    public string? VerticalGridJustification { get; set; }

    [JsonProperty("horizontalPattern")]
    public string? HorizontalPattern { get; set; }

    [JsonProperty("verticalPattern")]
    public string? VerticalPattern { get; set; }
}

/// <summary>
/// Curtain panel settings.
/// </summary>
public class CurtainPanelSettingsJson
{
    [JsonProperty("defaultPanelTypeName")]
    public string? DefaultPanelTypeName { get; set; }

    [JsonProperty("isLoadable")]
    public bool IsLoadable { get; set; }
}

/// <summary>
/// Curtain mullion settings.
/// </summary>
public class CurtainMullionSettingsJson
{
    [JsonProperty("horizontalMullionTypeName")]
    public string? HorizontalMullionTypeName { get; set; }

    [JsonProperty("verticalMullionTypeName")]
    public string? VerticalMullionTypeName { get; set; }

    [JsonProperty("intermediateHorizontalMullionTypeName")]
    public string? IntermediateHorizontalMullionTypeName { get; set; }

    [JsonProperty("intermediateVerticalMullionTypeName")]
    public string? IntermediateVerticalMullionTypeName { get; set; }
}

/// <summary>
/// Dependency on a curtain panel or mullion.
/// </summary>
public class CurtainDependencyJson
{
    [JsonProperty("familyName")]
    public string? FamilyName { get; set; }

    [JsonProperty("typeName")]
    public string? TypeName { get; set; }

    [JsonProperty("category")]
    public string? Category { get; set; }

    [JsonProperty("dependencyType")]
    public string? DependencyType { get; set; } // Panel or Mullion

    [JsonProperty("inLibrary")]
    public bool InLibrary { get; set; }

    [JsonProperty("libraryVersion")]
    public int? LibraryVersion { get; set; }
}
