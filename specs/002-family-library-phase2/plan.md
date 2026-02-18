# Implementation Plan: Family Library Phase 2

**Branch**: `002-family-library-phase2` | **Date**: 2026-02-18 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/002-family-library-phase2/spec.md`
**Depends On**: `001-family-library-mvp` (completed)

## Summary

Phase 2 расширяет MVP тремя направлениями:

1. **Сканер проектов (Module 6)** — массовая проверка и обновление семейств в любых проектах
2. **Change Tracking (Module 7)** — отслеживание изменений, diff, changelog
3. **System Families MEP** — поддержка Group A (полностью) и Group B (Pipes, Ducts)

**Technical approach**: Расширение существующей архитектуры с переиспользованием MVP паттернов.

---

## Technical Context

**Language/Version**: C# (.NET 10 Backend, .NET Framework 4.8 + .NET 8 Plugin), TypeScript (Angular 21)
**Primary Dependencies**: ASP.NET Core, EF Core 9, Revit API 2020-2026, PrimeNG 19, Tailwind CSS 4
**Storage**: SQL Server 2025, Azure Blob (Azurite for dev), Revit Extensible Storage
**Testing**: xUnit, TestContainers, Angular Testing Library
**Target Platform**: Revit 2020-2026, Web Browser (Angular SPA)
**Project Type**: Web application (Backend + Frontend + Revit Plugin)
**Performance Goals**:
- Scanner: 1000 families in 5 seconds
- Mass update: 50 families in 60 seconds
**Constraints**: Virtual scroll for 5000+ families, batch requests for performance
**Scale/Scope**: ~5000 families per project, ~50 concurrent updates

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| **I. Clean Architecture** | ✅ PASS | Plugin/Backend/Frontend layers already established in MVP |
| **II. .NET Best Practices** | ✅ PASS | Primary constructors, async patterns, EF Core NoTracking |
| **III. Revit Plugin Architecture** | ✅ PASS | Flat command structure, clean code rules |
| **IV. Angular Frontend** | ✅ PASS | Signals, standalone, OnPush, PrimeNG + Tailwind |
| **V. Azure Integration** | ✅ PASS | Azurite for dev, patterns established in MVP |

**Violations**: None

---

## Project Structure

### Documentation (this feature)

```text
specs/002-family-library-phase2/
├── spec.md              # Feature specification (updated)
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output (additions only)
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── webview-events-phase2.md  # New events for Phase 2
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (existing from MVP)

```text
src/
├── FamilyLibrary.Api/           # Backend API
│   ├── Controllers/
│   │   ├── FamiliesController.cs      # + batch-check, changes endpoints
│   │   └── ProjectsController.cs      # NEW: scan, batch-update
│   ├── Services/
│   │   ├── FamilyService.cs           # + GetChangesAsync, DetectLocalChanges
│   │   └── ProjectScannerService.cs   # NEW
│   └── DTOs/
│       └── ChangeDetectionDtos.cs     # NEW
│
├── FamilyLibrary.Domain/        # Domain entities
│   ├── Entities/
│   │   └── FamilyVersion.cs          # SnapshotJSON already exists
│   └── Enums/
│       └── ChangeCategory.cs         # NEW
│
├── FamilyLibrary.Plugin/        # Revit Plugin
│   ├── Commands/
│   │   ├── UpdateFamiliesCommand/     # NEW: Scanner command
│   │   └── StampFamilyCommand/        # Existing (reuse services)
│   └── Services/
│       ├── FamilyScannerService.cs    # Extend for any project
│       ├── SnapshotService.cs         # NEW: JSON snapshots
│       └── ChangeDetectionService.cs  # NEW: Diff computation
│
├── FamilyLibrary.Web/           # Angular Frontend
│   ├── features/
│   │   ├── scanner/                   # NEW: Scanner page
│   │   └── library/
│   │       └── components/
│   │           └── changelog/         # NEW: Changelog component
│   └── core/
│       └── services/
│           └── change-detection.service.ts  # NEW
│
└── FamilyLibrary.Infrastructure/
    └── Persistence/
        └── Configurations/
            └── FamilyVersionConfiguration.cs  # Index on Hash

tests/
├── FamilyLibrary.Application.Tests/
│   └── ChangeDetectionServiceTests.cs  # NEW
└── FamilyLibrary.Plugin.Tests/
    └── SnapshotServiceTests.cs         # NEW
```

**Structure Decision**: Расширение существующей структуры MVP без изменений в архитектуре.

---

## Phase 0: Research Items

### R8: Snapshot JSON Schema for Change Detection

**Question**: What fields must SnapshotJSON contain for accurate diff detection?

**Decision** (from specification.md):
```json
{
  "version": 2,
  "familyName": "FreeAxez_Table_v2",
  "category": "Furniture",
  "types": ["Type_A", "Type_B", "Type_C"],
  "parameters": [
    {"name": "Width", "value": "600", "group": "Dimensions"},
    {"name": "Height", "value": "800", "group": "Dimensions"}
  ],
  "hasGeometryChanges": true,
  "txtHash": "abc123..."
}
```

**Change Categories**:
| Category | Detection |
|----------|-----------|
| ✏️ Name | Compare familyName |
| 📁 Category | Compare category |
| ➕➖ Types | Compare types[] array |
| 📝 Parameters | Compare parameters[] array |
| 🔧 Geometry | hasGeometryChanges flag (Hash-based) |
| 📄 TXT | Compare txtHash |

