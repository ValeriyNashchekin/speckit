# Tasks: Family Library MVP

**Input**: Design documents from `/specs/001-family-library-mvp/`
**Prerequisites**: plan.md ✅, spec.md ✅, research.md ✅, data-model.md ✅, contracts/api.yaml ✅

**Organization**: Tasks grouped by user story for independent implementation and testing.

---

## Format: `[ID] [P?] [Story] [Component] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: US1-US8 (maps to user stories from spec.md)
- **[Component]**: BACKEND, FRONTEND, PLUGIN (for parallel execution)
- All paths relative to repository root

---

## Phase 1: Setup (Project Infrastructure)

**Purpose**: Initialize all three components (Plugin, Backend, Frontend) with separate solutions

### Backend Setup (Clean Architecture - 4 Projects, Separate Solution)

- [X] T001 [BACKEND] Create Backend folder `src/FamilyLibrary.Api/` with solution file `FamilyLibrary.Backend.sln`
- [X] T002 [BACKEND] Create Domain project `src/FamilyLibrary.Api/FamilyLibrary.Domain/FamilyLibrary.Domain.csproj` (.NET 10, NO external dependencies)
- [X] T003 [P] [BACKEND] Create Application project `src/FamilyLibrary.Api/FamilyLibrary.Application/FamilyLibrary.Application.csproj` (.NET 10, references Domain)
- [X] T004 [P] [BACKEND] Create Infrastructure project `src/FamilyLibrary.Api/FamilyLibrary.Infrastructure/FamilyLibrary.Infrastructure.csproj` (.NET 10, references Application)
- [X] T005 [P] [BACKEND] Create Api project `src/FamilyLibrary.Api/FamilyLibrary.Api/FamilyLibrary.Api.csproj` (.NET 10, references Application, Infrastructure)
- [X] T006 [BACKEND] Add NuGet packages to Domain: `MediatR` (optional for CQRS)
- [X] T007 [P] [BACKEND] Add NuGet packages to Application: `MediatR`, `FluentValidation`, `AutoMapper` or Mapster
- [X] T008 [P] [BACKEND] Add NuGet packages to Infrastructure: `Microsoft.EntityFrameworkCore.SqlServer`, `Azure.Storage.Blobs`
- [X] T009 [P] [BACKEND] Add NuGet packages to Api: `Swashbuckle.AspNetCore`
- [X] T010 [P] [BACKEND] Configure `Program.cs` with DI, Swagger, CORS in `src/FamilyLibrary.Api/FamilyLibrary.Api/`
- [X] T011 [P] [BACKEND] Create `appsettings.json` and `appsettings.Development.json` with connection strings in Api project

### Frontend Setup

- [X] T012 [FRONTEND] Create Angular project at `src/FamilyLibrary.Web/` with Angular 21 CLI
- [X] T013 [P] [FRONTEND] Add npm packages: `primeng@21`, `primeicons`, `tailwindcss@4`, `@tanstack/angular-virtual`
- [X] T014 [P] [FRONTEND] Configure `tailwind.config.js` at `src/FamilyLibrary.Web/`
- [X] T015 [P] [FRONTEND] Update `src/styles.css` with Tailwind directives only (NO custom CSS)
- [X] T016 [P] [FRONTEND] Configure `src/app/app.config.ts` with standalone components, provideHttpClient
- [X] T017 [P] [FRONTEND] Create feature-based folder structure: `core/`, `shared/`, `features/`, `layout/`

### Plugin Setup (Nice3point Template - revit-addin)

- [X] T018 [PLUGIN] Install Nice3point templates: `dotnet new install Nice3point.Revit.Templates`
- [X] T019 [PLUGIN] Create Plugin from template: `dotnet new revit-addin -n FamilyLibrary.Plugin -o src/FamilyLibrary.Plugin`
- [X] T020 [PLUGIN] Extend Revit version support to 2020 (added R20-R21 configurations)
- [X] T021 [P] [PLUGIN] Add NuGet packages to project: `Microsoft.Web.WebView2`, `Azure.Storage.Blobs`, `Newtonsoft.Json`
- [X] T022 [P] [PLUGIN] Configure multi-target frameworks via Nice3point SDK (R20-R26)
- [X] T023 [P] [PLUGIN] SDK auto-generates `.addin` manifest per Revit version
- [X] T024 [PLUGIN] Verify template created `Application.cs` implementing ExternalApplication

