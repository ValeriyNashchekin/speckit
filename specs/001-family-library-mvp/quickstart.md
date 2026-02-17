# Quickstart: Family Library MVP

Local development setup guide for Family Library system.

---

## Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| Visual Studio 2022 | 17.8+ | Backend & Plugin development |
| Node.js | 20+ | Frontend development |
| .NET SDK | 10 | Backend |
| .NET Framework | 4.8 SDK | Revit Plugin (2020-2024) |
| Docker Desktop | Latest | Azurite + SQL Server containers |
| Revit | 2024 or 2026 | Plugin testing |
| SQL Server | 2019+ / LocalDB | Database |

---

## 1. Clone & Setup

```bash
git clone https://github.com/ValeriyNashchekin/speckit.git
cd speckit
git checkout 001-family-library-mvp
```

---

## 2. Database Setup

### Option A: LocalDB (Simplest)

```bash
# Create database
sqllocaldb create mssqllocaldb
sqllocaldb start mssqllocaldb

# Connection string
Server=(localdb)\\mssqllocaldb;Database=FamilyLibrary;Trusted_Connection=true
```

### Option B: Docker SQL Server

```bash
docker run -d \
  --name family-library-sql \
  -e "ACCEPT_EULA=Y" \
  -e "SA_PASSWORD=YourStrong@Passw0rd" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

### Option C: Docker Compose (Recommended)

```bash
# Start all infrastructure services
docker-compose up -d

# This starts:
# - SQL Server on port 1433
# - Azurite on ports 10000-10002
```

### Run Migrations

```bash
# From repository root
dotnet ef database update --project src/FamilyLibrary.Infrastructure --startup-project src/FamilyLibrary.Api
```

---

## 3. Blob Storage (Azurite)

### Start Azurite Container

```bash
docker run -d \
  --name azurite \
  -p 10000:10000 \
  -p 10001:10001 \
  -p 10002:10002 \
  mcr.microsoft.com/azure-storage/azurite

# Create container
az storage container create --name family-library --connection-string "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;"
```

### Connection String (appsettings.Development.json)

```json
{
  "ConnectionStrings": {
    "AzureBlob": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;"
  }
}
```

---

## 4. Backend API (Clean Architecture)

The backend follows **Layered Clean Architecture** with 4 projects:

```
src/
├── FamilyLibrary.Domain/         # Entities, Enums, Interfaces (NO dependencies)
├── FamilyLibrary.Application/    # Services, DTOs, Validators, Mappers
├── FamilyLibrary.Infrastructure/ # DbContext, Repositories, BlobStorage
└── FamilyLibrary.Api/            # Controllers, Middleware, Program.cs
```

### Run Backend

```bash
# From repository root
cd src/FamilyLibrary.Api

# Restore dependencies
dotnet restore

# Run
dotnet run

# API will be available at:
# http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

### appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FamilyLibrary;Trusted_Connection=true",
    "AzureBlob": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

---

## 5. Frontend (Angular)

```bash
cd src/FamilyLibrary.Web

# Install dependencies
npm install

# Start dev server
npm start

# App will be available at:
# http://localhost:4200
```

### Generate TypeScript Models from OpenAPI

```bash
# Install openapi-generator-cli (if not installed)
npm install -g @openapitools/openapi-generator-cli

# Generate models from API spec
openapi-generator-cli generate \
  -i specs/001-family-library-mvp/contracts/api.yaml \
  -g typescript-angular \
  -o src/FamilyLibrary.Web/src/app/core/api/generated \
  --additional-properties=npmName=familyLibraryApi,supportsES6=true,withInterfaces=true
```

### Environment (environment.development.ts)

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api'
};
```

---

## 6. Revit Plugin

### Build

```bash
cd src/FamilyLibrary.Plugin

# Build for Revit 2024
dotnet build -c Release -f net48

# Build for Revit 2026
dotnet build -c Release -f net8.0-windows
```

### Install

1. Copy built files to Revit addins folder:
   - Revit 2024: `C:\ProgramData\Autodesk\Revit\Addins\2024\`
   - Revit 2026: `C:\ProgramData\Autodesk\Revit\Addins\2026\`

2. Copy `.addin` manifest file

### .addin Manifest

```xml
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>Family Library</Name>
    <Assembly>FamilyLibrary.Plugin.dll</Assembly>
    <FullClassName>FamilyLibrary.Plugin.Application</FullClassName>
    <AddInId>YOUR-GUID-HERE</AddInId>
    <VendorId>FREE</VendorId>
    <VendorDescription>FreeAxez</VendorDescription>
  </AddIn>
</RevitAddIns>
```

---

## 7. Development Workflow

### Start All Services

```bash
# Terminal 1: Infrastructure (Docker Compose)
docker-compose up -d

# OR start individually:
# docker start family-library-sql
# docker start azurite

# Terminal 2: Backend
cd src/FamilyLibrary.Api && dotnet run

