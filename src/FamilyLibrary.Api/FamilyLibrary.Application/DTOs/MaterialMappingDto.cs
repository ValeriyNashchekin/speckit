namespace FamilyLibrary.Application.DTOs;

/// <summary>
/// DTO for MaterialMapping entity representing a mapping between template and project materials.
/// </summary>
public record MaterialMappingDto
{
    /// <summary>
    /// Unique identifier of the material mapping.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Identifier of the project this mapping belongs to.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// Name of the material in the template.
    /// </summary>
    public required string TemplateMaterialName { get; init; }

    /// <summary>
    /// Name of the material in the project.
    /// </summary>
    public required string ProjectMaterialName { get; init; }

    /// <summary>
    /// Timestamp when the mapping was created.
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Timestamp when the mapping was last used, if ever.
    /// </summary>
    public DateTime? LastUsedAt { get; init; }
}

/// <summary>
/// DTO for creating a new material mapping.
/// </summary>
public record CreateMaterialMappingRequest
{
    /// <summary>
    /// Identifier of the project this mapping belongs to.
    /// </summary>
    public Guid ProjectId { get; init; }

    /// <summary>
    /// Name of the material in the template.
    /// </summary>
    public required string TemplateMaterialName { get; init; }

    /// <summary>
    /// Name of the material in the project.
    /// </summary>
    public required string ProjectMaterialName { get; init; }
}

/// <summary>
/// DTO for updating an existing material mapping.
/// </summary>
public record UpdateMaterialMappingRequest
{
    /// <summary>
    /// New name of the material in the project.
    /// </summary>
    public required string ProjectMaterialName { get; init; }
}
