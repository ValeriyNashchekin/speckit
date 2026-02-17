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

### Backend Setup

- [ ] T001 Create solution file `FreeAxez.FamilyLibrary.sln` at repository root
- [ ] T002 Create Backend project `src/FreeAxez.FamilyLibrary.Api/FreeAxez.FamilyLibrary.Api.csproj` with .NET 10
- [ ] T003 [P] Add NuGet packages to Backend: `Microsoft.EntityFrameworkCore.SqlServer`, `Azure.Storage.Blobs`, `Swashbuckle.AspNetCore`
- [ ] T004 [P] Configure `Program.cs` with DI, Swagger, CORS in `src/FreeAxez.FamilyLibrary.Api/`
- [ ] T005 [P] Create `appsettings.json` and `appsettings.Development.json` with connection strings

### Frontend Setup

- [ ] T006 Create Angular project at `src/FreeAxez.FamilyLibrary.Web/` with Angular 21 CLI
- [ ] T007 [P] Add npm packages: `primeng@19`, `primeicons`, `tailwindcss@4`, `@tanstack/virtual`
- [ ] T008 [P] Configure `tailwind.config.js` at `src/FreeAxez.FamilyLibrary.Web/`
- [ ] T009 [P] Update `src/styles.css` with Tailwind directives only (no custom CSS)
- [ ] T010 [P] Configure `src/app/app.config.ts` with standalone components, provideHttpClient

### Plugin Setup

- [ ] T011 Create Plugin project `src/FreeAxez.FamilyLibrary.Plugin/FreeAxez.FamilyLibrary.Plugin.csproj` (multi-target: net48;net8.0-windows)
- [ ] T012 [P] Add NuGet packages: `Microsoft.Web.WebView2`, `Newtonsoft.Json`, `Azure.Storage.Blobs`
- [ ] T013 [P] Create `.addin` manifest files for Revit 2024 and 2026
- [ ] T014 Create Plugin entry point `PluginApplication.cs` implementing IExternalApplication

### Infrastructure Setup

- [ ] T015 Create `docker-compose.yml` at repository root with Azurite + SQL Server services
- [ ] T016 [P] Create `.github/workflows/ci.yml` for build and test automation
- [ ] T017 [P] Create `.editorconfig` for code style consistency

**Checkpoint**: All projects created and build successfully

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

### Backend Foundation

- [ ] T018 Create `src/FreeAxez.FamilyLibrary.Api/Data/AppDbContext.cs` with DbContext configuration
- [ ] T019 [P] Create all Entity classes in `src/FreeAxez.FamilyLibrary.Api/Models/Entities/` per data-model.md
- [ ] T020 [P] Create all DTOs in `src/FreeAxez.FamilyLibrary.Api/Models/DTOs/` for API contracts
- [ ] T021 Create initial EF Core migration: `dotnet ef migrations add InitialCreate`
- [ ] T022 [P] Create `src/FreeAxez.FamilyLibrary.Api/Repositories/Interfaces/` with all repository interfaces
- [ ] T023 [P] Create `src/FreeAxez.FamilyLibrary.Api/Repositories/` with repository implementations
- [ ] T024 Create `src/FreeAxez.FamilyLibrary.Api/Services/Interfaces/` with all service interfaces
- [ ] T025 Configure BlobStorageService in `src/FreeAxez.FamilyLibrary.Api/Services/BlobStorageService.cs`
- [ ] T026 Create base `BaseController.cs` with common error handling in `src/FreeAxez.FamilyLibrary.Api/Controllers/`

### Frontend Foundation

- [ ] T027 Create `src/FreeAxez.FamilyLibrary.Web/src/app/core/api/api.service.ts` for HTTP client wrapper
- [ ] T028 [P] Create `src/FreeAxez.FamilyLibrary.Web/src/app/core/interceptors/` with auth and error interceptors
- [ ] T029 [P] Generate TypeScript models from OpenAPI using openapi-generator-cli
- [ ] T030 Create `src/FreeAxez.FamilyLibrary.Web/src/app/core/models/` with all interfaces matching DTOs
- [ ] T031 Create `src/FreeAxez.FamilyLibrary.Web/src/app/shared/layout/` with app shell (header, sidebar)

### Plugin Foundation

