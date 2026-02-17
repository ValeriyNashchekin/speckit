# Implementation Plan: Family Library MVP

**Branch**: `001-family-library-mvp` | **Date**: 2026-02-17 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-family-library-mvp/spec.md`

## Summary

Family Library — корпоративная система управления семействами Revit для FreeAxez. MVP включает:
- **8 User Stories** для 3 ролей (Администратор, БИМ-менеджер, Проектировщик)
- **5 модулей**: Роли, Распознавание, Stamp, Версионирование, Type Catalog
- **3 компонента**: Revit Plugin, .NET Backend, Angular Frontend
- **System Families**: группы A (CompoundStructure) и E (простые)

Technical approach: Clean Architecture с разделением слоёв, Azure Blob для файлов, Extensible Storage для локальных метаданных, WebView2 для UI в Revit.

---

## Technical Context

**Language/Version**: C# (.NET 10 / .NET Framework 4.8), TypeScript (Angular 21)
**Primary Dependencies**:
- Plugin: Revit API 2020-2026, WebView2, Azure.Storage.Blobs, Newtonsoft.Json
- Backend: EF Core 9, ASP.NET Core, Azure.Storage.Blobs, Swashbuckle
- Frontend: Angular 21, PrimeNG 19, Tailwind CSS 4, @tanstack/virtual

**Storage**: SQL Server 2025 (Azure SQL ready), Azure Blob Storage (Azurite for MVP)
**Testing**: xUnit + Moq (Backend), Jest + Spectator (Frontend), Revit Test Framework (Plugin)
**Target Platform**: Windows 10/11, Revit 2020-2026
**Project Type**: Multi-component (Plugin + Backend + Frontend)
**Performance Goals**:
- Stamp: <1 sec
- Publish: <10 sec (50MB max)
- Library search: <2 sec (50 items pagination)
- Project scan: <5 sec (1000 families)
- Virtual scroll: 60 FPS (5000+ rows)

**Constraints**:
- Offline Stamp (no network required)
- Multi-target: .NET Framework 4.8 (Revit 2020-2024) + .NET 8 (Revit 2025-2026)
- WebView2 embedded + standalone browser mode

**Scale/Scope**:
- 10,000 families in library
- 100 versions per family
- 50 concurrent users
- 100 GB database max

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Evidence |
|-----------|--------|----------|
| I. Clean Architecture | ✅ PASS | Entities/UseCases/Adapters/Frameworks layers defined for each component |
| II. .NET Best Practices | ✅ PASS | Primary constructors, nullable types, async patterns, EF Core no-tracking |
| III. Revit Plugin Architecture | ✅ PASS | Flat command structure, clean code rules (15-25 lines functions, 250-350 classes) |
| IV. Angular Frontend | ✅ PASS | Signals, standalone components, OnPush, PrimeNG everywhere, Tailwind only |
| V. Azure Integration | ✅ PASS | Azurite for MVP, SAS tokens, Managed Identity ready |

**Gate Result**: ✅ PASSED — no violations

---

## Project Structure

### Documentation (this feature)

```text
specs/001-family-library-mvp/
├── spec.md              # Feature specification
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (OpenAPI)
│   └── api.yaml
├── checklists/
│   └── requirements.md  # Quality checklist
└── tasks.md             # Phase 2 output (NOT created yet)
```

### Source Code (repository root)

```text
src/
├── FreeAxez.FamilyLibrary.Plugin/       # Revit Plugin
│   ├── Commands/
│   │   ├── StampFamilyCommand/
│   │   │   ├── ViewModels/
│   │   │   ├── Views/
│   │   │   ├── Models/
│   │   │   ├── Services/
│   │   │   └── Enums/
│   │   ├── PublishFamilyCommand/
│   │   ├── LoadFamilyCommand/
│   │   ├── OpenLibraryCommand/
│   │   └── ManageRolesCommand/
│   ├── Core/
│   │   ├── Entities/          # Clean C#, no Revit API
│   │   ├── UseCases/          # Business logic
│   │   └── Interfaces/        # Contracts
│   ├── Infrastructure/
│   │   ├── ExtensibleStorage/
│   │   ├── Hashing/
│   │   └── WebView2/
│   └── FreeAxez.FamilyLibrary.Plugin.csproj (multi-target)
│
├── FreeAxez.FamilyLibrary.Api/          # Backend API
│   ├── Controllers/
│   ├── Services/
│   ├── Repositories/
│   ├── Models/
│   │   ├── Entities/
│   │   └── DTOs/
│   ├── Data/
│   │   └── Migrations/
│   └── FreeAxez.FamilyLibrary.Api.csproj
│
└── FreeAxez.FamilyLibrary.Web/          # Angular Frontend
    ├── src/
    │   ├── app/
    │   │   ├── features/
    │   │   │   ├── library/
    │   │   │   │   ├── components/
    │   │   │   │   ├── services/
    │   │   │   │   └── models/
    │   │   │   ├── roles/
    │   │   │   ├── queue/
    │   │   │   └── scanner/
    │   │   ├── shared/
    │   │   └── core/
    │   │       ├── api/
    │   │       └── interceptors/
    │   ├── tailwind.config.js
    │   └── styles.css
    └── angular.json

