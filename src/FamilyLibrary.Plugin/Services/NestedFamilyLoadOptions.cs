using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;

namespace FamilyLibrary.Plugin.Services;

/// <summary>
/// Family load options that handles nested families based on user choices.
/// Used when loading a parent family that contains nested shared families.
/// </summary>
public class NestedFamilyLoadOptions : IFamilyLoadOptions
{
    private readonly Dictionary<string, NestedLoadChoice> _choices;

    /// <summary>
    /// Creates load options with user choices for nested families.
    /// </summary>
    /// <param name="choices">Dictionary of choices keyed by nested family name.</param>
    public NestedFamilyLoadOptions(Dictionary<string, NestedLoadChoice> choices)
    {
        _choices = choices ?? new Dictionary<string, NestedLoadChoice>();
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
    /// </summary>
    public bool OnSharedFamilyFound(
        Family sharedFamily,
        bool familyInUse,
        out FamilySource source,
        out bool overwriteParameterValues)
    {
        if (sharedFamily == null)
        {
            source = FamilySource.Family;
            overwriteParameterValues = true;
            return true;
        }

        var familyName = sharedFamily.Name;

        // Check if we have a specific choice for this nested family
        if (_choices.TryGetValue(familyName, out var choice))
        {
            if (choice.UseLibraryVersion)
            {
                // User wants library version - load from project for now
                // Will be updated in phase 2 from library
                source = FamilySource.Project;
                overwriteParameterValues = false;
                return true;
            }

            // Use RFA embedded version
            source = FamilySource.Family;
            overwriteParameterValues = true;
            return true;
        }

        // No specific choice - use defaults (load from RFA)
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