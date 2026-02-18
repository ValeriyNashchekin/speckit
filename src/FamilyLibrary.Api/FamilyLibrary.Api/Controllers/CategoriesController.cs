using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FamilyLibrary.Api.Controllers;

/// <summary>
/// Controller for managing categories.
/// </summary>
[ApiController]
[Route("api/categories")]
public class CategoriesController(ICategoryService service) : BaseController
{
    /// <summary>
    /// Gets all categories.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of all categories.</returns>
    [HttpGet]
    [ProducesResponseType<List<CategoryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CategoryDto>>> GetAll(CancellationToken ct)
    {
        var result = await service.GetAllAsync(ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a category by ID.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The category.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<CategoryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="dto">The create DTO.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created category ID.</returns>
    [HttpPost]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateCategoryDto dto, CancellationToken ct)
    {
        var id = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Updates an existing category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="dto">The update DTO.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto, CancellationToken ct)
    {
        await service.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    /// <summary>
    /// Deletes a category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await service.DeleteAsync(id, ct);
        return NoContent();
    }
}
