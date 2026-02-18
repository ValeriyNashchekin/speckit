using FamilyLibrary.Application.Common;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Application.Interfaces;
using FamilyLibrary.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FamilyLibrary.Api.Controllers;

/// <summary>
/// Controller for managing system types.
/// </summary>
[ApiController]
[Route("api/system-types")]
public class SystemTypesController(ISystemTypeService service) : BaseController
{
    /// <summary>
    /// Gets all system types with pagination and optional filtering.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Page size (default: 10).</param>
    /// <param name="roleId">Filter by role ID.</param>
    /// <param name="group">Filter by system family group.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paged list of system types.</returns>
    [HttpGet]
    [ProducesResponseType<PagedResult<SystemTypeDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<SystemTypeDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? roleId = null,
        [FromQuery] SystemFamilyGroup? group = null,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(page, pageSize, roleId, group, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a system type by ID.
    /// </summary>
    /// <param name="id">The system type ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The system type.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<SystemTypeDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SystemTypeDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new system type.
    /// </summary>
    /// <param name="dto">The create DTO.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created system type ID.</returns>
    [HttpPost]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateSystemTypeDto dto, CancellationToken ct)
    {
        var id = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Updates an existing system type.
    /// </summary>
    /// <param name="id">The system type ID.</param>
    /// <param name="dto">The update DTO.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSystemTypeDto dto, CancellationToken ct)
    {
        await service.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    /// <summary>
    /// Deletes a system type.
    /// </summary>
    /// <param name="id">The system type ID.</param>
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

    /// <summary>
    /// Gets all system types for a specific role.
    /// </summary>
    /// <param name="roleId">The role ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of system types for the role.</returns>
    [HttpGet("by-role/{roleId:guid}")]
    [ProducesResponseType<IReadOnlyList<SystemTypeDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SystemTypeDto>>> GetByRoleId(Guid roleId, CancellationToken ct)
    {
        var result = await service.GetByRoleIdAsync(roleId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets all system types for a specific category.
    /// Useful for Group A categories (Walls, Floors, Roofs, Ceilings, StructuralFoundation) that use CompoundStructure.
    /// </summary>
    /// <param name="category">The category name (e.g., "Walls", "Floors", "Roofs", "Ceilings", "StructuralFoundation").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of system types for the category.</returns>
    [HttpGet("by-category/{category}")]
    [ProducesResponseType<IReadOnlyList<SystemTypeDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<SystemTypeDto>>> GetByCategory(string category, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            return BadRequest("Category cannot be null or empty.");
        }

        var result = await service.GetByCategoryAsync(category, ct);
        return Ok(result);
    }
}
