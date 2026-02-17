# Tasks: Family Library MVP

**Input**: Design documents from `/specs/001-family-library-mvp/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/api.yaml ✅

**Organization**: Tasks grouped by user story for independent implementation and testing.

---

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: US1-US8 (maps to user stories from spec.md)
- All paths relative to repository root

---

## Phase 1: Setup (Project Infrastructure)

**Purpose**: Initialize all three components (Plugin, Backend, Frontend)

### Backend Setup (Clean Architecture - 4 Projects)

- [ ] T001 Create solution file `FamilyLibrary.sln` at repository root
- [ ] T002 Create Domain project `src/FamilyLibrary.Domain/FamilyLibrary.Domain.csproj` (.NET 10, NO external dependencies)
- [ ] T003 [P] Create Application project `src/FamilyLibrary.Application/FamilyLibrary.Application.csproj` (.NET 10, references Domain)
- [ ] T004 [P] Create Infrastructure project `src/FamilyLibrary.Infrastructure/FamilyLibrary.Infrastructure.csproj` (.NET 10, references Application)
- [ ] T005 [P] Create Api project `src/FamilyLibrary.Api/FamilyLibrary.Api.csproj` (.NET 10, references Application, Infrastructure)
- [ ] T006 Add NuGet packages to Domain: `MediatR` (optional for CQRS)
- [ ] T007 [P] Add NuGet packages to Application: `MediatR`, `FluentValidation`, `AutoMapper` or Mapster
- [ ] T008 [P] Add NuGet packages to Infrastructure: `Microsoft.EntityFrameworkCore.SqlServer`, `Azure.Storage.Blobs`
- [ ] T009 [P] Add NuGet packages to Api: `Swashbuckle.AspNetCore`
- [ ] T010 [P] Configure `Program.cs` with DI, Swagger, CORS in `src/FamilyLibrary.Api/`
- [ ] T011 [P] Create `appsettings.json` and `appsettings.Development.json` with connection strings in Api project

### Frontend Setup

- [ ] T012 Create Angular project at `src/FamilyLibrary.Web/` with Angular 21 CLI
- [ ] T013 [P] Add npm packages: `primeng@19`, `primeicons`, `tailwindcss@4`, `@tanstack/virtual`
- [ ] T014 [P] Configure `tailwind.config.js` at `src/FamilyLibrary.Web/`
- [ ] T015 [P] Update `src/styles.css` with Tailwind directives only (NO custom CSS)
- [ ] T016 [P] Configure `src/app/app.config.ts` with standalone components, provideHttpClient
- [ ] T017 [P] Create feature-based folder structure: `core/`, `shared/`, `features/`, `layout/`

### Plugin Setup

- [ ] T018 Create Plugin project `src/FamilyLibrary.Plugin/FamilyLibrary.Plugin.csproj` (multi-target: net48;net8.0-windows)
- [ ] T019 [P] Add NuGet packages: `Microsoft.Web.WebView2`, `Newtonsoft.Json`, `Azure.Storage.Blobs`
- [ ] T020 [P] Create `.addin` manifest files for Revit 2024 and 2026
- [ ] T021 Create Plugin entry point `PluginApplication.cs` implementing IExternalApplication

### Infrastructure Setup

- [ ] T022 Create `docker-compose.yml` at repository root with Azurite + SQL Server services
- [ ] T023 [P] Create `.github/workflows/ci.yml` for build and test automation
- [ ] T024 [P] Create `.editorconfig` for code style consistency

**Checkpoint**: All projects created and build successfully

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Backend Foundation - Domain Layer

- [ ] T025 Create all Entity classes in `src/FamilyLibrary.Domain/Entities/` per data-model.md (FamilyRole, Category, Tag, Family, FamilyVersion, SystemType, Draft, FamilyNameMapping)
- [ ] T026 [P] Create all Enums in `src/FamilyLibrary.Domain/Enums/` (RoleType, DraftStatus, SystemFamilyGroup, RecognitionOperator, LogicalOperator)
- [ ] T027 [P] Create Domain Exceptions in `src/FamilyLibrary.Domain/Exceptions/` (EntityNotFoundException, BusinessRuleException)
- [ ] T028 [P] Create Repository Interfaces in `src/FamilyLibrary.Domain/Interfaces/` (IFamilyRoleRepository, IFamilyRepository, etc.)

### Backend Foundation - Application Layer

- [ ] T029 Create all DTOs in `src/FamilyLibrary.Application/DTOs/` for API contracts
- [ ] T030 [P] Create Mapper profiles in `src/FamilyLibrary.Application/Mappings/` (AutoMapper or Mapster)
- [ ] T031 [P] Create Validators in `src/FamilyLibrary.Application/Validators/` using FluentValidation
- [ ] T032 Create Service Interfaces in `src/FamilyLibrary.Application/Interfaces/` (IFamilyRoleService, IFamilyService, etc.)
- [ ] T033 [P] Create Common Behaviors in `src/FamilyLibrary.Application/Common/` (PagedResult, Result pattern)

### Backend Foundation - Infrastructure Layer

- [ ] T034 Create `src/FamilyLibrary.Infrastructure/Data/AppDbContext.cs` with DbContext configuration
- [ ] T035 [P] Create Entity Configurations in `src/FamilyLibrary.Infrastructure/Data/Configurations/`
- [ ] T036 Create all Repository implementations in `src/FamilyLibrary.Infrastructure/Repositories/`
- [ ] T037 Create `src/FamilyLibrary.Infrastructure/Services/BlobStorageService.cs`
- [ ] T038 Create initial EF Core migration: `dotnet ef migrations add InitialCreate --project src/FamilyLibrary.Infrastructure --startup-project src/FamilyLibrary.Api`
- [ ] T039 [P] Create DependencyInjection.cs in Infrastructure for service registration

### Backend Foundation - Api Layer

- [ ] T040 Create base `BaseController.cs` with common error handling in `src/FamilyLibrary.Api/Controllers/`
- [ ] T041 [P] Create Global Exception Handler Middleware in `src/FamilyLibrary.Api/Middleware/`
- [ ] T042 Configure Dependency Injection in `Program.cs` (Layered: Domain → Application → Infrastructure)

### Frontend Foundation

- [ ] T043 Create `src/FamilyLibrary.Web/src/app/core/api/api.service.ts` for HTTP client wrapper
- [ ] T044 [P] Create `src/FamilyLibrary.Web/src/app/core/interceptors/` with auth and error interceptors
- [ ] T045 [P] Generate TypeScript models from OpenAPI using openapi-generator-cli
- [ ] T046 Create `src/FamilyLibrary.Web/src/app/core/models/` with all interfaces matching DTOs
- [ ] T047 Create `src/FamilyLibrary.Web/src/app/layout/main-layout/` with app shell (header, sidebar)
- [ ] T048 [P] Create `src/FamilyLibrary.Web/src/app/shared/components/` for reusable PrimeNG wrappers

### Plugin Foundation

- [ ] T049 Create `src/FamilyLibrary.Plugin/Core/Entities/` with shared domain entities (no Revit API)
- [ ] T050 [P] Create `src/FamilyLibrary.Plugin/Core/Interfaces/` with service contracts
- [ ] T051 Create `src/FamilyLibrary.Plugin/Infrastructure/ExtensibleStorage/EsSchema.cs` with GUID definition
- [ ] T052 [P] Create `src/FamilyLibrary.Plugin/Infrastructure/ExtensibleStorage/EsService.cs` for ES read/write
- [ ] T053 Create `src/FamilyLibrary.Plugin/Infrastructure/Hashing/ContentHashService.cs` per research.md R1-R2
- [ ] T054 Create `src/FamilyLibrary.Plugin/Infrastructure/WebView2/WebViewHost.cs` for embedded browser

**Checkpoint**: Foundation ready - user story implementation can begin

---

## Phase 3: User Story 1 - Create and Manage Family Roles (Priority: P1) 🎯 MVP

**Goal**: Administrators can create, edit, and import family roles

**Independent Test**: Create role "FreeAxez_Table" → verify Name is read-only → import 10 roles from Excel

### Backend - US1

- [ ] T055 [P] [US1] Create `FamilyRoleService.cs` in `src/FamilyLibrary.Application/Services/`
- [ ] T056 [P] [US1] Create `FamilyRoleValidator.cs` in `src/FamilyLibrary.Application/Validators/`
- [ ] T057 [US1] Create `FamilyRoleController.cs` in `src/FamilyLibrary.Api/Controllers/`
- [ ] T058 [US1] Implement CRUD endpoints: GET /roles, POST /roles, PUT /roles/{id}, DELETE /roles/{id}
- [ ] T059 [US1] Implement POST /roles/import for Excel upload and preview
- [ ] T060 [US1] Implement batch create logic with duplicate skip in `FamilyRoleService.cs`
- [ ] T061 [P] [US1] Create `CategoryService.cs` and `TagService.cs` in `src/FamilyLibrary.Application/Services/`
- [ ] T062 [P] [US1] Create `CategoryController.cs` and `TagController.cs` in `src/FamilyLibrary.Api/Controllers/`

### Frontend - US1

- [ ] T063 [P] [US1] Create `roles.service.ts` in `src/FamilyLibrary.Web/src/app/features/roles/services/`
- [ ] T064 [P] [US1] Create `role-list.component.ts` using p-table in `src/FamilyLibrary.Web/src/app/features/roles/components/`
- [ ] T065 [US1] Create `role-editor.component.ts` using p-dialog for create/edit
- [ ] T066 [US1] Create `role-import.component.ts` with p-fileUpload for Excel import
- [ ] T067 [US1] Add roles routes to `src/FamilyLibrary.Web/src/app/app.routes.ts`
- [ ] T068 [US1] Create roles state management using Signals in `roles.store.ts`

### Integration - US1

- [ ] T069 [US1] Wire frontend role-list to backend API
- [ ] T070 [US1] Test: Create role → verify read-only Name → delete role with families attached (should fail)

**Checkpoint**: US1 complete - Roles CRUD + Excel import working

---

## Phase 4: User Story 2 - Configure Name Recognition Rules (Priority: P1)

**Goal**: Administrators can configure recognition rules with Visual and Formula editors

**Independent Test**: Create rule "(FB OR Desk) AND Wired" → test on "FB_Field_Wired_v2" → matches

### Backend - US2

- [ ] T071 [P] [US2] Create `RecognitionRuleService.cs` with formula parser in `src/FamilyLibrary.Application/Services/`
- [ ] T072 [P] [US2] Create `RecognitionRuleValidator.cs` in `src/FamilyLibrary.Application/Validators/`
- [ ] T073 [US2] Create `RecognitionRuleController.cs` in `src/FamilyLibrary.Api/Controllers/`
- [ ] T074 [US2] Implement CRUD endpoints: GET/POST/PUT/DELETE /recognition-rules
- [ ] T075 [US2] Implement POST /recognition-rules/validate for syntax validation
- [ ] T076 [US2] Implement POST /recognition-rules/test for testing rule against family name
- [ ] T077 [US2] Implement POST /recognition-rules/check-conflicts for conflict detection

### Frontend - US2

- [ ] T078 [P] [US2] Create `rules.service.ts` in `src/FamilyLibrary.Web/src/app/features/recognition-rules/services/`
- [ ] T079 [US2] Create `rule-editor.component.ts` with tab view (Visual mode, Formula mode)
- [ ] T080 [US2] Create `rule-visual-builder.component.ts` with recursive tree for conditions
- [ ] T081 [US2] Create `rule-test-dialog.component.ts` using p-dialog for testing
- [ ] T082 [US2] Implement formula ↔ visual sync logic
- [ ] T083 [US2] Add conflict warnings display using p-messages

### Integration - US2

- [ ] T084 [US2] Wire rule editor to backend API
- [ ] T085 [US2] Test: Create rule → test on name → verify result → check conflicts

**Checkpoint**: US2 complete - Recognition rules working with Visual + Formula editors

---

## Phase 5: User Story 3 - Manage Loadable Families in Template (Priority: P1)

**Goal**: BIM Manager can see families from template, stamp and publish to library

**Independent Test**: Open template → select family → choose role → Stamp → Publish → family in library

### Plugin - US3 (Core)

- [ ] T086 [P] [US3] Create `StampFamilyCommand/` structure in `src/FamilyLibrary.Plugin/Commands/`
- [ ] T087 [US3] Create `FamilyScannerService.cs` in `src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Services/`
- [ ] T088 [US3] Create `StampService.cs` implementing ES write logic
- [ ] T089 [US3] Create `PublishService.cs` with Blob upload + API integration
- [ ] T090 [US3] Create `LibraryQueueViewModel.cs` for Tab 2 state management
- [ ] T091 [US3] Create `LibraryQueueView.xaml` with 3 tabs (All Families, Queue, Status)

### Backend - US3

- [ ] T092 [P] [US3] Create `FamilyService.cs` in `src/FamilyLibrary.Application/Services/`
- [ ] T093 [P] [US3] Create `FamilyValidator.cs` in `src/FamilyLibrary.Application/Validators/`
- [ ] T094 [US3] Create `FamilyController.cs` in `src/FamilyLibrary.Api/Controllers/`
- [ ] T095 [US3] Implement POST /families/publish with file upload
- [ ] T096 [US3] Implement POST /families/validate-hash for duplicate detection
- [ ] T097 [US3] Implement POST /families/batch-check for status checking
- [ ] T098 [P] [US3] Create `DraftService.cs` in `src/FamilyLibrary.Application/Services/`
- [ ] T099 [US3] Create `DraftController.cs` in `src/FamilyLibrary.Api/Controllers/`
- [ ] T100 [US3] Implement Draft CRUD: GET/POST/PUT/DELETE /drafts

### Frontend - US3

- [ ] T101 [P] [US3] Create `queue.service.ts` in `src/FamilyLibrary.Web/src/app/features/queue/services/`
- [ ] T102 [US3] Create `queue.component.ts` with p-tabView for 3 tabs
- [ ] T103 [US3] Create `family-list.component.ts` using p-table with virtual scroll for Tab 1
- [ ] T104 [US3] Create `draft-list.component.ts` with status badges for Tab 2
- [ ] T105 [US3] Create `library-status.component.ts` for Tab 3

### WebView2 Integration - US3

- [ ] T106 [US3] Implement event handlers: `revit:ready`, `revit:families:list` in Plugin
- [ ] T107 [US3] Implement event handlers: `ui:stamp`, `ui:publish` in Plugin
- [ ] T108 [US3] Create `RevitBridgeService` in Frontend for WebView2 communication

### Integration - US3

- [ ] T109 [US3] Test: Open template → scan families → select → stamp → publish → verify in library

**Checkpoint**: US3 complete - Loadable families Stamp and Publish working

---

## Phase 6: User Story 4 - Manage System Families (Priority: P2)

**Goal**: BIM Manager can manage WallType, FloorType, etc. with JSON serialization

**Independent Test**: Create role for WallType → Stamp → Publish → JSON saved in database

### Backend - US4

- [ ] T110 [P] [US4] Create `SystemTypeService.cs` in `src/FamilyLibrary.Application/Services/`
- [ ] T111 [P] [US4] Create `SystemTypeValidator.cs` in `src/FamilyLibrary.Application/Validators/`
- [ ] T112 [US4] Create `SystemTypeController.cs` in `src/FamilyLibrary.Api/Controllers/`
- [ ] T113 [US4] Implement CRUD: GET/POST /system-types with JSON storage
- [ ] T114 [US4] Implement CompoundStructure JSON serialization for Group A
- [ ] T115 [US4] Implement simple parameter JSON serialization for Group E

### Plugin - US4

- [ ] T116 [P] [US4] Create `SystemTypeScannerService.cs` in `src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Services/`
- [ ] T117 [US4] Create `CompoundStructureSerializer.cs` for WallType/FloorType/RoofType
- [ ] T118 [US4] Create `SystemTypePublisher.cs` for JSON upload
- [ ] T119 [US4] Implement material mapping warning dialog in UI

### Frontend - US4

- [ ] T120 [P] [US4] Add System Types tab to queue component
- [ ] T121 [US4] Create `system-type-detail.component.ts` showing JSON structure
- [ ] T122 [US4] Create `material-warning-dialog.component.ts` for missing materials

### Integration - US4

- [ ] T123 [US4] Test: Stamp WallType → Publish → Pull Update → verify structure applied

**Checkpoint**: US4 complete - System Families Groups A, E working

---

## Phase 7: User Story 5 - Browse Family Library (Priority: P1)

**Goal**: Designers can browse library inside Revit with cards/table views, filters

**Independent Test**: Open library → search by name → open detail page → see version history

### Backend - US5

- [ ] T124 [US5] Implement GET /families with search, filters, pagination in FamilyController
- [ ] T125 [P] [US5] Implement GET /families/{id} with versions and types
- [ ] T126 [P] [US5] Implement GET /families/{id}/versions for history

### Frontend - US5

- [ ] T127 [P] [US5] Create `library.service.ts` in `src/FamilyLibrary.Web/src/app/features/library/services/`
- [ ] T128 [US5] Create `library.component.ts` with view toggle (cards/table)
- [ ] T129 [US5] Create `family-card.component.ts` using p-card for grid view
- [ ] T130 [US5] Create `family-table.component.ts` using p-table with virtual scroll
- [ ] T131 [US5] Create `library-filters.component.ts` with p-dropdown, p-multiSelect
- [ ] T132 [US5] Create `family-detail.component.ts` with version table
- [ ] T133 [US5] Add library routes and navigation

### Plugin - US5

- [ ] T134 [US5] Create `OpenLibraryCommand.cs` in `src/FamilyLibrary.Plugin/Commands/`
- [ ] T135 [US5] Wire OpenLibraryCommand to WebView2 host with library URL

### Integration - US5

- [ ] T136 [US5] Test: Open library in Revit → search → filter → view details

**Checkpoint**: US5 complete - Library browsing working inside Revit

---

## Phase 8: User Story 6 - Load Family to Project (Priority: P1)

**Goal**: Designers can load families from library to current project

**Independent Test**: Find family → Load → family loaded with original filename

### Backend - US6

- [ ] T137 [US6] Implement GET /families/{id}/download/{version} returning SAS token or file in FamilyController
- [ ] T138 [P] [US6] Implement status check integration with batch-check endpoint

### Plugin - US6

- [ ] T139 [P] [US6] Create `LoadFamilyCommand/` structure in `src/FamilyLibrary.Plugin/Commands/`
- [ ] T140 [US6] Create `FamilyDownloader.cs` in `src/FamilyLibrary.Plugin/Commands/LoadFamilyCommand/Services/`
- [ ] T141 [US6] Create `FamilyLoader.cs` wrapping Revit LoadFamily() API
- [ ] T142 [US6] Implement file rename to OriginalFileName logic

### Frontend - US6

- [ ] T143 [US6] Add "Load to Project" button to family-detail component
- [ ] T144 [US6] Wire button to `ui:load-family` WebView2 event

### Integration - US6

- [ ] T145 [US6] Test: Select family → Load → verify loaded with correct name

**Checkpoint**: US6 complete - Family loading working

---

## Phase 9: User Story 7 - Stamp/Publish from Family Editor (Priority: P2)

**Goal**: BIM Manager can publish directly from Family Editor without opening template

**Independent Test**: Open family in Family Editor → Publish command → UI shows only Tab 2

### Plugin - US7

- [ ] T146 [US7] Create `PublishFromEditorCommand.cs` detecting Document.Kind == FamilyDocument
- [ ] T147 [US7] Modify LibraryQueueView to hide Tab 1 and Tab 3 in Family Editor mode
- [ ] T148 [US7] Auto-add current family to Queue when in Family Editor

### Integration - US7

- [ ] T149 [US7] Test: Open family in editor → run command → publish → verify in library

**Checkpoint**: US7 complete - Publish from Family Editor working

---

## Phase 10: User Story 8 - Type Catalogs Management (Priority: P2)

**Goal**: BIM Manager can attach TXT files to families; Designers can select types

**Independent Test**: Publish with TXT → Load → type selection dialog appears

### Backend - US8

- [ ] T150 [US8] Modify /families/publish to accept optional txtFile parameter in FamilyController
- [ ] T151 [US8] Implement TXT hash calculation for version tracking

### Plugin - US8

- [ ] T152 [P] [US8] Create `TypeCatalogParser.cs` in `src/FamilyLibrary.Plugin/Commands/LoadFamilyCommand/Services/`
- [ ] T153 [US8] Create `TypeSelectionWindow.xaml` with dynamic columns from TXT headers
- [ ] T154 [US8] Create `TypeSelectionViewModel.cs` with search, filter, multi-select
- [ ] T155 [US8] Implement LoadFamilySymbol for each selected type

### Frontend - US8

- [ ] T156 [US8] Add type table to family-detail component when catalog exists
- [ ] T157 [US8] Show type selection preview in load dialog

### Integration - US8

- [ ] T158 [US8] Test: Publish with TXT → Load → select 3 types → verify only 3 loaded

**Checkpoint**: US8 complete - Type Catalogs working

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: Improvements affecting multiple user stories

### Documentation

- [ ] T159 [P] Update quickstart.md with docker-compose instructions
- [ ] T160 [P] Add OpenAPI codegen instructions to quickstart.md
- [ ] T161 [P] Create API usage examples in `docs/api-examples.md`

### Performance

- [ ] T162 Optimize batch-check endpoint for 500+ families
- [ ] T163 [P] Add database indexes per data-model.md specifications
- [ ] T164 Test virtual scroll performance with 5000+ rows

### Security

- [ ] T165 [P] Add input validation on all endpoints (Validators)
- [ ] T166 [P] Configure CORS for production domains
- [ ] T167 Add rate limiting middleware

### Error Handling

- [ ] T168 [P] Implement global exception handler in Api layer
- [ ] T169 [P] Add user-friendly error messages in Frontend (p-toast)
- [ ] T170 Implement retry logic with exponential backoff in Plugin

### Testing

- [ ] T171 [P] Add Application layer unit tests for FamilyRoleService
- [ ] T172 [P] Add Application layer unit tests for RecognitionRuleService
- [ ] T173 [P] Add Frontend tests for roles feature
- [ ] T174 Run quickstart.md validation end-to-end

---

## Dependencies & Execution Order

### Phase Dependencies

```
Phase 1 (Setup)
    ↓
