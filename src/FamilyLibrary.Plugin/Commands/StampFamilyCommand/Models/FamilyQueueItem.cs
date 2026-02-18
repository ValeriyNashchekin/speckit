using FamilyLibrary.Plugin.Core.Entities;

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;

/// <summary>
/// Represents a family item in the library queue.
/// Pure domain model without Revit API dependencies.
/// </summary>
public class FamilyQueueItem
{
    public string UniqueId { get; set; } = string.Empty;
    public string FamilyName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? SourcePath { get; set; }
    public bool IsSelected { get; set; }
    public bool HasStamp { get; set; }
    public bool IsSystemFamily { get; set; }
    public EsStampData? StampData { get; set; }
    public QueueItemStatus Status { get; set; }
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Indicates if family has local modifications compared to library version.
    /// Only applicable for stamped families with valid RoleId.
    /// </summary>
    public bool HasLocalChanges { get; set; }
}

/// <summary>
/// Status of a queue item during processing.
/// </summary>
public enum QueueItemStatus
{
    Pending,
    Scanned,
    Stamped,
    Published,
    Failed
}
