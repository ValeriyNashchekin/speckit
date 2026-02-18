# Tasks: Family Library Phase 3

**Input**: Design documents from `/specs/003-family-library-phase3/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/
**Depends On**: `002-family-library-phase2` (completed)

**Tests**: Not explicitly requested - implementation tasks only.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

---

## Entity Coverage Matrix

| Entity | Role | API Endpoints | UI Exposure | Story |
|--------|------|---------------|-------------|-------|
| FamilyDependency | Primary | GET dependencies, GET used-in | DependenciesList, UsedInList | US1, US5 |
| MaterialMapping | Primary | Full CRUD | List, Editor | US4 |
| NestedFamilyInfo | Supporting | N/A (DTO) | PreLoadSummary | US2, US6 |
| RailingType | Supporting | N/A (serialized) | — | US3 |
| CurtainWallType | Supporting | N/A (serialized) | — | US3 |
| StackedWallType | Supporting | N/A (serialized) | — | US3 |

**Role**: Primary = main focus of story, Supporting = referenced by primary
**UI Exposure**: CRUD = full management UI, List = display only, — = backend only

---

## Format: `[ID] [P?] [Story] [Component?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: US1-US6 (maps to user stories from spec.md)
- **[Component]**: [BACKEND], [FRONTEND], [PLUGIN]
- Include exact file paths in descriptions

---

## Path Conventions

Based on plan.md structure:
- **Backend**: `src/FamilyLibrary.Api/`, `src/FamilyLibrary.Domain/`, `src/FamilyLibrary.Infrastructure/`
- **Frontend**: `src/FamilyLibrary.Web/src/app/`
- **Plugin**: `src/FamilyLibrary.Plugin/`
- **Tests**: `tests/FamilyLibrary.Application.Tests/`, `tests/FamilyLibrary.Plugin.Tests/`

---

## Phase 1: Setup (Database Migration)

**Purpose**: Add Phase 3 entities to database

- [X] T001 [BACKEND] Create FamilyDependency entity in `src/FamilyLibrary.Domain/Entities/FamilyDependencyEntity.cs` → Artifacts: [FamilyDependencyEntity.cs](src/FamilyLibrary.Api/FamilyLibrary.Domain/Entities/FamilyDependencyEntity.cs)
- [X] T002 [P] [BACKEND] Create MaterialMapping entity in `src/FamilyLibrary.Domain/Entities/MaterialMappingEntity.cs` → Artifacts: [MaterialMappingEntity.cs](src/FamilyLibrary.Api/FamilyLibrary.Domain/Entities/MaterialMappingEntity.cs)
- [X] T003 [BACKEND] Create FamilyDependencyConfiguration in `src/FamilyLibrary.Infrastructure/Data/Configurations/FamilyDependencyConfiguration.cs` → Artifacts: [FamilyDependencyConfiguration.cs](src/FamilyLibrary.Api/FamilyLibrary.Infrastructure/Data/Configurations/FamilyDependencyConfiguration.cs)
- [X] T004 [P] [BACKEND] Create MaterialMappingConfiguration in `src/FamilyLibrary.Infrastructure/Data/Configurations/MaterialMappingConfiguration.cs` → Artifacts: [MaterialMappingConfiguration.cs](src/FamilyLibrary.Api/FamilyLibrary.Infrastructure/Data/Configurations/MaterialMappingConfiguration.cs)
- [X] T005 [BACKEND] Add FamilyDependencies and MaterialMappings to DbContext in `src/FamilyLibrary.Infrastructure/Data/AppDbContext.cs`
- [X] T006 [BACKEND] Create migration AddPhase3Entities in `src/FamilyLibrary.Infrastructure/Data/Migrations/`

**Checkpoint**: Database schema updated, migration ready to apply

---

## Phase 2: Foundational (Shared Services)

**Purpose**: Core services that multiple user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T007 [BACKEND] Create NestedFamilyDto in `src/FamilyLibrary.Application/DTOs/NestedFamilyDto.cs` → Artifacts: [NestedFamilyDto.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/DTOs/NestedFamilyDto.cs)
- [X] T008 [P] [BACKEND] Create PreLoadSummaryDto in `src/FamilyLibrary.Application/DTOs/PreLoadSummaryDto.cs` → Artifacts: [PreLoadSummaryDto.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/DTOs/PreLoadSummaryDto.cs)
- [X] T009 [P] [BACKEND] Create UsedInDto in `src/FamilyLibrary.Application/DTOs/UsedInDto.cs` → Artifacts: [UsedInDto.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/DTOs/UsedInDto.cs)
- [X] T010 [P] [BACKEND] Create MaterialMappingDto in `src/FamilyLibrary.Application/DTOs/MaterialMappingDto.cs` → Artifacts: [MaterialMappingDto.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/DTOs/MaterialMappingDto.cs)
- [X] T011 [BACKEND] Create NestedFamilyService in `src/FamilyLibrary.Application/Services/NestedFamilyService.cs` → Artifacts: [INestedFamilyService.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/Interfaces/INestedFamilyService.cs), [NestedFamilyService.cs](src/FamilyLibrary.Api/FamilyLibrary.Infrastructure/Services/NestedFamilyService.cs)
- [X] T012 [BACKEND] Create MaterialMappingService in `src/FamilyLibrary.Application/Services/MaterialMappingService.cs` → Artifacts: [IMaterialMappingService.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/Interfaces/IMaterialMappingService.cs), [MaterialMappingService.cs](src/FamilyLibrary.Api/FamilyLibrary.Infrastructure/Services/MaterialMappingService.cs)
- [X] T013 [PLUGIN] Create NestedFamilyInfo model in `src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Models/NestedFamilyInfo.cs` → Artifacts: [NestedFamilyInfo.cs](src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Models/NestedFamilyInfo.cs)
- [X] T014 [PLUGIN] Create NestedDetectionService in `src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Services/NestedDetectionService.cs` → Artifacts: [NestedDetectionService.cs](src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Services/NestedDetectionService.cs)
- [X] T015 [PLUGIN] Create NestedFamilyLoadOptions in `src/FamilyLibrary.Plugin/Services/NestedFamilyLoadOptions.cs` → Artifacts: [NestedFamilyLoadOptions.cs](src/FamilyLibrary.Plugin/Services/NestedFamilyLoadOptions.cs), [NestedLoadChoice.cs](src/FamilyLibrary.Plugin/Services/NestedLoadChoice.cs)
- [X] T016 [PLUGIN] Create NestedFamilyLoadService in `src/FamilyLibrary.Plugin/Services/NestedFamilyLoadService.cs` → Artifacts: [NestedFamilyLoadService.cs](src/FamilyLibrary.Plugin/Services/NestedFamilyLoadService.cs)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Manage Nested Families (Priority: P1) 🎯 MVP

**Goal**: БИМ-менеджер видит вложенные семейства при Publish

**Independent Test**: При Publish родительского семейства показывается список nested с их статусами

**Primary Entity**: FamilyDependency
**Supporting Entities**: Family (existing)

### Backend - US1

- [X] T017 [US1] [BACKEND] Add GetDependenciesAsync to NestedFamilyService in `src/FamilyLibrary.Application/Services/NestedFamilyService.cs` → Artifacts: [NestedFamilyService.cs](src/FamilyLibrary.Api/FamilyLibrary.Infrastructure/Services/NestedFamilyService.cs)
- [X] T018 [US1] [BACKEND] Add POST /api/families/{id}/dependencies endpoint in `src/FamilyLibrary.Api/Controllers/FamiliesController.cs` → Artifacts: [FamiliesController.cs](src/FamilyLibrary.Api/FamilyLibrary.Api/Controllers/FamiliesController.cs)

### Plugin - US1

- [X] T019 [US1] [PLUGIN] Integrate NestedDetectionService into StampFamilyCommand flow in `src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/` → Artifacts: [PublishService.cs](src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Services/PublishService.cs)
- [X] T020 [US1] [PLUGIN] Send revit:nested:detected event after detection in `src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/` → Artifacts: [NestedDetectedEvent.cs](src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Models/NestedDetectedEvent.cs), [LibraryQueueViewModel.cs](src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/ViewModels/LibraryQueueViewModel.cs)
- [X] T021 [US1] [PLUGIN] Save FamilyDependencies to API after detection in `src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Services/` → Artifacts: [FamiliesController.cs](src/FamilyLibrary.Api/FamilyLibrary.Api/Controllers/FamiliesController.cs), [NestedFamilyService.cs](src/FamilyLibrary.Api/FamilyLibrary.Infrastructure/Services/NestedFamilyService.cs)

### Frontend - US1

- [X] T022 [P] [US1] [FRONTEND] Create nested-family.service.ts in `src/FamilyLibrary.Web/src/app/core/services/` → Artifacts: [nested-family.service.ts](src/FamilyLibrary.Web/src/app/core/services/nested-family.service.ts)
- [X] T023 [US1] [FRONTEND] Create dependencies-list component in `src/FamilyLibrary.Web/src/app/features/library/components/dependencies-list/` → Artifacts: [dependencies-list.component.ts](src/FamilyLibrary.Web/src/app/features/library/components/dependencies-list/dependencies-list.component.ts), [dependencies-list.component.html](src/FamilyLibrary.Web/src/app/features/library/components/dependencies-list/dependencies-list.component.html)
- [X] T024 [US1] [FRONTEND] Handle revit:nested:detected event in Queue component in `src/FamilyLibrary.Web/src/app/features/queue/components/` → Artifacts: [queue.component.ts](src/FamilyLibrary.Web/src/app/features/queue/components/queue/queue.component.ts), [revit-bridge.service.ts](src/FamilyLibrary.Web/src/app/core/services/revit-bridge.service.ts)
- [X] T025 [US1] [FRONTEND] Show warning badge when Shared nested not published in `src/FamilyLibrary.Web/src/app/features/queue/components/` → Artifacts: [queue.component.html](src/FamilyLibrary.Web/src/app/features/queue/components/queue/queue.component.html)

**Checkpoint**: При Publish показывается список вложенных семейств со статусами

---

## Phase 4: User Story 2 - Load Family with Nested (Priority: P1) 🎯 MVP

**Goal**: Проектировщик загружает семейство со всеми вложенными

**Independent Test**: Pre-Load Summary показывает все nested и их версии, загрузка работает корректно

**Primary Entity**: NestedFamilyInfo (DTO)
**Supporting Entities**: FamilyDependency

### Backend - US2

- [X] T026 [US2] [BACKEND] Add GetPreLoadSummaryAsync to NestedFamilyService in `src/FamilyLibrary.Application/Services/NestedFamilyService.cs` → Artifacts: [NestedFamilyService.cs](src/FamilyLibrary.Api/FamilyLibrary.Infrastructure/Services/NestedFamilyService.cs)
- [X] T027 [US2] [BACKEND] Add GET /api/families/{id}/pre-load-summary endpoint in `src/FamilyLibrary.Api/Controllers/FamiliesController.cs` → Artifacts: [FamiliesController.cs](src/FamilyLibrary.Api/FamilyLibrary.Api/Controllers/FamiliesController.cs)

### Plugin - US2

- [X] T028 [US2] [PLUGIN] Implement PreLoadSummary logic in NestedFamilyLoadService in `src/FamilyLibrary.Plugin/Services/NestedFamilyLoadService.cs` → Artifacts: [NestedFamilyLoadService.cs](src/FamilyLibrary.Plugin/Services/NestedFamilyLoadService.cs)
- [X] T029 [US2] [PLUGIN] Send revit:load:preview event before load in `src/FamilyLibrary.Plugin/Services/NestedFamilyLoadService.cs` → Artifacts: [NestedFamilyLoadService.cs](src/FamilyLibrary.Plugin/Services/NestedFamilyLoadService.cs), [RevitBridge.cs](src/FamilyLibrary.Plugin/Infrastructure/WebView2/RevitBridge.cs)
- [X] T030 [US2] [PLUGIN] Handle ui:load-with-nested event from UI in `src/FamilyLibrary.Plugin/Services/NestedFamilyLoadService.cs` → Artifacts: [NestedFamilyLoadService.cs](src/FamilyLibrary.Plugin/Services/NestedFamilyLoadService.cs)
- [X] T031 [US2] [PLUGIN] Implement two-phase load (parent first, then override nested) in `src/FamilyLibrary.Plugin/Services/NestedFamilyLoadService.cs` → Artifacts: [NestedFamilyLoadService.cs](src/FamilyLibrary.Plugin/Services/NestedFamilyLoadService.cs)

### Frontend - US2

- [X] T032 [P] [US2] [FRONTEND] Create pre-load-summary component in `src/FamilyLibrary.Web/src/app/features/scanner/components/pre-load-summary/` → Artifacts: [pre-load-summary.component.ts](src/FamilyLibrary.Web/src/app/features/scanner/components/pre-load-summary/pre-load-summary.component.ts), [pre-load-summary.component.html](src/FamilyLibrary.Web/src/app/features/scanner/components/pre-load-summary/pre-load-summary.component.html)
- [X] T033 [US2] [FRONTEND] Handle revit:load:preview event in Library component in `src/FamilyLibrary.Web/src/app/features/library/components/` → Artifacts: [family-detail.component.ts](src/FamilyLibrary.Web/src/app/features/library/components/family-detail/family-detail.component.ts)
- [X] T034 [US2] [FRONTEND] Send ui:load-with-nested event with user choices in `src/FamilyLibrary.Web/src/app/features/scanner/components/pre-load-summary/` → Artifacts: [pre-load-summary.component.ts](src/FamilyLibrary.Web/src/app/features/scanner/components/pre-load-summary/pre-load-summary.component.ts)
- [X] T035 [US2] [FRONTEND] Show version comparison (RFA vs Library vs Project) in `src/FamilyLibrary.Web/src/app/features/scanner/components/pre-load-summary/` → Artifacts: [pre-load-summary.component.html](src/FamilyLibrary.Web/src/app/features/scanner/components/pre-load-summary/pre-load-summary.component.html)

**Checkpoint**: Pre-Load Summary работает, загрузка с контролем версий nested работает

---

## Phase 5: User Story 6 - Resolve Nested Version Conflicts (Priority: P1)

**Goal**: Проектировщик контролирует версию вложенных при загрузке

**Independent Test**: При конфликте версий пользователь может выбрать источник для каждого nested

**Primary Entity**: NestedLoadChoice (in NestedFamilyLoadOptions)
**Supporting Entities**: PreLoadSummaryDto

### Frontend - US6

- [ ] T036 [US6] [FRONTEND] Add per-nested action selector in pre-load-summary in `src/FamilyLibrary.Web/src/app/features/scanner/components/pre-load-summary/`
- [ ] T037 [US6] [FRONTEND] Show recommended action per nested family in `src/FamilyLibrary.Web/src/app/features/scanner/components/pre-load-summary/`
- [ ] T038 [US6] [FRONTEND] Add "Load All" / "Load Selected" / "Cancel" buttons in `src/FamilyLibrary.Web/src/app/features/scanner/components/pre-load-summary/`
- [ ] T039 [US6] [FRONTEND] Send individual choices per nested in ui:load-with-nested in `src/FamilyLibrary.Web/src/app/features/scanner/components/pre-load-summary/`

### Plugin - US6

- [ ] T040 [US6] [PLUGIN] Process individual NestedLoadChoice per family in NestedFamilyLoadOptions in `src/FamilyLibrary.Plugin/Services/NestedFamilyLoadOptions.cs`
- [ ] T041 [US6] [PLUGIN] Download and load library versions for selected nested in `src/FamilyLibrary.Plugin/Services/NestedFamilyLoadService.cs`

**Checkpoint**: Конфликты версий разрешаются пользователем для каждого nested отдельно

---

## Phase 6: User Story 5 - See "Used In" for Nested Families (Priority: P2)

**Goal**: БИМ-менеджер видит где вложенное семейство используется

**Independent Test**: На странице nested семейства показывается список родительских

**Primary Entity**: FamilyDependency (reverse query)

### Backend - US5

- [ ] T042 [US5] [BACKEND] Add GetUsedInAsync to NestedFamilyService in `src/FamilyLibrary.Application/Services/NestedFamilyService.cs`
- [ ] T043 [US5] [BACKEND] Add GET /api/families/{id}/used-in endpoint in `src/FamilyLibrary.Api/Controllers/FamiliesController.cs`

### Frontend - US5

- [ ] T044 [P] [US5] [FRONTEND] Create used-in-list component in `src/FamilyLibrary.Web/src/app/features/library/components/used-in-list/`
- [ ] T045 [US5] [FRONTEND] Add "Used In" section to family detail page in `src/FamilyLibrary.Web/src/app/features/library/components/family-detail/`
- [ ] T046 [US5] [FRONTEND] Show warning if parent has old nested version in `src/FamilyLibrary.Web/src/app/features/library/components/used-in-list/`
- [ ] T047 [US5] [FRONTEND] Add link to parent family on click in `src/FamilyLibrary.Web/src/app/features/library/components/used-in-list/`

**Checkpoint**: "Used In" показывает все родительские семейства для nested

---

## Phase 7: User Story 3 - Manage Complex System Families (Priority: P2)

**Goal**: БИМ-менеджер управляет Railings и Curtain Walls в библиотеке

**Independent Test**: RailingType и CurtainWallType сериализуются с dependencies, Pull Update проверяет зависимости

**Primary Entities**: RailingType, CurtainWallType, StackedWallType

### Plugin - US3 (Serializers)

- [ ] T048 [P] [US3] [PLUGIN] Create RailingSerializer in `src/FamilyLibrary.Plugin/Services/RailingSerializer.cs`
- [ ] T049 [P] [US3] [PLUGIN] Create CurtainSerializer in `src/FamilyLibrary.Plugin/Services/CurtainSerializer.cs`
- [ ] T050 [P] [US3] [PLUGIN] Create StackedWallSerializer in `src/FamilyLibrary.Plugin/Services/StackedWallSerializer.cs`

### Plugin - US3 (Integration)

- [ ] T051 [US3] [PLUGIN] Register Group C serializers in SystemFamilySerializerFactory in `src/FamilyLibrary.Plugin/Services/`
- [ ] T052 [US3] [PLUGIN] Register Group D serializers in SystemFamilySerializerFactory in `src/FamilyLibrary.Plugin/Services/`
- [ ] T053 [US3] [PLUGIN] Add dependency validation at Pull Update for Group C in `src/FamilyLibrary.Plugin/Commands/PullUpdateCommand/`
- [ ] T054 [US3] [PLUGIN] Add dependency validation at Pull Update for Group D in `src/FamilyLibrary.Plugin/Commands/PullUpdateCommand/`
- [ ] T055 [US3] [PLUGIN] Show error if StackedWall child types missing in `src/FamilyLibrary.Plugin/Commands/PullUpdateCommand/`

**Checkpoint**: Railings, Curtain Walls, Stacked Walls сериализуются и Pull Update проверяет зависимости

---

## Phase 8: User Story 4 - Auto Material Mapping (Priority: P2)

**Goal**: БИМ-менеджер настраивает маппинг материалов для автоматической замены

**Independent Test**: Material Mapping работает при Pull Update, CRUD UI работает

**Primary Entity**: MaterialMapping

### Backend - US4

- [ ] T056 [US4] [BACKEND] Create MaterialMappingsController in `src/FamilyLibrary.Api/Controllers/MaterialMappingsController.cs`
- [ ] T057 [US4] [BACKEND] Implement ListAsync (filter by projectId) in MaterialMappingService in `src/FamilyLibrary.Application/Services/MaterialMappingService.cs`
- [ ] T058 [US4] [BACKEND] Implement CreateAsync in MaterialMappingService in `src/FamilyLibrary.Application/Services/MaterialMappingService.cs`
- [ ] T059 [US4] [BACKEND] Implement UpdateAsync in MaterialMappingService in `src/FamilyLibrary.Application/Services/MaterialMappingService.cs`
- [ ] T060 [US4] [BACKEND] Implement DeleteAsync with confirmation check in MaterialMappingService in `src/FamilyLibrary.Application/Services/MaterialMappingService.cs`
- [ ] T061 [US4] [BACKEND] Implement LookupAsync for Pull Update in MaterialMappingService in `src/FamilyLibrary.Application/Services/MaterialMappingService.cs`

### Plugin - US4

- [ ] T062 [US4] [PLUGIN] Create MaterialMappingClient in `src/FamilyLibrary.Plugin/Services/MaterialMappingClient.cs`
- [ ] T063 [US4] [PLUGIN] Integrate mapping lookup in Pull Update flow in `src/FamilyLibrary.Plugin/Commands/PullUpdateCommand/`
- [ ] T064 [US4] [PLUGIN] Send revit:material:fallback event when mapping not found in `src/FamilyLibrary.Plugin/Commands/PullUpdateCommand/`
- [ ] T065 [US4] [PLUGIN] Handle ui:material-mapping:save event from UI in `src/FamilyLibrary.Plugin/Services/MaterialMappingClient.cs`

### Frontend - US4

- [ ] T066 [P] [US4] [FRONTEND] Create material-mapping.service.ts in `src/FamilyLibrary.Web/src/app/core/services/`
- [ ] T067 [US4] [FRONTEND] Create material-mappings-list component in `src/FamilyLibrary.Web/src/app/features/settings/components/material-mappings/`
- [ ] T068 [US4] [FRONTEND] Create material-mapping-editor dialog in `src/FamilyLibrary.Web/src/app/features/settings/components/material-mappings/`
- [ ] T069 [US4] [FRONTEND] Add Material Mappings route to Settings in `src/FamilyLibrary.Web/src/app/features/settings/`
- [ ] T070 [US4] [FRONTEND] Create material-fallback-dialog component in `src/FamilyLibrary.Web/src/app/features/scanner/components/`
- [ ] T071 [US4] [FRONTEND] Handle revit:material:fallback event in scanner in `src/FamilyLibrary.Web/src/app/features/scanner/`
- [ ] T072 [US4] [FRONTEND] Send ui:material-mapping:save with "Remember" option in `src/FamilyLibrary.Web/src/app/features/scanner/components/material-fallback-dialog/`

**Checkpoint**: Material Mapping работает автоматически при Pull Update, CRUD UI работает

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T073 [P] [FRONTEND] Add 🔗 badge for families with dependencies in Library list in `src/FamilyLibrary.Web/src/app/features/library/components/`
- [ ] T074 [P] [FRONTEND] Add tooltip "Contains X nested Shared families" in `src/FamilyLibrary.Web/src/app/features/library/components/`
- [ ] T075 [BACKEND] Add IX_FamilyDependency_NestedRoleName index if not exists in migration
- [ ] T076 [P] [BACKEND] Update API documentation for new endpoints
- [ ] T077 [PLUGIN] Add NestedFamilyLoadOptions unit tests in `tests/FamilyLibrary.Plugin.Tests/`
- [ ] T078 [P] [PLUGIN] Add RailingSerializer tests in `tests/FamilyLibrary.Plugin.Tests/`
- [ ] T079 [P] [PLUGIN] Add CurtainSerializer tests in `tests/FamilyLibrary.Plugin.Tests/`
- [ ] T080 [P] [BACKEND] Add NestedFamilyService tests in `tests/FamilyLibrary.Application.Tests/`
- [ ] T081 [P] [BACKEND] Add MaterialMappingService tests in `tests/FamilyLibrary.Application.Tests/`
- [ ] T082 Run quickstart.md validation scenarios

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3-8)**: All depend on Foundational phase completion
  - US1, US2, US6 (P1) can proceed first
  - US3, US4, US5 (P2) can proceed in parallel or after P1
