#nullable enable
using Autodesk.Revit.DB;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;
using FamilyLibrary.Plugin.Core.Interfaces;
using FamilyLibrary.Plugin.Infrastructure.ExtensibleStorage;

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;

/// <summary>
/// Service for detecting nested families within a parent family.
/// Uses EditFamily to open family document and scan for nested families.
/// </summary>
public class NestedDetectionService
{
    private readonly IEsService _esService;

    /// <summary>
    /// Default constructor for production use.
    /// </summary>
    public NestedDetectionService() : this(new EsService())
    {
    }

    /// <summary>
    /// Constructor for testing with mocked IEsService.
    /// </summary>
    public NestedDetectionService(IEsService esService)
    {
        _esService = esService ?? throw new ArgumentNullException(nameof(esService));
    }

    /// <summary>
    /// Detects all nested families within a parent family.
    /// Opens family document via EditFamily, scans for nested families,
    /// checks FAMILY_SHARED parameter, and reads ES stamp if present.
    /// </summary>
    /// <param name="document">The project document containing the parent family.</param>
    /// <param name="parentFamily">The parent family to scan for nested families.</param>
    /// <returns>List of NestedFamilyInfo objects representing detected nested families.</returns>
    public List<NestedFamilyInfo> Detect(Document document, Family parentFamily)
    {
        if (document == null)
            throw new ArgumentNullException(nameof(document));
        if (parentFamily == null)
            throw new ArgumentNullException(nameof(parentFamily));

        // Validate family is editable
        if (parentFamily.IsInPlace || !parentFamily.IsEditable)
        {
            return new List<NestedFamilyInfo>();
        }

        Document? familyDoc = null;
        try
        {
            // Open family document for editing (creates independent copy)
            familyDoc = document.EditFamily(parentFamily);

            // Collect all nested families ONCE
            var nestedFamilies = new FilteredElementCollector(familyDoc)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .Where(f => f.IsValidObject)
                .ToList();

            // Build result list
            var result = new List<NestedFamilyInfo>();
            foreach (var nested in nestedFamilies)
            {
                var info = CreateNestedFamilyInfo(nested);
                result.Add(info);
            }

            return result;
        }
        finally
        {
            // Always close family document without saving
            // EditFamily creates independent copy, no changes to save
            if (familyDoc != null && familyDoc.IsValidObject)
            {
                familyDoc.Close(false);
            }
        }
    }

    /// <summary>
    /// Creates NestedFamilyInfo from a nested family element.
    /// Checks IsShared via FAMILY_SHARED parameter and reads ES stamp.
    /// </summary>
    private NestedFamilyInfo CreateNestedFamilyInfo(Family nested)
    {
        var isShared = CheckIsShared(nested);
        var stampData = _esService.ReadStamp(nested);
        var hasStamp = stampData?.IsValid == true;

        return new NestedFamilyInfo
        {
            FamilyName = nested.Name,
            IsShared = isShared,
            HasRole = hasStamp,
            RoleName = hasStamp && stampData != null ? stampData.RoleName : null,
            Status = DetermineStatus(isShared, hasStamp)
        };
    }

    /// <summary>
    /// Checks if family is shared via FAMILY_SHARED built-in parameter.
    /// Uses BuiltInParameter for reliability and performance.
    /// </summary>
    private bool CheckIsShared(Family family)
    {
        var param = family.get_Parameter(BuiltInParameter.FAMILY_SHARED);
        if (param == null)
            return false;

        return param.AsInteger() == 1;
    }

    /// <summary>
    /// Determines the status of a nested family.
    /// Possible values: "ready", "not_shared", "no_role".
    /// </summary>
    private static string DetermineStatus(bool isShared, bool hasRole)
    {
        if (!isShared)
            return "not_shared";
        if (!hasRole)
            return "no_role";
        return "ready";
    }
}