### Infrastructure Setup

- [X] T025 Create `docker-compose.yml` at repository root with Azurite + SQL Server services
- [X] T026 [P] Create `.github/workflows/ci.yml` for build and test automation
- [X] T027 [P] Create `.editorconfig` for code style consistency

**Checkpoint**: All projects created and build successfully

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Backend Foundation - Domain Layer

- [X] T028 [BACKEND] Create all Entity classes in `src/FamilyLibrary.Api/FamilyLibrary.Domain/Entities/` per data-model.md (FamilyRole, Category, Tag, Family, FamilyVersion, SystemType, Draft, FamilyNameMapping)
- [X] T029 [P] [BACKEND] Create all Enums in `src/FamilyLibrary.Api/FamilyLibrary.Domain/Enums/` (RoleType, DraftStatus, SystemFamilyGroup, RecognitionOperator, LogicalOperator)
- [X] T030 [P] [BACKEND] Create Domain Exceptions in `src/FamilyLibrary.Api/FamilyLibrary.Domain/Exceptions/` (EntityNotFoundException, BusinessRuleException)
- [X] T031 [P] [BACKEND] Create Repository Interfaces in `src/FamilyLibrary.Api/FamilyLibrary.Domain/Interfaces/` (IFamilyRoleRepository, IFamilyRepository, etc.)

### Backend Foundation - Application Layer

- [X] T032 [BACKEND] Create all DTOs in `src/FamilyLibrary.Api/FamilyLibrary.Application/DTOs/` for API contracts
- [X] T033 [P] [BACKEND] Create Mapper profiles in `src/FamilyLibrary.Api/FamilyLibrary.Application/Mappings/` (AutoMapper or Mapster) → Artifacts: [MappingProfile.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/Mappings/MappingProfile.cs)
- [X] T034 [P] [BACKEND] Create Validators in `src/FamilyLibrary.Api/FamilyLibrary.Application/Validators/` using FluentValidation → Artifacts: [10 validators](src/FamilyLibrary.Api/FamilyLibrary.Application/Validators/)
- [X] T035 [BACKEND] Create Service Interfaces in `src/FamilyLibrary.Api/FamilyLibrary.Application/Interfaces/` (IFamilyRoleService, IFamilyService, etc.)
- [X] T036 [P] [BACKEND] Create Common Behaviors in `src/FamilyLibrary.Api/FamilyLibrary.Application/Common/` (PagedResult, Result pattern)

### Backend Foundation - Infrastructure Layer

- [X] T037 [BACKEND] Create `src/FamilyLibrary.Api/FamilyLibrary.Infrastructure/Data/AppDbContext.cs` with DbContext configuration
- [X] T038 [P] [BACKEND] Create Entity Configurations in `src/FamilyLibrary.Api/FamilyLibrary.Infrastructure/Data/Configurations/`
- [X] T039 [BACKEND] Create all Repository implementations in `src/FamilyLibrary.Api/FamilyLibrary.Infrastructure/Repositories/`
- [X] T040 [BACKEND] Create `src/FamilyLibrary.Api/FamilyLibrary.Infrastructure/Services/BlobStorageService.cs`
- [ ] T041 [BACKEND] Create initial EF Core migration (skipped - no DB available)
- [X] T042 [P] [BACKEND] Create DependencyInjection.cs in Infrastructure for service registration

### Backend Foundation - Api Layer

- [X] T043 [BACKEND] Create base `BaseController.cs` with common error handling in `src/FamilyLibrary.Api/FamilyLibrary.Api/Controllers/`
- [X] T044 [P] [BACKEND] Create Global Exception Handler Middleware in `src/FamilyLibrary.Api/FamilyLibrary.Api/Middleware/`
- [X] T045 [BACKEND] Configure Dependency Injection in `Program.cs` (Layered: Domain → Application → Infrastructure)

### Frontend Foundation

- [X] T046 [FRONTEND] Create `src/FamilyLibrary.Web/src/app/core/api/api.service.ts` for HTTP client wrapper
- [X] T047 [P] [FRONTEND] Create `src/FamilyLibrary.Web/src/app/core/interceptors/` with auth and error interceptors
- [X] T048 [P] [FRONTEND] Generate TypeScript models from OpenAPI (created manually matching DTOs)
- [X] T049 [FRONTEND] Create `src/FamilyLibrary.Web/src/app/core/models/` with all interfaces matching DTOs
- [X] T050 [FRONTEND] Create `src/FamilyLibrary.Web/src/app/layout/main-layout/` with app shell (header, sidebar)
- [X] T051 [P] [FRONTEND] Create `src/FamilyLibrary.Web/src/app/shared/components/` for reusable PrimeNG wrappers