- [ ] T032 Create `src/FreeAxez.FamilyLibrary.Plugin/Core/Entities/` with shared domain entities (no Revit API)
- [ ] T033 [P] Create `src/FreeAxez.FamilyLibrary.Plugin/Core/Interfaces/` with service contracts
- [ ] T034 Create `src/FreeAxez.FamilyLibrary.Plugin/Infrastructure/ExtensibleStorage/EsSchema.cs` with GUID definition
- [ ] T035 [P] Create `src/FreeAxez.FamilyLibrary.Plugin/Infrastructure/ExtensibleStorage/EsService.cs` for ES read/write
- [ ] T036 Create `src/FreeAxez.FamilyLibrary.Plugin/Infrastructure/Hashing/ContentHashService.cs` per research.md R1-R2
- [ ] T037 Create `src/FreeAxez.FamilyLibrary.Plugin/Infrastructure/WebView2/WebViewHost.cs` for embedded browser

**Checkpoint**: Foundation ready - user story implementation can begin

---

## Phase 3: User Story 1 - Create and Manage Family Roles (Priority: P1) 🎯 MVP

**Goal**: Administrators can create, edit, and import family roles

**Independent Test**: Create role "FreeAxez_Table" → verify Name is read-only → import 10 roles from Excel

### Backend - US1

- [ ] T038 [P] [US1] Create `FamilyRoleController.cs` in `src/FreeAxez.FamilyLibrary.Api/Controllers/`
- [ ] T039 [P] [US1] Create `FamilyRoleService.cs` in `src/FreeAxez.FamilyLibrary.Api/Services/`
- [ ] T040 [US1] Implement CRUD endpoints: GET /roles, POST /roles, PUT /roles/{id}, DELETE /roles/{id}
- [ ] T041 [US1] Implement POST /roles/import for Excel upload and preview
- [ ] T042 [US1] Implement batch create logic with duplicate skip in `FamilyRoleService.cs`
- [ ] T043 [P] [US1] Create `CategoryController.cs` and `TagController.cs` in `src/FreeAxez.FamilyLibrary.Api/Controllers/`

### Frontend - US1

- [ ] T044 [P] [US1] Create `roles.service.ts` in `src/FreeAxez.FamilyLibrary.Web/src/app/features/roles/services/`
- [ ] T045 [P] [US1] Create `role-list.component.ts` using p-table in `src/FreeAxez.FamilyLibrary.Web/src/app/features/roles/components/`
- [ ] T046 [US1] Create `role-editor.component.ts` using p-dialog for create/edit
- [ ] T047 [US1] Create `role-import.component.ts` with p-fileUpload for Excel import
- [ ] T048 [US1] Add roles routes to `src/FreeAxez.FamilyLibrary.Web/src/app/app.routes.ts`
- [ ] T049 [US1] Create roles state management using Signals in `roles.store.ts`

### Integration - US1

- [ ] T050 [US1] Wire frontend role-list to backend API
- [ ] T051 [US1] Test: Create role → verify read-only Name → delete role with families attached (should fail)

**Checkpoint**: US1 complete - Roles CRUD + Excel import working

---

## Phase 4: User Story 2 - Configure Name Recognition Rules (Priority: P1)

**Goal**: Administrators can configure recognition rules with Visual and Formula editors

**Independent Test**: Create rule "(FB OR Desk) AND Wired" → test on "FB_Field_Wired_v2" → matches

### Backend - US2

- [ ] T052 [P] [US2] Create `RecognitionRuleController.cs` in `src/FreeAxez.FamilyLibrary.Api/Controllers/`
- [ ] T053 [P] [US2] Create `RecognitionRuleService.cs` with formula parser in `src/FreeAxez.FamilyLibrary.Api/Services/`
- [ ] T054 [US2] Implement CRUD endpoints: GET/POST/PUT/DELETE /recognition-rules
- [ ] T055 [US2] Implement POST /recognition-rules/validate for syntax validation
- [ ] T056 [US2] Implement POST /recognition-rules/test for testing rule against family name
- [ ] T057 [US2] Implement POST /recognition-rules/check-conflicts for conflict detection

### Frontend - US2

