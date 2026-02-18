namespace FamilyLibrary.Domain.Enums;

/// <summary>
/// Status of a family after scanning against the library.
/// </summary>
public enum FamilyScanStatus
{
    UpToDate = 0,
    UpdateAvailable = 1,
    LegacyMatch = 2,
    Unmatched = 3,
    LocalModified = 4
}
