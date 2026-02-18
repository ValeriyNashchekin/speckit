using Autodesk.Revit.DB;
using FamilyLibrary.Plugin.Core.Models;

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;

/// <summary>
/// Facade for detecting local changes in families before publish.
/// Wraps PluginChangeDetectionService for convenient usage in publish workflow.
/// </summary>
public class LocalChangeDetector
{
    private readonly PluginChangeDetectionService _detectionService;

    public LocalChangeDetector() : this(new PluginChangeDetectionService())
    {
    }

    public LocalChangeDetector(PluginChangeDetectionService detectionService)
    {
        _detectionService = detectionService ?? throw new ArgumentNullException(nameof(detectionService));
    }

    /// <summary>
    /// Detects local changes between current family state and library snapshot.
    /// Used before publishing to show what will be updated.
    /// </summary>
    /// <param name="family">The family to check for changes.</param>
    /// <param name="doc">The Revit document containing the family.</param>
    /// <param name="librarySnapshot">The snapshot from the library to compare against.</param>
    /// <returns>ChangeSet with detected local modifications.</returns>
    public ChangeSet DetectChanges(Family family, Document doc, FamilySnapshot librarySnapshot)
    {
        if (family == null)
            throw new ArgumentNullException(nameof(family));

        if (doc == null)
            throw new ArgumentNullException(nameof(doc));

        if (librarySnapshot == null)
            return new ChangeSet();

        return _detectionService.DetectChanges(family, doc, librarySnapshot);
    }

    /// <summary>
    /// Checks if a family has any local changes compared to library snapshot.
    /// Quick check without detailed change information.
    /// </summary>
    /// <param name="family">The family to check.</param>
    /// <param name="doc">The Revit document.</param>
    /// <param name="librarySnapshot">The library snapshot to compare against.</param>
    /// <returns>True if there are local changes, false otherwise.</returns>
    public bool HasLocalChanges(Family family, Document doc, FamilySnapshot librarySnapshot)
    {
        if (librarySnapshot == null)
            return false;

        var changeSet = DetectChanges(family, doc, librarySnapshot);
        return changeSet.HasChanges;
    }
}