- [ ] T058 [P] [US2] Create `rules.service.ts` in `src/FreeAxez.FamilyLibrary.Web/src/app/features/roles/services/`
- [ ] T059 [US2] Create `rule-editor.component.ts` with tab view (Visual mode, Formula mode)
- [ ] T060 [US2] Create `rule-visual-builder.component.ts` with recursive tree for conditions
- [ ] T061 [US2] Create `rule-test-dialog.component.ts` using p-dialog for testing
- [ ] T062 [US2] Implement formula ↔ visual sync logic
- [ ] T063 [US2] Add conflict warnings display using p-messages

### Integration - US2

- [ ] T064 [US2] Wire rule editor to backend API
- [ ] T065 [US2] Test: Create rule → test on name → verify result → check conflicts

**Checkpoint**: US2 complete - Recognition rules working with Visual + Formula editors

---

## Phase 5: User Story 3 - Manage Loadable Families in Template (Priority: P1)

**Goal**: BIM Manager can see families from template, stamp and publish to library

**Independent Test**: Open template → select family → choose role → Stamp → Publish → family in library

### Plugin - US3 (Core)

- [ ] T066 [P] [US3] Create `StampFamilyCommand/` structure in `src/FreeAxez.FamilyLibrary.Plugin/Commands/`
- [ ] T067 [US3] Create `FamilyScannerService.cs` in `src/FreeAxez.FamilyLibrary.Plugin/Commands/StampFamilyCommand/Services/`
- [ ] T068 [US3] Create `StampService.cs` implementing ES write logic
- [ ] T069 [US3] Create `PublishService.cs` with Blob upload + API integration
- [ ] T070 [US3] Create `LibraryQueueViewModel.cs` for Tab 2 state management
- [ ] T071 [US3] Create `LibraryQueueView.xaml` with 3 tabs (All Families, Queue, Status)

### Backend - US3

- [ ] T072 [P] [US3] Create `FamilyController.cs` in `src/FreeAxez.FamilyLibrary.Api/Controllers/`
- [ ] T073 [P] [US3] Create `FamilyService.cs` in `src/FreeAxez.FamilyLibrary.Api/Services/`
- [ ] T074 [US3] Implement POST /families/publish with file upload
- [ ] T075 [US3] Implement POST /families/validate-hash for duplicate detection
- [ ] T076 [US3] Implement POST /families/batch-check for status checking
- [ ] T077 [P] [US3] Create `DraftController.cs` in `src/FreeAxez.FamilyLibrary.Api/Controllers/`
- [ ] T078 [US3] Implement Draft CRUD: GET/POST/PUT/DELETE /drafts

### Frontend - US3

- [ ] T079 [P] [US3] Create `queue.service.ts` in `src/FreeAxez.FamilyLibrary.Web/src/app/features/queue/services/`
- [ ] T080 [US3] Create `queue.component.ts` with p-tabView for 3 tabs
- [ ] T081 [US3] Create `family-list.component.ts` using p-table with virtual scroll for Tab 1
- [ ] T082 [US3] Create `draft-list.component.ts` with status badges for Tab 2
- [ ] T083 [US3] Create `library-status.component.ts` for Tab 3

### WebView2 Integration - US3

- [ ] T084 [US3] Implement event handlers: `revit:ready`, `revit:families:list` in Plugin
- [ ] T085 [US3] Implement event handlers: `ui:stamp`, `ui:publish` in Plugin
- [ ] T086 [US3] Create `RevitBridgeService` in Frontend for WebView2 communication

### Integration - US3

- [ ] T087 [US3] Test: Open template → scan families → select → stamp → publish → verify in library

**Checkpoint**: US3 complete - Loadable families Stamp and Publish working

---

## Phase 6: User Story 4 - Manage System Families (Priority: P2)

**Goal**: BIM Manager can manage WallType, FloorType, etc. with JSON serialization

**Independent Test**: Create role for WallType → Stamp → Publish → JSON saved in database

### Backend - US4

- [ ] T088 [P] [US4] Create `SystemTypeController.cs` in `src/FreeAxez.FamilyLibrary.Api/Controllers/`
- [ ] T089 [P] [US4] Create `SystemTypeService.cs` in `src/FreeAxez.FamilyLibrary.Api/Services/`
- [ ] T090 [US4] Implement CRUD: GET/POST /system-types with JSON storage
- [ ] T091 [US4] Implement CompoundStructure JSON serialization for Group A
- [ ] T092 [US4] Implement simple parameter JSON serialization for Group E

