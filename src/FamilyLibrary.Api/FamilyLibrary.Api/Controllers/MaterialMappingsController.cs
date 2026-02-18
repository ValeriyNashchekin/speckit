using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FamilyLibrary.Api.Controllers;

/// <summary>
/// Controller for managing material mappings.
/// Maps template/library material names to project-specific material names.
/// </summary>
[ApiController]
[Route("api/material-mappings")]
public class MaterialMappingsController(IMaterialMappingService service) : BaseController
{
    /// <summary>
    /// Gets all material mappings for a specific project.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of material mappings for the project.</returns>
    [HttpGet]
    [ProducesResponseType<List<MaterialMappingDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MaterialMappingDto>>> GetAll([FromQuery] Guid projectId, CancellationToken ct)
    {
        var result = await service.GetAllAsync(projectId, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a material mapping by ID.
    /// </summary>
    /// <param name="id">The mapping identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The material mapping.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<MaterialMappingDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MaterialMappingDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new material mapping.
    /// </summary>
    /// <param name="dto">The create request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created mapping ID.</returns>
    [HttpPost]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateMaterialMappingRequest dto, CancellationToken ct)
    {
        var id = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Updates an existing material mapping.
    /// </summary>
    /// <param name="id">The mapping identifier.</param>
    /// <param name="dto">The update request.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMaterialMappingRequest dto, CancellationToken ct)
    {
        await service.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    /// <summary>
    /// Deletes a material mapping.
    /// </summary>
    /// <param name="id">The mapping identifier.</param>
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
    /// Looks up a material mapping by template material name.
    /// Used during Pull Update to find the project material name for a template material.
    /// </summary>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="templateName">The template material name to look up.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The material mapping if found, otherwise 404.</returns>
    [HttpGet("lookup")]
    [ProducesResponseType<MaterialMappingDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MaterialMappingDto>> Lookup(
        [FromQuery] Guid projectId,
        [FromQuery] string templateName,
        CancellationToken ct)
    {
        var result = await service.LookupAsync(projectId, templateName, ct);
        
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
