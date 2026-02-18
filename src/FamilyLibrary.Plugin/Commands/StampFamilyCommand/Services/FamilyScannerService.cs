using Autodesk.Revit.DB;
using FamilyLibrary.Plugin.Core.Entities;
using FamilyLibrary.Plugin.Core.Interfaces;
using FamilyLibrary.Plugin.Infrastructure.ExtensibleStorage;

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
