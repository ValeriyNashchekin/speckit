using FamilyLibrary.Domain.Enums;

namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// Request for batch checking multiple families against the library.
/// </summary>
public record BatchCheckRequest
{
    public List<FamilyCheckItem> Families { get; init; } = [];
}

/// <summary>
/// Single family item for batch check.
/// </summary>
public record FamilyCheckItem
{
    public string RoleName { get; init; } = string.Empty;
    public string Hash { get; init; } = string.Empty;
}

/// <summary>
/// Response for batch check operation.
/// </summary>
public record BatchCheckResponse
{
    public List<FamilyCheckResult> Results { get; init; } = [];
}

/// <summary>
/// Result for a single family check.
/// </summary>
public record FamilyCheckResult
{
    public string RoleName { get; init; } = string.Empty;
    public FamilyScanStatus Status { get; init; }
    public int? LibraryVersion { get; init; }
    public int? CurrentVersion { get; init; }
    public string? LibraryHash { get; init; }
}
