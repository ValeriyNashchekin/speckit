using System.Net;
using System.Text.Json;
using FamilyLibrary.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FamilyLibrary.Api.Middleware;

/// <summary>
/// Global exception handling middleware using ProblemDetails (RFC 7807).
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

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

        var (statusCode, problemDetails) = exception switch
        {
            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                CreateProblemDetails(
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    notFoundEx.Message,
                    new Dictionary<string, object?>
                    {
                        ["entityName"] = notFoundEx.EntityName,
                        ["key"] = notFoundEx.Key?.ToString()
                    })),

            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                CreateProblemDetails(
                    StatusCodes.Status400BadRequest,
                    "Validation Failed",
                    validationEx.Message,
                    new Dictionary<string, object?>
                    {
                        ["property"] = validationEx.PropertyName
                    })),

            BusinessRuleException businessEx => (
                StatusCodes.Status422UnprocessableEntity,
                CreateProblemDetails(
                    StatusCodes.Status422UnprocessableEntity,
                    "Business Rule Violation",
                    businessEx.Message)),

            ArgumentException argEx => (
                StatusCodes.Status400BadRequest,
                CreateProblemDetails(
                    StatusCodes.Status400BadRequest,
                    "Invalid Argument",
                    argEx.Message,
                    new Dictionary<string, object?>
                    {
                        ["property"] = argEx.ParamName
                    })),

            _ => (
                StatusCodes.Status500InternalServerError,
                CreateProblemDetails(
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "An unexpected error occurred. Please try again later."))
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, JsonOptions));
    }

    private static ProblemDetails CreateProblemDetails(
        int status,
        string title,
        string detail,
        IDictionary<string, object?>? extensions = null)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail,
            Type = GetProblemType(status)
        };

        if (extensions != null)
        {
            foreach (var (key, value) in extensions)
            {
                problem.Extensions[key] = value;
            }
        }

        return problem;
    }

    private static string GetProblemType(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        StatusCodes.Status422UnprocessableEntity => "https://tools.ietf.org/html/rfc4918#section-11.2",
        StatusCodes.Status500InternalServerError => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        _ => "about:blank"
    };
}
