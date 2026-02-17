# Feature Specification: Family Library Phase 2

**Feature Branch**: `002-family-library-phase2`
**Created**: 2026-02-17
**Status**: Draft
**Input**: Family Library Phase 2 — расширение MVP: System Families MEP, Scanner, Change Tracking
**Depends On**: `001-family-library-mvp`

---

## Overview

Phase 2 расширяет функциональность MVP тремя направлениями:

1. **System Families MEP** — поддержка групп A (полностью) и B (Pipes, Ducts)
2. **Сканер проектов** — массовая проверка и обновление семейств в любых проектах
3. **Change Tracking** — отслеживание изменений, diff, changelog

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Scan and Update Families in Any Project (Priority: P1)

Как пользователь (любая роль), я хочу проверить семейства в любом проекте на актуальность и обновить их до последних версий из библиотеки.

**Why this priority**: Ключевая фича для Adoption — пользователи видят ценность в обновлении проектов.

**Independent Test**: Открыть проект → запустить сканер → увидеть статусы всех семейств → обновить выбранные.

**Acceptance Scenarios**:

1. **Given** пользователь в любом проекте (не только шаблон), **When** выполняет команду "Update Families from Library", **Then** открывается страница сканера
2. **Given** сканер открыт, **When** сканирование завершено, **Then** отображается таблица: Family Name, Category, Current Role, Status, Actions
3. **Given** сканер показывает результаты, **When** фильтрует по статусу "Update Available", **Then** показаны только семейства с доступными обновлениями
4. **Given** выбраны семейства со статусом "Update Available", **When** нажимает "Update Selected", **Then** все выбранные обновлены, показан progress bar
5. **Given** семейство без клейма найдено по Legacy Recognition, **When** статус "Legacy Match", **Then** колонка Role показывает "(auto: RoleName)"

**Statuses**:
- 🟢 Up to date — Hash совпадает с библиотекой
- 🟡 Update Available — в библиотеке новая версия
- 🔵 Legacy Match — найдено по правилам распознавания
- ⚪ Unmatched — без клейма, не найдено по правилам

---

### User Story 2 - View Change History (Priority: P1)

Как пользователь, я хочу видеть историю изменений семейства по версиям (changelog), чтобы понимать что изменилось.

**Why this priority**: Прозрачность изменений — важно для доверия к системе.

**Independent Test**: Открыть страницу семейства → посмотреть changelog → увидеть diff между версиями.

**Acceptance Scenarios**:

1. **Given** пользователь на странице семейства, **When** просматривает changelog, **Then** видит список версий с датами и авторами
2. **Given** changelog отображается, **When** смотрит Version 2 → Version 3, **Then** видит: ✏️ Name changed, ➕ Type added, 📝 Parameter changed, 🔧 Geometry changed
3. **Given** changelog для версии, **When** нажимает "Show Details", **Then** разворачивается детальный diff параметров
4. **Given** CommitMessage заполнен при Publish, **When** просматривает версию, **Then** видит описание изменений

---

### User Story 3 - See Local Changes Before Publish (Priority: P2)

Как БИМ-менеджер, я хочу видеть локальные изменения семейства до Publish, чтобы понимать что будет опубликовано.

**Why this priority**: Контроль изменений перед публикацией.

**Independent Test**: Редактировать семейство → открыть Queue → увидеть "Local Modified" → посмотреть diff.

**Acceptance Scenarios**:

1. **Given** семейство в Queue, **When** ContentHash изменился, **Then** статус "Local Modified", показан icon изменений
2. **Given** семейство со статусом "Local Modified", **When** нажимает "View Changes", **Then** открывается modal с diff
3. **Given** modal с изменениями, **When** просматривает, **Then** видит: ✏️ Name change, ➕ Types added/removed, 📝 Parameters changed, 🔧 Geometry flag
4. **Given** modal открыт, **When** нажимает "Discard Changes", **Then** предупреждение о потере изменений

---

### User Story 4 - Pre-Update Preview (Priority: P2)

Как Проектировщик, я хочу видеть что изменится при обновлении семейства, до того как подтвержу обновление.

**Why this priority**: Предотвращение случайных изменений в проекте.

**Independent Test**: Нажать Update на семействе → увидеть preview → подтвердить или отменить.

**Acceptance Scenarios**:

1. **Given** Проектировщик нажимает "Update" на семействе, **When** есть изменения, **Then** показывается Pre-Update Preview
2. **Given** Pre-Update Preview открыт, **When** просматривает, **Then** видит summary: ✏️ Name changed, ➕ 2 types added, 📝 3 parameters changed
3. **Given** Pre-Update Preview открыт, **When** нажимает "Show Details", **Then** видит полный diff
4. **Given** Pre-Update Preview открыт, **When** подтверждает, **Then** обновление выполняется

---

### User Story 5 - Manage MEP System Families (Priority: P2)

Как БИМ-менеджер, я хочу управлять типами MEP систем (трубы, воздуховоды) в библиотеке.

**Why this priority**: MEP — важная часть проектов, требует отдельной поддержки.

