using System.Collections.Concurrent;

namespace FamilyLibrary.Api.Middleware;

/// <summary>
/// Simple rate limiting middleware using sliding window algorithm.
/// Limits requests per IP address within a configurable time window.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private static readonly ConcurrentDictionary<string, RateLimitEntry> _clients = new();
    private readonly int _maxRequests = 100;
    private readonly TimeSpan _window = TimeSpan.FromMinutes(1);

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);
        var now = DateTime.UtcNow;

        var entry = _clients.AddOrUpdate(
            clientId,
            _ => new RateLimitEntry { Count = 1, WindowStart = now },
            (_, existing) =>
            {
                if (now - existing.WindowStart > _window)
                {
                    return new RateLimitEntry { Count = 1, WindowStart = now };
                }
                existing.Count++;
                return existing;
            });

        var remainingRequests = _maxRequests - entry.Count;
        context.Response.Headers["X-RateLimit-Limit"] = _maxRequests.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = Math.Max(0, remainingRequests).ToString();

        if (entry.Count > _maxRequests)
        {
            _logger.LogWarning(
                "Rate limit exceeded for client {ClientId}. Requests: {Count}, Limit: {MaxRequests}",
                clientId, entry.Count, _maxRequests);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = _window.TotalSeconds.ToString();
            await context.Response.WriteAsJsonAsync(new
            {
                Status = 429,
                Message = "Too many requests. Please try again later."
            });
            return;
        }

        await _next(context);
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Use IP address as client identifier
        // In production, consider using X-Forwarded-For header if behind a proxy
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Optionally include user ID if authenticated for more precise limiting
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(userId) ? ipAddress : $"{ipAddress}:{userId}";
    }

    private class RateLimitEntry
    {
        public int Count { get; set; }
        public DateTime WindowStart { get; set; }
    }
}
