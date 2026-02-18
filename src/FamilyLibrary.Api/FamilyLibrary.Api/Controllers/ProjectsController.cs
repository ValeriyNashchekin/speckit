using FamilyLibrary.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace FamilyLibrary.Api.Controllers;

/// <summary>
/// Controller for project-related operations.
/// </summary>
[ApiController]
[Route("api/projects")]
public class ProjectsController : BaseController
{
    /// <summary>
    /// Scans project families and returns comparison with library.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Scan result with families and their status.</returns>
    /// <remarks>
    /// This is a stub endpoint. Actual scanning is performed by the Revit Plugin
    /// which calls the batch-check endpoint directly.
    /// </remarks>
    [HttpPost("{id}/scan")]
    [ProducesResponseType<ScanResultDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ScanResultDto>> ScanProject(
        Guid id,
        CancellationToken ct)
    {
        // Stub - actual scanning done by Plugin
        await Task.CompletedTask;
        return Ok(new ScanResultDto
        {
            Families = [],
            TotalCount = 0,
            Summary = new ScanSummaryDto()
        });
    }

    /// <summary>
    /// Batch updates families in a project from the library.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success or failure status.</returns>
    /// <remarks>
    /// This is a stub endpoint. Actual updating is performed by the Revit Plugin
    /// which downloads families directly using the download endpoint.
    /// </remarks>
    [HttpPost("{id}/batch-update")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> BatchUpdate(
        Guid id,
        CancellationToken ct)
    {
        // Stub - actual updating done by Plugin
        await Task.CompletedTask;
        return Ok();
    }
}
