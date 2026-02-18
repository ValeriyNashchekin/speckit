using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;
using Newtonsoft.Json;

#nullable enable

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;

/// <summary>
/// Serializes RailingType (Group C) to JSON format.
/// Extracts baluster dependencies for Pull Update validation.
/// Supports multi-version Revit (2020-2026).
/// </summary>
public class RailingSerializer
{
    public RailingStructureJson Serialize(RailingType railingType, Document document)
    {
        var result = new RailingStructureJson
        {
            TypeName = railingType.Name,
            Category = "Railings",
            SystemFamily = "Railing"
        };

        if (document == null || railingType == null)
            return result;

        SerializeParameters(railingType, result.Parameters);
        SerializeRailingStructure(railingType, document, result.RailingStructure, result.Dependencies);

        return result;
    }

    public string SerializeToJson(RailingType railingType, Document document)
    {
        var model = Serialize(railingType, document);
        return JsonConvert.SerializeObject(model, Formatting.Indented);
    }

    public List<RailingDependencyJson> ExtractDependencies(RailingType railingType, Document document)
    {
        var dependencies = new List<RailingDependencyJson>();

        if (railingType == null || document == null)
            return dependencies;

        var balusterPlacement = railingType.BalusterPlacement;
        if (balusterPlacement == null || !balusterPlacement.IsValidObject)
            return dependencies;

        var balusterPattern = balusterPlacement.BalusterPattern;
        if (balusterPattern != null && balusterPattern.IsValidObject)
        {
            ExtractBalusterPatternDependencies(balusterPattern, document, dependencies);
        }

        var topRailTypeId = railingType.TopRailType;
        if (topRailTypeId != null && topRailTypeId != ElementId.InvalidElementId)
        {
            var topRailType = document.GetElement(topRailTypeId);
            if (topRailType != null)
            {
                dependencies.Add(new RailingDependencyJson
                {
                    TypeName = topRailType.Name,
                    Category = "Top Rails",
                    InLibrary = false
                });
            }
        }

        ExtractHandrailDependencies(railingType, document, dependencies);

        return dependencies;
    }

    public List<string> ValidateDependencies(RailingStructureJson railingJson, Document document)
    {
        var missing = new List<string>();

        if (railingJson?.Dependencies == null || document == null)
            return missing;

        var balusterFamilies = GetBalusterFamilies(document);

        foreach (var dep in railingJson.Dependencies)
        {
            if (string.IsNullOrEmpty(dep.FamilyName) && string.IsNullOrEmpty(dep.TypeName))
                continue;

            var exists = CheckBalusterExists(dep, balusterFamilies);
            if (!exists)
            {
                var name = string.IsNullOrEmpty(dep.TypeName)
                    ? dep.FamilyName
                    : dep.FamilyName + " : " + dep.TypeName;
                missing.Add(name ?? "Unknown");
            }
        }

        return missing;
    }

    private void SerializeParameters(RailingType railingType, RailingParametersJson parameters)
    {
        try
        {
            var heightParam = railingType.get_Parameter(BuiltInParameter.RAILING_HEIGHT);
            if (heightParam != null)
            {
                parameters.Height = heightParam.AsDouble();
            }

            parameters.PrimaryHandrailHeight = railingType.PrimaryHandrailHeight;
            parameters.SecondaryHandrailHeight = railingType.SecondaryHandrailHeight;
        }
        catch
        {
        }
    }

    private void SerializeRailingStructure(
        RailingType railingType,
        Document document,
        RailingStructureDataJson structure,
        List<RailingDependencyJson> dependencies)
    {
        var topRailTypeId = railingType.TopRailType;
        if (topRailTypeId != null && topRailTypeId != ElementId.InvalidElementId)
        {
            var topRailType = document.GetElement(topRailTypeId);
            if (topRailType != null)
            {
                structure.TopRailTypeName = topRailType.Name;
                dependencies.Add(new RailingDependencyJson
                {
                    TypeName = topRailType.Name,
                    Category = "Top Rails",
                    InLibrary = false
                });
            }
        }

        SerializeHandrails(railingType, document, structure, dependencies);
        SerializeBalusterPlacement(railingType, document, structure.BalusterPlacement, dependencies);
    }

    private void SerializeHandrails(
        RailingType railingType,
        Document document,
        RailingStructureDataJson structure,
        List<RailingDependencyJson> dependencies)
    {
        try
        {
            var primaryHandrailTypeId = railingType.PrimaryHandrailType;
            if (primaryHandrailTypeId != null && primaryHandrailTypeId != ElementId.InvalidElementId)
            {
                var handrailType = document.GetElement(primaryHandrailTypeId);
                if (handrailType != null)
                {
                    structure.PrimaryHandrailTypeName = handrailType.Name;
                    dependencies.Add(new RailingDependencyJson
                    {
                        TypeName = handrailType.Name,
                        Category = "Handrails",
                        InLibrary = false
                    });
                }
            }

            var secondaryHandrailTypeId = railingType.SecondaryHandrailType;
            if (secondaryHandrailTypeId != null && secondaryHandrailTypeId != ElementId.InvalidElementId)
            {
                var handrailType = document.GetElement(secondaryHandrailTypeId);
                if (handrailType != null)
                {
                    structure.SecondaryHandrailTypeName = handrailType.Name;
                    dependencies.Add(new RailingDependencyJson
                    {
                        TypeName = handrailType.Name,
                        Category = "Handrails",
                        InLibrary = false
                    });
                }
            }
        }
        catch
        {
        }
    }

