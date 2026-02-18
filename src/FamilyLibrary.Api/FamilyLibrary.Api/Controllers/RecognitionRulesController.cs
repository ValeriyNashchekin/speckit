using FamilyLibrary.Application.Common;
using FamilyLibrary.Application.DTOs;
using FamilyLibrary.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FamilyLibrary.Api.Controllers;

/// <summary>
/// Controller for managing recognition rules.
/// </summary>
[ApiController]
[Route("api/recognition-rules")]
public class RecognitionRulesController(IRecognitionRuleService service) : BaseController
{
    /// <summary>
    /// Gets all recognition rules with pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Page size (default: 10).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paged list of recognition rules.</returns>
    [HttpGet]
    [ProducesResponseType<PagedResult<RecognitionRuleDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<RecognitionRuleDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await service.GetAllAsync(page, pageSize, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets a recognition rule by ID.
    /// </summary>
    /// <param name="id">The rule ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The recognition rule.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<RecognitionRuleDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecognitionRuleDto>> GetById(Guid id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new recognition rule.
    /// </summary>
    /// <param name="dto">The create DTO.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created rule ID.</returns>
    [HttpPost]
    [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateRecognitionRuleDto dto, CancellationToken ct)
    {
        var id = await service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Updates an existing recognition rule.
    /// </summary>
    /// <param name="id">The rule ID.</param>
    /// <param name="dto">The update DTO.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRecognitionRuleDto dto, CancellationToken ct)
    {
        await service.UpdateAsync(id, dto, ct);
        return NoContent();
    }

    /// <summary>
    /// Deletes a recognition rule.
    /// </summary>
    /// <param name="id">The rule ID.</param>
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
    /// Validates the formula syntax.
    /// </summary>
    /// <param name="request">The validation request containing the formula.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the formula is valid, false otherwise.</returns>
    [HttpPost("validate")]
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<bool>> ValidateFormula(
        [FromBody] ValidateFormulaRequest request,
        CancellationToken ct)
    {
        var result = await service.ValidateFormulaAsync(request.Formula, ct);
        return Ok(result);
    }

    /// <summary>
    /// Tests a rule against a family name.
    /// </summary>
    /// <param name="request">The test request containing the rule ID and family name.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the rule matches the family name, false otherwise.</returns>
    [HttpPost("test")]
    [ProducesResponseType<bool>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<bool>> TestRule(
        [FromBody] TestRuleRequest request,
        CancellationToken ct)
    {
        var result = await service.TestRuleAsync(request.Id, request.FamilyName, ct);
        return Ok(result);
    }

    /// <summary>
    /// Checks for conflicts between recognition rules.
    /// </summary>
    /// <param name="request">The request containing an optional rule ID to exclude from the check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of conflicts found.</returns>
    [HttpPost("check-conflicts")]
    [ProducesResponseType<List<ConflictDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ConflictDto>>> CheckConflicts(
        [FromBody] CheckConflictsRequest? request,
        CancellationToken ct)
    {
        var excludeId = request?.ExcludeId;
        var result = await service.CheckConflictsAsync(excludeId, ct);
        return Ok(result);
    }
}

/// <summary>
/// Request model for formula validation.
/// </summary>
public record ValidateFormulaRequest
{
    /// <summary>
    /// The formula to validate.
    /// </summary>
    public required string Formula { get; init; }
}

/// <summary>
/// Request model for rule testing.
/// </summary>
public record TestRuleRequest
{
    /// <summary>
    /// The ID of the rule to test.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// The family name to test against.
    /// </summary>
    public required string FamilyName { get; init; }
}

/// <summary>
/// Request model for conflict checking.
/// </summary>
public record CheckConflictsRequest
{
    /// <summary>
    /// Optional rule ID to exclude from conflict checking.
    /// </summary>
    public Guid? ExcludeId { get; init; }
}
