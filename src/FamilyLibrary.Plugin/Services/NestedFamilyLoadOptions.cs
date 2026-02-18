using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace FamilyLibrary.Plugin.Services;

/// <summary>
/// Family load options that handles nested families based on user choices.
/// Used when loading a parent family that contains nested shared families.
/// T040: Process individual NestedLoadChoice per family.
/// </summary>
/// <remarks>
/// Choice mapping:
/// - UseLibraryVersion=false: Use FamilySource.Family (load from RFA embedded version)
/// - UseLibraryVersion=true: Use FamilySource.Project (use existing project version,
///   then NestedFamilyLoadService will override with library version in Phase 2)
/// </remarks>
public class NestedFamilyLoadOptions : IFamilyLoadOptions
{
    private readonly Dictionary<string, NestedLoadChoice> _choices;
    private readonly List<string> _processedFamilies;

    /// <summary>
    /// Gets the list of families that were processed with library choice.
    /// Used by NestedFamilyLoadService to determine which families need library override.
    /// </summary>
    public IReadOnlyList<string> LibrarySourceFamilies => _processedFamilies;

    /// <summary>
    /// Creates load options with user choices for nested families.
    /// </summary>
    /// <param name="choices">Dictionary of choices keyed by nested family name.</param>
    public NestedFamilyLoadOptions(Dictionary<string, NestedLoadChoice> choices)
    {
        _choices = choices ?? new Dictionary<string, NestedLoadChoice>(StringComparer.OrdinalIgnoreCase);
        _processedFamilies = new List<string>();
    }

    /// <summary>
    /// Called when a family is found in the target document.
    /// Determines whether to overwrite based on user choices.
    /// </summary>
    public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
    {
        // Default: overwrite to get latest version
        overwriteParameterValues = true;
        return true;
    }

    /// <summary>
    /// Called when a shared family is found in the target document.
    /// Determines whether to load from project or from the RFA file based on choices.
    /// T040: Maps NestedLoadChoice to FamilySource enum.
    /// </summary>
    /// <param name="sharedFamily">The shared family found in the document.</param>
    /// <param name="familyInUse">Indicates if family instances are placed in project.</param>
    /// <param name="source">Output: FamilySource.Family for RFA version, FamilySource.Project for library version.</param>
    /// <param name="overwriteParameterValues">Output: Whether to overwrite parameter values.</param>
    /// <returns>True to continue loading, false to cancel.</returns>
    public bool OnSharedFamilyFound(
        Family sharedFamily,
        bool familyInUse,
        out FamilySource source,
        out bool overwriteParameterValues)
    {
        // Default values for null family case
        source = FamilySource.Family;
        overwriteParameterValues = true;

        if (sharedFamily == null)
        {
            return true;
        }

        var familyName = sharedFamily.Name;

        // Check if we have a specific choice for this nested family
        if (_choices.TryGetValue(familyName, out var choice))
        {
            if (choice.UseLibraryVersion)
            {
                // User wants library version
                // Use FamilySource.Project to keep existing project version during Phase 1
                // NestedFamilyLoadService will download and override in Phase 2
                source = FamilySource.Project;
                overwriteParameterValues = false;
                _processedFamilies.Add(familyName);
                return true;
            }

            // Use RFA embedded version (source = 'rfa' in UI)
            source = FamilySource.Family;
            overwriteParameterValues = true;
            return true;
        }

        // No specific choice - use default (load from RFA)
        source = FamilySource.Family;
        overwriteParameterValues = true;
        return true;
    }
}

/// <summary>
/// Simple family load options that always overwrites.
/// Used for loading nested families from library.
/// </summary>
public class SimpleFamilyLoadOptions : IFamilyLoadOptions
{
    public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues)
    {
        overwriteParameterValues = true;
        return true;
    }

    public bool OnSharedFamilyFound(
        Family sharedFamily,
        bool familyInUse,
        out FamilySource source,
        out bool overwriteParameterValues)
    {
        source = FamilySource.Family;
        overwriteParameterValues = true;
        return true;
    }
}