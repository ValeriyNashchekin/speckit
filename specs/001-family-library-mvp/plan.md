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
├── # ============================================================
├── # BACKEND: Clean Architecture (Layered) - 4 Projects
├── # ============================================================
│
├── FamilyLibrary.Domain/                     # Core Layer (NO external dependencies)
│   ├── Entities/                             # Business entities
│   │   ├── FamilyRole.cs
│   │   ├── Category.cs
│   │   ├── Tag.cs
│   │   ├── Family.cs
│   │   ├── FamilyVersion.cs
│   │   ├── SystemType.cs
│   │   ├── Draft.cs
│   │   └── RecognitionRule.cs
│   ├── ValueObjects/                         # Immutable value objects
│   │   └── ContentHash.cs
│   ├── Enums/                                # Domain enumerations
│   │   ├── RoleType.cs
│   │   ├── DraftStatus.cs
│   │   ├── SystemFamilyGroup.cs
│   │   └── RecognitionOperator.cs
│   ├── Events/                               # Domain events
│   │   ├── FamilyPublishedEvent.cs
│   │   └── RoleCreatedEvent.cs
│   ├── Exceptions/                           # Domain exceptions
│   │   ├── EntityNotFoundException.cs
│   │   └── BusinessRuleViolationException.cs
│   ├── Interfaces/                           # Repository interfaces
│   │   ├── IFamilyRoleRepository.cs
│   │   ├── IFamilyRepository.cs
│   │   ├── ISystemTypeRepository.cs
│   │   └── IDraftRepository.cs
│   └── FamilyLibrary.Domain.csproj           # ONLY System dependencies
│
├── FamilyLibrary.Application/               # Application Layer
│   ├── DTOs/                                 # Data Transfer Objects
│   │   ├── FamilyRoles/
│   │   │   ├── FamilyRoleDto.cs
│   │   │   ├── CreateRoleRequest.cs
│   │   │   └── UpdateRoleRequest.cs
│   │   ├── Families/
│   │   │   ├── FamilyDto.cs
│   │   │   ├── FamilyDetailDto.cs
│   │   │   └── FamilyVersionDto.cs
│   │   └── Common/
│   │       └── PagedResult.cs
│   ├── Interfaces/                           # Service interfaces
│   │   ├── IFamilyRoleService.cs
│   │   ├── IFamilyService.cs
│   │   ├── IRecognitionService.cs
│   │   ├── IBlobStorageService.cs
│   │   └── IHashService.cs
│   ├── Services/                             # Application services
│   │   ├── FamilyRoleService.cs
│   │   ├── FamilyService.cs
│   │   ├── RecognitionRuleService.cs
│   │   └── ExcelImportService.cs
│   ├── Validators/                           # FluentValidation validators
│   │   ├── CreateRoleValidator.cs
│   │   └── RecognitionRuleValidator.cs
│   ├── Mappers/                              # AutoMapper profiles
│   │   └── MappingProfile.cs
│   ├── Specifications/                       # Query specifications
│   │   └── FamilyRoleSpecification.cs
│   ├── DependencyInjection.cs                # DI registration
│   └── FamilyLibrary.Application.csproj
│
├── FamilyLibrary.Infrastructure/            # Infrastructure Layer
│   ├── Data/
│   │   ├── AppDbContext.cs                   # EF Core DbContext
│   │   ├── Configurations/                   # EF Core configurations
│   │   │   ├── FamilyRoleConfiguration.cs
│   │   │   ├── FamilyConfiguration.cs
│   │   │   └── SystemTypeConfiguration.cs
│   │   └── Migrations/                       # EF Core migrations
│   ├── Repositories/                         # Repository implementations
│   │   ├── FamilyRoleRepository.cs
│   │   ├── FamilyRepository.cs
│   │   ├── SystemTypeRepository.cs
│   │   └── DraftRepository.cs
│   ├── Services/                             # External services
│   │   ├── BlobStorageService.cs             # Azure Blob implementation
│   │   └── HashService.cs                    # Content hash implementation
│   ├── DependencyInjection.cs                # DI registration
│   └── FamilyLibrary.Infrastructure.csproj
│
├── FamilyLibrary.Api/                       # Presentation Layer
│   ├── Controllers/                          # API endpoints
│   │   ├── FamilyRolesController.cs
│   │   ├── CategoriesController.cs
│   │   ├── TagsController.cs
│   │   ├── RecognitionRulesController.cs
│   │   ├── FamiliesController.cs
│   │   ├── SystemTypesController.cs
│   │   └── DraftsController.cs
│   ├── Filters/                              # Action filters
│   │   └── ValidationFilter.cs
│   ├── Middleware/                           # Custom middleware
│   │   ├── ExceptionHandlingMiddleware.cs
│   │   └── RequestLoggingMiddleware.cs
│   ├── Program.cs                            # Composition root
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── FamilyLibrary.Api.csproj
│
├── # ============================================================
├── # PLUGIN: Revit Plugin (Flat Command Structure)
├── # ============================================================
│
├── FamilyLibrary.Plugin/                    # Revit Plugin
│   ├── Commands/                             # Flat command structure
│   │   ├── OpenLibraryCommand/
│   │   │   ├── OpenLibraryCommand.cs         # IExternalCommand
│   │   │   └── OpenLibraryAvailability.cs
│   │   ├── StampFamilyCommand/
│   │   │   ├── ViewModels/
│   │   │   │   └── QueueViewModel.cs
│   │   │   ├── Views/
│   │   │   │   └── QueueWindow.xaml
│   │   │   ├── Models/
│   │   │   │   └── QueueItemModel.cs        # Clean C#, no Revit API
│   │   │   ├── Services/
│   │   │   │   ├── FamilyScannerService.cs   # Revit API here
│   │   │   │   ├── StampService.cs
│   │   │   │   └── PublishService.cs
│   │   │   ├── Enums/
│   │   │   │   └── QueueStatus.cs
│   │   │   └── StampFamilyCommand.cs
│   │   ├── LoadFamilyCommand/
│   │   │   ├── Services/
│   │   │   │   ├── FamilyDownloader.cs
│   │   │   │   └── TypeCatalogParser.cs
│   │   │   ├── Views/
│   │   │   │   └── TypeSelectionWindow.xaml
│   │   │   └── LoadFamilyCommand.cs
│   │   └── PublishFromEditorCommand/
│   │       └── PublishFromEditorCommand.cs
│   ├── Core/                                 # Shared plugin core
│   │   ├── Entities/                         # Clean C#, no Revit API
│   │   ├── Interfaces/
│   │   └── Events/
│   ├── Infrastructure/
│   │   ├── ExtensibleStorage/
│   │   │   ├── EsSchema.cs
│   │   │   └── EsService.cs
│   │   ├── Hashing/
│   │   │   ├── PartAtomNormalizer.cs
│   │   │   └── ContentHashService.cs
│   │   └── WebView2/
│   │       ├── WebViewHost.cs
│   │       └── RevitBridge.cs
│   ├── PluginApplication.cs                  # IExternalApplication
│   ├── FamilyLibrary.Plugin.csproj          # Multi-target: net48;net8.0-windows
│   └── FamilyLibrary.Plugin.addin
│
├── # ============================================================
├── # FRONTEND: Angular 21 (Feature-Based with Core/Shared)
├── # ============================================================
│
├── FamilyLibrary.Web/                       # Angular Frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── core/                         # App-wide concerns
│   │   │   │   ├── services/
│   │   │   │   │   ├── api.service.ts
│   │   │   │   │   ├── auth.service.ts
│   │   │   │   │   └── revit-bridge.service.ts
│   │   │   │   ├── interceptors/
│   │   │   │   │   ├── auth.interceptor.ts
│   │   │   │   │   ├── error.interceptor.ts
│   │   │   │   │   └── loading.interceptor.ts
│   │   │   │   ├── guards/
│   │   │   │   │   └── auth.guard.ts
│   │   │   │   ├── models/
│   │   │   │   │   ├── api-response.model.ts
│   │   │   │   │   └── pagination.model.ts
│   │   │   │   └── index.ts                  # Barrel export
│   │   │   │
│   │   │   ├── shared/                       # Reusable across features
│   │   │   │   ├── components/
│   │   │   │   │   ├── loading-spinner/
│   │   │   │   │   ├── empty-state/
│   │   │   │   │   ├── error-message/
│   │   │   │   │   └── confirmation-dialog/
│   │   │   │   ├── directives/
│   │   │   │   │   └── permission.directive.ts
│   │   │   │   ├── pipes/
│   │   │   │   │   ├── truncate.pipe.ts
│   │   │   │   │   └── safe-url.pipe.ts
│   │   │   │   ├── utils/
│   │   │   │   │   └── form.utils.ts
│   │   │   │   └── index.ts
│   │   │   │
│   │   │   ├── features/                     # Business features
│   │   │   │   ├── roles/                    # US1: Family Roles
│   │   │   │   │   ├── components/
│   │   │   │   │   │   ├── role-list/
│   │   │   │   │   │   │   ├── role-list.component.ts
│   │   │   │   │   │   │   └── role-list.component.html
│   │   │   │   │   │   ├── role-editor/
│   │   │   │   │   │   └── role-import/
│   │   │   │   │   ├── services/
│   │   │   │   │   │   └── roles.service.ts
│   │   │   │   │   ├── models/
│   │   │   │   │   │   └── role.model.ts
│   │   │   │   │   ├── pages/
│   │   │   │   │   │   └── roles.page.ts
│   │   │   │   │   ├── roles.routes.ts
│   │   │   │   │   └── index.ts
│   │   │   │   │
│   │   │   │   ├── recognition-rules/       # US2: Recognition Rules
│   │   │   │   │   ├── components/
│   │   │   │   │   │   ├── rule-editor/
│   │   │   │   │   │   ├── rule-visual-builder/
│   │   │   │   │   │   └── rule-test-dialog/
│   │   │   │   │   ├── services/
│   │   │   │   │   │   └── rules.service.ts
│   │   │   │   │   ├── models/
│   │   │   │   │   │   └── rule.model.ts
│   │   │   │   │   ├── rules.routes.ts
│   │   │   │   │   └── index.ts
│   │   │   │   │
│   │   │   │   ├── queue/                   # US3: Library Queue
│   │   │   │   │   ├── components/
│   │   │   │   │   │   ├── queue-tabs/
│   │   │   │   │   │   ├── family-list/
│   │   │   │   │   │   ├── draft-list/
│   │   │   │   │   │   └── library-status/
│   │   │   │   │   ├── services/
│   │   │   │   │   │   └── queue.service.ts
│   │   │   │   │   ├── models/
│   │   │   │   │   │   └── draft.model.ts
│   │   │   │   │   ├── queue.routes.ts
│   │   │   │   │   └── index.ts
│   │   │   │   │
│   │   │   │   ├── library/                 # US5: Library Browser
│   │   │   │   │   ├── components/
│   │   │   │   │   │   ├── library-grid/
│   │   │   │   │   │   ├── library-table/
│   │   │   │   │   │   ├── family-card/
│   │   │   │   │   │   ├── family-detail/
│   │   │   │   │   │   └── library-filters/
│   │   │   │   │   ├── services/
│   │   │   │   │   │   └── library.service.ts
│   │   │   │   │   ├── models/
│   │   │   │   │   │   └── family.model.ts
│   │   │   │   │   ├── pages/
│   │   │   │   │   │   └── library.page.ts
│   │   │   │   │   ├── library.routes.ts
│   │   │   │   │   └── index.ts
│   │   │   │   │
│   │   │   │   └── system-types/            # US4: System Families
│   │   │   │       ├── components/
│   │   │   │       ├── services/
│   │   │   │       ├── models/
│   │   │   │       └── index.ts
│   │   │   │
│   │   │   ├── layout/                      # App layout
│   │   │   │   ├── main-layout/
│   │   │   │   │   ├── main-layout.component.ts
│   │   │   │   │   └── main-layout.component.html
│   │   │   │   ├── header/
│   │   │   │   └── sidebar/
│   │   │   │
│   │   │   ├── app.component.ts
│   │   │   ├── app.routes.ts
│   │   │   └── app.config.ts                # Providers, HTTP, interceptors
│   │   │
│   │   ├── assets/
│   │   │   └── images/
│   │   ├── environments/
│   │   │   ├── environment.ts
│   │   │   └── environment.production.ts
│   │   ├── styles.css                       # Tailwind directives ONLY
│   │   ├── tailwind.config.js
│   │   └── main.ts
│   │
│   ├── angular.json
│   ├── package.json
│   └── tsconfig.json
│
├── # ============================================================
├── # TESTS
├── # ============================================================
│
├── tests/
│   ├── FamilyLibrary.Domain.Tests/
│   │   └── Entities/
│   ├── FamilyLibrary.Application.Tests/
│   │   ├── Services/
│   │   └── Validators/
│   ├── FamilyLibrary.Infrastructure.Tests/
│   │   └── Repositories/
│   ├── FamilyLibrary.Api.Tests/
│   │   ├── Controllers/
│   │   └── Integration/
│   ├── FamilyLibrary.Plugin.Tests/
│   │   └── Services/
│   └── FamilyLibrary.Web.Tests/
│       └── features/
│
├── # ============================================================
├── # INFRASTRUCTURE
├── # ============================================================
│
├── docker-compose.yml                        # Azurite + SQL Server
├── .github/workflows/ci.yml
└── FamilyLibrary.sln                        # Solution file
```

**Structure Decision**:

### Backend (.NET)
- **Layered Clean Architecture** с 4 отдельными проектами
- **Domain**: только System dependencies, business entities, interfaces
- **Application**: DTOs, services, validators, mappers
- **Infrastructure**: EF Core, repositories, external services
- **Api**: Controllers, middleware, composition root
- **Dependency flow**: Api → Infrastructure → Application → Domain

### Plugin (Revit)
- **Flat command structure** per constitution
- **Models/** = clean C#, NO Revit API
- **Services/** = ALL Revit API calls
- **ViewModels/** = NO Revit API in constructors

### Frontend (Angular 21)
- **Feature-based structure** с core/shared/features
- **Standalone components** по умолчанию
- **Signals** для state management
- **PrimeNG** для всех UI компонентов
- **Tailwind** для стилизации (NO custom CSS)
- **Barrel exports** (index.ts) для clean imports

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