- **Polish (Phase 9)**: Depends on all user stories being complete

### User Story Dependencies

```
Foundational (Phase 2) complete
         │
         ├─────────────┬─────────────┬─────────────┐
         ▼             ▼             ▼             ▼
      US1 (P1)      US2 (P1)      US3 (P2)      US4 (P2)
    Nested       PreLoad      Complex      Material
    Detection    Summary      SystemFam    Mapping
         │             │             │             │
         ▼             │             ▼             ▼
      US5 (P2)         │          (uses)       (uses)
    Used In           │             │             │
         │             ▼             │             │
         └──────────► US6 (P1) ◄─────┘             │
                    Resolve                     │
                    Conflicts                    │
                         │                       │
                         └───────────────────────┘
                                    ▼
                              Polish (Phase 9)
```

### Within Each User Story

- Backend entities before services
- Services before controllers
- Plugin models before services
- Frontend services before components
- Core implementation before integration

### Parallel Opportunities

**Within Phase 1 (Setup)**:
- T001, T002 (entities) can run in parallel
- T003, T004 (configurations) can run in parallel

**Within Phase 2 (Foundational)**:
- T007-T010 (DTOs) can all run in parallel
- T013-T016 (Plugin models/services) can run in parallel

**Within Each User Story**:
- All [P] marked tasks can run in parallel
- Frontend and Plugin tasks can run in parallel with Backend