Phase 2 (Foundational) ← BLOCKS ALL USER STORIES
    ↓
┌───────────────────────────────────────────────────────┐
│  Phase 3-10 (User Stories) - Can run in PARALLEL      │
│  Priority order if sequential: US1 → US2 → US3 → ... │
└───────────────────────────────────────────────────────┘
    ↓
Phase 11 (Polish)
```

### User Story Dependencies

| Story | Depends On | Can Run Parallel With |
|-------|------------|----------------------|
| US1 (Roles) | Foundational | US2, US5, US6 |
| US2 (Rules) | Foundational + US1 (for role selection) | US5, US6 |
| US3 (Loadable Families) | Foundational + US1 + US2 | US4, US7, US8 |
| US4 (System Families) | Foundational + US1 | US3, US5, US6 |
| US5 (Library Browser) | Foundational | US1, US2, US4 |
| US6 (Load Family) | Foundational + US5 | US1, US2, US4 |
| US7 (Family Editor) | Foundational + US3 | US4, US8 |
| US8 (Type Catalogs) | Foundational + US3 | US4, US7 |

### Critical Path (Minimum Viable Product)

```
Setup → Foundational → US1 → US2 → US3 → US5 → US6 = MVP
```

---

## Parallel Opportunities

### Within Setup Phase
```bash
# Can run in parallel (Backend):
T003 + T004 + T005  # Application, Infrastructure, Api projects
T007 + T008 + T009  # NuGet packages per layer

