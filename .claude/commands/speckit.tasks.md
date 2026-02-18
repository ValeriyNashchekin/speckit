---
description: Generate an actionable, dependency-ordered tasks.md for the feature based on available design artifacts.
handoffs: 
  - label: Analyze For Consistency
    agent: speckit.analyze
    prompt: Run a project analysis for consistency
    send: true
  - label: Implement Project
    agent: speckit.implement
    prompt: Start the implementation in phases
    send: true
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Outline

1. **Setup**: Run `.specify/scripts/powershell/check-prerequisites.ps1 -Json` from repo root and parse FEATURE_DIR and AVAILABLE_DOCS list. All paths must be absolute. For single quotes in args like "I'm Groot", use escape syntax: e.g 'I'\''m Groot' (or double-quote if possible: "I'm Groot").

2. **Load design documents**: Read from FEATURE_DIR:
   - **Required**: plan.md (tech stack, libraries, structure), spec.md (user stories with priorities)
   - **Optional**: data-model.md (entities), contracts/ (API endpoints), research.md (decisions), quickstart.md (test scenarios)
   - Note: Not all projects have all documents. Generate tasks based on what's available.

3. **Execute task generation workflow**:
   - Load plan.md and extract tech stack, libraries, project structure
   - Load spec.md and extract user stories with their priorities (P1, P2, P3, etc.)
   - If data-model.md exists: Extract entities and map to user stories
   - If contracts/ exists: Map endpoints to user stories
   - If research.md exists: Extract decisions for setup tasks
   - **Perform Entity Coverage Analysis** (see below)
   - Generate tasks organized by user story (see Task Generation Rules below)
   - Generate dependency graph showing user story completion order
   - Create parallel execution examples per user story
   - Validate coverage completeness (see Coverage Validation below)

4. **Generate tasks.md**: Use `.specify/templates/tasks-template.md` as structure, fill with:
   - Correct feature name from plan.md
   - Phase 1: Setup tasks (project initialization)
   - Phase 2: Foundational tasks (blocking prerequisites for all user stories)
   - Phase 3+: One phase per user story (in priority order from spec.md)
   - Each phase includes: story goal, independent test criteria, tests (if requested), implementation tasks
   - Final Phase: Polish & cross-cutting concerns
   - All tasks must follow the strict checklist format (see Task Generation Rules below)
   - Clear file paths for each task
   - Dependencies section showing story completion order
   - Parallel execution examples per story
   - Implementation strategy section (MVP first, incremental delivery)

5. **Report**: Output path to generated tasks.md and summary:
   - Total task count
   - Task count per user story
   - Entity Coverage Matrix (primary vs supporting entities)
   - Parallel opportunities identified
   - Independent test criteria for each story
   - Suggested MVP scope (typically just User Story 1)
   - Coverage validation results:
     - API coverage: endpoints → service methods mapped
     - Entity coverage: all entities have appropriate tasks
     - FK coverage: selectors + integration tasks for all FK fields
   - Format validation: Confirm ALL tasks follow the checklist format (checkbox, ID, labels, file paths)

Context for task generation: $ARGUMENTS

The tasks.md should be immediately executable - each task must be specific enough that an LLM can complete it without additional context.

## Task Generation Rules

**CRITICAL**: Tasks MUST be organized by user story to enable independent implementation and testing.

**Tests are OPTIONAL**: Only generate test tasks if explicitly requested in the feature specification or if user requests TDD approach.

### Checklist Format (REQUIRED)

Every task MUST strictly follow this format:

```text
- [ ] [TaskID] [P?] [Story?] [Component?] Description with file path
```

**Format Components**:

1. **Checkbox**: ALWAYS start with `- [ ]` (markdown checkbox)
2. **Task ID**: Sequential number (T001, T002, T003...) in execution order
3. **[P] marker**: Include ONLY if task is parallelizable (different files, no dependencies on incomplete tasks)
4. **[Story] label**: REQUIRED for user story phase tasks only
   - Format: [US1], [US2], [US3], etc. (maps to user stories from spec.md)
   - Setup phase: NO story label
   - Foundational phase: NO story label
   - User Story phases: MUST have story label
   - Polish phase: NO story label
5. **[Component] marker**: REQUIRED for implementation tasks
   - [BACKEND] - Tasks in src/FamilyLibrary.Api/ (Backend .NET)
   - [FRONTEND] - Tasks in src/FamilyLibrary.Web/ (Angular)
   - [PLUGIN] - Tasks in src/FamilyLibrary.Plugin/ (Revit)
   - Setup/Foundational phase: NO component marker (shared)
6. **Description**: Clear action with exact file path

**Examples**:

- ✅ CORRECT: `- [ ] T001 Create project structure per implementation plan`
- ✅ CORRECT: `- [ ] T005 [P] [BACKEND] Implement authentication middleware in src/FamilyLibrary.Api/FamilyLibrary.Api/Middleware/`
- ✅ CORRECT: `- [ ] T012 [P] [US1] [BACKEND] Create User entity in src/FamilyLibrary.Api/FamilyLibrary.Domain/Entities/`
- ✅ CORRECT: `- [ ] T014 [US1] [FRONTEND] Create UserService in src/FamilyLibrary.Web/src/app/services/`
- ✅ CORRECT: `- [ ] T089 [US3] [PLUGIN] Create StampFamilyCommand in src/FamilyLibrary.Plugin/FamilyLibrary.Plugin/Commands/`
- ❌ WRONG: `- [ ] Create User model` (missing ID, Story label, Component)
- ❌ WRONG: `T001 [US1] Create model` (missing checkbox)
- ❌ WRONG: `- [ ] [US1] Create User model` (missing Task ID)
- ❌ WRONG: `- [ ] T001 [US1] Create model` (missing file path)