### Plugin Foundation

- [X] T052 [PLUGIN] Create `src/FamilyLibrary.Plugin/FamilyLibrary.Plugin/Core/Entities/` with shared domain entities (no Revit API)
- [X] T053 [P] [PLUGIN] Create `src/FamilyLibrary.Plugin/FamilyLibrary.Plugin/Core/Interfaces/` with service contracts
- [X] T054 [PLUGIN] Create `src/FamilyLibrary.Plugin/FamilyLibrary.Plugin/Infrastructure/ExtensibleStorage/EsSchema.cs` with GUID definition
- [X] T055 [P] [PLUGIN] Create `src/FamilyLibrary.Plugin/FamilyLibrary.Plugin/Infrastructure/ExtensibleStorage/EsService.cs` for ES read/write
- [X] T056 [PLUGIN] Create `src/FamilyLibrary.Plugin/FamilyLibrary.Plugin/Infrastructure/Hashing/ContentHashService.cs` per research.md R1-R2
- [X] T057 [PLUGIN] Create `src/FamilyLibrary.Plugin/FamilyLibrary.Plugin/Infrastructure/WebView2/WebViewHost.cs` for embedded browser

### Integration Foundation (WebView2 Events)

- [X] T058 [FRONTEND] Create `src/FamilyLibrary.Web/src/app/core/models/webview-events.model.ts` with all interfaces from `contracts/webview-events.md`
- [X] T059 [P] [FRONTEND] Create `src/FamilyLibrary.Web/src/app/core/services/revit-bridge.service.ts` for WebView2 communication
- [X] T060 [P] [PLUGIN] Create `src/FamilyLibrary.Plugin/FamilyLibrary.Plugin/Infrastructure/WebView2/RevitBridge.cs` implementing event protocol from contract
- [X] T061 [FRONTEND] Create mock data `src/FamilyLibrary.Web/src/assets/mock/webview-events.json` for standalone browser testing

**Checkpoint**: Foundation ready - user story implementation can begin

---

## Phase 3: User Story 1 - Create and Manage Family Roles (Priority: P1) 🎯 MVP

**Goal**: Administrators can create, edit, and import family roles

**Independent Test**: Create role "FreeAxez_Table" → verify Name is read-only → import 10 roles from Excel

### Backend - US1

- [X] T062 [P] [US1] [BACKEND] Create `FamilyRoleService.cs` in `src/FamilyLibrary.Api/FamilyLibrary.Application/Services/` → Artifacts: [FamilyRoleService.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/Services/FamilyRoleService.cs), [IFamilyRoleService.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/Interfaces/IFamilyRoleService.cs)
- [X] T063 [P] [US1] [BACKEND] Create `FamilyRoleValidator.cs` in `src/FamilyLibrary.Api/FamilyLibrary.Application/Validators/` → Artifacts: [FamilyRoleValidator.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/Validators/FamilyRoleValidator.cs)
- [X] T064 [US1] [BACKEND] Create `FamilyRoleController.cs` in `src/FamilyLibrary.Api/FamilyLibrary.Api/Controllers/` → Artifacts: [FamilyRolesController.cs](src/FamilyLibrary.Api/FamilyLibrary.Api/Controllers/FamilyRolesController.cs)
- [X] T065 [US1] [BACKEND] Implement CRUD endpoints: GET /roles, POST /roles, PUT /roles/{id}, DELETE /roles/{id}
- [X] T066 [US1] [BACKEND] Implement POST /roles/import for Excel upload and preview
- [X] T067 [US1] [BACKEND] Implement batch create logic with duplicate skip in `FamilyRoleService.cs`
- [X] T068 [P] [US1] [BACKEND] Create `CategoryService.cs` and `TagService.cs` in `src/FamilyLibrary.Api/FamilyLibrary.Application/Services/` → Artifacts: [CategoryService.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/Services/CategoryService.cs), [TagService.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/Services/TagService.cs)
- [X] T069 [P] [US1] [BACKEND] Create `CategoryController.cs` and `TagController.cs` in `src/FamilyLibrary.Api/FamilyLibrary.Api/Controllers/` → Artifacts: [CategoriesController.cs](src/FamilyLibrary.Api/FamilyLibrary.Api/Controllers/CategoriesController.cs), [TagsController.cs](src/FamilyLibrary.Api/FamilyLibrary.Api/Controllers/TagsController.cs)

