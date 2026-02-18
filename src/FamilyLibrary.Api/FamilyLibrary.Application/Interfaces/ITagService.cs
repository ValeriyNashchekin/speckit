using FamilyLibrary.Application.DTOs;

namespace FamilyLibrary.Application.Interfaces;

/// <summary>
/// Service interface for Tag operations.
/// </summary>
public interface ITagService
{
    /// <summary>
    /// Gets all tags.
    /// </summary>
    Task<List<TagDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tag by ID.
    /// </summary>
    Task<TagDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new tag.
    /// </summary>
    Task<Guid> CreateAsync(CreateTagDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing tag.
    /// </summary>
    Task UpdateAsync(Guid id, UpdateTagDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a tag.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
