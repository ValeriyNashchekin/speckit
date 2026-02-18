# Dependency Injection Patterns Analysis

**Analysis Date:** 2026-02-17
**Project:** Family Library API & Plugin
**Analyst:** DI Analyst Agent

---

## Executive Summary

The Family Library project demonstrates **overall good DI organization** following Clean Architecture principles.

**Key Strengths:**
- Well-organized DI extension methods (`AddInfrastructure`, `AddApplication`)
- Consistent use of `Scoped` lifetime for services and repositories
- Clean separation of concerns with proper abstraction layers
- Middleware properly resolved from DI container
- FluentValidation auto-registered via assembly scanning

**Issues Summary:**
- Critical: 0
- High: 1
- Medium: 2
- Low: 2

---

## 1. DI Organization & Extension Methods

### 1.1 Infrastructure Layer (Excellent)
**File:** `src/FamilyLibrary.Api/FamilyLibrary.Infrastructure/DependencyInjection.cs`

```csharp
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
{
    // DbContext
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

    // Repositories
    services.AddScoped<IFamilyRoleRepository, FamilyRoleRepository>();
    services.AddScoped<ICategoryRepository, CategoryRepository>();
    services.AddScoped<ITagRepository, TagRepository>();
    services.AddScoped<IFamilyRepository, FamilyRepository>();
    services.AddScoped<IFamilyVersionRepository, FamilyVersionRepository>();
    services.AddScoped<ISystemTypeRepository, SystemTypeRepository>();
    services.AddScoped<IDraftRepository, DraftRepository>();
    services.AddScoped<IRecognitionRuleRepository, RecognitionRuleRepository>();
    services.AddScoped<IFamilyNameMappingRepository, FamilyNameMappingRepository>();

    // Services
    services.AddScoped<IBlobStorageService, BlobStorageService>();

    return services;
}
```

**Assessment:** ✅ Excellent
- All infrastructure dependencies grouped in single composable method
- Properly passes `IConfiguration` for connection strings
- Clear separation between DbContext, Repositories, and Services

### 1.2 Application Layer (Excellent)
**File:** `src/FamilyLibrary.Api/FamilyLibrary.Application/DependencyInjection.cs`

```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    // Register all validators from the Application assembly
    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    services.AddFluentValidationAutoValidation();

    // Register Application services
    services.AddScoped<IFamilyRoleService, FamilyRoleService>();
    services.AddScoped<ICategoryService, CategoryService>();
    services.AddScoped<ITagService, TagService>();
    services.AddScoped<IRecognitionRuleService, RecognitionRuleService>();
    services.AddScoped<IFamilyService, FamilyService>();
    services.AddScoped<IDraftService, DraftService>();
    services.AddScoped<ISystemTypeService, SystemTypeService>();

    return services;
}
```

**Assessment:** ✅ Excellent
- Uses assembly scanning for validators (avoids manual registration)
- FluentValidation configured correctly
- All application services registered in one place

### 1.3 API Layer (Needs Improvement)
**File:** `src/FamilyLibrary.Api/FamilyLibrary.Api/Program.cs`

Current Program.cs contains middleware and CORS configuration inline:

```csharp
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

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(/* ... */);
```

**Recommendation:** Extract to `FamilyLibrary.Api/DependencyInjection.cs`:

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        // Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(/* ... */);

        // CORS
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:4200", "https://localhost:5001"];

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}
```

---

## 2. Critical Issue: Mapster Mappings Not Initialized

**Priority:** HIGH
**File:** `src/FamilyLibrary.Api/FamilyLibrary.Application/Mappings/MappingProfile.cs`

```csharp
public static class MappingProfile
{
    public static void ConfigureMappings()
    {
        // FamilyRole mappings
        TypeAdapterConfig<FamilyRoleEntity, FamilyRoleDto>.NewConfig();
        // ... many more mappings defined
    }
}
```

**Problem:** The `ConfigureMappings()` method is **NEVER called**. Mapster mappings will not work.

**Impact:** HIGH - Without initialization, custom mappings will not be applied.

**Fix Required:**
```csharp
// In AddApplication() method:
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    // Configure Mapster mappings - MUST BE FIRST
    MappingProfile.ConfigureMappings();

    // Register all validators...
    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    // ...
}
```

---

## 3. Lifetime Registration Analysis

| Component | Current Lifetime | Correct? | Notes |
|-----------|------------------|----------|-------|
| `AppDbContext` | Scoped (implicit via AddDbContext) | ✅ | DbContext should be Scoped |
| All Repositories | Scoped | ✅ | Repositories depend on Scoped DbContext |
| All Application Services | Scoped | ✅ | Services depend on Scoped repositories |
| `BlobStorageService` | Scoped | ⚠️ | Could be Singleton (see below) |
| Validators | Scoped (via FluentValidation) | ✅ | Default for FluentValidation |
| Controllers | Scoped (default) | ✅ | ASP.NET Core default |
| Middleware | Transient (default) | ✅ | Created per request |

### 3.1 Issue: BlobStorageService Should Be Singleton
**Priority:** LOW
**File:** `src/FamilyLibrary.Api/FamilyLibrary.Infrastructure/Services/BlobStorageService.cs`

```csharp
public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BlobStorage")
            ?? throw new InvalidOperationException("BlobStorage connection string not found.");
        _blobServiceClient = new BlobServiceClient(connectionString);
    }
}
```

**Analysis:**
- `BlobServiceClient` is thread-safe and intended to be reused
- Currently registered as `Scoped`, creating new client per request
- Connection strings don't change at runtime

**Recommendation:**
```csharp
// In DependencyInjection.cs
services.AddSingleton<IBlobStorageService, BlobStorageService>();
```

---

## 4. Captive Dependency Analysis

**Result:** ✅ No captive dependencies found.

A captive dependency occurs when a service with a longer lifetime depends on a service with a shorter lifetime.

**Analysis:**
- All services are `Scoped` (same lifetime) ✅
- No `Singleton` services depending on `Scoped` services ✅
- Middleware resolves services per-request via `InvokeAsync` parameters ✅

### 4.1 Middleware Example (Correct)
**File:** `src/FamilyLibrary.Api/FamilyLibrary.Api/Middleware/ExceptionHandlingMiddleware.cs`

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
}
```

