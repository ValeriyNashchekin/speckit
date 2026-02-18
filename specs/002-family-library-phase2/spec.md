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

**Scanner Pages:**

| Страница | Назначение | Доступ |
|----------|------------|--------|
| **Add to Library** | Загрузка семейств из проекта в библиотеку | БИМ-менеджер, Администратор |
| **Update from Library** | Обновление семейств в проекте из библиотеки | Все роли |

**Acceptance Scenarios**:

1. **Given** пользователь в любом проекте (не только шаблон), **When** выполняет команду "Update Families from Library", **Then** открывается страница "Update from Library"
2. **Given** сканер открыт, **When** сканирование завершено, **Then** отображается таблица: Family Name, Category, Current Role, Status, Actions
3. **Given** сканер показывает результаты, **When** фильтрует по статусу "Update Available", **Then** показаны только семейства с доступными обновлениями
4. **Given** выбраны семейства со статусом "Update Available", **When** нажимает "Update Selected", **Then** все выбранные обновлены, показан progress bar
5. **Given** семейство без клейма найдено по Legacy Recognition, **When** статус "Legacy Match", **Then** колонка Role показывает "(auto: RoleName)"
6. **Given** БИМ-менеджер на странице "Update from Library", **When** выбирает Legacy/Unmatched семейства, **Then** доступна кнопка "Stamp Selected"
7. **Given** проект с 5000+ семейств, **When** сканирование завершено, **Then** virtual scroll обеспечивает отзывчивость UI

**Scan Algorithm:**

```
Для каждого семейства в проекте:
  1. Проверить Extensible Storage
     - Есть RoleName + ContentHash? → Stamped
     - Нет? → Legacy check

  2. Если Stamped:
     - Запросить версию из библиотеки по RoleName
     - Сравнить Hash
     - Hash совпадает → Up to date
     - Hash отличается → Update Available

  3. Если Legacy:
     - Применить правила распознавания к FamilyName
     - Найдено совпадение? → Legacy Match (авто-роль)
     - Не найдено? → Unmatched
```

**Statuses**:

| Статус | Описание | Действия |
|--------|----------|----------|
| 🟢 Up to date | Hash совпадает с библиотекой | — |
| 🟡 Update Available | В библиотеке новая версия | Update |
| 🔵 Legacy Match | Найдено по правилам имени | Update, Stamp (БИМ-менеджер) |
| ⚪ Unmatched | Не найдено | Stamp с ручным выбором роли (БИМ-менеджер) |

---

### User Story 2 - View Change History (Priority: P1)

Как пользователь, я хочу видеть историю изменений семейства по версиям (changelog), чтобы понимать что изменилось.

**Why this priority**: Прозрачность изменений — важно для доверия к системе.

**Independent Test**: Открыть страницу семейства → посмотреть changelog → увидеть diff между версиями.

**Categories of Tracked Changes:**

| Категория | Изменение | Детектирование |
|-----------|-----------|----------------|
| ✏️ **Имя** | Семейство переименовано | Сравнить FamilyName |
| 📁 **Категория** | Изменилась категория Revit | Сравнить Category |
| ➕➖ **Типы** | Добавлен/удалён тип | Сравнить список TypeName |
| 📝 **Параметры** | Добавлен/удалён/изменён параметр | Сравнить ParameterName + Value |
| 🔧 **Геометрия** | Изменилась геометрия | Факт изменения (по Hash) |
| 📄 **TXT** | Изменился Type Catalog | Сравнить строки/хеш |

**Acceptance Scenarios**:

1. **Given** пользователь на странице семейства, **When** просматривает changelog, **Then** видит список версий с датами и авторами
2. **Given** changelog отображается, **When** смотрит Version 2 → Version 3, **Then** видит: ✏️ Name changed, ➕ Type added, 📝 Parameter changed, 🔧 Geometry changed
3. **Given** changelog для версии, **When** нажимает "Show Details", **Then** разворачивается детальный diff параметров
4. **Given** CommitMessage заполнен при Publish, **When** просматривает версию, **Then** видит описание изменений
5. **Given** изменилась категория Revit, **When** просматривает changelog, **Then** видит 📁 Category changed
6. **Given** изменился Type Catalog, **When** просматривает changelog, **Then** видит 📄 TXT changed

**Changelog Format:**

```
Version 3 → Version 4 (2026-02-15 by admin@freeaxez.com)
├── ✏️ Name: "FreeAxez_Table_v2" → "FreeAxez_Table_v3"
├── ➕ Type added: "Type_D"
├── 📝 Parameter changed: "Height" 800 → 900
└── 🔧 Geometry changed

Version 2 → Version 3 (2026-02-10 by bim@freeaxez.com)
├── ➕ Type added: "Type_C"
└── 📝 Parameter changed: "Width" 500 → 600
```

---

### User Story 3 - See Local Changes Before Publish (Priority: P2)

