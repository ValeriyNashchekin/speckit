using Autodesk.Revit.DB;
using FamilyLibrary.Plugin.Core.Enums;
using FamilyLibrary.Plugin.Core.Models;

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Services;

/// <summary>
/// Plugin-side change detection for local modifications before publish.
/// Compares current family state with library snapshot.
/// </summary>
public class PluginChangeDetectionService
{
    private readonly SnapshotService _snapshotService;

    public PluginChangeDetectionService() : this(new SnapshotService())
    {
    }

    public PluginChangeDetectionService(SnapshotService snapshotService)
    {
        _snapshotService = snapshotService;
    }

    /// <summary>
    /// Detects changes between current family state and a previous snapshot.
    /// </summary>
    /// <param name="family">The family to compare.</param>
    /// <param name="document">The project document.</param>
    /// <param name="previousSnapshot">The previous snapshot to compare against.</param>
    /// <returns>A ChangeSet with detected modifications.</returns>
    public ChangeSet DetectChanges(Family family, Document document, FamilySnapshot previousSnapshot)
    {
        if (family == null)
            throw new ArgumentNullException(nameof(family));

        if (document == null)
            throw new ArgumentNullException(nameof(document));

        var changeSet = new ChangeSet();

        if (previousSnapshot == null)
        {
            return changeSet;
        }

        var currentSnapshot = _snapshotService.CreateSnapshot(family, document);

        // Compare basic properties
        DetectPropertyChanges(currentSnapshot, previousSnapshot, changeSet);

        // Compare types
        DetectTypeChanges(currentSnapshot, previousSnapshot, changeSet);

        // Compare parameters
        DetectParameterChanges(currentSnapshot, previousSnapshot, changeSet);

        return changeSet;
    }

    private void DetectPropertyChanges(FamilySnapshot current, FamilySnapshot previous, ChangeSet changeSet)
    {
        if (current.FamilyName != previous.FamilyName)
        {
            changeSet.Items.Add(new ChangeItem
            {
                Category = ChangeCategory.Category,
                PreviousValue = previous.FamilyName,
                CurrentValue = current.FamilyName
            });
        }

        if (current.Category != previous.Category)
        {
            changeSet.Items.Add(new ChangeItem
            {
                Category = ChangeCategory.Category,
                PreviousValue = previous.Category,
                CurrentValue = current.Category
            });
        }
    }

    private void DetectTypeChanges(FamilySnapshot current, FamilySnapshot previous, ChangeSet changeSet)
    {
        var addedTypes = current.Types.Except(previous.Types).ToList();
        var removedTypes = previous.Types.Except(current.Types).ToList();

        if (addedTypes.Count > 0 || removedTypes.Count > 0)
        {
            changeSet.Items.Add(new ChangeItem
            {
                Category = ChangeCategory.Types,
                AddedItems = addedTypes,
                RemovedItems = removedTypes
            });
        }
    }

    private void DetectParameterChanges(FamilySnapshot current, FamilySnapshot previous, ChangeSet changeSet)
    {
        var paramChanges = new List<ParameterChange>();

        var currentParams = current.Parameters.ToDictionary(p => p.Name);
        var previousParams = previous.Parameters.ToDictionary(p => p.Name);

        // Check for changed and removed parameters
        foreach (var prevParam in previousParams)
        {
            if (!currentParams.TryGetValue(prevParam.Key, out var currentParam))
            {
                paramChanges.Add(new ParameterChange
                {
                    Name = prevParam.Key,
                    PreviousValue = prevParam.Value.Value,
                    CurrentValue = null
                });
            }
            else if (currentParam.Value != prevParam.Value.Value)
            {
                paramChanges.Add(new ParameterChange
                {
                    Name = prevParam.Key,
                    PreviousValue = prevParam.Value.Value,
                    CurrentValue = currentParam.Value
                });
            }
        }

        // Check for added parameters
        foreach (var currentParam in currentParams)
        {
            if (!previousParams.ContainsKey(currentParam.Key))
            {
                paramChanges.Add(new ParameterChange
                {
                    Name = currentParam.Key,
                    PreviousValue = null,
                    CurrentValue = currentParam.Value.Value
                });
            }
        }

        if (paramChanges.Count > 0)
        {
            changeSet.Items.Add(new ChangeItem
            {
                Category = ChangeCategory.Parameters,
                ParameterChanges = paramChanges
            });
        }
    }
}
