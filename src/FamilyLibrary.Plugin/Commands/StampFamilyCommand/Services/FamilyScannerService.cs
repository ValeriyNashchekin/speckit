using Autodesk.Revit.DB;
using FamilyLibrary.Plugin.Core.Entities;
using FamilyLibrary.Plugin.Core.Interfaces;
using FamilyLibrary.Plugin.Core.Models;
using FamilyLibrary.Plugin.Infrastructure.ExtensibleStorage;
using Newtonsoft.Json;

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;

/// <summary>
/// Service for scanning document for loadable families.
/// Uses FilteredElementCollector with optimized filters.
/// </summary>
public class FamilyScannerService
{
    private readonly IEsService _esService;

    public FamilyScannerService() : this(new EsService())
    {
    }

    public FamilyScannerService(IEsService esService)
    {
        _esService = esService;
    }

    /// <summary>
    /// Scan document for all loadable families (FamilySymbol elements).
    /// Returns list of FamilyInfo with stamp data.
    /// </summary>
    public List<FamilyInfo> ScanLoadableFamilies(Document document)
    {
        if (document == null) return new List<FamilyInfo>();

        // Collect FamilySymbols ONCE - avoid collector in loop
        var familySymbols = new FilteredElementCollector(document)
            .OfClass(typeof(FamilySymbol))
            .WhereElementIsElementType()
            .Cast<FamilySymbol>()
            .ToList();

        // Get unique families by FamilyId
        var familyMap = new Dictionary<ElementId, Family>();
        foreach (var symbol in familySymbols)
        {
            var family = symbol.Family;
            if (family != null && !familyMap.ContainsKey(family.Id))
            {
                familyMap[family.Id] = family;
            }
        }

        // Build result list
        var result = new List<FamilyInfo>();
        foreach (var kvp in familyMap)
        {
            var family = kvp.Value;

            // Skip in-place and system families
            if (family.IsInPlace) continue;

            var info = CreateFamilyInfo(family);
            result.Add(info);
        }

        return result;
    }

    private FamilyInfo CreateFamilyInfo(Family family)
    {
        var stampData = _esService.ReadStamp(family);
        var category = family.FamilyCategory;

        return new FamilyInfo
        {
            UniqueId = family.UniqueId,
            Name = family.Name,
            CategoryName = category?.Name ?? "Uncategorized",
            IsSystemFamily = false,
            HasStamp = stampData?.IsValid == true,
            StampData = stampData
        };
    }
}

/// <summary>
/// Service for creating family snapshots.
/// Captures family state for comparison and version tracking.
/// </summary>
public class SnapshotService
{
    private const int SnapshotVersion = 1;

    /// <summary>
    /// Creates a snapshot of a family's current state.
    /// </summary>
    /// <param name="family">The family element to snapshot.</param>
    /// <param name="document">The Revit document.</param>
    /// <returns>FamilySnapshot object.</returns>
    public FamilySnapshot CreateSnapshot(Family family, Document document)
    {
        if (family == null)
            throw new ArgumentNullException(nameof(family));
        if (document == null)
            throw new ArgumentNullException(nameof(document));

        var types = GetFamilyTypeNames(family, document);
        var parameters = GetFamilyParameters(family, document);

        return new FamilySnapshot
        {
            Version = SnapshotVersion,
            FamilyName = family.Name,
            Category = family.FamilyCategory?.Name ?? "Uncategorized",
            Types = types,
            Parameters = parameters,
            HasGeometryChanges = false,
            TxtHash = null
        };
    }

    /// <summary>
    /// Creates a snapshot and serializes it to JSON.
    /// </summary>
    /// <param name="family">The family element to snapshot.</param>
    /// <param name="document">The Revit document.</param>
    /// <returns>JSON string representation of the snapshot.</returns>
    public string CreateSnapshotJson(Family family, Document document)
    {
        var snapshot = CreateSnapshot(family, document);
        return JsonConvert.SerializeObject(snapshot, Formatting.Indented);
    }

    /// <summary>
    /// Gets all type names for a family.
    /// Collects FamilySymbols ONCE per family.
    /// </summary>
    private List<string> GetFamilyTypeNames(Family family, Document document)
    {
        var typeNames = new List<string>();
        var symbolIds = family.GetFamilySymbolIds();

        foreach (var symbolId in symbolIds)
        {
            var symbol = document.GetElement(symbolId) as FamilySymbol;
            if (symbol != null)
            {
                typeNames.Add(symbol.Name);
            }
        }

        return typeNames;
    }

    /// <summary>
    /// Gets family parameters from the first FamilySymbol.
    /// Family elements do not expose parameters directly - use FamilySymbol.
    /// </summary>
    private List<ParameterSnapshot> GetFamilyParameters(Family family, Document document)
    {
        var parameters = new List<ParameterSnapshot>();

        // Get parameters from first FamilySymbol (parameters are same across types)
        var symbolIds = family.GetFamilySymbolIds();
        foreach (var symbolId in symbolIds)
        {
            var symbol = document.GetElement(symbolId) as FamilySymbol;
            if (symbol == null) continue;

            foreach (Parameter param in symbol.Parameters)
            {
                if (param == null || param.Definition == null || param.IsReadOnly)
                    continue;

                var snapshot = new ParameterSnapshot
                {
                    Name = param.Definition.Name,
                    Value = GetParameterValueAsString(param),
                    Group = GetParameterGroupName(param)
                };

                parameters.Add(snapshot);
            }
            break; // Only need parameters from first symbol
        }

        return parameters;
    }

    /// <summary>
    /// Gets parameter value as string based on storage type.
    /// </summary>
    private static string GetParameterValueAsString(Parameter param)
    {
        if (param == null)
            return null;

        switch (param.StorageType)
        {
            case StorageType.String:
                return param.AsString();
            case StorageType.Double:
                return param.AsDouble().ToString("F6");
            case StorageType.Integer:
                return param.AsInteger().ToString();
            case StorageType.ElementId:
                return GetElementIdValue(param.AsElementId()).ToString();
            case StorageType.None:
            default:
                return null;
        }
    }

    /// <summary>
    /// Gets the integer value of an ElementId in a version-compatible way.
    /// Revit 2024+ uses Value property, earlier versions use IntegerValue.
    /// </summary>
    private static int GetElementIdValue(ElementId elementId)
    {
        if (elementId == null || elementId == ElementId.InvalidElementId)
            return -1;

#if REVIT2024 || REVIT2025 || REVIT2026
        return (int)elementId.Value;
#else
        return elementId.IntegerValue;
#endif
    }

    /// <summary>
    /// Gets parameter group name in a version-compatible way.
    /// Revit 2024+ uses GetGroupTypeId(), earlier versions use ParameterGroup property.
    /// </summary>
    private static string GetParameterGroupName(Parameter param)
    {
        if (param?.Definition == null)
            return string.Empty;

#if REVIT2024 || REVIT2025 || REVIT2026
        var groupId = param.Definition.GetGroupTypeId();
        return groupId?.TypeId ?? string.Empty;
#else
        return param.Definition.ParameterGroup.ToString();
#endif
    }
}
