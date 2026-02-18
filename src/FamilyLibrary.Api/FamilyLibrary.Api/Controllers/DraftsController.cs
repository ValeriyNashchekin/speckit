using FamilyLibrary.Application.Common;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Application.Interfaces;
using FamilyLibrary.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FamilyLibrary.Api.Controllers;

/// <summary>
/// Controller for managing draft families.
/// </summary>
[ApiController]
[Route("api/drafts")]
public class DraftsController(IDraftService service) : BaseController
{
    /// <summary>
    /// Gets all drafts with pagination and optional filtering by template.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Page size (default: 10).</param>
    /// <param name="templateId">Filter by template ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paged list of drafts.</returns>
    [HttpGet]
    [ProducesResponseType<PagedResult<DraftDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<DraftDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? templateId = null,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(page, pageSize, templateId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a draft by ID.
    /// </summary>
    /// <param name="id">The draft ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The draft.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<DraftDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DraftDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    /// <summary>
    /// Creates a new draft.
    /// </summary>
    /// <param name="dto">The create DTO.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created draft ID.</returns>
    [HttpPost]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateDraftDto dto, CancellationToken ct)
    {
        var id = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Updates an existing draft.
    /// </summary>
    /// <param name="id">The draft ID.</param>
    /// <param name="dto">The update DTO.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDraftDto dto, CancellationToken ct)
    {
        await service.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    /// <summary>
    /// Deletes a draft.
    /// </summary>
    /// <param name="id">The draft ID.</param>
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
    /// Updates the status of a draft.
    /// </summary>
    /// <param name="id">The draft ID.</param>
    /// <param name="status">The new status.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] DraftStatus status,
        CancellationToken ct)
    {
        await service.UpdateStatusAsync(id, status, ct);
        return NoContent();
    }

    /// <summary>
    /// Creates or updates multiple drafts based on FamilyUniqueId.
    /// Existing drafts have their LastSeen updated, new ones are created.
    /// </summary>
    /// <param name="drafts">The list of drafts to create or update.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BatchCreateOrUpdate(
        [FromBody] IReadOnlyList<CreateDraftDto> drafts,
        CancellationToken ct)
    {
        await service.BatchCreateOrUpdateAsync(drafts, ct);
        return NoContent();
    }
}
