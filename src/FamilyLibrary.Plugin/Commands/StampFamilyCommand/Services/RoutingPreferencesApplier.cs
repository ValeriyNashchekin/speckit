using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Mechanical;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;

#nullable enable

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;

/// <summary>
/// Applies RoutingPreferences from JSON to PipeType/DuctType during pull update.
/// Finds existing segments and fittings by their element ID or name.
/// Logs warnings for elements not found.
/// </summary>
public class RoutingPreferencesApplier
{
    /// <summary>
    /// Applies routing preferences to a PipeType from JSON data.
    /// </summary>
    /// <param name="pipeType">The PipeType to update.</param>
    /// <param name="json">The routing preferences JSON data.</param>
    /// <param name="document">The document containing the PipeType.</param>
    /// <returns>List of warnings for elements not found.</returns>
    public List<string> Apply(PipeType pipeType, RoutingPreferencesJson json, Document document)
    {
        var warnings = new List<string>();

        if (pipeType == null || json == null || document == null)
            return warnings;

        var rpm = pipeType.RoutingPreferenceManager;
        if (rpm == null)
        {
            warnings.Add("RoutingPreferenceManager is null for PipeType");
            return warnings;
        }

        ApplyRoutingPreferences(rpm, json, document, warnings);
        return warnings;
    }

    /// <summary>
    /// Applies routing preferences to a DuctType from JSON data.
    /// </summary>
    /// <param name="ductType">The DuctType to update.</param>
    /// <param name="json">The routing preferences JSON data.</param>
    /// <param name="document">The document containing the DuctType.</param>
    /// <returns>List of warnings for elements not found.</returns>
    public List<string> Apply(DuctType ductType, RoutingPreferencesJson json, Document document)
    {
        var warnings = new List<string>();

        if (ductType == null || json == null || document == null)
            return warnings;

        var rpm = ductType.RoutingPreferenceManager;
        if (rpm == null)
        {
            warnings.Add("RoutingPreferenceManager is null for DuctType");
            return warnings;
        }

        ApplyRoutingPreferences(rpm, json, document, warnings);
        return warnings;
    }

    /// <summary>
    /// Common implementation for applying routing preferences.
    /// </summary>
    private void ApplyRoutingPreferences(
        RoutingPreferenceManager rpm,
        RoutingPreferencesJson json,
        Document document,
        List<string> warnings)
    {
        // Clear existing rules
        ClearAllRules(rpm);

        // Cache elements ONCE for performance
        var elementsCache = new MepElementsCache(document);

        // Apply rules for each group type
        ApplyRulesForGroup(rpm, RoutingPreferenceRuleGroupType.Segments, json.Segments, elementsCache, warnings);
        ApplyRulesForGroup(rpm, RoutingPreferenceRuleGroupType.Elbows, json.Elbows, elementsCache, warnings);
        ApplyRulesForGroup(rpm, RoutingPreferenceRuleGroupType.Junctions, json.Junctions, elementsCache, warnings);
        ApplyRulesForGroup(rpm, RoutingPreferenceRuleGroupType.Transitions, json.Transitions, elementsCache, warnings);
        ApplyRulesForGroup(rpm, RoutingPreferenceRuleGroupType.Crosses, json.Crosses, elementsCache, warnings);
        ApplyRulesForGroup(rpm, RoutingPreferenceRuleGroupType.Unions, json.Unions, elementsCache, warnings);
        ApplyRulesForGroup(rpm, RoutingPreferenceRuleGroupType.MechanicalJoints, json.MechanicalJoints, elementsCache, warnings);
        ApplyRulesForGroup(rpm, RoutingPreferenceRuleGroupType.Caps, json.Caps, elementsCache, warnings);
        ApplyRulesForGroup(rpm, RoutingPreferenceRuleGroupType.TransitionsRectangularToRound, json.TransitionsRectangularToRound, elementsCache, warnings);
        ApplyRulesForGroup(rpm, RoutingPreferenceRuleGroupType.TransitionsRectangularToOval, json.TransitionsRectangularToOval, elementsCache, warnings);
        ApplyRulesForGroup(rpm, RoutingPreferenceRuleGroupType.TransitionsOvalToRound, json.TransitionsOvalToRound, elementsCache, warnings);
    }

    /// <summary>
    /// Clears all rules from all routing preference groups.
    /// </summary>
    private void ClearAllRules(RoutingPreferenceManager rpm)
    {
        var groupTypes = new[]
        {
            RoutingPreferenceRuleGroupType.Segments,
            RoutingPreferenceRuleGroupType.Elbows,
            RoutingPreferenceRuleGroupType.Junctions,
            RoutingPreferenceRuleGroupType.Transitions,
            RoutingPreferenceRuleGroupType.Unions,
            RoutingPreferenceRuleGroupType.MechanicalJoints,
            RoutingPreferenceRuleGroupType.Crosses,
            RoutingPreferenceRuleGroupType.Caps,
            RoutingPreferenceRuleGroupType.TransitionsRectangularToRound,
            RoutingPreferenceRuleGroupType.TransitionsRectangularToOval,
            RoutingPreferenceRuleGroupType.TransitionsOvalToRound
        };

        foreach (var groupType in groupTypes)
        {
            ClearRulesInGroup(rpm, groupType);
        }
    }

    /// <summary>
    /// Clears all rules in a specific group.
    /// Remove from end to start to avoid index issues.
    /// </summary>
    private void ClearRulesInGroup(RoutingPreferenceManager rpm, RoutingPreferenceRuleGroupType groupType)
    {
        var ruleCount = rpm.GetNumberOfRules(groupType);
        
        for (int i = ruleCount - 1; i >= 0; i--)
        {
            try
            {
                rpm.RemoveRule(groupType, i);
            }
            catch
            {
                // Ignore errors during cleanup
            }
        }
    }