**Component Auto-Detection for Parallel Execution**:

When generating tasks, automatically assign component markers based on file paths:
- Paths containing `FamilyLibrary.Api/` → [BACKEND]
- Paths containing `FamilyLibrary.Web/` → [FRONTEND]
- Paths containing `FamilyLibrary.Plugin/` → [PLUGIN]

### Task Organization

**Source Priority for Task Generation:**
1. **contracts/api.yaml** - Every endpoint group triggers task consideration
2. **data-model.md** - Entity relationships inform task dependencies and supporting entities
3. **spec.md** - Acceptance criteria determine UI exposure type
4. **plan.md** - Project structure guides file paths

**From User Stories (spec.md)** - PRIMARY ORGANIZATION:
- Each user story (P1, P2, P3...) gets its own phase
- Identify primary entity for each story
- Identify supporting entities via FK relationships and acceptance criteria
- Map all related components to their story:
  - Primary entity: full stack tasks
  - Supporting entities: targeted tasks (selector, service, or CRUD based on usage)
  - Composition tasks for integrating supporting entities into primary forms

**From Contracts (api.yaml)**:
- Every endpoint group (tag) represents an entity
- Map each endpoint to the user story it primarily serves
- Supporting entities used by primary entity → included in same story phase
- If tests requested: Each contract → contract test task [P] before implementation

**From Data Model**:
- Map each entity to the user story(ies) that need it
- FK relationships indicate supporting entity usage
- If entity serves multiple stories: Put in earliest story or Foundational phase
- Relationships → service layer + selector component tasks

**From Setup/Infrastructure**:
- Shared infrastructure → Setup phase (Phase 1)
- Foundational/blocking tasks → Foundational phase (Phase 2)
- Story-specific setup → within that story's phase

### Phase Structure

- **Phase 1**: Setup (project initialization)
- **Phase 2**: Foundational (blocking prerequisites - MUST complete before user stories)
- **Phase 3+**: User Stories in priority order (P1, P2, P3...)
  - Within each story: Tests (if requested) → Models → Services → Endpoints → Integration
  - Each phase should be a complete, independently testable increment
- **Final Phase**: Polish & Cross-Cutting Concerns

### Entity Coverage Analysis

Before generating tasks, perform this analysis to ensure all entities are covered:

**Step 1: Extract all entities from data-model.md**
- List each entity with its fields and relationships
- Identify FK relationships between entities

**Step 2: Extract all API endpoint groups from contracts/api.yaml**
- Group endpoints by tag (e.g., Roles, Categories, Tags)
- Note CRUD operations available for each group

**Step 3: Classify entities by role in user story**
For each entity, determine if it is:
- **Primary Entity**: Main focus of the user story (from story title/goal)
- **Supporting Entity**: Referenced by primary entity or mentioned in acceptance criteria
  - Indicators: FK relationship, "select X" in acceptance criteria, "filter by X"
- **Internal Entity**: Used by system but not directly user-facing

**Step 4: Determine UI exposure for each entity**
For each entity with API endpoints:
- If acceptance criteria mention "create/edit/delete X" → CRUD UI needed
- If acceptance criteria mention "select X" or "filter by X" → Selector UI needed
- If entity appears in other entity's form as FK → Selector UI needed
- If entity only has GET endpoints → Read-only display

**Step 5: Generate Entity Coverage Matrix**
Include this matrix in tasks.md header for transparency:

```text
| Entity | Role | API Endpoints | UI Exposure | Story |
|--------|------|---------------|-------------|-------|
| FamilyRole | Primary | Full CRUD | List, Editor | US1 |
| Category | Supporting | Full CRUD | Selector | US1 |
| Tag | Supporting | Full CRUD | Multi-Selector | US1 |
```

### Task Generation by Entity Type

**Primary Entities** get full stack tasks:
- Backend: Entity, DTO, Service, Controller, Validator, Repository
- Frontend: Service, Store, List component, Editor component, Routes

**Supporting Entities** get targeted tasks based on UI exposure:
- Backend: Entity, DTO, Service, Controller (if not exists)
- Frontend (CRUD UI): Service, List, Editor, Routes
- Frontend (Selector only): Service, Selector component
- Integration: Embed selector into parent form(s)

**Internal Entities** get:
- Backend only (typically in Foundational phase)

### UI Composition Tasks

For each form/dialog that contains FK fields, generate composition tasks:

```text
- [ ] T0XX [USn] [FRONTEND] Integrate [Entity]Selector into [ParentComponent]
- [ ] T0XX [USn] [FRONTEND] Wire [field] dropdown to [Entity]Service.getAll()
```

Examples:
- RoleEditor has categoryId → CategorySelector integration task
- RoleEditor has tagIds → TagSelector integration task
- LibraryFilters has categoryId → CategoryDropdown integration task

### Coverage Validation

Before finalizing tasks.md, verify:

**API Coverage:**
- Every GET endpoint → has frontend service method to call it
- Every POST/PUT/DELETE → has UI trigger (form submit, button click)

**Entity Coverage:**
- Every entity in data-model.md → has backend tasks
- Every entity in acceptance criteria → has frontend tasks
- Every entity with CRUD API → has CRUD UI OR explicit note why not needed

**FK Coverage:**
- Every FK field in forms → has selector component
- Every selector component → has integration task into parent form
- Every selector → wired to corresponding service

**Acceptance Criteria Coverage:**
- Each acceptance scenario → has corresponding implementation task(s)
- "Select X" in criteria → X Selector task exists
- "Filter by X" in criteria → X filter task exists
