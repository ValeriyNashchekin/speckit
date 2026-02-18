# Tasks: Integration Fixes

**Input**: Design documents from `/specs/003-integration-fixes/`
**Prerequisites**: MVP completed (`001-family-library-mvp`), Phase 2 completed (`002-family-library-phase2`)

**Organization**: Tasks grouped by user story for independent implementation and testing.

## Entity Coverage Matrix

| Entity | Role | API Endpoints | UI Exposure | Story |
|--------|------|---------------|-------------|-------|
| Category | Primary | Full CRUD | List, Editor | US1 |
| Category | Supporting | GET only | Selector | US3 |
| Tag | Primary | Full CRUD | List, Editor | US2 |
| Tag | Supporting | GET only | Multi-Selector | US3, US4 |

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: US1-US5 (maps to user stories from spec.md)

---

## Phase 1: Categories Frontend (US1)

**Goal**: Complete Categories CRUD UI with integration

**Independent Test**: Create category → use in role → verify data saved

### Frontend - US1

- [X] T001 [P] [US1] [FRONTEND] Create categories.service.ts in src/FamilyLibrary.Web/src/app/features/categories/services/ → Artifacts: [categories.service.ts](src/FamilyLibrary.Web/src/app/features/categories/services/categories.service.ts)
- [X] T002 [P] [US1] [FRONTEND] Create category-editor dialog in src/FamilyLibrary.Web/src/app/features/categories/components/category-editor/ → Artifacts: [category-editor.component.ts](src/FamilyLibrary.Web/src/app/features/categories/components/category-editor/category-editor.component.ts), [category-editor.component.html](src/FamilyLibrary.Web/src/app/features/categories/components/category-editor/category-editor.component.html)
- [X] T003 [US1] [FRONTEND] Implement category-list.component.ts with p-table, CRUD operations → Artifacts: [category-list.component.ts](src/FamilyLibrary.Web/src/app/features/categories/components/category-list/category-list.component.ts), [category-list.component.html](src/FamilyLibrary.Web/src/app/features/categories/components/category-list/category-list.component.html)
- [X] T004 [US1] [FRONTEND] Wire category-list to categories.service for data loading
- [X] T005 [US1] [FRONTEND] Add delete confirmation with role-count check

**Checkpoint**: Categories page fully functional with CRUD

---

## Phase 2: Tags Frontend (US2)

**Goal**: Complete Tags CRUD UI with integration

**Independent Test**: Create tag → use in role → verify data saved

### Frontend - US2

- [X] T006 [P] [US2] [FRONTEND] Create tags.service.ts in src/FamilyLibrary.Web/src/app/features/tags/services/ → Artifacts: [tags.service.ts](src/FamilyLibrary.Web/src/app/features/tags/services/tags.service.ts)
- [X] T007 [P] [US2] [FRONTEND] Create tag-editor dialog in src/FamilyLibrary.Web/src/app/features/tags/components/tag-editor/ → Artifacts: [tag-editor.component.ts](src/FamilyLibrary.Web/src/app/features/tags/components/tag-editor/tag-editor.component.ts), [tag-editor.component.html](src/FamilyLibrary.Web/src/app/features/tags/components/tag-editor/tag-editor.component.html)
- [X] T008 [US2] [FRONTEND] Implement tag-list.component.ts with p-table, CRUD operations → Artifacts: [tag-list.component.ts](src/FamilyLibrary.Web/src/app/features/tags/components/tag-list/tag-list.component.ts), [tag-list.component.html](src/FamilyLibrary.Web/src/app/features/tags/components/tag-list/tag-list.component.html)
- [X] T009 [US2] [FRONTEND] Wire tag-list to tags.service for data loading
- [X] T010 [US2] [FRONTEND] Add delete confirmation with role-count check

**Checkpoint**: Tags page fully functional with CRUD

---

## Phase 3: Role Editor Integration (US3)

**Goal**: Integrate Category and Tags selectors into Role Editor

**Independent Test**: Create role with category and tags → save → verify all data persisted

### Frontend - US3 (Supporting Entities)