**Across User Stories**:
- US1, US2, US3, US4 can all start after Foundational (if staffed)
- US5 depends on US1 (uses same entities)
- US6 can start after US2 (extends PreLoadSummary)

---

## Parallel Example: User Story 1

```bash
# Launch DTOs together:
Task: "Create NestedFamilyDto"
Task: "Create PreLoadSummaryDto"
Task: "Create UsedInDto"
Task: "Create MaterialMappingDto"

# Launch Plugin models together:
Task: "Create NestedFamilyInfo model"
Task: "Create NestedDetectionService"
Task: "Create NestedFamilyLoadOptions"
Task: "Create NestedFamilyLoadService"

# Launch Frontend together:
Task: "Create nested-family.service.ts"
Task: "Create dependencies-list component"
```

---

## Parallel Example: User Story 3 (Complex System Families)

```bash
# Launch all serializers together:
Task: "Create RailingSerializer"
Task: "Create CurtainSerializer"
Task: "Create StackedWallSerializer"
```

---

## Parallel Example: User Story 4 (Material Mapping)

```bash
# Launch Frontend components together:
Task: "Create material-mapping.service.ts"
Task: "Create material-mappings-list component"
Task: "Create material-mapping-editor dialog"
Task: "Create material-fallback-dialog component"
```

