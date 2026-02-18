namespace FamilyLibrary.Domain.Enums;

/// <summary>
/// Recommended action for loading a nested family into a Revit project.
/// </summary>
public enum NestedLoadAction
{
    /// <summary>
    /// Load the family from the RFA file (first time use or RFA version is the only source).
    /// </summary>
    LoadFromRfa = 0,

    /// <summary>
    /// Update from the library (library has a newer version).
    /// </summary>
    UpdateFromLibrary = 1,

    /// <summary>
    /// Keep the current project version (project already has same or newer version).
    /// </summary>
    KeepProjectVersion = 2,

    /// <summary>
    /// No action needed (non-shared family embedded in parent).
    /// </summary>
    NoAction = 3
}
