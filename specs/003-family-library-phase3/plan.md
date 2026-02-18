# Implementation Plan: Family Library Phase 3

**Branch**: `003-family-library-phase3` | **Date**: 2026-02-18 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/003-family-library-phase3/spec.md`
**Depends On**: `002-family-library-phase2` (completed)

## Summary

Phase 3 завершает систему Family Library поддержкой сложных случаев:

1. **Module 8: Nested Families** — зависимости между родительскими и вложенными семействами
2. **System Families Groups C, D** — Railings, Stairs, Curtain Walls, Cable Trays
3. **Material Mapping Server** — автоматический маппинг материалов между шаблоном и проектами

**Technical approach**: Расширение существующей архитектуры с добавлением новых entities (FamilyDependency, MaterialMapping) и serializers для complex system families.

---

## Technical Context

**Language/Version**: C# (.NET 10 Backend, .NET Framework 4.8 + .NET 8 Plugin), TypeScript (Angular 21)
**Primary Dependencies**: ASP.NET Core, EF Core 9, Revit API 2020-2026, PrimeNG 19, Tailwind CSS 4
**Storage**: SQL Server 2025, Azure Blob (Azurite for dev), Revit Extensible Storage
**Testing**: xUnit, TestContainers, Angular Testing Library
**Target Platform**: Revit 2020-2026, Web Browser (Angular SPA)
**Project Type**: Web application (Backend + Frontend + Revit Plugin)
**Performance Goals**:
- Nested detection at Publish: < 3 sec per family
- Load family with 5+ nested: < 15 sec
- Material Mapping lookup: < 100 ms
**Constraints**: EditFamily() is heavy (1-3 sec), cache dependencies
**Scale/Scope**: ~500 nested dependencies, ~100 material mappings per project

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| **I. Clean Architecture** | ✅ PASS | Entities (FamilyDependency, MaterialMapping) are pure C#, use cases in services |
| **II. .NET Best Practices** | ✅ PASS | Primary constructors, async patterns, EF Core NoTracking |
| **III. Revit Plugin Architecture** | ✅ PASS | NestedFamilyService in Services/, models in Models/, flat structure |
| **IV. Angular Frontend** | ✅ PASS | Signals, standalone, OnPush, PrimeNG + Tailwind |
| **V. Azure Integration** | ✅ PASS | Azurite for dev, existing patterns |

**Violations**: None

---

## Project Structure

### Documentation (this feature)

```text
specs/003-family-library-phase3/
├── spec.md              # Feature specification (updated)
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output (additions only)
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   └── webview-events-phase3.md  # New events for Phase 3
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (existing + additions)

```text
src/
├── FamilyLibrary.Api/           # Backend API
│   ├── Controllers/
│   │   ├── FamiliesController.cs      # + dependencies, used-in endpoints
│   │   └── MaterialMappingsController.cs  # NEW: CRUD
│   └── Services/
│       ├── NestedFamilyService.cs         # NEW
│       ├── MaterialMappingService.cs      # NEW
│       └── SystemFamilySerializer.cs      # EXTEND: Groups C, D
│
├── FamilyLibrary.Domain/        # Domain entities
│   ├── Entities/
│   │   ├── FamilyDependency.cs           # NEW
│   │   └── MaterialMapping.cs            # NEW
│   └── Enums/
│       └── SystemFamilyGroup.cs          # EXTEND: Groups C, D
│
├── FamilyLibrary.Plugin/        # Revit Plugin
│   ├── Commands/
│   │   └── StampFamilyCommand/
│   │       └── Services/
│   │           └── NestedDetectionService.cs  # NEW
│   └── Services/
│       ├── NestedFamilyLoadService.cs    # NEW: IFamilyLoadOptions
│       ├── RailingSerializer.cs          # NEW: Group C
│       ├── CurtainSerializer.cs          # NEW: Group D
│       └── MaterialMappingClient.cs      # NEW: Client-side mapping
│
├── FamilyLibrary.Web/           # Angular Frontend
│   ├── features/
│   │   ├── library/
│   │   │   └── components/
│   │   │       ├── dependencies-list/    # NEW
│   │   │       └── used-in-list/         # NEW
│   │   ├── settings/
│   │   │   └── material-mappings/        # NEW: CRUD UI
│   │   └── scanner/
│   │       └── pre-load-summary/         # EXTEND: nested versions
│   └── core/
│       └── services/
│           ├── nested-family.service.ts  # NEW
│           └── material-mapping.service.ts  # NEW
│
└── FamilyLibrary.Infrastructure/
    └── Persistence/
        ├── Configurations/
        │   ├── FamilyDependencyConfiguration.cs  # NEW
        │   └── MaterialMappingConfiguration.cs   # NEW
        └── Migrations/
            └── AddPhase3Entities.cs        # NEW

tests/
├── FamilyLibrary.Application.Tests/
│   ├── NestedFamilyServiceTests.cs        # NEW
│   └── MaterialMappingServiceTests.cs     # NEW
└── FamilyLibrary.Plugin.Tests/
    ├── RailingSerializerTests.cs          # NEW
    └── CurtainSerializerTests.cs          # NEW
```

