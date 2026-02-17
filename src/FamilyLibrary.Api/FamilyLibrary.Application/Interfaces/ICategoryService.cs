using FamilyLibrary.Application.DTOs;

namespace FamilyLibrary.Application.Interfaces;

/// <summary>
/// Service interface for Category operations.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Gets all categories.
    /// </summary>
    Task<List<CategoryDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category by ID.
    /// </summary>
    Task<CategoryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new category.
    /// </summary>
    Task<Guid> CreateAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    Task UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a category.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