    /// <summary>
    /// Applies rules for a specific routing preference group.
    /// </summary>
    private void ApplyRulesForGroup(
        RoutingPreferenceManager rpm,
        RoutingPreferenceRuleGroupType groupType,
        List<RoutingRuleJson> rules,
        MepElementsCache cache,
        List<string> warnings)
    {
        if (rules == null || rules.Count == 0)
            return;

        foreach (var ruleJson in rules)
        {
            var elementId = FindElementId(ruleJson, cache, groupType);
            
            if (elementId == null || elementId == ElementId.InvalidElementId)
            {
                var warning = string.Format(
                    "{0} not found: ID={1}, Name={2}",
                    groupType,
                    ruleJson.MepPartId,
                    ruleJson.MepPartName ?? "null");
                warnings.Add(warning);
                continue;
            }

            try
            {
                var rule = new RoutingPreferenceRule(elementId, ruleJson.Description ?? string.Empty);
                AddCriteriaToRule(rule, ruleJson);
                rpm.AddRule(groupType, rule);
            }
            catch (Exception ex)
            {
                warnings.Add(string.Format("Failed to add {0} rule: {1}", groupType, ex.Message));
            }
        }
    }

    /// <summary>
    /// Finds ElementId for an MEP part by ID or name.
    /// </summary>
    private ElementId FindElementId(
        RoutingRuleJson ruleJson,
        MepElementsCache cache,
        RoutingPreferenceRuleGroupType groupType)
    {
        // First try by ID if valid
        if (ruleJson.MepPartId > 0)
        {
            var elementById = cache.FindElementById(ruleJson.MepPartId);
            if (elementById != null)
                return elementById.Id;
        }

        // Try by name
        if (!string.IsNullOrEmpty(ruleJson.MepPartName))
        {
            return cache.FindElementIdByName(ruleJson.MepPartName, groupType);
        }

        return ElementId.InvalidElementId;
    }

    /// <summary>
    /// Adds criteria to a routing preference rule.
    /// </summary>
    private void AddCriteriaToRule(RoutingPreferenceRule rule, RoutingRuleJson ruleJson)
    {
        if (ruleJson.Criteria == null || ruleJson.Criteria.Count == 0)
            return;

        foreach (var criterionJson in ruleJson.Criteria)
        {
            var criterion = CreateCriterion(criterionJson);
            if (criterion != null)
            {
                rule.AddCriterion(criterion);
            }
        }
    }

    /// <summary>
    /// Creates a routing criterion from JSON.
    /// </summary>
    private RoutingCriterionBase CreateCriterion(RoutingCriterionJson criterionJson)
    {
        if (criterionJson == null || string.IsNullOrEmpty(criterionJson.Type))
            return null;

        // Handle PrimarySizeCriterion (most common)
        if (criterionJson.Type.Equals("PrimarySizeCriterion", StringComparison.OrdinalIgnoreCase))
        {
            return new PrimarySizeCriterion(criterionJson.MinimumSize, criterionJson.MaximumSize);
        }

        // Add other criterion types as needed
        return null;
    }

    #region Nested Cache Class

    /// <summary>
    /// Caches MEP elements to avoid repeated FilteredElementCollector calls.
    /// </summary>
    private class MepElementsCache
    {
        private readonly Document _document;
        private readonly Dictionary<int, Element> _elementsById;
        private readonly Dictionary<string, ElementId> _segmentsByName;
        private readonly Dictionary<string, ElementId> _familySymbolsByName;

        public MepElementsCache(Document document)
        {
            _document = document;
            _elementsById = new Dictionary<int, Element>();
            _segmentsByName = new Dictionary<string, ElementId>(StringComparer.OrdinalIgnoreCase);
            _familySymbolsByName = new Dictionary<string, ElementId>(StringComparer.OrdinalIgnoreCase);

            BuildCache();
        }

        private void BuildCache()
        {
            // Cache PipeSegments
            var pipeSegments = new FilteredElementCollector(_document)
                .OfClass(typeof(PipeSegment))
                .ToElements();

            foreach (var segment in pipeSegments)
            {
                CacheElement(segment);
                if (!string.IsNullOrEmpty(segment.Name))
                {
                    _segmentsByName[segment.Name] = segment.Id;
                }
            }

            // Note: DuctSegment class does not exist in Revit API
            // Ducts use Segment base class which is covered by PipeSegment collection
            // or are handled differently in MEP systems

            // Cache FamilySymbols (for fittings)
            var familySymbols = new FilteredElementCollector(_document)
                .OfClass(typeof(FamilySymbol))
                .ToElements();

            foreach (var symbol in familySymbols)
            {
                CacheElement(symbol);
                if (!string.IsNullOrEmpty(symbol.Name))
                {
                    _familySymbolsByName[symbol.Name] = symbol.Id;
                }
            }
        }

        private void CacheElement(Element element)
        {
            var idValue = GetElementIdValue(element.Id);
            if (idValue > 0 && !_elementsById.ContainsKey(idValue))
            {
                _elementsById[idValue] = element;
            }
        }

        public Element FindElementById(int id)
        {
            return _elementsById.TryGetValue(id, out var element) ? element : null;
        }

        public ElementId FindElementIdByName(string name, RoutingPreferenceRuleGroupType groupType)
        {
            // Segments use segment cache
            if (groupType == RoutingPreferenceRuleGroupType.Segments)
            {
                return _segmentsByName.TryGetValue(name, out var id) ? id : ElementId.InvalidElementId;
            }

            // Fittings use FamilySymbol cache
            return _familySymbolsByName.TryGetValue(name, out var symbolId) 
                ? symbolId 
                : ElementId.InvalidElementId;
        }

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

    #endregion
}