**Structure Decision**: Расширение существующей структуры MVP/Phase2 без изменений в архитектуре.

---

## Phase 0: Research Items

### R13: Nested Family Detection via EditFamily

**Question**: How to efficiently detect Shared nested families without performance impact?

**Decision**: Detect at Publish time, cache in metadata.

**Approach**:
```csharp
// From loaded Family in project
Family parentFamily = ...;
Document familyDoc = doc.EditFamily(parentFamily);

FilteredElementCollector collector = new FilteredElementCollector(familyDoc);
IList<Family> nestedFamilies = collector
    .OfClass(typeof(Family))
    .Cast<Family>()
    .ToList();

foreach (Family nested in nestedFamilies)
{
    string name = nested.Name;
    bool isShared = nested.get_Parameter(BuiltInParameter.FAMILY_SHARED)?.AsInteger() == 1;
    // Record dependency: parent → nested (shared/not shared)
}
```

**Performance**: EditFamily() is 1-3 sec. Execute once at Publish, store in FamilyDependency table.

**Alternatives Considered**:
| Alternative | Rejected Because |
|-------------|------------------|
| Detect at scan time | Too slow (EditFamily per family) |
| Store in ES | Limited space, already used for stamp |

---

### R14: IFamilyLoadOptions for Version Control

**Question**: How to control which version of nested family loads?

**Decision**: Implement IFamilyLoadOptions.OnSharedFamilyFound with user choice.

**Implementation**:
```csharp
public class NestedFamilyLoadOptions : IFamilyLoadOptions
{
    private readonly Dictionary<string, FamilySource> _nestedSources;

    public bool OnSharedFamilyFound(
        Family sharedFamily,
        bool familyInUse,
        out FamilySource source,
        out bool overwriteParameterValues)
    {
        var familyName = sharedFamily.Name;

        // User chose library version? Don't load from RFA, load from library after
        if (_nestedSources.TryGetValue(familyName, out var chosenSource))
        {
            source = chosenSource;
        }
        else
        {
            source = FamilySource.Family; // Default: use from RFA
        }

        overwriteParameterValues = true;
        return true;
    }
}
```

**Load Order**:
1. LoadFamily("Parent.rfa") → loads Parent + Nested from RFA
2. If library version newer → LoadFamily("Nested_v5.rfa") → overwrites
3. Result: Parent + Nested at correct version

---

### R15: RailingType Serialization (Group C)

**Question**: How to serialize RailingType with baluster dependencies?

**Decision**: Custom serializer extracting railingStructure and dependencies.

**JSON Structure**:
```json
{
  "typeName": "Railing_Glass_900",
  "category": "Railings",
  "systemFamily": "Railing",
  "parameters": { "Height": 900 },
  "railingStructure": {
    "topRailTypeName": "Circular - 50mm",
    "balusterPlacement": {
      "pattern": [{ "balusterFamilyName": "Baluster-Round", "balusterTypeName": "25mm" }]
    }
  },
  "dependencies": [
    { "familyName": "Baluster-Round", "typeName": "25mm", "inLibrary": true }
  ]
}
```

**Revit API Approach**:
```csharp
// RailingType → Baluster patterns
var railingType = element as RailingType;
var balusterPatterns = railingType?.get_Parameter(BuiltInParameter.RAILING_BALUSTER_PATTERN);
// Extract baluster family names and type names
// Store as dependencies for Pull Update validation
```

---

### R16: CurtainWallType Serialization (Group D)

**Question**: How to serialize CurtainWallType with grid/panels/mullions?

**Decision**: Custom serializer for CurtainGrid, panels, mullions.

**JSON Structure**:
```json
{
  "typeName": "Curtain_Wall_Storefront",
  "kind": "Curtain",
  "grid": { "horizontalSpacing": 1200, "verticalSpacing": 2400 },
  "panels": { "defaultPanelTypeName": "System Panel" },
  "mullions": {
    "horizontalMullion": "Rectangular Mullion",
    "verticalMullion": "Rectangular Mullion"
  }
}
```

