using FamilyLibrary.Application.DTOs;

namespace FamilyLibrary.Application.Interfaces;

/// <summary>
/// Service interface for material mapping operations.
/// Maps template/library material names to project-specific material names.
/// </summary>
public interface IMaterialMappingService
{
    /// <summary>
    /// Gets all material mappings for a specific project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of material mappings for the project.</returns>
    Task<List<MaterialMappingDto>> GetAllAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a material mapping by its unique identifier.
    /// </summary>
    /// <param name="id">The mapping identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The material mapping.</returns>
    /// <exception cref="FamilyLibrary.Domain.Exceptions.NotFoundException">Thrown when mapping is not found.</exception>
    Task<MaterialMappingDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new material mapping.
    /// </summary>
    /// <param name="dto">The create request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The identifier of the created mapping.</returns>
    /// <exception cref="FamilyLibrary.Domain.Exceptions.ValidationException">Thrown when validation fails.</exception>
    Task<Guid> CreateAsync(CreateMaterialMappingRequest dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing material mapping's project material name.
    /// </summary>
    /// <param name="id">The mapping identifier.</param>
    /// <param name="dto">The update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="FamilyLibrary.Domain.Exceptions.NotFoundException">Thrown when mapping is not found.</exception>
    Task UpdateAsync(Guid id, UpdateMaterialMappingRequest dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a material mapping.
    /// </summary>
    /// <param name="id">The mapping identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="FamilyLibrary.Domain.Exceptions.NotFoundException">Thrown when mapping is not found.</exception>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Looks up a material mapping by template material name and updates its LastUsedAt timestamp.
    /// Used during Pull Update to find the project material name for a template material.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="templateMaterialName">The template material name to look up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The material mapping if found, null otherwise.</returns>
    Task<MaterialMappingDto?> LookupAsync(Guid projectId, string templateMaterialName, CancellationToken cancellationToken = default);
}