Как БИМ-менеджер, я хочу видеть локальные изменения семейства до Publish, чтобы понимать что будет опубликовано.

**Why this priority**: Контроль изменений перед публикацией.

**Independent Test**: Редактировать семейство → открыть Queue → увидеть "Local Modified" → посмотреть diff.

**Local Modified Detection:**
- Вычисляется при открытии страницы Add to Library
- Текущий Hash ≠ Hash в Extensible Storage → статус "Local Modified"

**Acceptance Scenarios**:

1. **Given** семейство в Queue, **When** ContentHash изменился, **Then** статус "Local Modified", показан icon изменений
2. **Given** семейство со статусом "Local Modified", **When** нажимает "View Changes", **Then** открывается modal с diff
3. **Given** modal с изменениями, **When** просматривает, **Then** видит: ✏️ Name change, ➕ Types added/removed, 📝 Parameters changed, 🔧 Geometry flag
4. **Given** modal открыт, **When** нажимает "Discard Changes", **Then** предупреждение о потере изменений
5. **Given** изменился Type Catalog, **When** просматривает View Changes, **Then** видит 📄 TXT changed

**View Changes Modal Format:**

```
Локальные изменения (не опубликованы):

✏️ Name: "FreeAxez_Table" → "FreeAxez_Table_v2"
➕ Type added: "Type_D" (всего типов: 4)
📝 Parameter changed:
   • Height: 800 → 900
   • Material: "Oak" → "Pine"
🔧 Geometry: изменена
📄 TXT: обновлён (3 строки изменены)

[Discard Changes] [Publish]
```

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
5. **Given** Pre-Update Preview открыт, **When** нажимает "Cancel", **Then** обновление отменяется без изменений

**Pre-Update Preview Format:**

```
Обновить BaseUnit v2 → v4?

Изменения:
• ✏️ Имя изменено
• ➕ Добавлено 2 типа
• 📝 Изменено 3 параметра
• 🔧 Геометрия обновлена
• 📄 TXT обновлён

[Показать детали] [Update] [Cancel]
```

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
- **FR-208**: Две страницы сканера ДОЛЖНЫ быть доступны: "Add to Library" (БИМ-менеджер) и "Update from Library" (все роли)
- **FR-209**: Stamp Selected ДОЛЖЕН быть доступен БИМ-менеджеру для Legacy/Unmatched семейств
- **FR-210**: Batch запросы к бэкенду ДОЛЖНЫ использоваться для производительности (не по одному)

**Модуль 7: Change Tracking**

- **FR-211**: SnapshotJSON ДОЛЖЕН сохраняться при каждом Publish
- **FR-212**: Changelog ДОЛЖЕН показывать diff между версиями: Name, Category, Types, Parameters, Geometry, TXT
- **FR-213**: Local Changes ДОЛЖНЫ детектироваться по сравнению Hash в ES и текущего Hash
- **FR-214**: Modal "View Changes" ДОЛЖЕН показывать diff для Local Modified
- **FR-215**: Pre-Update Preview ДОЛЖЕН показываться перед Update
- **FR-216**: Иконки изменений ДОЛЖНЫ быть: ✏️ Name, 📁 Category, ➕➖ Types, 📝 Parameters, 🔧 Geometry, 📄 TXT
- **FR-217**: CommitMessage ДОЛЖЕН отображаться в истории версий
- **FR-218**: SnapshotJSON ДОЛЖЕН содержать: version, familyName, category, types, parameters, hasGeometryChanges, txtHash

**Модуль: System Families (Phase 2)**

- **FR-221**: Группа A (Roofs, Ceilings, Foundations) ДОЛЖНА поддерживаться аналогично Walls/Floors
- **FR-222**: Группа B (Pipes, Ducts) ДОЛЖНА сериализовать routingPreferences в JSON
- **FR-223**: Pull Update для MEP типов ДОЛЖЕН обновлять RoutingPreferences
- **FR-224**: Фитинги в routingPreferences ДОЛЖНЫ маппиться по имени

### Key Entities

- **ChangeSnapshot**: FamilyVersionId, SnapshotJSON, ChangedFields (computed)
- **LocalChange**: FamilyId, Changes (name, category, types, parameters, geometry, txt), DetectedAt

### Snapshot JSON Structure

```json
{
  "version": 2,
  "familyName": "FreeAxez_Table_v2",
  "category": "Furniture",
  "types": ["Type_A", "Type_B", "Type_C"],
  "parameters": [
    {"name": "Width", "value": "600", "group": "Dimensions"},
    {"name": "Height", "value": "800", "group": "Dimensions"}
  ],
  "hasGeometryChanges": true,
  "txtHash": "abc123..."
}
```

### Scanner Performance

- **Virtual scroll** для таблиц (5000+ семейств)
- **Batch запросы** к бэкенду (не по одному)
- **Кеширование** правил распознавания на клиенте

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
| `/api/families/batch-check` | POST | Массовая проверка статусов (по списку RoleName + Hash) |
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