**Independent Test**: Создать роль для PipeType → Stamp → Publish → JSON с RoutingPreferences сохранён.

**Acceptance Scenarios**:

**Группа A (полностью):**
1. **Given** RoofType, CeilingType, FoundationType, **When** Publish, **Then** CompoundStructure сериализуется корректно (как WallType/FloorType в MVP)

**Группа B (MEP):**
2. **Given** PipeType "Standard_DN50", **When** Publish, **Then** JSON содержит: typeName, category, systemFamily, parameters, routingPreferences
3. **Given** DuctType, **When** Publish, **Then** JSON содержит routingPreferences с segments, fittings
4. **Given** Pull Update для PipeType, **When** применяется, **Then** RoutingPreferences обновлены

---

### Edge Cases

- **Given** сканирование проекта с 5000+ семейств, **When** выполняется, **Then** virtual scroll обеспечивает отзывчивость
- **Given** одновременно обновляется 100 семейств, **When** одно падает, **Then** остальные продолжаются, показан summary с ошибками
- **Given** changelog для семейства с 50 версиями, **When** открывается, **Then** пагинация по версиям

---

## Requirements *(mandatory)*

### Functional Requirements

**Модуль 6: Сканер проектов**

- **FR-201**: Команда "Update Families from Library" ДОЛЖНА быть доступна в любом проекте (не только шаблон)
- **FR-202**: Сканер ДОЛЖЕН показывать таблицу с колонками: Family Name, Category, Current Role, Status, Actions
- **FR-203**: Фильтры ДОЛЖНЫ включать: Status (All/Update Available/Legacy/Unmatched/Up to date), Category
- **FR-204**: Массовые операции ДОЛЖНЫ включать: Update Selected, Update All Available
- **FR-205**: При обновлении ДОЛЖЕН показываться progress bar
- **FR-206**: Статусы ДОЛЖНЫ вычисляться: Hash comparison (Stamped) или Legacy Recognition (unstamped)
- **FR-207**: Для Legacy Match ДОЛЖНА показываться auto-определённая роль

**Модуль 7: Change Tracking**

- **FR-211**: SnapshotJSON ДОЛЖЕН сохраняться при каждом Publish
- **FR-212**: Changelog ДОЛЖЕН показывать diff между версиями: Name, Types, Parameters, Geometry flag
- **FR-213**: Local Changes ДОЛЖНЫ детектироваться по сравнению Hash в ES и текущего Hash
- **FR-214**: Modal "View Changes" ДОЛЖЕН показывать diff для Local Modified
- **FR-215**: Pre-Update Preview ДОЛЖЕН показываться перед Update
- **FR-216**: Иконки изменений ДОЛЖНЫ быть: ✏️ Name, ➕➖ Types, 📝 Parameters, 🔧 Geometry
- **FR-217**: CommitMessage ДОЛЖЕН отображаться в истории версий

**Модуль: System Families (Phase 2)**

- **FR-221**: Группа A (Roofs, Ceilings, Foundations) ДОЛЖНА поддерживаться аналогично Walls/Floors
- **FR-222**: Группа B (Pipes, Ducts) ДОЛЖНА сериализовать routingPreferences в JSON
- **FR-223**: Pull Update для MEP типов ДОЛЖЕН обновлять RoutingPreferences
- **FR-224**: Фитинги в routingPreferences ДОЛЖНЫ маппиться по имени

### Key Entities

- **ChangeSnapshot**: FamilyVersionId, SnapshotJSON, ChangedFields (computed)
- **LocalChange**: FamilyId, Changes (name, types, parameters, geometry), DetectedAt

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-201**: Сканер обрабатывает 1000 семейств за 5 секунд
- **SC-202**: Mass update 50 семейств завершается за 60 секунд
- **SC-203**: 90% пользователей понимают changelog без объяснений
- **SC-204**: Pre-Update Preview сокращает accidental updates на 80%
- **SC-205**: MEP System Families покрывают 95% типовых случаев

---

## Technical Context

### API Endpoints (Additions)

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/families/{id}/changes` | GET | Diff между версиями |
| `/api/families/local-changes` | POST | Detect local changes |
| `/api/projects/{id}/scan` | POST | Сканировать проект |
| `/api/projects/{id}/batch-update` | POST | Mass update |

### JSON Structure (Group B - MEP)

```json
{
  "typeName": "Standard_DN50",
  "category": "Pipes",
  "systemFamily": "Pipe Types",
  "parameters": { "Routing Preference": "Standard" },
  "routingPreferences": {
    "segments": [{ "materialName": "Carbon Steel", "scheduleType": "40" }],
    "fittings": [{ "familyName": "Elbow", "typeName": "Standard" }]
  }
}
```

---

## Out of Scope (Phase 3)

- System Families группы C (Railings, Stairs), D (Curtain)
- Cable Trays, Conduits
- Nested Families dependencies
- Серверный Material Mapping

---

## Dependencies

- **Requires**: `001-family-library-mvp` (все модули MVP)
- Revit API для RoutingPreferences
- Expanded System Families coverage
