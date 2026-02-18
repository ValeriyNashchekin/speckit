# Family Library MVP - Quick Start Guide

This guide will help you get the Family Library system up and running quickly using Docker Compose.

---

## Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| Docker Desktop | Latest | Infrastructure services |
| .NET SDK | 10 | Backend development |
| Node.js | 20+ | Frontend development |
| Revit | 2020-2026 | Plugin testing (optional) |

---

## Quick Start with Docker Compose

### 1. Clone and Navigate

```bash
git clone https://github.com/ValeriyNashchekin/speckit.git
cd speckit
git checkout 001-family-library-mvp
```

### 2. Start Infrastructure

```bash
docker-compose up -d
```

This starts:
- **SQL Server** - `localhost:1433` (user: `sa`, password: `YourStrong@Passw0rd`)
- **Azurite** (Azure Blob Storage emulator) - `localhost:10000-10002`

Wait for SQL Server health check to pass:

```bash
docker-compose ps
```

### 3. Run Database Migrations

```bash
cd src/FamilyLibrary.Api
dotnet ef database update --project ../FamilyLibrary.Infrastructure --startup-project ./FamilyLibrary.Api
```

### 4. Start Backend API

```bash
cd src/FamilyLibrary.Api/FamilyLibrary.Api
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`

### 5. Start Frontend

```bash
cd src/FamilyLibrary.Web
npm install
npm start
```

The Angular app will be available at: `http://localhost:4200`

---

## Docker Compose Services

The `docker-compose.yml` includes:

```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    ports:
      - "1433:1433"
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    volumes:
      - sqlserver_data:/var/opt/mssql
    healthcheck:
      test: /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -C -Q "SELECT 1"

  azurite:
    image: mcr.microsoft.com/azure-storage/azurite
    ports:
      - "10000:10000"  # Blob service
      - "10001:10001"  # Queue service
      - "10002:10002"  # Table service
    volumes:
      - azurite_data:/data
```

### Common Docker Commands

```bash
# Start services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Stop and remove volumes (clean slate)
docker-compose down -v

# Restart a specific service
docker-compose restart sqlserver
```

---

## Development Setup

### Backend Configuration

Create `src/FamilyLibrary.Api/FamilyLibrary.Api/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=FamilyLibrary;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True",
    "AzureBlob": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

### Frontend Configuration

Create `src/FamilyLibrary.Web/src/environments/environment.development.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api'
};
```

---

## OpenAPI Code Generation

### Prerequisites

Install OpenAPI Generator CLI:

```bash
npm install -g @openapitools/openapi-generator-cli
```

### Generate TypeScript Client (Angular)

Generate Angular services and models from the API spec:

```bash
# From repository root
openapi-generator-cli generate \
  -i specs/001-family-library-mvp/contracts/api.yaml \
  -g typescript-angular \
  -o src/FamilyLibrary.Web/src/app/core/api/generated \
  --additional-properties=npmName=familyLibraryApi,supportsES6=true,withInterfaces=true,ngVersion=20.0.0
```

Or generate from running API:

```bash
openapi-generator-cli generate \
  -i http://localhost:5000/swagger/v1/swagger.json \
  -g typescript-angular \
  -o src/FamilyLibrary.Web/src/app/core/api/generated \
  --additional-properties=ngVersion=20.0.0
```

### Generate C# Client (Revit Plugin)

Generate C# API client for the Revit plugin:

```bash
openapi-generator-cli generate \
  -i specs/001-family-library-mvp/contracts/api.yaml \
  -g csharp-netcore \
  -o src/FamilyLibrary.Plugin/ApiClients/generated \
  --additional-properties=targetFramework=net8.0,nullableReferenceTypes=true
```

### Generated Client Usage (Angular)

```typescript
// In your service
import { FamilyRoleService, FamilyRole } from '@app/core/api/generated';

@Injectable({ providedIn: 'root' })
export class RoleFacadeService {
  constructor(private api: FamilyRoleService) {}

  getRoles(): Observable<FamilyRole[]> {
    return this.api.apiRolesGet();
  }
}
```

### Generated Client Usage (C# Plugin)

```csharp
// In your plugin
using FamilyLibrary.ApiClients.Generated.Api;
using FamilyLibrary.ApiClients.Generated.Client;
using FamilyLibrary.ApiClients.Generated.Model;

var configuration = new Configuration 
{ 
    BasePath = "http://localhost:5000/api" 
};
var rolesApi = new RolesApi(configuration);

var roles = await rolesApi.ApiRolesGetAsync();
```

---

## Revit Plugin Setup

### Build Plugin

```bash
cd src/FamilyLibrary.Plugin

# Build for specific Revit version
dotnet build -c Debug.R24   # Revit 2024
dotnet build -c Debug.R25   # Revit 2025
dotnet build -c Debug.R26   # Revit 2026

# Build for all versions
dotnet build -c Release
```

### Install Plugin

Copy built files to Revit addins folder:

- **Revit 2024**: `C:\ProgramData\Autodesk\Revit\Addins\2024\`
- **Revit 2025**: `C:\ProgramData\Autodesk\Revit\Addins\2025\`
- **Revit 2026**: `C:\ProgramData\Autodesk\Revit\Addins\2026\`

---

## Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| SQL Server connection failed | Wait for health check, verify container is running |
| Azurite connection failed | Check port 10000 is not in use by another service |
| Migration fails | Ensure connection string is correct, SQL Server is running |
| CORS errors in frontend | Verify backend CORS allows `http://localhost:4200` |
| EF Core migration error | Use `--project` and `--startup-project` flags correctly |
| WebView2 blank in Revit | Install WebView2 Runtime, clear cache |

### Reset Environment

```bash
# Reset database
dotnet ef database drop --force --project src/FamilyLibrary.Infrastructure --startup-project src/FamilyLibrary.Api
dotnet ef database update --project src/FamilyLibrary.Infrastructure --startup-project src/FamilyLibrary.Api

# Reset Docker containers
docker-compose down -v
docker-compose up -d

# Clear npm cache
cd src/FamilyLibrary.Web
rm -rf node_modules
npm cache clean --force
npm install
```

---

## Next Steps

1. Review the full [API Examples](./api-examples.md) documentation
2. Read the [specification](../specs/001-family-library-mvp/spec.md) for feature details
3. Check the [detailed quickstart](../specs/001-family-library-mvp/quickstart.md) for advanced setup

---

## Useful Links

- Backend Swagger UI: http://localhost:5000/swagger
- Frontend Dev Server: http://localhost:4200
- Azurite Explorer: Use Azure Storage Explorer to connect to `http://127.0.0.1:10000/devstoreaccount1`
