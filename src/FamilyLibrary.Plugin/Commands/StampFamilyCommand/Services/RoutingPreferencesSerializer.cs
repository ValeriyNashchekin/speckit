using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;
using Newtonsoft.Json;

#nullable enable

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;

/// <summary>
/// Serializes RoutingPreferences from PipeType/DuctType to JSON format.
/// Supports multi-version Revit (2020-2026).
/// </summary>
public class RoutingPreferencesSerializer
{
    /// <summary>
    /// Serializes PipeType routing preferences to structured JSON model.
    /// </summary>
    /// <param name="pipeType">The pipe type to serialize.</param>
    /// <param name="document">The document containing the pipe type.</param>
    /// <returns>Structured routing preferences model.</returns>
    public RoutingPreferencesJson Serialize(PipeType pipeType, Document document)
    {
        return SerializeMepCurveType(pipeType, document);
    }

    /// <summary>
    /// Serializes DuctType routing preferences to structured JSON model.
    /// </summary>
    /// <param name="ductType">The duct type to serialize.</param>
    /// <param name="document">The document containing the duct type.</param>
    /// <returns>Structured routing preferences model.</returns>
    public RoutingPreferencesJson Serialize(DuctType ductType, Document document)
    {
        return SerializeMepCurveType(ductType, document);
    }

    /// <summary>
    /// Serializes PipeType routing preferences to JSON string.
    /// </summary>
    /// <param name="pipeType">The pipe type to serialize.</param>
    /// <param name="document">The document containing the pipe type.</param>
    /// <returns>JSON representation of routing preferences.</returns>
    public string SerializeToJson(PipeType pipeType, Document document)
    {
        var model = Serialize(pipeType, document);
        return JsonConvert.SerializeObject(model, Formatting.Indented);
    }

    /// <summary>
    /// Serializes DuctType routing preferences to JSON string.
    /// </summary>
    /// <param name="ductType">The duct type to serialize.</param>
    /// <param name="document">The document containing the duct type.</param>
    /// <returns>JSON representation of routing preferences.</returns>
    public string SerializeToJson(DuctType ductType, Document document)
    {
        var model = Serialize(ductType, document);
        return JsonConvert.SerializeObject(model, Formatting.Indented);
    }

    /// <summary>
    /// Generic serialization for MEPCurveType (base class for PipeType and DuctType).
    /// </summary>
    private RoutingPreferencesJson SerializeMepCurveType(MEPCurveType mepCurveType, Document document)
    {
        var result = new RoutingPreferencesJson();

        if (mepCurveType == null || document == null)
            return result;

        var routingManager = mepCurveType.RoutingPreferenceManager;
        if (routingManager == null || !routingManager.IsValidObject)
            return result;

        // Serialize preferred junction type
        result.PreferredJunctionType = routingManager.PreferredJunctionType.ToString();

        // Serialize all routing preference groups
        result.Segments = SerializeRules(routingManager, RoutingPreferenceRuleGroupType.Segments, document);
        result.Elbows = SerializeRules(routingManager, RoutingPreferenceRuleGroupType.Elbows, document);
        result.Junctions = SerializeRules(routingManager, RoutingPreferenceRuleGroupType.Junctions, document);
        result.Transitions = SerializeRules(routingManager, RoutingPreferenceRuleGroupType.Transitions, document);
        result.Crosses = SerializeRules(routingManager, RoutingPreferenceRuleGroupType.Crosses, document);
        result.Unions = SerializeRules(routingManager, RoutingPreferenceRuleGroupType.Unions, document);
        result.MechanicalJoints = SerializeRules(routingManager, RoutingPreferenceRuleGroupType.MechanicalJoints, document);
        result.Caps = SerializeRules(routingManager, RoutingPreferenceRuleGroupType.Caps, document);
        result.TransitionsRectangularToRound = SerializeRules(routingManager, RoutingPreferenceRuleGroupType.TransitionsRectangularToRound, document);
        result.TransitionsRectangularToOval = SerializeRules(routingManager, RoutingPreferenceRuleGroupType.TransitionsRectangularToOval, document);
        result.TransitionsOvalToRound = SerializeRules(routingManager, RoutingPreferenceRuleGroupType.TransitionsOvalToRound, document);

        return result;
    }

    /// <summary>
    /// Serializes all rules in a specific routing preference group.
    /// </summary>
    private List<RoutingRuleJson> SerializeRules(
        RoutingPreferenceManager manager,
        RoutingPreferenceRuleGroupType groupType,
        Document document)
    {
        var rules = new List<RoutingRuleJson>();
        var numberOfRules = manager.GetNumberOfRules(groupType);

        for (int i = 0; i < numberOfRules; i++)
        {
            var rule = manager.GetRule(groupType, i);
            if (rule == null || !rule.IsValidObject)
                continue;

            var ruleJson = SerializeRule(rule, document);
            rules.Add(ruleJson);
        }

        return rules;
    }

    /// <summary>
    /// Serializes a single routing preference rule.
    /// </summary>
    private RoutingRuleJson SerializeRule(RoutingPreferenceRule rule, Document document)
    {
        var ruleJson = new RoutingRuleJson
        {
            MepPartId = GetElementIdValue(rule.MEPPartId),
            Description = rule.Description
        };

        // Get MEP part name
        if (rule.MEPPartId != ElementId.InvalidElementId)
        {
            var mepPart = document.GetElement(rule.MEPPartId);
            ruleJson.MepPartName = mepPart?.Name;
        }

        // Serialize criteria
        for (int i = 0; i < rule.NumberOfCriteria; i++)
        {
            var criterion = rule.GetCriterion(i);
            if (criterion == null || !criterion.IsValidObject)
                continue;

            var criterionJson = SerializeCriterion(criterion);
            ruleJson.Criteria.Add(criterionJson);
        }

        return ruleJson;
    }

    /// <summary>
    /// Serializes a routing criterion based on its type.
    /// </summary>
    private RoutingCriterionJson SerializeCriterion(RoutingCriterionBase criterion)
    {
        var criterionJson = new RoutingCriterionJson
        {
            Type = criterion.GetType().Name
        };

        // Handle PrimarySizeCriterion (most common)
        if (criterion is PrimarySizeCriterion sizeCriterion)
        {
            criterionJson.MinimumSize = sizeCriterion.MinimumSize;
            criterionJson.MaximumSize = sizeCriterion.MaximumSize;
        }

        return criterionJson;
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
