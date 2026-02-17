using Microsoft.AspNetCore.Mvc;

namespace FamilyLibrary.Api.Controllers;

/// <summary>
/// Base controller with common functionality.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult Success<T>(T data)
    {
        return Ok(data);
    }

    protected IActionResult Success()
    {
        return Ok();
    }

    protected IActionResult CreatedResult<T>(string actionName, object routeValues, T data)
    {
        return CreatedAtAction(actionName, routeValues, data);
    }

    protected IActionResult NotFoundResult(string entityName, object key)
    {
        return NotFound(new { Entity = entityName, Key = key, Message = $"{entityName} with key '{key}' was not found." });
    }

    protected IActionResult ValidationError(string message, string? property = null)
    {
        return BadRequest(new { Property = property, Message = message });
    }
}
