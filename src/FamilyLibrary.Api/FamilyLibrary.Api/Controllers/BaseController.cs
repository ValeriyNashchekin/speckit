using Microsoft.AspNetCore.Mvc;

namespace FamilyLibrary.Api.Controllers;

/// <summary>
/// Base controller with common functionality.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    // Standard ControllerBase methods are used directly:
    // - Ok(result) for successful responses with data
    // - NotFound() for 404 responses
    // - BadRequest(ModelState) for validation errors
    // - CreatedAtAction(...) for 201 Created responses
    // - NoContent() for 204 responses
    // - Conflict() for 409 responses
    //
    // All unhandled exceptions are caught by ExceptionHandlingMiddleware
    // and returned as ProblemDetails (RFC 7807).
}
