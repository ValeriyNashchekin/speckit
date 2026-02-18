using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Domain.Entities;

namespace FamilyLibrary.Application.Interfaces;

/// <summary>
/// Service for detecting changes between family snapshots.
/// </summary>
public interface IChangeDetectionService
{
    /// <summary>
    /// Detects changes between previous and current family snapshots.
    /// </summary>
    /// <param name="previous">Previous snapshot (null for new families).</param>
    /// <param name="current">Current snapshot.</param>
    /// <returns>Set of detected changes.</returns>
    ChangeSetDto DetectChanges(FamilySnapshot? previous, FamilySnapshot current);
}
