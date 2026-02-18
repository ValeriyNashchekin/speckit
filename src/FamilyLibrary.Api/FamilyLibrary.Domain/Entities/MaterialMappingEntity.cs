namespace FamilyLibrary.Domain.Entities;

/// <summary>
/// Maps template/library material names to project-specific material names.
/// Used for material substitution when loading families into projects.
/// </summary>
public class MaterialMappingEntity : BaseEntity
{
    /// <summary>
    /// The unique identifier for the project this mapping belongs to.
    /// Comes from Revit's project identifier.
    /// </summary>
    public Guid ProjectId { get; private set; }

    /// <summary>
    /// The material name as it appears in the template or family library.
    /// Maximum length: 200 characters.
    /// </summary>
    public string TemplateMaterialName { get; private set; } = null!;

    /// <summary>
    /// The material name as it should be used in the target project.
    /// Maximum length: 200 characters.
    /// </summary>
    public string ProjectMaterialName { get; private set; } = null!;

    /// <summary>
    /// The timestamp when this mapping was last applied.
    /// Null if the mapping has never been used.
    /// </summary>
    public DateTime? LastUsedAt { get; private set; }

    // Private constructor for EF Core
    private MaterialMappingEntity() { }

    /// <summary>
    /// Creates a new material mapping for a project.
    /// </summary>
    /// <param name="projectId">The project identifier from Revit.</param>
    /// <param name="templateMaterialName">The material name in the template/library.</param>
    /// <param name="projectMaterialName">The material name in the target project.</param>
    /// <exception cref="ArgumentNullException">Thrown when any string parameter is null or empty.</exception>
    public MaterialMappingEntity(
        Guid projectId,
        string templateMaterialName,
        string projectMaterialName)
    {
        if (projectId == Guid.Empty)
            throw new ArgumentException("Project ID cannot be empty.", nameof(projectId));

        ProjectId = projectId;
        TemplateMaterialName = ValidateMaterialName(templateMaterialName, nameof(templateMaterialName));
        ProjectMaterialName = ValidateMaterialName(projectMaterialName, nameof(projectMaterialName));
    }

    /// <summary>
    /// Updates the project material name for this mapping.
    /// </summary>
    /// <param name="projectMaterialName">The new project material name.</param>
    /// <exception cref="ArgumentNullException">Thrown when projectMaterialName is null or empty.</exception>
    public void Update(string projectMaterialName)
    {
        ProjectMaterialName = ValidateMaterialName(projectMaterialName, nameof(projectMaterialName));
        SetUpdated();
    }

    /// <summary>
    /// Marks this mapping as used, updating the LastUsedAt timestamp.
    /// </summary>
    public void MarkAsUsed()
    {
        LastUsedAt = DateTime.UtcNow;
    }

    private static string ValidateMaterialName(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(paramName, "Material name cannot be null or empty.");

        if (value.Length > 200)
            throw new ArgumentException("Material name cannot exceed 200 characters.", paramName);

        return value;
    }
}
