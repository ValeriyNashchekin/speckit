# Tasks: Family Library Phase 2

**Input**: Design documents from `/specs/002-family-library-phase2/`
**Prerequisites**: MVP completed (`001-family-library-mvp`)

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Format: `[ID] [P?] [Story] [Component] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: US1-US5 (maps to user stories from spec.md)
- **[Component]**: BACKEND, FRONTEND, PLUGIN

---

## Phase 1: Foundational (Blocking Prerequisites)

**Purpose**: Shared infrastructure that MUST complete before user stories

- [X] T001 [P] [BACKEND] Create ChangeCategory enum → Artifacts: [ChangeCategory.cs](src/FamilyLibrary.Api/FamilyLibrary.Domain/Enums/ChangeCategory.cs)
- [X] T002 [P] [BACKEND] Create FamilyScanStatus enum → Artifacts: [FamilyScanStatus.cs](src/FamilyLibrary.Api/FamilyLibrary.Domain/Enums/FamilyScanStatus.cs)
- [X] T003 [P] [BACKEND] Create FamilySnapshot model → Artifacts: [FamilySnapshot.cs](src/FamilyLibrary.Api/FamilyLibrary.Domain/Entities/FamilySnapshot.cs)
- [X] T004 [P] [BACKEND] Create ChangeSet DTOs → Artifacts: [ChangeDetectionDtos.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/DTOs/ChangeDetectionDtos.cs)
- [X] T005 [P] [BACKEND] Create BatchCheckRequest/Response DTOs → Artifacts: [BatchCheckDtos.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/DTOs/BatchCheckDtos.cs)
- [X] T006 [P] [BACKEND] Create ScanResult DTOs → Artifacts: [ScanResultDtos.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/DTOs/ScanResultDtos.cs)
- [X] T007 [P] [FRONTEND] Add scanner models → Artifacts: [scanner.models.ts](src/FamilyLibrary.Web/src/app/core/models/scanner.models.ts)
- [X] T008 [P] [FRONTEND] Add Phase 2 WebView2 events → Artifacts: [webview-events.model.ts](src/FamilyLibrary.Web/src/app/core/models/webview-events.model.ts)
- [X] T009 [P] [PLUGIN] Create ChangeCategory enum → Artifacts: [ChangeCategory.cs](src/FamilyLibrary.Plugin/Core/Enums/ChangeCategory.cs)
- [X] T010 [P] [PLUGIN] Create FamilyScanStatus enum → Artifacts: [FamilyScanStatus.cs](src/FamilyLibrary.Plugin/Core/Enums/FamilyScanStatus.cs)
- [X] T011 [P] [PLUGIN] Create FamilySnapshot model → Artifacts: [FamilySnapshot.cs](src/FamilyLibrary.Plugin/Core/Models/FamilySnapshot.cs)
- [X] T012 [P] [PLUGIN] Create ChangeSet model → Artifacts: [ChangeSet.cs](src/FamilyLibrary.Plugin/Core/Models/ChangeSet.cs)

**Checkpoint**: Enums and DTOs ready - user story implementation can begin

---

## Phase 2: User Story 1 - Scan and Update Families (Priority: P1) 🎯 MVP

**Goal**: Users can scan any project for family status and update from library

**Independent Test**: Open project → run scanner → see all statuses → update selected

### Backend for US1

- [X] T013 [US1] [BACKEND] Add BatchCheckAsync to IFamilyService → Artifacts: [IFamilyService.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/Interfaces/IFamilyService.cs)
- [X] T014 [US1] [BACKEND] Implement BatchCheckAsync in FamilyService → Artifacts: [FamilyService.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/Services/FamilyService.cs), [FamilyRepository.cs](src/FamilyLibrary.Api/FamilyLibrary.Infrastructure/Repositories/FamilyRepository.cs)
- [X] T015 [US1] [BACKEND] Add batch-check endpoint to FamiliesController → Artifacts: [FamiliesController.cs](src/FamilyLibrary.Api/FamilyLibrary.Api/Controllers/FamiliesController.cs)
- [X] T016 [US1] [BACKEND] Create ProjectsController with scan/batch-update stubs → Artifacts: [ProjectsController.cs](src/FamilyLibrary.Api/FamilyLibrary.Api/Controllers/ProjectsController.cs)

### Plugin for US1

- [X] T017 [P] [US1] [PLUGIN] Create LegacyRecognitionService → Artifacts: [LegacyRecognitionService.cs](src/FamilyLibrary.Plugin/Commands/UpdateFamiliesCommand/Services/LegacyRecognitionService.cs)
- [X] T018 [P] [US1] [PLUGIN] Create ProjectScannerService → Artifacts: [ProjectScannerService.cs](src/FamilyLibrary.Plugin/Commands/UpdateFamiliesCommand/Services/ProjectScannerService.cs)
- [X] T019 [P] [US1] [PLUGIN] Create FamilyUpdaterService → Artifacts: [FamilyUpdaterService.cs](src/FamilyLibrary.Plugin/Commands/UpdateFamiliesCommand/Services/FamilyUpdaterService.cs)
- [X] T020 [US1] [PLUGIN] Create UpdateFamiliesCommand → Artifacts: [UpdateFamiliesCommand.cs](src/FamilyLibrary.Plugin/Commands/UpdateFamiliesCommand/UpdateFamiliesCommand.cs)
- [X] T021 [US1] [PLUGIN] Create UpdateFamiliesAvailability → Artifacts: [UpdateFamiliesAvailability.cs](src/FamilyLibrary.Plugin/Commands/UpdateFamiliesCommand/UpdateFamiliesAvailability.cs)
- [X] T022 [US1] [PLUGIN] Add scanner WebView2 events to RevitBridge → Artifacts: [RevitBridge.cs](src/FamilyLibrary.Plugin/Infrastructure/WebView2/RevitBridge.cs)
- [X] T023 [US1] [PLUGIN] Create ScannerWindow → Artifacts: [ScannerWindow.xaml](src/FamilyLibrary.Plugin/Commands/UpdateFamiliesCommand/Views/ScannerWindow.xaml)

### Frontend for US1

- [X] T024 [P] [US1] [FRONTEND] Create scanner.service.ts → Artifacts: [scanner.service.ts](src/FamilyLibrary.Web/src/app/features/scanner/services/scanner.service.ts)
- [X] T025 [P] [US1] [FRONTEND] Create scanner.component.ts → Artifacts: [scanner.component.ts](src/FamilyLibrary.Web/src/app/features/scanner/scanner.component.ts)
- [X] T026 [P] [US1] [FRONTEND] Create scanner-table.component.ts → Artifacts: [scanner-table.component.ts](src/FamilyLibrary.Web/src/app/features/scanner/components/scanner-table.component.ts)
- [X] T027 [P] [US1] [FRONTEND] Create scanner-filters.component.ts → Artifacts: [scanner-filters.component.ts](src/FamilyLibrary.Web/src/app/features/scanner/components/scanner-filters.component.ts)
- [X] T028 [P] [US1] [FRONTEND] Create update-progress.component.ts → Artifacts: [update-progress.component.ts](src/FamilyLibrary.Web/src/app/features/scanner/components/update-progress.component.ts)
- [X] T029 [US1] [FRONTEND] Add scanner route → Artifacts: [app.routes.ts](src/FamilyLibrary.Web/src/app/app.routes.ts)
- [X] T030 [US1] [FRONTEND] Add scanner navigation item → Artifacts: [main-layout.component.ts](src/FamilyLibrary.Web/src/app/layout/main-layout/main-layout.component.ts)

**Checkpoint**: Scanner fully functional - can scan, filter, update families

---

## Phase 3: User Story 2 - View Change History (Priority: P1)

**Goal**: Users can see version changelog with diff between versions

**Independent Test**: Open family page → view changelog → see diff

### Backend for US2

- [X] T031 [US2] [BACKEND] Create ChangeDetectionService in src/FamilyLibrary.Infrastructure/Services/ChangeDetectionService.cs → Artifacts: [IChangeDetectionService.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/Interfaces/IChangeDetectionService.cs), [ChangeDetectionService.cs](src/FamilyLibrary.Api/FamilyLibrary.Infrastructure/Services/ChangeDetectionService.cs)
- [X] T032 [US2] [BACKEND] Add GetChangesAsync to IFamilyService in src/FamilyLibrary.Application/Interfaces/IFamilyService.cs → Artifacts: [IFamilyService.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/Interfaces/IFamilyService.cs)
- [X] T033 [US2] [BACKEND] Implement GetChangesAsync in FamilyService in src/FamilyLibrary.Infrastructure/Services/FamilyService.cs → Artifacts: [FamilyService.cs](src/FamilyLibrary.Api/FamilyLibrary.Application/Services/FamilyService.cs)
- [X] T034 [US2] [BACKEND] Add changes endpoint to FamiliesController in src/FamilyLibrary.Api/Controllers/FamiliesController.cs → Artifacts: [FamiliesController.cs](src/FamilyLibrary.Api/FamilyLibrary.Api/Controllers/FamiliesController.cs)
- [X] T035 [US2] [BACKEND] Add versions endpoint with changes to FamiliesController in src/FamilyLibrary.Api/Controllers/FamiliesController.cs → Artifacts: Already existed in FamiliesController.cs

### Plugin for US2

- [X] T036 [P] [US2] [PLUGIN] Create SnapshotService in src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Services/ → Artifacts: [FamilyScannerService.cs](src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Services/FamilyScannerService.cs) (SnapshotService class inside)
- [X] T037 [US2] [PLUGIN] Create PluginChangeDetectionService in src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Services/ → Artifacts: [PluginChangeDetectionService.cs](src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Services/PluginChangeDetectionService.cs)
- [X] T038 [US2] [PLUGIN] Update FamilyPublisher to create SnapshotJSON on publish → Artifacts: [PublishService.cs](src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Services/PublishService.cs), [PublishRequest.cs](src/FamilyLibrary.Plugin/Core/Entities/PublishRequest.cs)

### Frontend for US2

- [X] T039 [P] [US2] [FRONTEND] Create changelog.service.ts in src/FamilyLibrary.Web/src/app/features/library/services/ → Artifacts: [changelog.service.ts](src/FamilyLibrary.Web/src/app/features/library/services/changelog.service.ts)
- [X] T040 [P] [US2] [FRONTEND] Create changelog.component.ts in src/FamilyLibrary.Web/src/app/features/library/components/changelog/ → Artifacts: [changelog.component.ts](src/FamilyLibrary.Web/src/app/features/library/components/changelog/changelog.component.ts), [changelog.component.html](src/FamilyLibrary.Web/src/app/features/library/components/changelog/changelog.component.html)
- [X] T041 [P] [US2] [FRONTEND] Create change-item.component.ts in src/FamilyLibrary.Web/src/app/features/library/components/changelog/ → Artifacts: [change-item.component.ts](src/FamilyLibrary.Web/src/app/features/library/components/changelog/change-item.component.ts), [change-item.component.html](src/FamilyLibrary.Web/src/app/features/library/components/changelog/change-item.component.html)
- [X] T042 [P] [US2] [FRONTEND] Create parameter-diff.component.ts in src/FamilyLibrary.Web/src/app/features/library/components/changelog/ → Artifacts: [parameter-diff.component.ts](src/FamilyLibrary.Web/src/app/features/library/components/changelog/parameter-diff.component.ts), [parameter-diff.component.html](src/FamilyLibrary.Web/src/app/features/library/components/changelog/parameter-diff.component.html)
- [ ] T043 [US2] [FRONTEND] Add changelog section to family-detail.component in src/FamilyLibrary.Web/src/app/features/library/components/family-detail/

**Checkpoint**: Changelog visible on family page with full diff

---

## Phase 4: User Story 3 - Local Changes Before Publish (Priority: P2)

**Goal**: BIM-managers see local modifications before publishing

**Independent Test**: Edit family → open Queue → see "Local Modified" → view diff

### Backend for US3

- [ ] T044 [US3] [BACKEND] Add DetectLocalChangesAsync to IFamilyService in src/FamilyLibrary.Application/Interfaces/IFamilyService.cs
- [ ] T045 [US3] [BACKEND] Implement DetectLocalChangesAsync in FamilyService in src/FamilyLibrary.Infrastructure/Services/FamilyService.cs
- [ ] T046 [US3] [BACKEND] Add local-changes endpoint to FamiliesController in src/FamilyLibrary.Api/Controllers/FamiliesController.cs

### Plugin for US3

- [ ] T047 [US3] [PLUGIN] Create LocalChangeDetector in src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Services/
- [ ] T048 [US3] [PLUGIN] Add Local Modified status detection to LibraryQueueViewModel
- [ ] T049 [US3] [PLUGIN] Add View Changes button to Queue tab
- [ ] T050 [US3] [PLUGIN] Add changes:result event to RevitBridge

### Frontend for US3

- [ ] T051 [P] [US3] [FRONTEND] Create view-changes-modal.component.ts in src/FamilyLibrary.Web/src/app/features/library/components/
- [ ] T052 [US3] [FRONTEND] Add View Changes button to queue-list component
- [ ] T053 [US3] [FRONTEND] Handle revit:changes:result event in RevitBridgeService

**Checkpoint**: Local changes visible in Queue with diff modal

---

## Phase 5: User Story 4 - Pre-Update Preview (Priority: P2)

**Goal**: Designers see what will change before confirming update

**Independent Test**: Click Update → see preview → confirm or cancel

### Backend for US4

- [ ] T054 [US4] [BACKEND] Add GetUpdatePreviewAsync to IFamilyService in src/FamilyLibrary.Application/Interfaces/IFamilyService.cs
- [ ] T055 [US4] [BACKEND] Implement GetUpdatePreviewAsync in FamilyService in src/FamilyLibrary.Infrastructure/Services/FamilyService.cs
- [ ] T056 [US4] [BACKEND] Add update-preview endpoint to FamiliesController in src/FamilyLibrary.Api/Controllers/FamiliesController.cs

### Plugin for US4

- [ ] T057 [US4] [PLUGIN] Create UpdatePreviewService in src/FamilyLibrary.Plugin/Commands/UpdateFamiliesCommand/Services/
- [ ] T058 [US4] [PLUGIN] Add pre-update preview check to FamilyUpdaterService
- [ ] T059 [US4] [PLUGIN] Send preview data before update via WebView2

### Frontend for US4

- [ ] T060 [P] [US4] [FRONTEND] Create pre-update-preview.component.ts in src/FamilyLibrary.Web/src/app/features/scanner/components/
- [ ] T061 [US4] [FRONTEND] Show preview dialog before update confirmation
- [ ] T062 [US4] [FRONTEND] Add Show Details expansion to preview

**Checkpoint**: Preview shown before any update with full diff

---

## Phase 6: User Story 5 - MEP System Families (Priority: P2)

**Goal**: Support Group A (full) and Group B (Pipes, Ducts) system families

**Independent Test**: Create PipeType role → Stamp → Publish → JSON with RoutingPreferences saved

### Backend for US5

- [ ] T063 [US5] [BACKEND] Add RoutingPreferencesJson model in src/FamilyLibrary.Application/DTOs/SystemTypeDtos.cs
- [ ] T064 [US5] [BACKEND] Update SystemTypeService for Group A categories (Roofs, Ceilings, Foundations)
- [ ] T065 [US5] [BACKEND] Update SystemTypeValidator for MEP types

### Plugin for US5

- [ ] T066 [P] [US5] [PLUGIN] Create RoutingPreferencesSerializer in src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Services/
- [ ] T067 [P] [US5] [PLUGIN] Create RoutingPreferencesApplier in src/FamilyLibrary.Plugin/Commands/StampFamilyCommand/Services/
- [ ] T068 [US5] [PLUGIN] Update SystemTypeScannerService for Group A categories
- [ ] T069 [US5] [PLUGIN] Update SystemTypeScannerService for Group B (Pipes, Ducts)
- [ ] T070 [US5] [PLUGIN] Update SystemTypePublisher with RoutingPreferences serialization
- [ ] T071 [US5] [PLUGIN] Update SystemTypePullService with RoutingPreferences apply

### Frontend for US5

- [ ] T072 [P] [US5] [FRONTEND] Create routing-preferences-display.component.ts in src/FamilyLibrary.Web/src/app/features/library/components/system-type-detail/
- [ ] T073 [US5] [FRONTEND] Add routing preferences section to system-type-detail

**Checkpoint**: MEP system families publish and update with RoutingPreferences

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Performance, testing, documentation

- [ ] T074 [P] [BACKEND] Add database index for batch check optimization
- [ ] T075 [P] [BACKEND] Add rate limiting for scan endpoints
- [ ] T076 [P] [PLUGIN] Implement recognition rules caching in LegacyRecognitionService
- [ ] T077 [P] [FRONTEND] Add virtual scroll to scanner table
- [ ] T078 [FRONTEND] Add error handling and toast messages for scan operations
- [ ] T079 [P] [BACKEND] Create unit tests for ChangeDetectionService in tests/FamilyLibrary.Application.Tests/
- [ ] T080 [P] [PLUGIN] Create unit tests for SnapshotService in tests/FamilyLibrary.Plugin.Tests/
- [ ] T081 [P] [PLUGIN] Create unit tests for LegacyRecognitionService in tests/FamilyLibrary.Plugin.Tests/
- [ ] T082 Update quickstart.md with Phase 2 features
- [ ] T083 Run quickstart.md validation scenarios

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Foundational)**: No dependencies - can start immediately
- **Phase 2 (US1 - Scanner)**: Depends on Phase 1 - 🎯 MVP target
- **Phase 3 (US2 - Changelog)**: Depends on Phase 1 - Independent of US1
- **Phase 4 (US3 - Local Changes)**: Depends on Phase 1 + Phase 3 (ChangeDetection)
- **Phase 5 (US4 - Pre-Update)**: Depends on Phase 1 + Phase 3 (ChangeDetection)
- **Phase 6 (US5 - MEP)**: Depends on Phase 1 - Independent
- **Phase 7 (Polish)**: Depends on all desired user stories

### User Story Independence

| Story | Can Start After | Depends On Other Stories? |
|-------|----------------|---------------------------|
| US1 (Scanner) | Phase 1 | No |
| US2 (Changelog) | Phase 1 | No |
| US3 (Local Changes) | Phase 1 + US2 | Uses ChangeDetection from US2 |
| US4 (Pre-Update) | Phase 1 + US2 | Uses ChangeDetection from US2 |
| US5 (MEP) | Phase 1 | No |

### Parallel Opportunities

**Phase 1 - All tasks can run in parallel:**
```bash
# Launch all Phase 1 tasks together:
Task: T001, T002, T003, T004, T005, T006, T007, T008, T009, T010, T011, T012
```

**After Phase 1 - Independent stories:**
```bash
# US1, US2, US5 can run in parallel:
Developer A: US1 (T013-T030)
Developer B: US2 (T031-T043)
Developer C: US5 (T063-T073)
```

**Within each story - Component parallelism:**
```bash
# US1 Backend + Plugin + Frontend in parallel (different files):
Task: T013-T016 [BACKEND]
Task: T017-T023 [PLUGIN]
Task: T024-T030 [FRONTEND]
```

---

## Implementation Strategy

### MVP First (US1 Only)

1. Complete Phase 1: Foundational
2. Complete Phase 2: US1 - Scanner
3. **STOP and VALIDATE**: Test scanner independently
4. Deploy/demo if ready

### Incremental Delivery

1. Foundational → Shared infrastructure ready
2. Add US1 → Scanner works → Deploy (MVP!)
3. Add US2 → Changelog works → Deploy
4. Add US3 → Local changes visible → Deploy
5. Add US4 → Pre-update preview → Deploy
6. Add US5 → MEP support → Deploy
7. Polish → Performance optimized → Final release

---

## Notes

- [P] tasks = different files, no dependencies on incomplete tasks
- [Story] label maps task to specific user story for traceability
- Each user story is independently completable and testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