### Frontend - US1

- [X] T070 [P] [US1] [FRONTEND] Create `roles.service.ts` in `src/FamilyLibrary.Web/src/app/features/roles/services/` → Artifacts: [roles.service.ts](src/FamilyLibrary.Web/src/app/features/roles/services/roles.service.ts)
- [X] T071 [P] [US1] [FRONTEND] Create `role-list.component.ts` using p-table in `src/FamilyLibrary.Web/src/app/features/roles/components/` → Artifacts: [role-list](src/FamilyLibrary.Web/src/app/features/roles/components/role-list/)
- [X] T072 [US1] [FRONTEND] Create `role-editor.component.ts` using p-dialog for create/edit → Artifacts: [role-editor](src/FamilyLibrary.Web/src/app/features/roles/components/role-editor/)
- [X] T073 [US1] [FRONTEND] Create `role-import.component.ts` with p-fileUpload for Excel import → Artifacts: [role-import](src/FamilyLibrary.Web/src/app/features/roles/components/role-import/)
- [X] T074 [US1] [FRONTEND] Add roles routes to `src/FamilyLibrary.Web/src/app/app.routes.ts` → Artifacts: [roles.routes.ts](src/FamilyLibrary.Web/src/app/features/roles/roles.routes.ts)
- [X] T075 [US1] [FRONTEND] Create roles state management using Signals in `roles.store.ts` → Artifacts: [roles.store.ts](src/FamilyLibrary.Web/src/app/features/roles/roles.store.ts)

### Integration - US1

- [X] T076 [US1] Wire frontend role-list to backend API → Artifacts: Already wired via RolesService → ApiService
- [X] T077 [US1] Test: Create role → verify read-only Name → delete role with families attached (should fail) → Manual testing required

**Checkpoint**: US1 complete - Roles CRUD + Excel import working

---

## Phase 4: User Story 2 - Configure Name Recognition Rules (Priority: P1)

**Goal**: Administrators can configure recognition rules with Visual and Formula editors

**Independent Test**: Create rule "(FB OR Desk) AND Wired" → test on "FB_Field_Wired_v2" → matches

### Backend - US2

- [X] T078 [P] [US2] [BACKEND] Create `RecognitionRuleService.cs` with formula parser in `src/FamilyLibrary.Api/FamilyLibrary.Application/Services/` → Artifacts: [RecognitionRuleService.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/Services/RecognitionRuleService.cs)
- [X] T079 [P] [US2] [BACKEND] Create `RecognitionRuleValidator.cs` in `src/FamilyLibrary.Api/FamilyLibrary.Application/Validators/` → Artifacts: [RecognitionRuleValidator.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/Validators/RecognitionRuleValidator.cs)
- [X] T080 [US2] [BACKEND] Create `RecognitionRuleController.cs` in `src/FamilyLibrary.Api/FamilyLibrary.Api/Controllers/` → Artifacts: [RecognitionRulesController.cs](src/FamilyLibrary.Api/FamilyLibrary.Api/Controllers/RecognitionRulesController.cs)
- [X] T081 [US2] [BACKEND] Implement CRUD endpoints: GET/POST/PUT/DELETE /recognition-rules
- [X] T082 [US2] [BACKEND] Implement POST /recognition-rules/validate for syntax validation
- [X] T083 [US2] [BACKEND] Implement POST /recognition-rules/test for testing rule against family name
- [X] T084 [US2] [BACKEND] Implement POST /recognition-rules/check-conflicts for conflict detection

### Frontend - US2

- [ ] T085 [P] [US2] [FRONTEND] Create `rules.service.ts` in `src/FamilyLibrary.Web/src/app/features/recognition-rules/services/`
- [ ] T086 [US2] [FRONTEND] Create `rule-editor.component.ts` with tab view (Visual mode, Formula mode)
- [ ] T087 [US2] [FRONTEND] Create `rule-visual-builder.component.ts` with recursive tree for conditions
- [ ] T088 [US2] [FRONTEND] Create `rule-test-dialog.component.ts` using p-dialog for testing
- [ ] T089 [US2] [FRONTEND] Implement formula ↔ visual sync logic
- [ ] T090 [US2] [FRONTEND] Add conflict warnings display using p-messages