# Terminal 3: Frontend
cd src/FamilyLibrary.Web && npm start

# Terminal 4: Plugin (build)
cd src/FamilyLibrary.Plugin && dotnet build -c Debug
```

### Test Revit Integration

1. Open Revit with test project/template
2. Load plugin (should auto-load from Addins folder)
3. Run "Open Family Library" command
4. WebView2 should open with Angular UI

---

## 8. Testing

### Backend Unit Tests

```bash
# Test Application layer
cd tests/FamilyLibrary.Application.Tests
dotnet test

# Test Infrastructure layer
cd tests/FamilyLibrary.Infrastructure.Tests
dotnet test
```

### Frontend Unit Tests

```bash
cd src/FamilyLibrary.Web
npm test
```

### Integration Tests

```bash
cd tests/FamilyLibrary.Api.Tests
dotnet test --filter "Category=Integration"
```

---

## 9. Debugging

### Backend (Visual Studio)

1. Open `FamilyLibrary.sln`
2. Set `FamilyLibrary.Api` as startup project
3. F5 to debug

### Frontend (VS Code)

1. Open `src/FamilyLibrary.Web`
2. Launch configuration included
3. F5 to debug

### Plugin (Revit)

1. Build plugin in Debug mode
2. Attach debugger to `Revit.exe`
3. Set breakpoints in plugin code

---

## 10. Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| Azurite connection failed | Check Docker is running, port 10000 available |
| Database migration fails | Run migration with correct project paths |
| WebView2 blank in Revit | Check WebView2 Runtime installed, clear cache |
| Plugin not loading | Check .addin file path, verify Revit version matches build |
| CORS errors | Ensure backend CORS allows `http://localhost:4200` |
| EF Core migration error | Use `--project` and `--startup-project` flags |

### Reset Environment

```bash
# Reset database
dotnet ef database drop --force --project src/FamilyLibrary.Infrastructure --startup-project src/FamilyLibrary.Api
dotnet ef database update --project src/FamilyLibrary.Infrastructure --startup-project src/FamilyLibrary.Api

# Reset Azurite
docker restart azurite

# Clear npm cache
cd src/FamilyLibrary.Web
npm cache clean --force
rm -rf node_modules
npm install
```

---

## Useful Commands

### EF Core Migrations

```bash
# Generate migration
dotnet ef migrations add MigrationName --project src/FamilyLibrary.Infrastructure --startup-project src/FamilyLibrary.Api

# Apply migrations
dotnet ef database update --project src/FamilyLibrary.Infrastructure --startup-project src/FamilyLibrary.Api

# Rollback migration
dotnet ef database update PreviousMigrationName --project src/FamilyLibrary.Infrastructure --startup-project src/FamilyLibrary.Api

# Remove last migration (not applied)
dotnet ef migrations remove --project src/FamilyLibrary.Infrastructure --startup-project src/FamilyLibrary.Api
```

### Build & Test

```bash
# Build all projects
dotnet build FamilyLibrary.sln

# Run all backend tests
dotnet test

# Frontend lint
cd src/FamilyLibrary.Web && npm run lint

# Frontend build for production
cd src/FamilyLibrary.Web && npm run build
```

### OpenAPI Code Generation

```bash
# Generate TypeScript client
openapi-generator-cli generate \
  -i specs/001-family-library-mvp/contracts/api.yaml \
  -g typescript-angular \
  -o src/FamilyLibrary.Web/src/app/core/api/generated

# Generate C# client (for Plugin if needed)
openapi-generator-cli generate \
  -i specs/001-family-library-mvp/contracts/api.yaml \
  -g csharp-netcore \
  -o src/FamilyLibrary.Plugin/ApiClients/generated
```

---

## Architecture Reference

```
┌─────────────────────────────────────────────────────────┐
│                    Api Layer                            │
│  src/FamilyLibrary.Api/                                │
│  (Controllers, Middleware, Program.cs)                  │
└───────────────────────────┬─────────────────────────────┘
                            │ depends on
┌───────────────────────────▼─────────────────────────────┐
│                Infrastructure Layer                     │
│  src/FamilyLibrary.Infrastructure/                     │
│  (DbContext, Repositories, BlobStorage, External)       │
└───────────────────────────┬─────────────────────────────┘
                            │ depends on
┌───────────────────────────▼─────────────────────────────┐
│                 Application Layer                       │
│  src/FamilyLibrary.Application/                        │
│  (Services, DTOs, Validators, Mappers, Interfaces)      │
└───────────────────────────┬─────────────────────────────┘
                            │ depends on
┌───────────────────────────▼─────────────────────────────┐
│                   Domain Layer                          │
│  src/FamilyLibrary.Domain/                             │
│  (Entities, Enums, Value Objects, Domain Interfaces)    │
│  NO external dependencies                               │
└─────────────────────────────────────────────────────────┘
```