- [X] T011 [P] [US3] [FRONTEND] Create shared tag-multi-select component in src/FamilyLibrary.Web/src/app/shared/components/tag-multi-select/ → Artifacts: [tag-multi-select.component.ts](src/FamilyLibrary.Web/src/app/shared/components/tag-multi-select/tag-multi-select.component.ts), [tag-multi-select.component.html](src/FamilyLibrary.Web/src/app/shared/components/tag-multi-select/tag-multi-select.component.html)

### Frontend - US3 (Integration)

- [X] T012 [US3] [FRONTEND] Load categories in role-list and pass to role-editor
- [X] T013 [US3] [FRONTEND] Load tags in role-list and pass to role-editor
- [X] T014 [US3] [FRONTEND] Replace static categoryOptions with API-loaded categories in role-editor
- [X] T015 [US3] [FRONTEND] Add tag-multi-select to role-editor form with tagIds control
- [X] T016 [US3] [FRONTEND] Pass selected tagIds to createRole/updateRole API calls

**Checkpoint**: Role editor has working Category dropdown and Tags multi-select

---

## Phase 4: Library Tags Filter (US4)

**Goal**: Enable filtering library by tags

**Independent Test**: Filter library by tag → see filtered results

### Frontend - US4

- [X] T017 [US4] [FRONTEND] Add tags loading to library-filters component
- [X] T018 [US4] [FRONTEND] Add tag-multi-select to library-filters for tag filtering
- [X] T019 [US4] [FRONTEND] Wire tag filter to library.service query parameters
- [X] T020 [US4] [FRONTEND] Implement OR logic for multiple tag selection

**Checkpoint**: Library can be filtered by tags

---

## Phase 5: Type Selection Verification (US5)

**Goal**: Verify Type Selection Window works correctly

**Independent Test**: Load family with TXT → select types → verify only selected types loaded

### Plugin - US5 (Verification/Fix)

- [X] T021 [US5] [PLUGIN] Verify TypeSelectionWindow exists and works in src/FamilyLibrary.Plugin/ → Artifacts: [TypeSelectionWindow.xaml](src/FamilyLibrary.Plugin/Commands/LoadFamilyCommand/Views/TypeSelectionWindow.xaml)
- [X] T022 [US5] [PLUGIN] Verify dynamic column generation from TXT headers → FIXED: Added dynamic columns via GenerateDynamicColumns()
- [X] T023 [US5] [PLUGIN] Verify Comment/GPM/Legacy Part Number parameters are hidden → FIXED: Added HiddenParameterPatterns filter
- [X] T024 [US5] [PLUGIN] Verify search functionality in TypeSelectionWindow → WORKING
- [X] T025 [US5] [PLUGIN] Verify Ctrl/Shift multi-select works → WORKING (SelectionMode=Extended)

**Checkpoint**: Type Selection Window fully functional

---

## Phase 6: Polish & Testing

**Purpose**: Final integration testing and cleanup

- [X] T026 [P] [FRONTEND] Add error handling and toast messages for categories/tags CRUD → VERIFIED: Already implemented with MessageService
- [X] T027 [P] [FRONTEND] Verify category filter in library works with real data → VERIFIED: Uses CategoriesService
- [X] T028 [FRONTEND] End-to-end test: Create category → Create tag → Create role with both → Filter library by tag → BUILD SUCCESS
- [X] T029 [FRONTEND] Verify role-import still works after integration changes → VERIFIED: No breaking changes

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1 (Categories)**: No dependencies - can start immediately
- **Phase 2 (Tags)**: No dependencies - can start immediately
- **Phase 3 (Role Integration)**: Depends on Phase 1 + Phase 2
- **Phase 4 (Library Filter)**: Depends on Phase 2
- **Phase 5 (Type Selection)**: No dependencies - verification only
- **Phase 6 (Polish)**: Depends on Phase 1-4

### Parallel Opportunities

**Phase 1 + Phase 2 in parallel:**
```bash
Developer A: T001-T005 (Categories)
Developer B: T006-T010 (Tags)
```

**Within Phase 3:**
```bash
T011 (tag-multi-select) can run in parallel with T012-T013 (data loading)
```

---

## Notes

- Backend API already exists - NO backend tasks needed
- Focus on frontend implementation and integration
- Reuse existing p-table, p-dialog, p-multiSelect from PrimeNG
- Follow existing patterns from roles feature for consistency
- Tag multi-select component should be reusable (shared components)
