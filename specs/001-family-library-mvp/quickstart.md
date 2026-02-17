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
| Docker Desktop | Latest | Azurite container |
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

### Run Migrations

```bash
cd src/FreeAxez.FamilyLibrary.Api
dotnet ef database update
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

## 4. Backend API

```bash
cd src/FreeAxez.FamilyLibrary.Api

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
cd src/FreeAxez.FamilyLibrary.Web

# Install dependencies
npm install

# Start dev server
npm start

# App will be available at:
# http://localhost:4200
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
cd src/FreeAxez.FamilyLibrary.Plugin

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
    <Assembly>FreeAxez.FamilyLibrary.Plugin.dll</Assembly>
    <FullClassName>FreeAxez.FamilyLibrary.Plugin.Application</FullClassName>
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
# Terminal 1: Database (if using Docker)
docker start family-library-sql

# Terminal 2: Azurite
docker start azurite

# Terminal 3: Backend
cd src/FreeAxez.FamilyLibrary.Api && dotnet run

# Terminal 4: Frontend
cd src/FreeAxez.FamilyLibrary.Web && npm start

# Terminal 5: Plugin (build)
cd src/FreeAxez.FamilyLibrary.Plugin && dotnet build -c Debug
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
cd tests/FreeAxez.FamilyLibrary.Api.Tests
dotnet test
```

### Frontend Unit Tests

```bash
cd src/FreeAxez.FamilyLibrary.Web
npm test
```

### Integration Tests

```bash
cd tests/FreeAxez.FamilyLibrary.Api.Tests
dotnet test --filter "Category=Integration"
```

---

## 9. Debugging

### Backend (Visual Studio)

1. Open `FreeAxez.FamilyLibrary.sln`
2. Set `FreeAxez.FamilyLibrary.Api` as startup project
3. F5 to debug

### Frontend (VS Code)

1. Open `src/FreeAxez.FamilyLibrary.Web`
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
| Database migration fails | Delete Migrations folder, run `dotnet ef migrations add Initial` |
| WebView2 blank in Revit | Check WebView2 Runtime installed, clear cache |
| Plugin not loading | Check .addin file path, verify Revit version matches build |
| CORS errors | Ensure backend CORS allows `http://localhost:4200` |

### Reset Environment

```bash
# Reset database
dotnet ef database drop --force
dotnet ef database update

# Reset Azurite
docker restart azurite

# Clear npm cache
npm cache clean --force
rm -rf node_modules
npm install
```

---

## Useful Commands

```bash
# Generate EF migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Rollback migration
dotnet ef database update PreviousMigrationName

# Build all projects
dotnet build

# Run all tests
dotnet test

# Frontend lint
npm run lint

# Frontend build for production
npm run build
```
