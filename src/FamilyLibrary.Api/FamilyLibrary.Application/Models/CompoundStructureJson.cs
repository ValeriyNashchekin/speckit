namespace FamilyLibrary.Application.Models;

/// <summary>
/// JSON model for CompoundStructure serialization (Group A: Walls, Floors, Roofs, Ceilings, Foundations).
/// </summary>
public record CompoundStructureJson
{
    /// <summary>
    /// List of layers in the compound structure.
    /// </summary>
    public required List<CompoundLayerJson> Layers { get; init; }

    /// <summary>
    /// Total thickness of the structure.
    /// </summary>
    public double TotalThickness { get; init; }

    /// <summary>
    /// Variable layer index (for variable thickness walls), -1 if none.
    /// </summary>
    public int VariableLayerIndex { get; init; } = -1;
}

/// <summary>
/// Single layer in a compound structure.
/// </summary>
public record CompoundLayerJson
{
    /// <summary>
    /// Layer name/function (e.g., "Finish", "Structure", "Core").
    /// </summary>
    public required string Function { get; init; }

    /// <summary>
    /// Layer thickness in project units (feet internally, display in UI).
    /// </summary>
    public double Thickness { get; init; }

    /// <summary>
    /// Material name (warning: materials may not exist in target project).
    /// </summary>
    public string? MaterialName { get; init; }

    /// <summary>
    /// Material Id in source project (for reference).
    /// </summary>
    public int? MaterialId { get; init; }

    /// <summary>
    /// Whether this layer is structural.
    /// </summary>
    public bool IsStructural { get; init; }

    /// <summary>
    /// Layer priority (for join behavior).
    /// </summary>
    public int Priority { get; init; }
}