---

## Implementation Strategy

### MVP First (User Stories 1, 2, 6 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL)
3. Complete Phase 3: US1 - Nested Detection
4. Complete Phase 4: US2 - Pre-Load Summary
5. Complete Phase 5: US6 - Version Conflicts
6. **STOP and VALIDATE**: Test nested families end-to-end
7. Deploy/demo if ready

### Incremental Delivery

1. Setup + Foundational → Foundation ready
2. Add US1 → Nested families visible at Publish
3. Add US2 → Pre-Load Summary works
4. Add US6 → Version conflicts resolved
5. Add US5 → "Used In" works
6. Add US3 → Complex System Families work
7. Add US4 → Material Mapping works
8. Polish → Tests, badges, documentation

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: US1 + US5 (Nested Families)
   - Developer B: US2 + US6 (Load with Nested)
   - Developer C: US3 (Complex System Families)
   - Developer D: US4 (Material Mapping)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story
- Each user story is independently completable and testable
- Tests are NOT included (not requested in spec)
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently

---

## Coverage Checklist

- [x] Every entity in data-model.md has backend tasks (FamilyDependency, MaterialMapping)
- [x] Every entity in acceptance criteria has frontend tasks
- [x] Every FK field has corresponding service/query
- [x] Every API endpoint has a consumer (frontend or plugin)
- [x] Entity Coverage Matrix is filled in
- [x] All tasks follow the checklist format (checkbox, ID, labels, file paths)