### Plugin - US4

- [ ] T093 [P] [US4] Create `SystemTypeScannerService.cs` in `src/FreeAxez.FamilyLibrary.Plugin/Commands/StampFamilyCommand/Services/`
- [ ] T094 [US4] Create `CompoundStructureSerializer.cs` for WallType/FloorType/RoofType
- [ ] T095 [US4] Create `SystemTypePublisher.cs` for JSON upload
- [ ] T096 [US4] Implement material mapping warning dialog in UI

### Frontend - US4

- [ ] T097 [P] [US4] Add System Types tab to queue component
- [ ] T098 [US4] Create `system-type-detail.component.ts` showing JSON structure
- [ ] T099 [US4] Create `material-warning-dialog.component.ts` for missing materials

### Integration - US4

- [ ] T100 [US4] Test: Stamp WallType → Publish → Pull Update → verify structure applied

**Checkpoint**: US4 complete - System Families Groups A, E working

---

## Phase 7: User Story 5 - Browse Family Library (Priority: P1)

**Goal**: Designers can browse library inside Revit with cards/table views, filters

**Independent Test**: Open library → search by name → open detail page → see version history

### Backend - US5

- [ ] T101 [US5] Implement GET /families with search, filters, pagination
- [ ] T102 [P] [US5] Implement GET /families/{id} with versions and types
- [ ] T103 [P] [US5] Implement GET /families/{id}/versions for history

### Frontend - US5

- [ ] T104 [P] [US5] Create `library.service.ts` in `src/FreeAxez.FamilyLibrary.Web/src/app/features/library/services/`
- [ ] T105 [US5] Create `library.component.ts` with view toggle (cards/table)
- [ ] T106 [US5] Create `family-card.component.ts` using p-card for grid view
- [ ] T107 [US5] Create `family-table.component.ts` using p-table with virtual scroll
- [ ] T108 [US5] Create `library-filters.component.ts` with p-dropdown, p-multiSelect
- [ ] T109 [US5] Create `family-detail.component.ts` with version table
- [ ] T110 [US5] Add library routes and navigation

### Plugin - US5

- [ ] T111 [US5] Create `OpenLibraryCommand.cs` in `src/FreeAxez.FamilyLibrary.Plugin/Commands/`
- [ ] T112 [US5] Wire OpenLibraryCommand to WebView2 host with library URL

### Integration - US5

- [ ] T113 [US5] Test: Open library in Revit → search → filter → view details

**Checkpoint**: US5 complete - Library browsing working inside Revit

---

## Phase 8: User Story 6 - Load Family to Project (Priority: P1)

**Goal**: Designers can load families from library to current project

**Independent Test**: Find family → Load → family loaded with original filename

### Backend - US6

- [ ] T114 [US6] Implement GET /families/{id}/download/{version} returning SAS token or file
- [ ] T115 [P] [US6] Implement status check integration with batch-check endpoint

### Plugin - US6

- [ ] T116 [P] [US6] Create `LoadFamilyCommand/` structure in `src/FreeAxez.FamilyLibrary.Plugin/Commands/`
- [ ] T117 [US6] Create `FamilyDownloader.cs` in `src/FreeAxez.FamilyLibrary.Plugin/Commands/LoadFamilyCommand/Services/`
- [ ] T118 [US6] Create `FamilyLoader.cs` wrapping Revit LoadFamily() API
- [ ] T119 [US6] Implement file rename to OriginalFileName logic

### Frontend - US6

- [ ] T120 [US6] Add "Load to Project" button to family-detail component
- [ ] T121 [US6] Wire button to `ui:load-family` WebView2 event

### Integration - US6

- [ ] T122 [US6] Test: Select family → Load → verify loaded with correct name

**Checkpoint**: US6 complete - Family loading working

---

## Phase 9: User Story 7 - Stamp/Publish from Family Editor (Priority: P2)

**Goal**: BIM Manager can publish directly from Family Editor without opening template

**Independent Test**: Open family in Family Editor → Publish command → UI shows only Tab 2

### Plugin - US7

- [ ] T123 [US7] Create `PublishFromEditorCommand.cs` detecting Document.Kind == FamilyDocument
- [ ] T124 [US7] Modify LibraryQueueView to hide Tab 1 and Tab 3 in Family Editor mode
- [ ] T125 [US7] Auto-add current family to Queue when in Family Editor