---

### R9: Batch Check API Performance

**Question**: How to efficiently check 1000+ families without N+1 queries?

**Decision**: Single batch endpoint with optimized SQL query.

**API Design**:
```http
POST /api/families/batch-check
Content-Type: application/json

{
  "families": [
    {"roleName": "FreeAxez_Table", "hash": "abc123"},
    {"roleName": "FreeAxez_Chair", "hash": "def456"}
  ]
}

Response:
{
  "results": [
    {"roleName": "FreeAxez_Table", "status": "UpToDate", "libraryVersion": 2},
    {"roleName": "FreeAxez_Chair", "status": "UpdateAvailable", "libraryVersion": 3, "currentVersion": 1}
  ]
}
```

**SQL Optimization**:
```sql
-- Single query with IN clause
SELECT f.RoleId, f.CurrentVersion, fv.Hash
FROM Families f
JOIN FamilyVersions fv ON f.Id = fv.FamilyId AND f.CurrentVersion = fv.Version
WHERE f.RoleId IN (@roleIds)
```

---

### R10: Legacy Recognition for Scanner

**Question**: How to match unstamped families to roles efficiently?

**Decision**: Client-side evaluation with cached recognition rules.

**Algorithm**:
```
1. Plugin fetches all recognition rules from server (cached)
2. For each unstamped family:
   a. Evaluate rules against FamilyName
   b. First match → Legacy Match status
   c. No match → Unmatched status
```

**Performance**: Rules cached for session, evaluated locally (no network per family).

---

### R11: MEP RoutingPreferences Serialization

**Question**: How to serialize PipeType/DuctType RoutingPreferences to JSON?

**Decision**: Custom serializer for RoutingPreferenceManager.

**JSON Structure** (from specification.md):
```json
{
  "typeName": "Standard_DN50",
  "category": "Pipes",
  "systemFamily": "Pipe Types",
  "parameters": { "Routing Preference": "Standard" },
  "routingPreferences": {
    "segments": [{ "materialName": "Carbon Steel", "scheduleType": "40" }],
    "fittings": [{ "familyName": "Elbow", "typeName": "Standard" }]
  }
}
```

**Revit API Approach**:
```csharp
// PipeType → RoutingPreferenceManager
var rpm = pipeType.get_Parameter(BuiltInParameter.ROUTING_PREFERENCE_PARAM);
// Iterate RoutingPreferenceRule sets
// Serialize to JSON structure
```

---

### R12: Change Diff Algorithm

**Question**: How to compute diff between two SnapshotJSON instances?

**Decision**: Field-by-field comparison with category-specific logic.

**Implementation**:
```csharp
public class ChangeDetectionService
{
    public ChangeSet DetectChanges(Snapshot previous, Snapshot current)
    {
        var changes = new ChangeSet();

        // Name change
        if (previous.FamilyName != current.FamilyName)
            changes.Add(ChangeCategory.Name, previous.FamilyName, current.FamilyName);

        // Category change
        if (previous.Category != current.Category)
            changes.Add(ChangeCategory.Category, previous.Category, current.Category);

        // Types change (added/removed)
        var addedTypes = current.Types.Except(previous.Types);
        var removedTypes = previous.Types.Except(current.Types);
        // ...

        // Parameters change
        var paramDiff = CompareParameters(previous.Parameters, current.Parameters);
        // ...

        return changes;
    }
}
```

---

## Phase 1: Design Artifacts

### Data Model Additions

No new entities required. Extensions to existing entities:

1. **FamilyVersion.SnapshotJSON** — Already exists, standardize schema
2. **FamilyVersion.OriginalCatalogName** — Already exists (MVP Phase 10)

**New Enums**:
```csharp
public enum ChangeCategory
{
    Name = 0,
    Category = 1,
    Types = 2,
    Parameters = 3,
    Geometry = 4,
    Txt = 5
}

public enum FamilyScanStatus
{
    UpToDate = 0,
    UpdateAvailable = 1,
    LegacyMatch = 2,
    Unmatched = 3,
    LocalModified = 4
}
```

### API Contracts (Additions)

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/families/batch-check` | POST | Batch status check |
| `/api/families/{id}/changes` | GET | Diff between versions |
| `/api/families/{id}/versions` | GET | Version history with changes |
| `/api/families/local-changes` | POST | Detect local modifications |
| `/api/projects/{id}/scan` | POST | Scan project families |
| `/api/projects/{id}/batch-update` | POST | Mass update families |

### WebView2 Events (Additions)

| Event | Direction | Purpose |
|-------|-----------|---------|
| `revit:scan:result` | Plugin → UI | Scan results with statuses |
| `revit:update:progress` | Plugin → UI | Update progress |
| `revit:update:complete` | Plugin → UI | Update completed |
| `ui:scan-project` | UI → Plugin | Request project scan |
| `ui:update-families` | UI → Plugin | Update selected families |
| `ui:stamp-legacy` | UI → Plugin | Stamp legacy families |

---

## Complexity Tracking

No violations requiring justification.

---

## Next Steps

1. Run `/speckit.tasks` to generate task breakdown
2. Implement Phase 2 modules:
   - Module 6: Scanner (US1)
   - Module 7: Change Tracking (US2, US3, US4)
   - System Families MEP (US5)