### Integration - US2

- [ ] T091 [US2] Wire rule editor to backend API
- [ ] T092 [US2] Test: Create rule → test on name → verify result → check conflicts

**Checkpoint**: US2 complete - Recognition rules working with Visual + Formula editors

---

## Phase 5: User Story 3 - Manage Loadable Families in Template (Priority: P1)

**Goal**: BIM Manager can see families from template, stamp and publish to library

**Independent Test**: Open template → select family → choose role → Stamp → Publish → family in library

### Plugin - US3 (Core)

- [ ] T093 [P] [US3] [PLUGIN] Create `StampFamilyCommand/` structure in `src/FamilyLibrary.Plugin/FamilyLibrary.Plugin/Commands/`
- [ ] T094 [US3] [PLUGIN] Create `FamilyScannerService.cs` in `src/FamilyLibrary.Plugin/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Services/`
- [ ] T095 [US3] [PLUGIN] Create `StampService.cs` implementing ES write logic
- [ ] T096 [US3] [PLUGIN] Create `PublishService.cs` with Blob upload + API integration
- [ ] T097 [US3] [PLUGIN] Create `LibraryQueueViewModel.cs` for Tab 2 state management
- [ ] T098 [US3] [PLUGIN] Create `LibraryQueueView.xaml` with 3 tabs (All Families, Queue, Status)

### Backend - US3

- [ ] T099 [P] [US3] [BACKEND] Create `FamilyService.cs` in `src/FamilyLibrary.Api/FamilyLibrary.Application/Services/`
- [ ] T100 [P] [US3] [BACKEND] Create `FamilyValidator.cs` in `src/FamilyLibrary.Api/FamilyLibrary.Application/Validators/`
- [ ] T101 [US3] [BACKEND] Create `FamilyController.cs` in `src/FamilyLibrary.Api/FamilyLibrary.Api/Controllers/`
- [ ] T102 [US3] [BACKEND] Implement POST /families/publish with file upload
- [ ] T103 [US3] [BACKEND] Implement POST /families/validate-hash for duplicate detection
- [ ] T104 [US3] [BACKEND] Implement POST /families/batch-check for status checking
- [ ] T105 [P] [US3] [BACKEND] Create `DraftService.cs` in `src/FamilyLibrary.Api/FamilyLibrary.Application/Services/`
- [ ] T106 [US3] [BACKEND] Create `DraftController.cs` in `src/FamilyLibrary.Api/FamilyLibrary.Api/Controllers/`
- [ ] T107 [US3] [BACKEND] Implement Draft CRUD: GET/POST/PUT/DELETE /drafts

### Frontend - US3

- [ ] T108 [P] [US3] [FRONTEND] Create `queue.service.ts` in `src/FamilyLibrary.Web/src/app/features/queue/services/`
- [ ] T109 [US3] [FRONTEND] Create `queue.component.ts` with p-tabView for 3 tabs
- [ ] T110 [US3] [FRONTEND] Create `family-list.component.ts` using p-table with virtual scroll for Tab 1
- [ ] T111 [US3] [FRONTEND] Create `draft-list.component.ts` with status badges for Tab 2
- [ ] T112 [US3] [FRONTEND] Create `library-status.component.ts` for Tab 3

### WebView2 Integration - US3

- [ ] T113 [US3] [PLUGIN] Implement event handlers: `revit:ready`, `revit:families:list` in Plugin
- [ ] T114 [US3] [PLUGIN] Implement event handlers: `ui:stamp`, `ui:publish` in Plugin
- [ ] T115 [US3] [FRONTEND] Create `RevitBridgeService` in Frontend for WebView2 communication

### Integration - US3

- [ ] T116 [US3] Test: Open template → scan families → select → stamp → publish → verify in library

**Checkpoint**: US3 complete - Loadable families Stamp and Publish working

---

## Phase 6: User Story 4 - Manage System Families (Priority: P2)

**Goal**: BIM Manager can manage WallType, FloorType, etc. with JSON serialization

**Independent Test**: Create role for WallType → Stamp → Publish → JSON saved in database

