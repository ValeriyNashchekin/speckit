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
    /// Gets changes between two versions of a family.
    /// </summary>
    /// <param name="id">The family ID.</param>
    /// <param name="fromVersion">The source version number.</param>
    /// <param name="toVersion">The target version number.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Change set describing the differences between versions.</returns>
    [HttpGet("{id:guid}/changes")]
    [ProducesResponseType<ChangeSetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChangeSetDto>> GetChanges(
        Guid id,
        [FromQuery] int fromVersion,
        [FromQuery] int toVersion,
        CancellationToken ct)
    {
        var result = await service.GetChangesAsync(id, fromVersion, toVersion, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a preview of changes that will occur when updating a family.
    /// US4: Pre-Update Preview - designers see what will change before confirming update.
    /// </summary>
    /// <param name="id">The family ID.</param>
    /// <param name="currentVersion">The current local version of the family.</param>
    /// <param name="targetVersion">The target library version to update to.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Change set describing what will change after the update.</returns>
    [HttpGet("{id:guid}/update-preview")]
    [ProducesResponseType<ChangeSetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChangeSetDto>> GetUpdatePreview(
        Guid id,
        [FromQuery] int currentVersion,
        [FromQuery] int targetVersion,
        CancellationToken ct)
    {
        var result = await service.GetUpdatePreviewAsync(id, currentVersion, targetVersion, ct);
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
    /// Batch checks multiple families against the library.
    /// Determines if families are up-to-date, need update, or are unmatched.
    /// </summary>
    /// <param name="request">The request containing families with role names and hashes.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Batch check response with status for each family.</returns>
    [HttpPost("batch-check")]
    [ProducesResponseType<BatchCheckResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<BatchCheckResponse>> BatchCheck(
        [FromBody] BatchCheckRequest request,
        CancellationToken ct)
    {
        var results = await service.BatchCheckAsync(request, ct);
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

    /// <summary>
    /// Detects local changes by comparing a local snapshot with the latest library version.
    /// </summary>
    /// <param name="id">The family ID.</param>
    /// <param name="request">The request containing the local snapshot JSON.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Change set describing the differences between local and library versions.</returns>
    [HttpPost("{id:guid}/local-changes")]
    [ProducesResponseType<ChangeSetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChangeSetDto>> DetectLocalChanges(
        Guid id,
        [FromBody] DetectLocalChangesRequest request,
        CancellationToken ct)
    {
        var result = await service.DetectLocalChangesAsync(id, request.LocalSnapshotJson, ct);
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
/// Request model for detecting local changes.
/// </summary>
public record DetectLocalChangesRequest
{
    /// <summary>
    /// JSON-serialized local snapshot of the family.
    /// </summary>
    public required string LocalSnapshotJson { get; init; }
}
