using FamilyLibrary.Application.Common;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Application.Interfaces;
using FamilyLibrary.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FamilyLibrary.Api.Controllers;

/// <summary>
/// Controller for managing family roles.
/// </summary>
[ApiController]
[Route("api/roles")]
public class FamilyRolesController(IFamilyRoleService service) : BaseController
{
    /// <summary>
    /// Gets all family roles with pagination and optional filtering.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Page size (default: 10).</param>
    /// <param name="type">Filter by role type.</param>
    /// <param name="categoryId">Filter by category ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paged list of family roles.</returns>
    [HttpGet]
    [ProducesResponseType<PagedResult<FamilyRoleDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<FamilyRoleDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] RoleType? type = null,
        [FromQuery] Guid? categoryId = null,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(page, pageSize, type, categoryId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a family role by ID.
    /// </summary>
    /// <param name="id">The role ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The family role.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<FamilyRoleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FamilyRoleDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new family role.
    /// </summary>
    /// <param name="dto">The create DTO.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created role ID.</returns>
    [HttpPost]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateFamilyRoleDto dto, CancellationToken ct)
    {
        var id = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Updates an existing family role.
    /// </summary>
    /// <param name="id">The role ID.</param>
    /// <param name="dto">The update DTO.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFamilyRoleDto dto, CancellationToken ct)
    {
        await service.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    /// <summary>
    /// Deletes a family role.
    /// </summary>
    /// <param name="id">The role ID.</param>
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
    /// Imports multiple family roles from a batch.
    /// </summary>
    /// <param name="dtos">The list of roles to import.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Result of the batch import operation.</returns>
    [HttpPost("import")]
    [ProducesResponseType<BatchCreateResult>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BatchCreateResult>> Import(
        [FromBody] IReadOnlyList<CreateFamilyRoleDto> dtos,
        CancellationToken ct)
    {
        var result = await service.ImportAsync(dtos, ct);
        return Ok(result);
    }
}
