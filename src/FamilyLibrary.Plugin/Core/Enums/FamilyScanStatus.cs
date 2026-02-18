namespace FamilyLibrary.Plugin.Core.Enums;

/// <summary>
/// Status of a family after scanning for changes.
/// </summary>
public enum FamilyScanStatus
{
    UpToDate = 0,
    UpdateAvailable = 1,
    LegacyMatch = 2,
    Unmatched = 3,
    LocalModified = 4
}
