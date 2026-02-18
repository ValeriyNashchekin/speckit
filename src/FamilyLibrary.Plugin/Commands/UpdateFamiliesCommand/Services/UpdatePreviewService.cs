using Autodesk.Revit.DB;
using FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;
using FamilyLibrary.Plugin.Core.Models;

namespace FamilyLibrary.Plugin.Commands.UpdateFamiliesCommand.Services;

/// <summary>
/// Service for computing update preview before applying changes.
/// Delegates to PluginChangeDetectionService for change detection.
/// </summary>
public class UpdatePreviewService
{
    private readonly PluginChangeDetectionService _changeDetectionService;

    public UpdatePreviewService() : this(new PluginChangeDetectionService())
    {
    }

    public UpdatePreviewService(PluginChangeDetectionService changeDetectionService)
    {
        _changeDetectionService = changeDetectionService 
            ?? throw new ArgumentNullException(nameof(changeDetectionService));
    }

    /// <summary>
    /// Computes a preview of changes between current family state and library version.
    /// </summary>
    /// <param name="family">The family to compare.</param>
    /// <param name="document">The Revit document.</param>
    /// <param name="librarySnapshot">The library snapshot to compare against.</param>
    /// <returns>ChangeSet with detected modifications.</returns>
    public ChangeSet ComputePreview(Family family, Document document, FamilySnapshot librarySnapshot)
    {
        if (family == null)
            throw new ArgumentNullException(nameof(family));

        if (document == null)
            throw new ArgumentNullException(nameof(document));

        if (librarySnapshot == null)
            throw new ArgumentNullException(nameof(librarySnapshot));

        return _changeDetectionService.DetectChanges(family, document, librarySnapshot);
    }
}