**Revit API Approach**:
```csharp
// WallType with Kind = WallKind.Curtain
var curtainWallType = wallType;
if (wallType.Kind == WallKind.Curtain)
{
    // CurtainGrid settings from type parameters
    var gridParams = new {
        horizontalSpacing = wallType.get_Parameter(BuiltInParameter.CURTAIN_WALL_HORIZ_SPACING)?.AsDouble(),
        verticalSpacing = wallType.get_Parameter(BuiltInParameter.CURTAIN_WALL_VERT_SPACING)?.AsDouble()
    };
    // Mullion types from parameters
}
```

---

### R17: Material Mapping Server-Side Storage

**Question**: How to store and apply material mappings per project?

**Decision**: MaterialMapping table with TemplateMaterialName → ProjectMaterialName per ProjectId.

**Table Structure**:
```csharp
public class MaterialMapping
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string TemplateMaterialName { get; init; } = string.Empty;
    public string ProjectMaterialName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? LastUsedAt { get; set; }
}
```

**Usage at Pull Update**:
```
1. JSON contains "Brick, Common"
2. Lookup MaterialMapping where TemplateMaterialName = "Brick, Common" AND ProjectId = current
3. If found → use ProjectMaterialName ("Кирпич красный")
4. If not found → fallback to MVP behavior (warning + variants)
```

---

### R18: StackedWallType Serialization

**Question**: How to serialize StackedWallType with child WallType references?

**Decision**: Store references by TypeName, validate at Pull Update.

**JSON Structure**:
```json
{
  "typeName": "Stacked_External",
  "kind": "Stacked",
  "stackedLayers": [
    { "wallTypeName": "Wall_Lower_Concrete", "height": 1200 },
    { "wallTypeName": "Wall_Upper_Brick", "height": 0 }
  ]
}
```

**Pull Update Validation**:
- Check if both WallType exist in project
- If missing → Error: "Cannot apply: missing child WallType 'Wall_Lower_Concrete'"

---

## Phase 1: Design Artifacts

### Data Model Additions

#### FamilyDependency (NEW)

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | `uniqueidentifier` | PK, NOT NULL | Auto-generated GUID |
| ParentFamilyId | `uniqueidentifier` | FK → Family, NOT NULL | Parent family |
| NestedFamilyName | `nvarchar(200)` | NOT NULL | Nested family name |
| NestedRoleName | `nvarchar(100)` | NULLABLE | Role if shared and stamped |
| IsShared | `bit` | NOT NULL | True if Shared nested |
| InLibrary | `bit` | NOT NULL | True if published to library |
| LibraryVersion | `int` | NULLABLE | Version in library if InLibrary |
| DetectedAt | `datetime2` | NOT NULL | When detected |

**Indexes**:
- `IX_FamilyDependency_ParentFamilyId` on ParentFamilyId
- `IX_FamilyDependency_NestedRoleName` on NestedRoleName (for "Used In" queries)

#### MaterialMapping (NEW)

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | `uniqueidentifier` | PK, NOT NULL | Auto-generated GUID |
| ProjectId | `uniqueidentifier` | NOT NULL | Project identifier |
| TemplateMaterialName | `nvarchar(200)` | NOT NULL | Material name in template |
| ProjectMaterialName | `nvarchar(200)` | NOT NULL | Material name in project |
| CreatedAt | `datetime2` | NOT NULL | Creation timestamp |
| LastUsedAt | `datetime2` | NULLABLE | Last usage timestamp |

**Indexes**:
- `IX_MaterialMapping_ProjectId_TemplateName` UNIQUE on (ProjectId, TemplateMaterialName)

### API Contracts (Additions)

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/families/{id}/dependencies` | GET | List nested dependencies |
| `/api/families/{id}/used-in` | GET | Where nested is used |
| `/api/material-mappings` | GET | List mappings (filter by projectId) |
| `/api/material-mappings` | POST | Create mapping |
| `/api/material-mappings/{id}` | PUT | Update mapping |
| `/api/material-mappings/{id}` | DELETE | Delete mapping |

### WebView2 Events (Additions)

| Event | Direction | Purpose |
|-------|-----------|---------|
| `revit:nested:detected` | Plugin → UI | Nested families found at Publish |
| `revit:load:preview` | Plugin → UI | Pre-load summary with nested versions |
| `ui:load-with-nested` | UI → Plugin | Load with specific nested versions |
| `ui:material-mapping:save` | UI → Plugin | Save mapping decision |

---

## Complexity Tracking

No violations requiring justification.

---

## Next Steps

1. Run `/speckit.tasks` to generate task breakdown
2. Implement Phase 3 modules:
   - Module 8: Nested Families (US1, US2, US5, US6)
   - System Families Groups C, D (US3)
   - Material Mapping (US4)
