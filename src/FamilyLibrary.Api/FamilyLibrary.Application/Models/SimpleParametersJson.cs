namespace FamilyLibrary.Application.Models;

/// <summary>
/// JSON model for simple parameter serialization (Group E: Levels, Grids, Ramps, Building Pads).
/// </summary>
public record SimpleParametersJson
{
    /// <summary>
    /// List of parameters.
    /// </summary>
    public required List<ParameterJson> Parameters { get; init; }
}

/// <summary>
/// Single parameter value.
/// </summary>
public record ParameterJson
{
    /// <summary>
    /// Parameter name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Parameter value as string (for serialization simplicity).
    /// </summary>
    public required string Value { get; init; }

    /// <summary>
    /// Parameter storage type (String, Double, Integer, ElementId).
    /// </summary>
    public required string StorageType { get; init; }

    /// <summary>
    /// Parameter group name.
    /// </summary>
    public string? Group { get; init; }

    /// <summary>
    /// Whether this is a shared parameter.
    /// </summary>
    public bool IsShared { get; init; }

    /// <summary>
    /// Unit type for display (e.g., "Length", "Area").
    /// </summary>
    public string? UnitType { get; init; }
}
