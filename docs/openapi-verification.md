# OpenAPI Contract Verification

## Overview

This document verifies that the OpenAPI/Swagger specification matches the actual Backend endpoints implementation.

**Generated:** 2026-02-17  
**API Version:** v1  
**Base URL:** `/api`

## Configuration

### Swagger Setup (`Program.cs`)

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Family Library API", 
        Version = "v1",
        Description = "API for managing Revit family library"
    });
    
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});
```

### XML Documentation Generation (`.csproj`)

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

## Endpoints Coverage

### Family Roles (`/api/roles`)

| Endpoint | Method | Documented | Response Type | Notes |
|----------|--------|------------|---------------|-------|
| `/api/roles` | GET | OK | `PagedResult<FamilyRoleDto>` | Paginated list with filters |
| `/api/roles/{id}` | GET | OK | `FamilyRoleDto` | Single role by ID |
| `/api/roles` | POST | OK | `Guid` (201 Created) | Create new role |
| `/api/roles/{id}` | PUT | OK | 204 No Content | Update role |
| `/api/roles/{id}` | DELETE | OK | 204 No Content | Delete role |
| `/api/roles/import` | POST | OK | `BatchCreateResult` | Batch import |

**Query Parameters (GET):**
- `page` (int, default: 1)
- `pageSize` (int, default: 10)
- `type` (RoleType?, optional)
- `categoryId` (Guid?, optional)

### Categories (`/api/categories`)

| Endpoint | Method | Documented | Response Type | Notes |
|----------|--------|------------|---------------|-------|
| `/api/categories` | GET | OK | `List<CategoryDto>` | All categories |
| `/api/categories/{id}` | GET | OK | `CategoryDto` | Single category |
| `/api/categories` | POST | OK | `Guid` (201 Created) | Create category |
| `/api/categories/{id}` | PUT | OK | 204 No Content | Update category |
| `/api/categories/{id}` | DELETE | OK | 204 No Content | Delete category |

### Tags (`/api/tags`)

| Endpoint | Method | Documented | Response Type | Notes |
|----------|--------|------------|---------------|-------|
| `/api/tags` | GET | OK | `List<TagDto>` | All tags |
| `/api/tags/{id}` | GET | OK | `TagDto` | Single tag |
| `/api/tags` | POST | OK | `Guid` (201 Created) | Create tag |
| `/api/tags/{id}` | PUT | OK | 204 No Content | Update tag |
| `/api/tags/{id}` | DELETE | OK | 204 No Content | Delete tag |

### Recognition Rules (`/api/recognition-rules`)

| Endpoint | Method | Documented | Response Type | Notes |
|----------|--------|------------|---------------|-------|
| `/api/recognition-rules` | GET | OK | `PagedResult<RecognitionRuleDto>` | Paginated list |
| `/api/recognition-rules/{id}` | GET | OK | `RecognitionRuleDto` | Single rule |
| `/api/recognition-rules` | POST | OK | `Guid` (201 Created) | Create rule |
| `/api/recognition-rules/{id}` | PUT | OK | 204 No Content | Update rule |
| `/api/recognition-rules/{id}` | DELETE | OK | 204 No Content | Delete rule |
| `/api/recognition-rules/validate` | POST | OK | `bool` | Validate formula syntax |
| `/api/recognition-rules/test` | POST | OK | `bool` | Test rule against family name |
| `/api/recognition-rules/check-conflicts` | POST | OK | `List<ConflictDto>` | Check for conflicts |

### Families (`/api/families`)

| Endpoint | Method | Documented | Response Type | Notes |
|----------|--------|------------|---------------|-------|
| `/api/families` | GET | OK | `PagedResult<FamilyDto>` | Paginated with filters |
| `/api/families/{id}` | GET | OK | `FamilyDetailDto` | Full details with versions |
| `/api/families/{id}/versions` | GET | OK | `List<FamilyVersionDto>` | Version history |
| `/api/families/{id}/download/{version?}` | GET | OK | `FamilyDownloadDto` | Download URL |
| `/api/families/publish` | POST | OK | `FamilyDto` (201 Created) | Upload family file |
| `/api/families/validate-hash` | POST | OK | `bool` | Check if hash exists |
| `/api/families/batch-check` | POST | OK | `List<FamilyStatusDto>` | Batch hash check |

**File Upload (POST /publish):**
- `file` (IFormFile, required) - .rfa file
- `txtFile` (IFormFile?, optional) - Type catalog .txt
- Max size: 50MB

### System Types (`/api/system-types`)

| Endpoint | Method | Documented | Response Type | Notes |
|----------|--------|------------|---------------|-------|
| `/api/system-types` | GET | OK | `PagedResult<SystemTypeDto>` | Paginated with filters |
| `/api/system-types/{id}` | GET | OK | `SystemTypeDto` | Single system type |
| `/api/system-types` | POST | OK | `Guid` (201 Created) | Create system type |
| `/api/system-types/{id}` | PUT | OK | 204 No Content | Update system type |
| `/api/system-types/{id}` | DELETE | OK | 204 No Content | Delete system type |
| `/api/system-types/by-role/{roleId}` | GET | OK | `IReadOnlyList<SystemTypeDto>` | By role ID |

### Drafts (`/api/drafts`)

| Endpoint | Method | Documented | Response Type | Notes |
|----------|--------|------------|---------------|-------|
| `/api/drafts` | GET | OK | `PagedResult<DraftDto>` | Paginated with filters |
| `/api/drafts/{id}` | GET | OK | `DraftDto` | Single draft |
| `/api/drafts` | POST | OK | `Guid` (201 Created) | Create draft |
| `/api/drafts/{id}` | PUT | OK | 204 No Content | Update draft |
| `/api/drafts/{id}` | DELETE | OK | 204 No Content | Delete draft |
| `/api/drafts/{id}/status` | PUT | OK | 204 No Content | Update status |
| `/api/drafts/batch` | POST | OK | 204 No Content | Batch create/update |

## Response Types

### HTTP Status Codes

| Code | Description | Usage |
|------|-------------|-------|
| 200 | OK | Successful GET, PUT (with body) |
| 201 | Created | Successful POST (create) |
| 204 | No Content | Successful PUT, DELETE |
| 400 | Bad Request | Validation error |
| 404 | Not Found | Entity not found |
| 429 | Too Many Requests | Rate limit exceeded |

### Paged Result Format

```json
{
  "items": [...],
  "totalCount": 100,
  "page": 1,
  "pageSize": 10,
  "totalPages": 10
}
```

## Authentication

**MVP Phase:** No authentication required (development mode).

## Rate Limiting

- Enabled via `RateLimitingMiddleware`
- Limits requests per IP address
- Returns 429 Too Many Requests when exceeded

## CORS Configuration

Allowed origins (configurable in `appsettings.json`):
- `http://localhost:4200` (Angular dev server)
- `https://localhost:5001`

## Generating OpenAPI Spec

### Method 1: Swagger UI
Navigate to `/swagger` when running in Development mode.

### Method 2: Export JSON
```bash
# Start the API
dotnet run --project src/FamilyLibrary.Api/FamilyLibrary.Api

# Export OpenAPI spec
curl http://localhost:5000/swagger/v1/swagger.json -o docs/openapi.json
```

## Verification Checklist

- [x] Swagger UI accessible at `/swagger`
- [x] OpenAPI JSON available at `/swagger/v1/swagger.json`
- [x] All endpoints documented with XML comments
- [x] XML documentation generation enabled
- [x] Response types specified with `[ProducesResponseType]`
- [x] HTTP status codes documented
- [x] Query parameters documented
- [x] All controllers follow RESTful conventions