tests/
├── FreeAxez.FamilyLibrary.Api.Tests/
│   ├── Unit/
│   └── Integration/
├── FreeAxez.FamilyLibrary.Plugin.Tests/
│   └── Unit/
└── FreeAxez.FamilyLibrary.Web.Tests/
    └── Unit/
```

**Structure Decision**: Multi-component Clean Architecture with:
- Plugin: Flat command structure per constitution
- Backend: Controllers/Services/Repositories/Entities
- Frontend: Feature-based modules (library, roles, queue, scanner)

---

## Complexity Tracking

> No violations to justify — all design decisions follow constitution.

---

## Phase 0: Research Tasks

Based on Technical Context, the following research is needed:

| # | Task | Priority | Status |
|---|------|----------|--------|
| R1 | PartAtom XML determinism validation | High | Pending |
| R2 | OLE Structured Storage streams reading | High | Pending |
| R3 | Extensible Storage + Transfer Project Standards | Medium | Pending |
| R4 | Extensible Storage Revit version upgrade | Medium | Pending |
| R5 | WebView2 + Angular integration patterns | Medium | Pending |
| R6 | Revit multi-target (.NET Framework 4.8 + .NET 8) | High | Pending |

**Output**: research.md with decisions and rationales

---

## Phase 1: Design Artifacts

| Artifact | Description |
|----------|-------------|
| data-model.md | Entity definitions, relationships, validation rules |
| contracts/api.yaml | OpenAPI 3.0 specification for all endpoints |
| quickstart.md | Local development setup guide |

---

## Implementation Phases

### Phase 1: Foundation (Estimated: Sprint 1-2)
- Project scaffolding (Plugin, API, Web)
- Database migrations (EF Core)
- Blob storage setup (Azurite)
- WebView2 host implementation

### Phase 2: Core Modules (Estimated: Sprint 3-5)
- Module 1: Role management (CRUD, Excel import)
- Module 2: Recognition rules (Visual + Formula editor)
- Module 3: Stamp (Extensible Storage, Hash)

### Phase 3: Library Operations (Estimated: Sprint 6-8)
- Module 4: Versioning (Publish, Pull Update)
- Module 5: Type Catalogs (TXT parsing, UI)
- System Families Groups A, E

### Phase 4: UI & Integration (Estimated: Sprint 9-10)
- Library browser (cards, table, filters)
- Queue management (3 tabs)
- WebView2 events integration
- Testing & QA

---

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| PartAtom XML non-deterministic | High | Spike R1 before implementation; Force Publish fallback |
| ES data loss on Revit upgrade | Medium | Server-side FamilyNameMapping; Legacy Recognition fallback |
| Worksharing conflicts on Stamp | Medium | Pre-check ownership; warning to user |
| Hash collision | Low | Warning dialog; user confirms new record |
| Multi-target complexity | Medium | Conditional compilation; shared code project |