    private void SerializeBalusterPlacement(
        RailingType railingType,
        Document document,
        BalusterPlacementJson placement,
        List<RailingDependencyJson> dependencies)
    {
        var balusterPlacement = railingType.BalusterPlacement;
        if (balusterPlacement == null || !balusterPlacement.IsValidObject)
            return;

        placement.UseBalusterPerTreadOnStairs = balusterPlacement.UseBalusterPerTreadOnStairs;
        placement.BalusterPerTreadNumber = balusterPlacement.BalusterPerTreadNumber;

        var balusterPattern = balusterPlacement.BalusterPattern;
        if (balusterPattern != null && balusterPattern.IsValidObject)
        {
            SerializeBalusterPattern(balusterPattern, document, placement.Pattern, dependencies);
        }
    }

    private void SerializeBalusterPattern(
        BalusterPattern pattern,
        Document document,
        List<BalusterPatternItemJson> patternItems,
        List<RailingDependencyJson> dependencies)
    {
        var balusterCount = pattern.GetBalusterCount();

        for (int i = 0; i < balusterCount; i++)
        {
            var balusterInfo = pattern.GetBaluster(i);
            if (balusterInfo == null || !balusterInfo.IsValidObject)
                continue;

            var item = new BalusterPatternItemJson
            {
                BaseOffset = balusterInfo.BaseOffset,
                TopOffset = balusterInfo.TopOffset,
                Spacing = 0
            };

            var balusterFamilyId = balusterInfo.BalusterFamilyId;
            if (balusterFamilyId != null && balusterFamilyId != ElementId.InvalidElementId)
            {
                var balusterFamily = document.GetElement(balusterFamilyId);
                if (balusterFamily is FamilySymbol familySymbol)
                {
                    item.BalusterFamilyName = familySymbol.FamilyName;
                    item.BalusterTypeName = familySymbol.Name;

                    dependencies.Add(new RailingDependencyJson
                    {
                        FamilyName = familySymbol.FamilyName,
                        TypeName = familySymbol.Name,
                        Category = GetBalusterCategory(familySymbol),
                        InLibrary = false
                    });
                }
            }

            patternItems.Add(item);
        }
    }

    private void ExtractBalusterPatternDependencies(
        BalusterPattern pattern,
        Document document,
        List<RailingDependencyJson> dependencies)
    {
        var balusterCount = pattern.GetBalusterCount();

        for (int i = 0; i < balusterCount; i++)
        {
            var balusterInfo = pattern.GetBaluster(i);
            if (balusterInfo == null || !balusterInfo.IsValidObject)
                continue;

            var balusterFamilyId = balusterInfo.BalusterFamilyId;
            if (balusterFamilyId == null || balusterFamilyId == ElementId.InvalidElementId)
                continue;

            var balusterFamily = document.GetElement(balusterFamilyId);
            if (balusterFamily is FamilySymbol familySymbol)
            {
                dependencies.Add(new RailingDependencyJson
                {
                    FamilyName = familySymbol.FamilyName,
                    TypeName = familySymbol.Name,
                    Category = GetBalusterCategory(familySymbol),
                    InLibrary = false
                });
            }
        }
    }

    private void ExtractHandrailDependencies(
        RailingType railingType,
        Document document,
        List<RailingDependencyJson> dependencies)
    {
        try
        {
            var primaryId = railingType.PrimaryHandrailType;
            if (primaryId != null && primaryId != ElementId.InvalidElementId)
            {
                var elem = document.GetElement(primaryId);
                if (elem != null)
                {
                    dependencies.Add(new RailingDependencyJson
                    {
                        TypeName = elem.Name,
                        Category = "Handrails",
                        InLibrary = false
                    });
                }
            }

            var secondaryId = railingType.SecondaryHandrailType;
            if (secondaryId != null && secondaryId != ElementId.InvalidElementId)
            {
                var elem = document.GetElement(secondaryId);
                if (elem != null)
                {
                    dependencies.Add(new RailingDependencyJson
                    {
                        TypeName = elem.Name,
                        Category = "Handrails",
                        InLibrary = false
                    });
                }
            }
        }
        catch
        {
        }
    }

    private Dictionary<string, HashSet<string>> GetBalusterFamilies(Document document)
    {
        var result = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        var familySymbols = new FilteredElementCollector(document)
            .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>();

        foreach (var symbol in familySymbols)
        {
            var familyName = symbol.FamilyName;
            var typeName = symbol.Name;

            if (!result.ContainsKey(familyName))
            {
                result[familyName] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }
            result[familyName].Add(typeName);
        }

        return result;
    }

    private bool CheckBalusterExists(RailingDependencyJson dep, Dictionary<string, HashSet<string>> families)
    {
        if (!string.IsNullOrEmpty(dep.FamilyName) && families.TryGetValue(dep.FamilyName, out var types))
        {
            if (string.IsNullOrEmpty(dep.TypeName))
                return true;

            return types.Contains(dep.TypeName);
        }

        if (!string.IsNullOrEmpty(dep.TypeName))
        {
            foreach (var typeSet in families.Values)
            {
                if (typeSet.Contains(dep.TypeName))
                    return true;
            }
        }

        return false;
    }

    private static string GetBalusterCategory(FamilySymbol symbol)
    {
        var category = symbol.Category;
        return category?.Name ?? string.Empty;
    }
}
