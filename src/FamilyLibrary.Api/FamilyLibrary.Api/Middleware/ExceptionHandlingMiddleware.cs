using System.Net;
using System.Text.Json;
using FamilyLibrary.Domain.Exceptions;

namespace FamilyLibrary.Api.Middleware;

/// <summary>
/// Global exception handling middleware.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred.");

        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            NotFoundException notFoundEx => new ErrorResponse
            {
                Status = (int)HttpStatusCode.NotFound,
                Title = "Not Found",
                Detail = notFoundEx.Message,
                EntityName = notFoundEx.EntityName,
                Key = notFoundEx.Key?.ToString()
            },
            ValidationException validationEx => new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Validation Failed",
                Detail = validationEx.Message,
                Property = validationEx.PropertyName
            },
            BusinessRuleException businessEx => new ErrorResponse
            {
                Status = (int)HttpStatusCode.UnprocessableEntity,
                Title = "Business Rule Violation",
                Detail = businessEx.Message
            },
            ArgumentException argEx => new ErrorResponse
            {
                Status = (int)HttpStatusCode.BadRequest,
                Title = "Invalid Argument",
                Detail = argEx.Message,
                Property = argEx.ParamName
            },
            _ => new ErrorResponse
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred."
            }
        };

        response.StatusCode = errorResponse.Status;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await response.WriteAsync(JsonSerializer.Serialize(errorResponse, options));
    }

    private class ErrorResponse
    {
        public int Status { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string? Property { get; set; }
        public string? EntityName { get; set; }
        public string? Key { get; set; }
    }
}
