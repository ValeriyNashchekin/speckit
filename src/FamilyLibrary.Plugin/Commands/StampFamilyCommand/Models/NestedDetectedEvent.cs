#nullable enable

namespace FamilyLibrary.Plugin.Commands.StampFamilyCommand.Models;

/// <summary>
/// Event payload for revit:nested:detected WebView2 event.
/// Emitted when nested families are detected during Publish workflow.
/// </summary>
public class NestedDetectedEvent
{
    /// <summary>
    /// Event type identifier.
    /// </summary>
    public string Type => "revit:nested:detected";

    /// <summary>
    /// Unique ID of the parent family element in Revit.
    /// </summary>
    public string ParentFamilyId { get; set; } = string.Empty;

    /// <summary>
    /// Name of the parent family.
    /// </summary>
    public string ParentFamilyName { get; set; } = string.Empty;

    /// <summary>
    /// List of detected nested families with their status.
    /// </summary>
    public List<NestedFamilyInfo> NestedFamilies { get; set; } = new();
}

/// <summary>
/// DTO for saving dependencies to API.
/// </summary>
public class SaveDependencyDto
{
    public string NestedFamilyName { get; set; } = string.Empty;
    public string? NestedRoleName { get; set; }
    public bool IsShared { get; set; }
    public bool InLibrary { get; set; }
    public int? LibraryVersion { get; set; }
}

/// <summary>
/// Request for saving dependencies to API.
/// </summary>
public class SaveDependenciesRequest
{
    public List<SaveDependencyDto> Dependencies { get; set; } = new();
}
