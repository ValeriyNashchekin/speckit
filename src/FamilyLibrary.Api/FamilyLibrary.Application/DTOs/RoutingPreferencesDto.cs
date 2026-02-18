namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// DTO for RoutingPreferences serialization (PipeType/DuctType).
/// Used for MEP System Families configuration.
/// </summary>
public record RoutingPreferencesDto
{
    /// <summary>
    /// List of routing segments with material and schedule information.
    /// </summary>
    public List<SegmentDto> Segments { get; init; } = [];

    /// <summary>
    /// List of routing fittings with family and angle information.
    /// </summary>
    public List<FittingDto> Fittings { get; init; } = [];
}

/// <summary>
/// DTO for a routing segment (material configuration).
/// </summary>
public record SegmentDto
{
    /// <summary>
    /// Name of the segment material.
    /// </summary>
    public required string MaterialName { get; init; }

    /// <summary>
    /// Schedule type for the segment (e.g., "Standard", "40", "80").
    /// </summary>
    public required string ScheduleType { get; init; }
}

/// <summary>
/// DTO for a routing fitting (transition component).
/// </summary>
public record FittingDto
{
    /// <summary>
    /// Revit family name for the fitting.
    /// </summary>
    public required string FamilyName { get; init; }

    /// <summary>
    /// Revit type name within the family.
    /// </summary>
    public required string TypeName { get; init; }

    /// <summary>
    /// Angle range for the fitting (e.g., "0-45", "45-90"). Can be null for non-angle fittings.
    /// </summary>
    public string? AngleRange { get; init; }
}