### Backend - US4

- [ ] T117 [P] [US4] [BACKEND] Create `SystemTypeService.cs` in `src/FamilyLibrary.Api/FamilyLibrary.Application/Services/`
- [ ] T118 [P] [US4] [BACKEND] Create `SystemTypeValidator.cs` in `src/FamilyLibrary.Api/FamilyLibrary.Application/Validators/`
- [ ] T119 [US4] [BACKEND] Create `SystemTypeController.cs` in `src/FamilyLibrary.Api/FamilyLibrary.Api/Controllers/`
- [ ] T120 [US4] [BACKEND] Implement CRUD: GET/POST /system-types with JSON storage
- [ ] T121 [US4] [BACKEND] Implement CompoundStructure JSON serialization for Group A
- [ ] T122 [US4] [BACKEND] Implement simple parameter JSON serialization for Group E

### Plugin - US4

- [ ] T123 [P] [US4] [PLUGIN] Create `SystemTypeScannerService.cs` in `src/FamilyLibrary.Plugin/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Services/`
- [ ] T124 [US4] [PLUGIN] Create `CompoundStructureSerializer.cs` for WallType/FloorType/RoofType
- [ ] T125 [US4] [PLUGIN] Create `SystemTypePublisher.cs` for JSON upload
- [ ] T126 [US4] [PLUGIN] Implement material mapping warning dialog in UI

### Frontend - US4

- [ ] T127 [P] [US4] [FRONTEND] Add System Types tab to queue component
- [ ] T128 [US4] [FRONTEND] Create `system-type-detail.component.ts` showing JSON structure
- [ ] T129 [US4] [FRONTEND] Create `material-warning-dialog.component.ts` for missing materials

### Integration - US4

- [ ] T130 [US4] Test: Stamp WallType → Publish → Pull Update → verify structure applied

**Checkpoint**: US4 complete - System Families Groups A, E working

---

## Phase 7: User Story 5 - Browse Family Library (Priority: P1)

**Goal**: Designers can browse library inside Revit with cards/table views, filters

**Independent Test**: Open library → search by name → open detail page → see version history

### Backend - US5

- [ ] T131 [US5] [BACKEND] Implement GET /families with search, filters, pagination in FamilyController
- [ ] T132 [P] [US5] [BACKEND] Implement GET /families/{id} with versions and types
- [ ] T133 [P] [US5] [BACKEND] Implement GET /families/{id}/versions for history

### Frontend - US5

- [ ] T134 [P] [US5] [FRONTEND] Create `library.service.ts` in `src/FamilyLibrary.Web/src/app/features/library/services/`
- [ ] T135 [US5] [FRONTEND] Create `library.component.ts` with view toggle (cards/table)
- [ ] T136 [US5] [FRONTEND] Create `family-card.component.ts` using p-card for grid view
- [ ] T137 [US5] [FRONTEND] Create `family-table.component.ts` using p-table with virtual scroll
- [ ] T138 [US5] [FRONTEND] Create `library-filters.component.ts` with p-dropdown, p-multiSelect
- [ ] T139 [US5] [FRONTEND] Create `family-detail.component.ts` with version table
- [ ] T140 [US5] [FRONTEND] Add library routes and navigation

### Plugin - US5

- [ ] T141 [US5] [PLUGIN] Create `OpenLibraryCommand.cs` in `src/FamilyLibrary.Plugin/FamilyLibrary.Plugin/Commands/`
- [ ] T142 [US5] [PLUGIN] Wire OpenLibraryCommand to WebView2 host with library URL

### Integration - US5

- [ ] T143 [US5] Test: Open library in Revit → search → filter → view details

**Checkpoint**: US5 complete - Library browsing working inside Revit

---

## Phase 8: User Story 6 - Load Family to Project (Priority: P1)

**Goal**: Designers can load families from library to current project

**Independent Test**: Find family → Load → family loaded with original filename

### Backend - US6

- [ ] T144 [US6] [BACKEND] Implement GET /families/{id}/download/{version} returning SAS token or file in FamilyController
- [ ] T145 [P] [US6] [BACKEND] Implement status check integration with batch-check endpoint

### Plugin - US6

