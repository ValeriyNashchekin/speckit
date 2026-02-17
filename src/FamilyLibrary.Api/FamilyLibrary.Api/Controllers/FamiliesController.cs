using FamilyLibrary.Application.Common;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FamilyLibrary.Api.Controllers;

/// <summary>
/// Controller for managing families.
/// </summary>
[ApiController]
[Route("api/families")]
public class FamiliesController(IFamilyService service) : BaseController
{
    /// <summary>
    /// Gets all families with pagination and optional filtering by role.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Page size (default: 10).</param>
    /// <param name="roleId">Filter by role ID.</param>
    /// <param name="searchTerm">Search term for family name (case-insensitive).</param>
    /// <param name="categoryId">Filter by category ID.</param>
    /// <param name="tagIds">Filter by tag IDs (comma-separated GUIDs).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paged list of families.</returns>
    [HttpGet]
    [ProducesResponseType<PagedResult<FamilyDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<FamilyDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? roleId = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] string? tagIds = null,
        CancellationToken ct = default)
    {
        // Parse comma-separated tag IDs
        List<Guid>? tagIdList = null;
        if (!string.IsNullOrWhiteSpace(tagIds))
        {
            tagIdList = tagIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => Guid.TryParse(s, out var guid) ? guid : (Guid?)null)
                .Where(g => g.HasValue)
                .Select(g => g!.Value)
                .ToList();

            if (tagIdList.Count == 0)
            {
                tagIdList = null;
            }
        }

        var result = await service.GetAllAsync(page, pageSize, roleId, searchTerm, categoryId, tagIdList, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a family by ID with full details including versions.
    /// </summary>
    /// <param name="id">The family ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The family with versions.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<FamilyDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    /// <summary>
    /// Gets version history for a family.
    /// </summary>
    /// <param name="id">The family ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of family versions.</returns>
    [HttpGet("{id:guid}/versions")]
    [ProducesResponseType<List<FamilyVersionDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<FamilyVersionDto>>> GetVersions(Guid id, CancellationToken ct)
    {
        var result = await service.GetVersionsAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Publishes a new family file.
    /// </summary>
    /// <param name="dto">The create DTO with family metadata.</param>
    /// <param name="file">The family file (.rfa).</param>
    /// <param name="txtFile">Optional type catalog file (.txt).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created family.</returns>
    [HttpPost("publish")]
    [RequestSizeLimit(52_428_800)] // 50MB
    [RequestFormLimits(MultipartBodyLengthLimit = 52_428_800)]
    [ProducesResponseType<FamilyDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Publish(
        [FromForm] CreateFamilyDto dto,
        IFormFile file,
        IFormFile? txtFile,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError(nameof(file), "File is required.");
            return BadRequest(ModelState);
        }

        await using var stream = file.OpenReadStream();

        Stream? txtStream = null;
        string? txtFileName = null;

        if (txtFile is not null && txtFile.Length > 0)
        {
            txtStream = txtFile.OpenReadStream();
            txtFileName = txtFile.FileName;
        }

        try
        {
            var result = await service.PublishAsync(dto, stream, file.FileName, txtStream, txtFileName, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        finally
        {
            if (txtStream is not null)
            {
                await txtStream.DisposeAsync();
            }
        }
    }

    /// <summary>
    /// Validates if a content hash already exists in the library.
    /// </summary>
    /// <param name="request">The request containing the hash to validate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if hash exists, false otherwise.</returns>
    [HttpPost("validate-hash")]
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> ValidateHash(
        [FromBody] ValidateHashRequest request,
        CancellationToken ct)
    {
        var exists = await service.ValidateHashAsync(request.Hash, ct);
        return Ok(exists);
    }

    /// <summary>
    /// Batch checks multiple hashes for existence.
    /// </summary>
    /// <param name="request">The request containing the list of hashes to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of family status results.</returns>
    [HttpPost("batch-check")]
    [ProducesResponseType<List<FamilyStatusDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<FamilyStatusDto>>> BatchCheck(
        [FromBody] BatchCheckRequest request,
        CancellationToken ct)
    {
        var results = await service.BatchCheckAsync(request.Hashes, ct);
        return Ok(results);
    }

    /// <summary>
    /// Gets download URL for a family version.
    /// </summary>
    /// <param name="id">The family ID.</param>
    /// <param name="version">Optional version number. If not specified, returns the latest version.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Download URL and metadata.</returns>
    [HttpGet("{id:guid}/download/{version:int?}")]
    [ProducesResponseType<FamilyDownloadDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FamilyDownloadDto>> Download(
        Guid id,
        int? version = null,
        CancellationToken ct = default)
    {
        var result = await service.GetDownloadUrlAsync(id, version, ct);
        return Ok(result);
    }
}

/// <summary>
/// Request model for hash validation.
/// </summary>
public record ValidateHashRequest
{
    /// <summary>
    /// The content hash to validate.
    /// </summary>
    public required string Hash { get; init; }
}

/// <summary>
/// Request model for batch hash check.
/// </summary>
public record BatchCheckRequest
{
    /// <summary>
    /// List of content hashes to check.
    /// </summary>
    public required List<string> Hashes { get; init; }
}
