using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// Result of scanning families against the library.
/// </summary>
public record ScanResultDto
{
    public List<ScannedFamilyDto> Families { get; init; } = [];
    public int TotalCount { get; init; }
    public ScanSummaryDto Summary { get; init; } = new();
}

/// <summary>
/// Summary statistics of the scan.
/// </summary>
public record ScanSummaryDto
{
    public int UpToDate { get; init; }
    public int UpdateAvailable { get; init; }
    public int LegacyMatch { get; init; }
    public int Unmatched { get; init; }
    public int LocalModified { get; init; }
}

/// <summary>
/// Information about a scanned family.
/// </summary>
public record ScannedFamilyDto
{
    public string UniqueId { get; init; } = string.Empty;
    public string FamilyName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string? RoleName { get; init; }
    public bool IsAutoRole { get; init; }
    public FamilyScanStatus Status { get; init; }
    public int? LocalVersion { get; init; }
    public int? LibraryVersion { get; init; }
    public string? LocalHash { get; init; }
    public string? LibraryHash { get; init; }
}