### Integration - US7

- [ ] T126 [US7] Test: Open family in editor → run command → publish → verify in library

**Checkpoint**: US7 complete - Publish from Family Editor working

---

## Phase 10: User Story 8 - Type Catalogs Management (Priority: P2)

**Goal**: BIM Manager can attach TXT files to families; Designers can select types

**Independent Test**: Publish with TXT → Load → type selection dialog appears

### Backend - US8

- [ ] T127 [US8] Modify /families/publish to accept optional txtFile parameter
- [ ] T128 [US8] Implement TXT hash calculation for version tracking

### Plugin - US8

- [ ] T129 [P] [US8] Create `TypeCatalogParser.cs` in `src/FreeAxez.FamilyLibrary.Plugin/Commands/LoadFamilyCommand/Services/`
- [ ] T130 [US8] Create `TypeSelectionWindow.xaml` with dynamic columns from TXT headers
- [ ] T131 [US8] Create `TypeSelectionViewModel.cs` with search, filter, multi-select
- [ ] T132 [US8] Implement LoadFamilySymbol for each selected type

### Frontend - US8

- [ ] T133 [US8] Add type table to family-detail component when catalog exists
- [ ] T134 [US8] Show type selection preview in load dialog

### Integration - US8

- [ ] T135 [US8] Test: Publish with TXT → Load → select 3 types → verify only 3 loaded

**Checkpoint**: US8 complete - Type Catalogs working

---

## Phase 11: Polish & Cross-Cutting Concerns

**Purpose**: Improvements affecting multiple user stories

### Documentation

- [ ] T136 [P] Update quickstart.md with docker-compose instructions
- [ ] T137 [P] Add OpenAPI codegen instructions to quickstart.md
- [ ] T138 [P] Create API usage examples in `docs/api-examples.md`

### Performance

- [ ] T139 Optimize batch-check endpoint for 500+ families
- [ ] T140 [P] Add database indexes per data-model.md specifications
- [ ] T141 Test virtual scroll performance with 5000+ rows

### Security

- [ ] T142 [P] Add input validation on all endpoints
- [ ] T143 [P] Configure CORS for production domains
- [ ] T144 Add rate limiting middleware

### Error Handling

- [ ] T145 [P] Implement global exception handler in Backend
- [ ] T146 [P] Add user-friendly error messages in Frontend (p-toast)
- [ ] T147 Implement retry logic with exponential backoff in Plugin

### Testing

- [ ] T148 [P] Add Backend unit tests for FamilyRoleService
- [ ] T149 [P] Add Backend unit tests for RecognitionRuleService
- [ ] T150 [P] Add Frontend tests for roles feature
- [ ] T151 Run quickstart.md validation end-to-end

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
# Can run in parallel:
T003 + T004 + T005  # Backend NuGet, Program.cs, appsettings
T007 + T008 + T009 + T010  # Frontend packages, Tailwind, config
T012 + T013  # Plugin NuGet, manifest
T016 + T017  # CI, editorconfig
```

### Within Foundational Phase
```bash
# Can run in parallel:
T019 + T020  # All entities + all DTOs
T022 + T023  # Repository interfaces + implementations
T028 + T029  # Interceptors + TypeScript generation
T033 + T035  # Plugin interfaces + ES service
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
| Phase 1: Setup | 17 | 12 tasks |
| Phase 2: Foundational | 20 | 10 tasks |
| Phase 3: US1 Roles | 14 | 5 tasks |
| Phase 4: US2 Rules | 14 | 4 tasks |
| Phase 5: US3 Loadable | 22 | 6 tasks |
| Phase 6: US4 System | 13 | 4 tasks |
| Phase 7: US5 Library | 13 | 4 tasks |
| Phase 8: US6 Load | 9 | 2 tasks |
| Phase 9: US7 Editor | 4 | 0 tasks |
| Phase 10: US8 Catalog | 9 | 2 tasks |
| Phase 11: Polish | 16 | 10 tasks |
| **TOTAL** | **151** | **59 tasks** |

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps task to user story for traceability
- Each user story should be independently testable
- Commit after each task or logical group
- Stop at any checkpoint to validate independently
