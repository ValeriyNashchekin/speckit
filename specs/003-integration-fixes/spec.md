# Feature Specification: Integration Fixes

**Feature Branch**: `003-integration-fixes`
**Created**: 2026-02-18
**Status**: Draft
**Prerequisites**: `001-family-library-mvp` completed, `002-family-library-phase2` completed

---

## Overview

Данная фича исправляет пробелы между полным ТЗ (specification.md) и реализацией MVP. Основные проблемы:

1. **Categories/Tags Frontend** — backend существует, frontend только заглушки
2. **Role-Editor Integration** — dropdown для Category есть, но без данных; Tags multi-select отсутствует
3. **Library Filters** — фильтр по Tags не работает
4. **Type Selection Window** — требуется верификация

---

## User Scenarios & Testing

### User Story 1 - Manage Categories (Priority: P1)

Как Администратор, я хочу управлять категориями ролей через отдельную страницу, чтобы организовать роли в логические группы.

**Independent Test**: Открыть страницу Categories → создать категорию "Furniture" → использовать её при создании роли.

**Acceptance Scenarios**:

1. **Given** Администратор на странице Categories, **When** открывает страницу, **Then** видит таблицу категорий с колонками Name, Description, SortOrder
2. **Given** Администратор нажимает "New Category", **When** вводит Name="Furniture" Description="Мебель", **Then** категория создана
3. **Given** категория "Furniture" существует, **When** Администратор редактирует Description, **Then** изменения сохранены
4. **Given** категория используется ролью, **When** пытается удалить, **Then** ошибка "Нельзя удалить категорию с привязанными ролями"
5. **Given** Администратор создаёт роль, **When** открывает dropdown Category, **Then** видит все созданные категории

---

### User Story 2 - Manage Tags (Priority: P1)

Как Администратор, я хочу управлять тегами ролей через отдельную страницу, чтобы помечать роли для фильтрации.

**Independent Test**: Открыть страницу Tags → создать тег "Standard" → использовать его при создании роли.

**Acceptance Scenarios**:

1. **Given** Администратор на странице Tags, **When** открывает страницу, **Then** видит таблицу тегов с колонками Name, Color
2. **Given** Администратор нажимает "New Tag", **When** вводит Name="Standard" Color="#3B82F6", **Then** тег создан
3. **Given** тег "Standard" существует, **When** Администратор редактирует Color, **Then** изменения сохранены
4. **Given** тег используется ролью, **When** пытается удалить, **Then** ошибка "Нельзя удалить тег с привязанными ролями"
5. **Given** Администратор создаёт роль, **When** открывает multi-select Tags, **Then** видит все созданные теги

---

### User Story 3 - Role Editor Integration (Priority: P1)

Как Администратор, я хочу выбирать категорию и теги при создании/редактировании роли.

**Independent Test**: Создать роль → выбрать категорию → выбрать 2 тега → сохранить → данные сохранены.

**Acceptance Scenarios**:

1. **Given** Администратор создаёт роль, **When** открывает dropdown Category, **Then** видит категории загруженные из API
2. **Given** Администратор создаёт роль, **When** выбирает категорию "Furniture", **Then** categoryId сохранён
3. **Given** Администратор создаёт роль, **When** открывает Tags multi-select, **Then** видит теги загруженные из API
4. **Given** Администратор создаёт роль, **When** выбирает теги "Standard" и "Priority", **Then** tagIds сохранены
5. **Given** роль с категорией и тегами, **When** редактирует роль, **Then** видит выбранные значения в форме

---

### User Story 4 - Library Tags Filter (Priority: P2)

Как Проектировщик, я хочу фильтровать библиотеку по тегам для быстрого поиска.

**Independent Test**: Открыть библиотеку → выбрать тег "Standard" → отображаются только роли с этим тегом.

**Acceptance Scenarios**:

1. **Given** библиотека открыта, **When** открывает фильтр Tags, **Then** видит multi-select с доступными тегами
2. **Given** фильтр Tags открыт, **When** выбирает тег "Standard", **Then** список фильтруется по выбранному тегу
3. **Given** выбрано 2 тега, **When** применяет фильтр, **Then** отображаются роли с ЛЮБЫМ из выбранных тегов (OR logic)

---

### User Story 5 - Type Selection Window Verification (Priority: P3)

Как БИМ-менеджер/Проектировщик, я хочу выбирать типы из TXT файла при загрузке семейства.

**Acceptance Scenarios**:

1. **Given** семейство с TXT файлом, **When** загружается, **Then** открывается диалог выбора типов
2. **Given** диалог открыт, **When** видит параметры, **Then** колонки созданы из заголовков TXT динамически
3. **Given** диалог открыт, **When** использует поиск, **Then** фильтрация работает по TypeName и значениям
4. **Given** диалог открыт, **When** использует Ctrl/Shift для выбора, **Then** множественный выбор работает
5. **Given** параметры "Comment", "GPM" в TXT, **When** диалог открыт, **Then** эти параметры скрыты

---

## Requirements

### Functional Requirements

**Categories Frontend:**
- FR-001: Система ДОЛЖНА показывать список категорий в таблице с Name, Description, SortOrder
- FR-002: Система ДОЛЖНА позволять создавать/редактировать/удалять категории через UI
- FR-003: Система НЕ ДОЛЖНА позволять удалять категории с привязанными ролями
- FR-004: Категории ДОЛЖНЫ загружаться из API при открытии role-editor

**Tags Frontend:**
- FR-005: Система ДОЛЖНА показывать список тегов в таблице с Name, Color
- FR-006: Система ДОЛЖНА позволять создавать/редактировать/удалять теги через UI
- FR-007: Система НЕ ДОЛЖНА позволять удалять теги с привязанными ролями
- FR-008: Теги ДОЛЖНЫ загружаться из API при открытии role-editor
- FR-009: Role-editor ДОЛЖЕН использовать multi-select для выбора нескольких тегов

**Library Filters:**
- FR-010: Фильтр Tags в библиотеке ДОЛЖЕН загружать теги из API
- FR-011: Фильтрация ДОЛЖНА работать по OR logic (любой из выбранных)

**Type Selection (Verification):**
- FR-012: Диалог выбора типов ДОЛЖЕН создавать колонки динамически из TXT
- FR-013: Параметры Comment/GPM/Legacy Part Number ДОЛЖНЫ быть скрыты

### Key Entities

Используются существующие сущности из MVP:
- **Category**: Id, Name, Description, SortOrder
- **Tag**: Id, Name, Color

---

## Entity Coverage Matrix

| Entity | Role | API Endpoints | UI Exposure | Story |
|--------|------|---------------|-------------|-------|
| Category | Primary (US1) | Full CRUD | List, Editor | US1 |
| Category | Supporting (US3) | GET only | Selector | US3 |
| Tag | Primary (US2) | Full CRUD | List, Editor | US2 |
| Tag | Supporting (US3) | GET only | Multi-Selector | US3 |
| Tag | Supporting (US4) | GET only | Filter Dropdown | US4 |

---

## Technical Context

### Existing Backend (Already Implemented)

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/categories` | GET | Получить все категории |
| `/api/categories` | POST | Создать категорию |
| `/api/categories/{id}` | PUT | Обновить категорию |
| `/api/categories/{id}` | DELETE | Удалить категорию |
| `/api/tags` | GET | Получить все теги |
| `/api/tags` | POST | Создать тег |
| `/api/tags/{id}` | PUT | Обновить тег |
| `/api/tags/{id}` | DELETE | Удалить тег |

### New Frontend Components

| Component | Path | Description |
|-----------|------|-------------|
| `categories.service.ts` | `src/FamilyLibrary.Web/src/app/features/categories/services/` | API client |
| `category-editor.component.ts` | `src/FamilyLibrary.Web/src/app/features/categories/components/` | Create/Edit dialog |
| `tags.service.ts` | `src/FamilyLibrary.Web/src/app/features/tags/services/` | API client |
| `tag-editor.component.ts` | `src/FamilyLibrary.Web/src/app/features/tags/components/` | Create/Edit dialog |
| `tag-multi-select.component.ts` | `src/FamilyLibrary.Web/src/app/shared/components/` | Reusable multi-select |

### Integration Points

| Parent Component | Child Component | Data Flow |
|------------------|-----------------|-----------|
| role-list | role-editor | categories[], tags[] loaded from API |
| role-editor | category-select | categoryId in/out |
| role-editor | tag-multi-select | tagIds[] in/out |
| library-filters | tag-multi-select | selectedTagIds → filter query |

---

## Out of Scope

- Nested Families (Module 8) — требует отдельной фазы
- Import rules from Excel verification — low priority
- Cards view in library — not related to integration

---

## Dependencies

- `001-family-library-mvp` — backend entities and API
- `002-family-library-phase2` — scanner, changelog (не влияет на integration)

---

## Success Criteria

- SC-001: Администратор может создать категорию и использовать её при создании роли
- SC-002: Администратор может создать тег и использовать его при создании роли
- SC-003: Проектировщик может фильтровать библиотеку по тегам
- SC-004: Диалог выбора типов работает корректно с TXT файлами