# Can run in parallel (Frontend):
T013 + T014 + T015 + T016 + T017  # Frontend packages, Tailwind, config, structure

# Can run in parallel (Plugin):
T019 + T020  # Plugin NuGet, manifest

# Can run in parallel (Infrastructure):
T023 + T024  # CI, editorconfig
```

### Within Foundational Phase
```bash
# Can run in parallel (Domain):
T026 + T027 + T028  # Enums, Exceptions, Repository Interfaces

# Can run in parallel (Application):
T030 + T031 + T033  # Mappers, Validators, Common

# Can run in parallel (Infrastructure):
T035 + T039  # Entity Configurations, DI registration

# Can run in parallel (Frontend):
T044 + T045 + T048  # Interceptors, TypeScript generation, Shared components

# Can run in parallel (Plugin):
T050 + T052  # Plugin interfaces + ES service
```

### User Stories Parallel Groups
```bash
# Group 1 (can start after Foundational):
US1 + US5 + US6  # Roles, Library Browser, Load Family

# Group 2 (can start after US1):
US2 + US4  # Rules, System Families

# Group 3 (requires US3):
US7 + US8  # Family Editor, Type Catalogs
```

---

## Implementation Strategy

### MVP First (Recommended)

1. Complete **Phase 1: Setup**
2. Complete **Phase 2: Foundational** (CRITICAL - blocks all)
3. Complete **US1: Roles** → Test independently
4. Complete **US2: Rules** → Test independently
5. Complete **US3: Loadable Families** → Test independently
6. Complete **US5: Library Browser** → Test independently
7. Complete **US6: Load Family** → Test independently
8. **STOP and VALIDATE**: End-to-end MVP test
9. Deploy/Demo MVP

### Incremental Delivery

| Increment | Stories | Value Delivered |
|-----------|---------|-----------------|
| Increment 1 | US1 + US2 | Admin can configure roles and rules |
| Increment 2 | US3 | BIM Manager can stamp/publish families |
| Increment 3 | US5 + US6 | Designers can browse and load families |
| Increment 4 | US4 + US7 + US8 | System families, Family Editor, Type Catalogs |

---

## Task Summary

| Phase | Task Count | Parallelizable |
|-------|------------|----------------|
| Phase 1: Setup | 24 | 18 tasks |
| Phase 2: Foundational | 30 | 15 tasks |
| Phase 3: US1 Roles | 16 | 7 tasks |
| Phase 4: US2 Rules | 15 | 4 tasks |
| Phase 5: US3 Loadable | 24 | 7 tasks |
| Phase 6: US4 System | 14 | 4 tasks |
| Phase 7: US5 Library | 13 | 4 tasks |
| Phase 8: US6 Load | 9 | 2 tasks |
| Phase 9: US7 Editor | 4 | 0 tasks |
| Phase 10: US8 Catalog | 9 | 2 tasks |
| Phase 11: Polish | 16 | 10 tasks |
| **TOTAL** | **174** | **73 tasks** |

---

## Clean Architecture Dependency Rule

```
┌─────────────────────────────────────────────────────────┐
│                    Api Layer                            │
│  (Controllers, Middleware, Program.cs)                  │
└───────────────────────────┬─────────────────────────────┘
                            │ depends on
┌───────────────────────────▼─────────────────────────────┐
│                Infrastructure Layer                     │
│  (DbContext, Repositories, BlobStorage, External)       │
└───────────────────────────┬─────────────────────────────┘
                            │ depends on
┌───────────────────────────▼─────────────────────────────┐
│                 Application Layer                       │
│  (Services, DTOs, Validators, Mappers, Interfaces)      │
└───────────────────────────┬─────────────────────────────┘
                            │ depends on
┌───────────────────────────▼─────────────────────────────┐
│                   Domain Layer                          │
│  (Entities, Enums, Value Objects, Domain Interfaces)    │
│  NO external dependencies                               │
└─────────────────────────────────────────────────────────┘
```

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps task to user story for traceability
- Each user story should be independently testable
- Commit after each task or logical group
- Stop at any checkpoint to validate independently
- **Clean Architecture**: Dependencies flow inward only
- **Domain Layer**: NO external NuGet packages (except MediatR for CQRS)
- **Application Layer**: Business logic, DTOs, Validators
- **Infrastructure Layer**: Data access, external services
- **Api Layer**: Controllers, HTTP concerns only
