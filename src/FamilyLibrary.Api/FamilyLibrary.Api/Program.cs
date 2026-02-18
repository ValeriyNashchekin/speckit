using System.Reflection;
using FamilyLibrary.Api.Middleware;
using FamilyLibrary.Application;
using FamilyLibrary.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

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
app.UseAuthorization();
app.MapControllers();

app.Run();
