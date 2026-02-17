namespace FamilyLibrary.Application.Common;

/// <summary>
/// Result of a batch create operation.
/// </summary>
public record BatchCreateResult
{
    /// <summary>
    /// IDs of successfully created entities.
    /// </summary>
    public IReadOnlyList<Guid> CreatedIds { get; init; } = [];
    
    /// <summary>
    /// Names of items that were skipped (duplicates).
    /// </summary>
    public IReadOnlyList<string> SkippedNames { get; init; } = [];
    
    /// <summary>
    /// Total number of items processed.
    /// </summary>
    public int TotalProcessed { get; init; }
    
    /// <summary>
    /// Number of items successfully created.
    /// </summary>
    public int CreatedCount => CreatedIds.Count;
    
    /// <summary>
    /// Number of items skipped.
    /// </summary>
    public int SkippedCount => SkippedNames.Count;
    
    public BatchCreateResult() { }
    
    public BatchCreateResult(IReadOnlyList<Guid> createdIds, IReadOnlyList<string> skippedNames, int totalProcessed)
    {
        CreatedIds = createdIds;
        SkippedNames = skippedNames;
        TotalProcessed = totalProcessed;
    }
}
