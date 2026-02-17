# Feature Specification: Family Library Phase 3

**Feature Branch**: `003-family-library-phase3`
**Created**: 2026-02-17
**Status**: Draft
**Input**: Family Library Phase 3 — Nested Families, Complex System Families, Material Mapping
**Depends On**: `002-family-library-phase2`

---

## Overview

Phase 3 завершает систему Family Library поддержкой сложных случаев:

1. **Nested Families** — зависимости между родительскими и вложенными семействами
2. **Complex System Families** — Railings, Stairs, Curtain Systems, Cable Trays
3. **Серверный Material Mapping** — автоматический маппинг материалов между шаблоном и проектами

---

## Scope Summary

### Module 8: Nested Families

| Фича | Описание |
|------|----------|
| Shared Nested Detection | Определение Shared вложенных семейств при Publish |
| Dependency Tracking | Хранение зависимостей в метаданных родителя |
| Load Order | Контроль порядка загрузки (parent → nested) |
| Version Conflicts | UI для разрешения конфликтов версий вложенных |
| "Used In" UI | Отображение где вложенное семейство используется |

### System Families Groups C, D

| Группа | Категории | Сложность |
|--------|-----------|-----------|
| **C** | Railings, Stairs, Top Rails, Handrails | Иерархическая структура, зависимости от Loadable |
| **D** | Curtain Walls, Curtain Systems, Mullions | Сетка, панели, профили |
| **B (extended)** | Cable Trays, Conduits | MEP расширение |

### Material Mapping Server

| Фича | Описание |
|------|----------|
| MaterialMapping table | TemplateMaterialName → ProjectMaterialName |
| Auto-mapping | При Pull Update автоматическая замена материалов |
| Manual override | UI для ручного маппинга |

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Manage Nested Families (Priority: P1)

Как БИМ-менеджер, я хочу видеть какие вложенные семейства содержит родительское, чтобы контролировать зависимости.

**Acceptance Scenarios**:

1. При Publish родительского семейства система определяет Shared nested families
2. В Tab 2 показывается колонка "Dependencies" с количеством вложенных
3. При клике открывается список вложенных с их статусами (в библиотеке / не опубликовано)
4. Warning если Shared nested не опубликован в библиотеке

### User Story 2 - Load Family with Nested (Priority: P1)

Как Проектировщик, я хочу загрузить семейство со всеми вложенными, чтобы они работали корректно.

**Acceptance Scenarios**:

1. Pre-Load Summary показывает все вложенные и их версии
2. Если в проекте уже есть nested более старой версии — предложение обновить
3. Если в библиотеке nested более новой версии чем в RFA — предложение обновить из библиотеки
4. Конфликт версий разрешается выбором пользователя

### User Story 3 - Manage Complex System Families (Priority: P2)

Как БИМ-менеджер, я хочу управлять Railings и Curtain Walls в библиотеке.

**Acceptance Scenarios**:

1. RailingType сериализуется с dependencies на Loadable Families (balusters)
2. Pull Update проверяет наличие зависимых Loadable Families в проекте
3. Curtain Wall сериализуется с grid, panels, mullions
4. Stacked Wall сериализуется с ссылками на дочерние WallType

### User Story 4 - Auto Material Mapping (Priority: P2)

Как БИМ-менеджер, я хочу настроить маппинг материалов, чтобы при Pull Update материалы автоматически заменялись.

**Acceptance Scenarios**:

1. Таблица MaterialMapping: TemplateMaterialName → ProjectMaterialName
2. При Pull Update материалы автоматически маппятся
3. Если маппинг не найден — показывается warning как в MVP
4. UI для управления маппингами (CRUD)

---

## Requirements *(mandatory)*

### Functional Requirements

**Модуль 8: Nested Families**

- **FR-301**: При Publish ДОЛЖЕН определяться список Shared nested families
- **FR-302**: Зависимости ДОЛЖНЫ храниться в метаданных родителя
- **FR-303**: UI библиотеки ДОЛЖЕН показывать бейдж 🔗 для семейств с зависимостями
- **FR-304**: Pre-Load Summary ДОЛЖЕН показывать версии nested (в RFA vs в библиотеке vs в проекте)
- **FR-305**: IFamilyLoadOptions.OnSharedFamilyFound ДОЛЖЕН контролировать источник nested
- **FR-306**: Nested семейство БЕЗ роли ДОЛЖНО записываться с roleName: null
- **FR-307**: Сканер ДОЛЖЕН показывать колонку "Nested In" для Shared nested

**System Families Groups C, D**

- **FR-311**: RailingType ДОЛЖЕН сериализоваться с railingStructure и dependencies
- **FR-312**: StairType ДОЛЖЕН поддерживаться (Phase 3 scope)
- **FR-313**: CurtainWallType ДОЛЖЕН сериализоваться с grid/panels/mullions
- **FR-314**: StackedWallType ДОЛЖЕН сериализоваться с ссылками на дочерние WallType
- **FR-315**: Pull Update ДОЛЖЕН проверять наличие зависимых Loadable Families

**Material Mapping**

- **FR-321**: Таблица MaterialMapping ДОЛЖНА хранить маппинги per project
- **FR-322**: При Pull Update ДОЛЖЕН применяться авто-маппинг материалов
- **FR-323**: UI ДОЛЖЕН позволять управлять маппингами
- **FR-324**: Если маппинг не найден — fallback на MVP поведение (warning + варианты)

### Key Entities

- **FamilyDependency**: ParentFamilyId, NestedFamilyName, NestedRoleName, IsShared, InLibrary, LibraryVersion
- **MaterialMapping**: TemplateMaterialName, ProjectMaterialName, ProjectId, CreatedAt

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-301**: 95% Nested families корректно загружаются с зависимостями
- **SC-302**: Material Mapping покрывает 80% типовых материалов автоматически
- **SC-303**: Complex System Families покрывают 90% случаев использования

---

## Technical Context

### JSON Structure (Group C - Railings)

```json
{
  "typeName": "Railing_Glass_900",
  "category": "Railings",
  "systemFamily": "Railing",
  "parameters": { "Height": 900 },
  "railingStructure": {
    "topRailTypeName": "Circular - 50mm",
    "balusterPlacement": {
      "pattern": [{ "balusterFamilyName": "Baluster-Round", "balusterTypeName": "25mm" }]
    }
  },
  "dependencies": [
    { "familyName": "Baluster-Round", "typeName": "25mm", "inLibrary": true }
  ]
}
```

### JSON Structure (Group D - Curtain Wall)

```json
{
  "typeName": "Curtain_Wall_Storefront",
  "kind": "Curtain",
  "grid": { "horizontalSpacing": 1200, "verticalSpacing": 2400 },
  "panels": { "defaultPanelTypeName": "System Panel" },
  "mullions": { "horizontalMullion": "Rectangular Mullion", "verticalMullion": "Rectangular Mullion" }
}
```

### API Endpoints (Additions)

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/families/{id}/dependencies` | GET | Список зависимостей |
| `/api/material-mappings` | GET/POST/PUT/DELETE | CRUD маппингов |

---

## Out of Scope

- In-Place Families
- Linked Models families
- Custom geometry generation

---

## Dependencies

- **Requires**: `002-family-library-phase2`
- Revit API для Curtain Systems, Railings, Stairs
- Expanded dependency tracking in DB