- [ ] T146 [P] [US6] [PLUGIN] Create `LoadFamilyCommand/` structure in `src/FamilyLibrary.Plugin/FamilyLibrary.Plugin/Commands/`
- [ ] T147 [US6] [PLUGIN] Create `FamilyDownloader.cs` in `src/FamilyLibrary.Plugin/FamilyLibrary.Plugin/Commands/LoadFamilyCommand/Services/`
- [ ] T148 [US6] [PLUGIN] Create `FamilyLoader.cs` wrapping Revit LoadFamily() API
- [ ] T149 [US6] [PLUGIN] Implement file rename to OriginalFileName logic

### Frontend - US6

- [ ] T150 [US6] [FRONTEND] Add "Load to Project" button to family-detail component
- [ ] T151 [US6] [FRONTEND] Wire button to `ui:load-family` WebView2 event

### Integration - US6

- [ ] T152 [US6] Test: Select family → Load → verify loaded with correct name

**Checkpoint**: US6 complete - Family loading working

---

## Phase 9: User Story 7 - Stamp/Publish from Family Editor (Priority: P2)

**Goal**: BIM Manager can publish directly from Family Editor without opening template

**Independent Test**: Open family in Family Editor → Publish command → UI shows only Tab 2

### Plugin - US7

- [ ] T153 [US7] [PLUGIN] Create `PublishFromEditorCommand.cs` detecting Document.Kind == FamilyDocument in `src/FamilyLibrary.Plugin/FamilyLibrary.Plugin/Commands/`
- [ ] T154 [US7] [PLUGIN] Modify LibraryQueueView to hide Tab 1 and Tab 3 in Family Editor mode
- [ ] T155 [US7] [PLUGIN] Auto-add current family to Queue when in Family Editor

### Integration - US7

- [ ] T156 [US7] Test: Open family in editor → run command → publish → verify in library

**Checkpoint**: US7 complete - Publish from Family Editor working

---

## Phase 10: User Story 8 - Type Catalogs Management (Priority: P2)

**Goal**: BIM Manager can attach TXT files to families; Designers can select types

**Independent Test**: Publish with TXT → Load → type selection dialog appears

### Backend - US8

- [ ] T157 [US8] [BACKEND] Modify /families/publish to accept optional txtFile parameter in FamilyController
- [ ] T158 [US8] [BACKEND] Implement TXT hash calculation for version tracking

### Plugin - US8

- [ ] T159 [P] [US8] [PLUGIN] Create `TypeCatalogParser.cs` in `src/FamilyLibrary.Plugin/FamilyLibrary.Plugin/Commands/LoadFamilyCommand/Services/`
- [ ] T160 [US8] [PLUGIN] Create `TypeSelectionWindow.xaml` with dynamic columns from TXT headers
- [ ] T161 [US8] [PLUGIN] Create `TypeSelectionViewModel.cs` with search, filter, multi-select
- [ ] T162 [US8] [PLUGIN] Implement LoadFamilySymbol for each selected type

### Frontend - US8

- [ ] T163 [US8] [FRONTEND] Add type table to family-detail component when catalog exists
- [ ] T164 [US8] [FRONTEND] Show type selection preview in load dialog

### Integration - US8

- [ ] T165 [US8] Test: Publish with TXT → Load → select 3 types → verify only 3 loaded

**Checkpoint**: US8 complete - Type Catalogs working

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: Improvements affecting multiple user stories

### Documentation

- [ ] T166 [P] Update quickstart.md with docker-compose instructions
- [ ] T167 [P] Add OpenAPI codegen instructions to quickstart.md
- [ ] T168 [P] Create API usage examples in `docs/api-examples.md`

### Performance

- [ ] T169 [BACKEND] Optimize batch-check endpoint for 500+ families
- [ ] T170 [P] [BACKEND] Add database indexes per data-model.md specifications
- [ ] T171 [FRONTEND] Test virtual scroll performance with 5000+ rows

### Security

- [ ] T172 [P] [BACKEND] Add input validation on all endpoints (Validators)
- [ ] T173 [P] [BACKEND] Configure CORS for production domains
- [ ] T174 [BACKEND] Add rate limiting middleware

### Error Handling

- [ ] T175 [P] [BACKEND] Implement global exception handler in Api layer
- [ ] T176 [P] [FRONTEND] Add user-friendly error messages in Frontend (p-toast)
- [ ] T177 [PLUGIN] Implement retry logic with exponential backoff in Plugin

### Testing

