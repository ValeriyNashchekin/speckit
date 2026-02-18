using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Application.Interfaces;
using FamilyLibrary.Domain.Entities;
using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Infrastructure.Services;

/// <summary>
/// Service for detecting changes between family snapshots.
/// Implements field-by-field comparison with category-specific logic.
/// </summary>
public class ChangeDetectionService : IChangeDetectionService
{
    /// <summary>
    /// Detects changes between previous and current family snapshots.
    /// </summary>
    /// <param name="previous">Previous snapshot (null for new families).</param>
    /// <param name="current">Current snapshot.</param>
    /// <returns>Set of detected changes.</returns>
    public ChangeSetDto DetectChanges(FamilySnapshot? previous, FamilySnapshot current)
    {
        var changes = new ChangeSetDto();

        // Handle new family case - all fields are considered "new"
        if (previous is null)
        {
            return DetectAllChangesForNewFamily(current);
        }

        // Name change
        if (previous.FamilyName != current.FamilyName)
        {
            changes.Items.Add(new ChangeItemDto
            {
                Category = ChangeCategory.Name,
                PreviousValue = previous.FamilyName,
                CurrentValue = current.FamilyName
            });
        }

        // Category change
        if (previous.Category != current.Category)
        {
            changes.Items.Add(new ChangeItemDto
            {
                Category = ChangeCategory.Category,
                PreviousValue = previous.Category,
                CurrentValue = current.Category
            });
        }

        // Types change
        DetectTypesChanges(previous, current, changes);

        // Parameters change
        DetectParametersChanges(previous, current, changes);

        // Geometry change (hash-based flag)
        if (current.HasGeometryChanges)
        {
            changes.Items.Add(new ChangeItemDto
            {
                Category = ChangeCategory.Geometry
            });
        }

        // TXT change
        if (previous.TxtHash != current.TxtHash)
        {
            changes.Items.Add(new ChangeItemDto
            {
                Category = ChangeCategory.Txt,
                PreviousValue = previous.TxtHash,
                CurrentValue = current.TxtHash
            });
        }

        return changes;
    }

    private static ChangeSetDto DetectAllChangesForNewFamily(FamilySnapshot current)
    {
        var changes = new ChangeSetDto();

        // Name
        changes.Items.Add(new ChangeItemDto
        {
            Category = ChangeCategory.Name,
            PreviousValue = null,
            CurrentValue = current.FamilyName
        });

        // Category
        changes.Items.Add(new ChangeItemDto
        {
            Category = ChangeCategory.Category,
            PreviousValue = null,
            CurrentValue = current.Category
        });

        // Types (all are "added")
        if (current.Types.Count > 0)
        {
            changes.Items.Add(new ChangeItemDto
            {
                Category = ChangeCategory.Types,
                AddedItems = current.Types,
                RemovedItems = []
            });
        }

        // Parameters (all are "added")
        if (current.Parameters.Count > 0)
        {
            changes.Items.Add(new ChangeItemDto
            {
                Category = ChangeCategory.Parameters,
                ParameterChanges = current.Parameters
                    .Select(p => new ParameterChangeDto
                    {
                        Name = p.Name,
                        PreviousValue = null,
                        CurrentValue = p.Value
                    })
                    .ToList()
            });
        }

        // TXT
        if (current.TxtHash is not null)
        {
            changes.Items.Add(new ChangeItemDto
            {
                Category = ChangeCategory.Txt,
                PreviousValue = null,
                CurrentValue = current.TxtHash
            });
        }

        return changes;
    }

    private static void DetectTypesChanges(FamilySnapshot previous, FamilySnapshot current, ChangeSetDto changes)
    {
        var addedTypes = current.Types.Except(previous.Types).ToList();
        var removedTypes = previous.Types.Except(current.Types).ToList();

        if (addedTypes.Count > 0 || removedTypes.Count > 0)
        {
            changes.Items.Add(new ChangeItemDto
            {
                Category = ChangeCategory.Types,
                AddedItems = addedTypes,
                RemovedItems = removedTypes
            });
        }
    }

    private static void DetectParametersChanges(FamilySnapshot previous, FamilySnapshot current, ChangeSetDto changes)
    {
        var paramChanges = CompareParameters(previous.Parameters, current.Parameters);

        if (paramChanges.Count > 0)
        {
            changes.Items.Add(new ChangeItemDto
            {
                Category = ChangeCategory.Parameters,
                ParameterChanges = paramChanges
            });
        }
    }

    private static List<ParameterChangeDto> CompareParameters(
        List<ParameterSnapshot> previous,
        List<ParameterSnapshot> current)
    {
        var changes = new List<ParameterChangeDto>();
        var prevDict = previous.ToDictionary(p => p.Name);
        var currDict = current.ToDictionary(p => p.Name);

        // Modified and added parameters
        foreach (var (name, curr) in currDict)
        {
            if (prevDict.TryGetValue(name, out var prev))
            {
                // Check if value changed
                if (prev.Value != curr.Value)
                {
                    changes.Add(new ParameterChangeDto
                    {
                        Name = name,
                        PreviousValue = prev.Value,
                        CurrentValue = curr.Value
                    });
                }
            }
            else
            {
                // New parameter
                changes.Add(new ParameterChangeDto
                {
                    Name = name,
                    PreviousValue = null,
                    CurrentValue = curr.Value
                });
            }
        }

        // Removed parameters
        foreach (var name in prevDict.Keys.Except(currDict.Keys))
        {
            changes.Add(new ParameterChangeDto
            {
                Name = name,
                PreviousValue = prevDict[name].Value,
                CurrentValue = null
            });
        }

        return changes;
    }
}
