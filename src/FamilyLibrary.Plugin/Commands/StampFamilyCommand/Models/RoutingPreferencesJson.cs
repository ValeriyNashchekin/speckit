using Newtonsoft.Json;

#nullable enable

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;

/// <summary>
/// JSON model for RoutingPreferences serialization.
/// Used for PipeType and DuctType routing preferences.
/// </summary>
public class RoutingPreferencesJson
{
    /// <summary>
    /// Preferred junction type (Tap or Tee).
    /// </summary>
    [JsonProperty("preferredJunctionType")]
    public string PreferredJunctionType { get; set; } = "Tee";

    /// <summary>
    /// Segment rules (pipe/duct segments).
    /// </summary>
    [JsonProperty("segments")]
    public List<RoutingRuleJson> Segments { get; set; } = new List<RoutingRuleJson>();

    /// <summary>
    /// Elbow fitting rules.
    /// </summary>
    [JsonProperty("elbows")]
    public List<RoutingRuleJson> Elbows { get; set; } = new List<RoutingRuleJson>();

    /// <summary>
    /// Junction fitting rules (tees, wyes, taps).
    /// </summary>
    [JsonProperty("junctions")]
    public List<RoutingRuleJson> Junctions { get; set; } = new List<RoutingRuleJson>();

    /// <summary>
    /// Transition fitting rules.
    /// </summary>
    [JsonProperty("transitions")]
    public List<RoutingRuleJson> Transitions { get; set; } = new List<RoutingRuleJson>();

    /// <summary>
    /// Cross fitting rules.
    /// </summary>
    [JsonProperty("crosses")]
    public List<RoutingRuleJson> Crosses { get; set; } = new List<RoutingRuleJson>();

    /// <summary>
    /// Union fitting rules.
    /// </summary>
    [JsonProperty("unions")]
    public List<RoutingRuleJson> Unions { get; set; } = new List<RoutingRuleJson>();

    /// <summary>
    /// Mechanical joint rules.
    /// </summary>
    [JsonProperty("mechanicalJoints")]
    public List<RoutingRuleJson> MechanicalJoints { get; set; } = new List<RoutingRuleJson>();

    /// <summary>
    /// Cap fitting rules.
    /// </summary>
    [JsonProperty("caps")]
    public List<RoutingRuleJson> Caps { get; set; } = new List<RoutingRuleJson>();

    /// <summary>
    /// Rectangular to round transition rules.
    /// </summary>
    [JsonProperty("transitionsRectangularToRound")]
    public List<RoutingRuleJson> TransitionsRectangularToRound { get; set; } = new List<RoutingRuleJson>();

    /// <summary>
    /// Rectangular to oval transition rules.
    /// </summary>
    [JsonProperty("transitionsRectangularToOval")]
    public List<RoutingRuleJson> TransitionsRectangularToOval { get; set; } = new List<RoutingRuleJson>();

    /// <summary>
    /// Oval to round transition rules.
    /// </summary>
    [JsonProperty("transitionsOvalToRound")]
    public List<RoutingRuleJson> TransitionsOvalToRound { get; set; } = new List<RoutingRuleJson>();
}

/// <summary>
/// Represents a single routing preference rule.
/// </summary>
public class RoutingRuleJson
{
    /// <summary>
    /// MEP part element ID (-1 if invalid).
    /// </summary>
    [JsonProperty("mepPartId")]
    public int MepPartId { get; set; } = -1;

    /// <summary>
    /// MEP part name (family symbol name).
    /// </summary>
    [JsonProperty("mepPartName")]
    public string? MepPartName { get; set; }

    /// <summary>
    /// Rule description.
    /// </summary>
    [JsonProperty("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Routing criteria for this rule (size ranges, etc.).
    /// </summary>
    [JsonProperty("criteria")]
    public List<RoutingCriterionJson> Criteria { get; set; } = new List<RoutingCriterionJson>();
}

/// <summary>
/// Represents a routing criterion (size range).
/// </summary>
public class RoutingCriterionJson
{
    /// <summary>
    /// Criterion type name (e.g., "PrimarySizeCriterion").
    /// </summary>
    [JsonProperty("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Minimum size in feet (for size-based criteria).
    /// </summary>
    [JsonProperty("minimumSize")]
    public double MinimumSize { get; set; }

    /// <summary>
    /// Maximum size in feet (for size-based criteria).
    /// </summary>
    [JsonProperty("maximumSize")]
    public double MaximumSize { get; set; }
}