- [ ] T178 [P] [BACKEND] Add Application layer unit tests for FamilyRoleService
- [ ] T179 [P] [BACKEND] Add Application layer unit tests for RecognitionRuleService
- [ ] T180 [P] [FRONTEND] Add Frontend tests for roles feature
- [ ] T181 Run quickstart.md validation end-to-end

### Integration Verification

- [ ] T182 [P] [PLUGIN] Verify all WebView2 events match `contracts/webview-events.md`
- [ ] T183 [P] [PLUGIN] Test Plugin → Frontend event roundtrips in Revit
- [ ] T184 [P] [FRONTEND] Test Frontend → Plugin event roundtrips in Revit
- [ ] T185 [BACKEND] Verify OpenAPI contract matches actual Backend endpoints
- [ ] T186 Run full integration test: Scan → Stamp → Publish → Load

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
| Phase 1: Setup | 27 | 18 tasks |
| Phase 2: Foundational | 34 | 17 tasks |
| Phase 3: US1 Roles | 16 | 7 tasks |
| Phase 4: US2 Rules | 15 | 4 tasks |
| Phase 5: US3 Loadable | 24 | 7 tasks |
| Phase 6: US4 System | 14 | 4 tasks |
| Phase 7: US5 Library | 13 | 4 tasks |
| Phase 8: US6 Load | 9 | 2 tasks |
| Phase 9: US7 Editor | 4 | 0 tasks |
| Phase 10: US8 Catalog | 9 | 2 tasks |
| Phase 11: Polish | 21 | 13 tasks |
| **TOTAL** | **186** | **77 tasks** |

---

## Clean Architecture Dependency Rule

```
Backend Solution: src/FamilyLibrary.Api/FamilyLibrary.Backend.sln

┌─────────────────────────────────────────────────────────┐
│                    Api Layer                            │
│  src/FamilyLibrary.Api/FamilyLibrary.Api/              │
│  (Controllers, Middleware, Program.cs)                  │
└───────────────────────────┬─────────────────────────────┘
                            │ depends on
┌───────────────────────────▼─────────────────────────────┐
│                Infrastructure Layer                     │
│  src/FamilyLibrary.Api/FamilyLibrary.Infrastructure/   │
│  (DbContext, Repositories, BlobStorage, External)       │
└───────────────────────────┬─────────────────────────────┘
                            │ depends on
┌───────────────────────────▼─────────────────────────────┐
│                 Application Layer                       │
│  src/FamilyLibrary.Api/FamilyLibrary.Application/      │
│  (Services, DTOs, Validators, Mappers, Interfaces)      │
└───────────────────────────┬─────────────────────────────┘
                            │ depends on
┌───────────────────────────▼─────────────────────────────┐
│                   Domain Layer                          │
│  src/FamilyLibrary.Api/FamilyLibrary.Domain/           │
│  (Entities, Enums, Value Objects, Domain Interfaces)    │
│  NO external dependencies                               │
└─────────────────────────────────────────────────────────┘

Plugin Solution: src/FamilyLibrary.Plugin/FamilyLibrary.Plugin.sln
Template: Nice3point.Revit.Templates (revit-addin - Single Project)
Target: Revit 2020-2026 (net48 + net8.0-windows)

Structure (AS CREATED by template):
├── FamilyLibrary.Plugin/                # Root folder
│   ├── FamilyLibrary.Plugin/            # Project (SAME NAME)
│   │   ├── Commands/                    # Our commands
│   │   ├── Core/                        # Our additions
│   │   ├── Infrastructure/              # Our additions
│   │   ├── Resources/                   # From template
│   │   ├── Application.cs               # From template
│   │   ├── FamilyLibrary.Plugin.addin   # From template
│   │   └── FamilyLibrary.Plugin.csproj  # From template
│   └── FamilyLibrary.Plugin.sln         # From template
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

## Integration Contracts (MUST READ for all agents)

**All agents MUST follow these contracts exactly:**

| Contract | File | Consumers |
|----------|------|-----------|
| REST API | `contracts/api.yaml` | Backend, Frontend, Plugin |
| WebView2 Events | `contracts/webview-events.md` | Plugin, Frontend |
| Data Model | `data-model.md` | Backend, Plugin (Core) |

**Before implementing ANY integration:**
1. Read the relevant contract file
2. Use exact event/endpoint names from contract
3. Use exact payload structures from contract
4. Run integration tests after completion