**Assessment:** ✅ Correct - `ILogger` is injected properly with no captive dependency.

---

## 5. Constructor Injection Patterns

### 5.1 Controllers (Excellent)
**File:** `src/FamilyLibrary.Api/FamilyLibrary.Api/Controllers/FamilyRolesController.cs`

```csharp
public class FamilyRolesController(IFamilyRoleService service) : BaseController
```

**Assessment:** ✅ Excellent
- Modern C# primary constructor syntax
- Clean and concise
- No injection of services not used by the controller

### 5.2 Services (Good)
**File:** `src/FamilyLibrary.Api/FamilyLibrary.Application/Services/FamilyService.cs`

```csharp
public class FamilyService : IFamilyService
{
    private readonly IFamilyRepository _familyRepository;
    private readonly IFamilyVersionRepository _versionRepository;
    private readonly IFamilyRoleRepository _roleRepository;
    private readonly IBlobStorageService _blobStorageService;

    public FamilyService(
        IFamilyRepository familyRepository,
        IFamilyVersionRepository versionRepository,
        IFamilyRoleRepository roleRepository,
        IBlobStorageService blobStorageService)
    {
        _familyRepository = familyRepository;
        _versionRepository = versionRepository;
        _roleRepository = roleRepository;
        _blobStorageService = blobStorageService;
    }
}
```

**Assessment:** ✅ Good
- Constructor injection with readonly fields
- All dependencies are interfaces (proper abstraction)
- Dependencies match the service's responsibilities

---

## 6. Testability & Reusability

### 6.1 Unit Tests Use Manual Mocking
**File:** `tests/FamilyLibrary.Application.Tests/Services/FamilyRoleServiceTests.cs`

```csharp
public class FamilyRoleServiceTests
{
    private readonly Mock<IFamilyRoleRepository> _repositoryMock;
    private readonly FamilyRoleService _sut;

    public FamilyRoleServiceTests()
    {
        _repositoryMock = new Mock<IFamilyRoleRepository>();
        _sut = new FamilyRoleService(_repositoryMock.Object);
    }
}
```

**Assessment:** ✅ Good
- Services are testable via constructor injection
- No DI container required for unit tests
- Manual mocking with Moq works well

### 6.2 Missing: Integration Test Helper
**Priority:** LOW

Integration tests would benefit from a shared test setup helper.

**Recommendation:**
```csharp
// tests/FamilyLibrary.Tests/Common/DiTestHelper.cs
public static class DiTestHelper
{
    public static IServiceCollection CreateTestServices()
    {
        var services = new ServiceCollection();

        // Use in-memory database
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("TestDb"));

        // Register dependencies
        services.AddInfrastructure(null!); // Test configuration
        services.AddApplication();

        return services;
    }
}
```

---

## 7. Plugin Project DI Analysis

### 7.1 Plugin Does Not Use DI Container
**File:** `src/FamilyLibrary.Plugin/Application.cs`

```csharp
public class Application : ExternalApplication
{
    public override void OnStartup()
    {
        CreateRibbon();
    }
}
```

**Observation:** The Revit plugin does NOT use .NET DI container. This is **expected and correct** for Revit plugins which:
1. Run in a non-.NET host (Revit)
2. Use Revit's own lifecycle management
3. Cannot use standard `Program.cs` / `WebHost` pattern

**Assessment:** ✅ Appropriate - Manual instantiation is correct for Revit plugins.

---

## 8. Summary of All Issues

### Critical Priority
- None

### High Priority
1. **Mapster mappings not initialized** - `MappingProfile.ConfigureMappings()` never called

### Medium Priority
1. **Missing `AddApi()` extension method** - API configuration in Program.cs
2. **Middleware configuration scattered** - RateLimitingMiddleware configured inline

### Low Priority
1. **BlobStorageService could be Singleton** - Currently Scoped but could be optimized
2. **No shared DI test helper** - Integration tests would benefit from helper

---

## 9. Recommended Fix Priority (Ordered)

1. **HIGH:** Initialize Mapster mappings in `AddApplication()` - breaks functionality
2. **MEDIUM:** Extract API configuration to `AddApi()` extension method - better organization
3. **MEDIUM:** Group middleware configuration - consistency
4. **LOW:** Consider `BlobStorageService` as Singleton for performance - optimization
5. **LOW:** Add DI test helper for integration tests - developer experience

---

## 10. Conclusion

**Overall DI Maturity Level:** Advanced

The Family Library API demonstrates **strong DI patterns** with:
- ✅ Clean extension method organization
- ✅ Proper layer separation
- ✅ No captive dependencies
- ✅ Good constructor injection patterns
- ✅ Testable service design

The main areas for improvement are:
1. Ensuring Mapster mappings are initialized (HIGH priority)
2. Extracting API-level configuration to extension methods (MEDIUM priority)
3. Optional: Optimizing BlobStorageService lifetime (LOW priority)
