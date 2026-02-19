using System.Reflection;
using System.Threading.RateLimiting;
using FamilyLibrary.Api.Middleware;
using FamilyLibrary.Application;
using FamilyLibrary.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Rate limiting configuration for scan endpoints
builder.Services.AddRateLimiter(options =>
{
    // ScanPolicy: 100 requests per minute per user
    options.AddFixedWindowLimiter("ScanPolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit = 100;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0; // No queuing - reject immediately when limit exceeded
    });

    // Custom response when rate limit is exceeded
    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();
        }

        await context.HttpContext.Response.WriteAsJsonAsync(new
        {
            Status = 429,
            Message = "Too many requests. Please try again later."
        }, cancellationToken);
    };
});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Configure Swagger document metadata
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Family Library API",
        Version = "v1",
        Description = "API for managing Revit family library - roles, categories, tags, families, and recognition rules"
    });

    // Include XML comments for detailed API documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// CORS for Angular frontend - configured from appsettings.json
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:4200", "https://localhost:5001"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Layered DI registration (Clean Architecture)
// Infrastructure: DbContext, Repositories, External Services
builder.Services.AddInfrastructure(builder.Configuration);

// Application: Services, Validators, Mappers
builder.Services.AddApplication();

var app = builder.Build();

// Global exception handling middleware - must be first in the pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Rate limiting middleware - limits requests per IP address
app.UseMiddleware<RateLimitingMiddleware>();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("DefaultPolicy");

// ASP.NET Core rate limiting middleware - must be after UseRouting
app.UseRateLimiter();

app.UseAuthorization();
app.MapControllers();

app.Run();
