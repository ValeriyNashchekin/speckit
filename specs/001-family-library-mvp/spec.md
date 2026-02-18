# Feature Specification: Family Library MVP

**Feature Branch**: `001-family-library-mvp`
**Created**: 2026-02-17
**Status**: Draft
**Input**: Family Library для FreeAxez — система управления библиотекой семейств Revit с версионированием, клеймлением и распознаванием

---

## Overview

Family Library — корпоративная система управления семействами Revit для компании FreeAxez. Система решает три ключевые проблемы:

1. **Распознавание при смене имён** — неизменяемая функциональная роль "прошивается" в семейство
2. **Версионирование** — SHA256-хеш на основе геометрии и параметров, отслеживание версий
3. **Каталогизация** — централизованная библиотека с метаданными, превью и поиском

**MVP Scope:** Базовая инфраструктура + Loadable Families + System Families (группы A, E)

---

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Create and Manage Family Roles (Priority: P1)

Как Администратор, я хочу создавать роли семейств с неизменяемыми системными именами (включая пакетный импорт через текст или Excel), чтобы быстро настроить систему под все семейства в шаблоне и гарантировать стабильность связей.

**Why this priority**: Роли — фундамент всей системы. Без них невозможно Stamp, Publish, распознавание. Это первая вещь, которую нужно настроить.

**Independent Test**: Создать роль "FreeAxez_Table" → проверить что Name read-only после создания → создать 10 ролей через текстовое поле → импортировать роли из Excel.

**Acceptance Scenarios**:

1. **Given** Администратор авторизован, **When** создаёт роль с Name="FreeAxez_Table" Description="Стол", **Then** роль создана, Name read-only, Type по умолчанию Loadable
2. **Given** Администратор на странице создания ролей, **When** вводит список имён в текстовое поле (по одному на строку) и нажимает "Создать", **Then** создано N ролей с пустыми Description/Category/Tags
3. **Given** Администратор загружает Excel с колонками Name/Type/Description/Category/Tags, **When** подтверждает импорт, **Then** все роли созданы, категории/теги созданы если не существовали
4. **Given** роль "FreeAxez_Table" уже существует, **When** Администратор пытается создать роль с таким же Name, **Then** дубликат пропускается без ошибки
5. **Given** к роли привязаны семейства, **When** Администратор пытается удалить роль, **Then** ошибка "Нельзя удалить роль с привязанными семействами"
6. **Given** Администратор на UI управления ролями, **When** открывает страницу, **Then** видит две вкладки: "Loadable Roles" и "System Roles"
7. **Given** роль создана с Type="Loadable", **When** пытается изменить Type, **Then** Type read-only

---

### User Story 2 - Configure Name Recognition Rules (Priority: P1)

Как Администратор, я хочу настраивать правила распознавания имён для каждой роли, чтобы система могла автоматически сопоставлять семейства из старых проектов с ролями по их именам файлов.

**Why this priority**: Legacy Recognition критичен для миграции существующих проектов. Без него старые проекты не смогут работать с библиотекой.

**Independent Test**: Создать правило "(FB OR Desk) AND Wired" для роли → протестировать на имени "FB_Field_Wired_v2" → правило соответствует.

**Acceptance Scenarios**:

1. **Given** Администратор на странице роли "FreeAxez_Table", **When** создаёт правило в Visual mode: AND(Contains:"Table", Contains:"FreeAxez"), **Then** правило сохранено
2. **Given** правило создано в Visual mode, **When** переключается в Formula mode, **Then** формула показывает "Table AND FreeAxez"
3. **Given** Администратор редактирует формулу в Formula mode, **When** вводит "(FB OR (Desk AND Mount)) AND Field AND Wired", **Then** формула парсится и синхронизируется с Visual mode
4. **Given** Администратор нажимает "Test" на правиле, **When** вводит тестовое имя "FB_Field_Wired_v2", **Then** результат "Соответствует"
5. **Given** Администратор нажимает "Test" на правиле, **When** вводит тестовое имя "Kitchen_Chair", **Then** результат "Не соответствует"
6. **Given** роль A с правилом "Table" и роль B с правилом "Desk", **When** Администратор проверяет конфликты, **Then** warning если имя подходит под оба правила
7. **Given** конфликт правил обнаружен, **When** Администратор продолжает сохранение, **Then** правило сохранено с warning
8. **Given** Excel с колонками RoleName/RuleFormula, **When** импортируется, **Then** правила валидируются, проверяются на конфликты, применяются

---

### User Story 3 - Manage Loadable Families in Template (Priority: P1)

Как БИМ-менеджер, я хочу видеть семейства из шаблона, выбрать те которые нужно добавить в библиотеку, и управлять их привязкой к ролям и публикацией.

**Why this priority**: Это основной рабочий процесс БИМ-менеджера — наполнение библиотеки контентом.

**Independent Test**: Открыть шаблон → выбрать семейство → выбрать роль → Stamp → Publish → семейство в библиотеке.

**Acceptance Scenarios**:

**Tab 1: All Families**

1. **Given** БИМ-менеджер на Tab 1 "All Families", **When** открывает страницу, **Then** видит все семейства из шаблона с колонками FamilyName, Revit Category
2. **Given** БИМ-менеджер на Tab 1, **When** выбирает семейства и нажимает "Add to Queue", **Then** семейства перемещены в Tab 2, Draft создан в базе, Статус = "New"

**Tab 2: Library Queue**

3. **Given** семейство в Tab 2 со статусом "New", **When** выбирает роль из dropdown, **Then** Статус = "Role Selected"
4. **Given** семейство в Tab 2 со статусом "Role Selected", **When** нажимает "Stamp", **Then** ES записан (RoleName, ContentHash), Статус = "Stamped"
5. **Given** семейство в Tab 2 со статусом "Stamped", **When** нажимает "Publish", **Then** RFA загружен в Azure Blob, метаданные в базе, ES обновлен (Version), Draft удалён, семейство перемещено в Tab 3
6. **Given** несколько семейств в Tab 2, **When** нажимает "Stamp Selected" / "Publish Selected", **Then** массовая операция выполнена

**Hash and Duplicates**

7. **Given** семейство "FreeAxez_Table" опубликовано, **When** БИМ-менеджер редактирует содержимое и Stamp снова, **Then** ContentHash изменился, PreviousHash записан, Статус = "Local Modified"
8. **Given** копия семейства с идентичным ContentHash (без изменения содержимого), **When** добавляется в Queue, **Then** warning "Это дубликат семейства X. Создать новую запись?"
9. **Given** копия семейства с изменённым содержимым, **When** добавляется в Queue, **Then** warning "Семейство имеет роль X, но контент отличается"

**Tab 3: Library Status**

10. **Given** семейство опубликовано в библиотеке, **When** открывает Tab 3, **Then** видит Local Version, Library Version, Status (Up to date / Update Available)
11. **Given** в библиотеке новая версия семейства, **When** нажимает "Pull Update", **Then** семейство обновлено из библиотеки

---

### User Story 4 - Manage System Families in Template (Priority: P2)

Как БИМ-менеджер, я хочу управлять типами системных семейств (стены, перекрытия, уровни) в библиотеке, чтобы стандартизировать параметры.

**Why this priority**: System Families — важная часть шаблона. MVP покрывает группы A (CompoundStructure: Walls, Floors, Roofs, Ceilings) и E (простые: Levels, Grids, Ramps).

**Independent Test**: Создать роль для WallType → Stamp → Publish → JSON сохранён в базе.

**Acceptance Scenarios**:

**Tab 1: All System Types**

1. **Given** БИМ-менеджер на Tab 1 "All System Types", **When** открывает страницу, **Then** видит типы системных семейств с колонками Type Name, Category, Group (A-E)
2. **Given** БИМ-менеджер на Tab 1, **When** выбирает WallType "Wall_External_200" и нажимает "Add to Queue", **Then** тип перемещён в Tab 2, Статус = "New"

**Tab 2: Library Queue (System Types)**

3. **Given** System Type в Queue со статусом "Role Selected", **When** нажимает "Stamp", **Then** ES записан на ElementType (RoleName, ContentHash)
4. **Given** WallType в Queue со статусом "Stamped", **When** нажимает "Publish", **Then** CompoundStructure сериализована в JSON, JSON сохранён в базе, ES обновлен (Version)

**Tab 3: Library Status (System Types)**

5. **Given** WallType в проекте (v1), в библиотеке v2, **When** БИМ-менеджер нажимает "Pull Update", **Then** CompoundStructure обновлена, ES обновлен (Version = 2)
6. **Given** WallType НЕ в проекте, в библиотеке есть, **When** нажимает "Pull Update", **Then** тип создан через Duplicate от существующего WallType той же категории
7. **Given** WallType НЕ в проекте, в категории нет других типов, **When** нажимает "Pull Update", **Then** ошибка "Невозможно создать тип: нет базового типа для дублирования"

**Material Mapping**

8. **Given** Pull Update для WallType, материал "Brick, Common" не найден в проекте, **When** применяется, **Then** warning с вариантами: [Выбрать существующий] [Создать новый] [Использовать Default] [Пропустить]
9. **Given** пользователь выбрал "Пропустить" для отсутствующего материала, **When** применяется CompoundStructure, **Then** слой без материала

**Local Changes Detection**

10. **Given** Pull Update для WallType с локальными изменениями, **When** подтверждает, **Then** warning "Тип был изменён локально. Обновление перезапишет локальные изменения."
11. **Given** warning о локальных изменениях, **When** нажимает "Показать различия", **Then** отображается diff структуры слоёв (библиотека vs локальный)

**JSON Structure (Group A - CompoundStructure)**

12. **Given** WallType публикуется, **When** JSON генерируется, **Then** содержит: typeName, category, systemFamily, parameters, compoundStructure (layers, structuralLayerIndex, totalThickness)

**JSON Structure (Group E - Simple)**

13. **Given** Level публикуется, **When** JSON генерируется, **Then** содержит: typeName, category, systemFamily, parameters (без compoundStructure)

---

### User Story 5 - Browse Family Library (Priority: P1)

Как Проектировщик, я хочу просматривать корпоративную библиотеку семейств внутри Revit, чтобы найти и загрузить нужные семейства.

**Why this priority**: Основная ценность для конечных пользователей — доступ к библиотеке.

**Independent Test**: Открыть библиотеку → найти семейство по имени → открыть страницу деталей → увидеть историю версий.

**Acceptance Scenarios**:

**Open Library**

1. **Given** Проектировщик в Revit, **When** выполняет команду "Open Family Library", **Then** открывается WebView2 с библиотекой, передан контекст (projectId, userRole)
2. **Given** UI открыт в WebView2, **When** плагин готов, **Then** отправлено событие revit:ready с контекстом

**View Modes**

3. **Given** библиотека открыта, **When** переключает вид на "Карточки", **Then** семейства отображаются карточками с превью, название, роль, категория
4. **Given** библиотека открыта, **When** переключает вид на "Таблица", **Then** семейства отображаются списком с колонками Name/Role/Category/Tags/Version

**Filters and Search**

5. **Given** библиотека открыта, **When** вводит текст в поиск, **Then** фильтрация по имени (case-insensitive, поиск по подстроке)
6. **Given** библиотека открыта, **When** выбирает фильтр по категории, **Then** список фильтруется по выбранной категории
7. **Given** библиотека открыта, **When** выбирает фильтр по тегам (multi-select), **Then** список фильтруется по выбранным тегам
8. **Given** библиотека открыта, **When** выбирает фильтр по типу (Loadable/System), **Then** список фильтруется по типу

**Family Detail Page**

9. **Given** Проектировщик кликает на семейство в списке, **When** открывается страница деталей, **Then** отображаются: превью семейства, описание, роль, категория, теги
10. **Given** страница деталей открыта, **When** просматривает историю, **Then** видит таблицу версий с датами и CommitMessage
11. **Given** страница деталей открыта и семейство имеет Type Catalog, **When** выбирает версию, **Then** видит таблицу типов с параметрами
12. **Given** страница деталей открыта, **When** нажимает "Back to Library", **Then** возвращается к списку

**Performance**

13. **Given** библиотека с 5000+ семейств, **When** открывает страницу, **Then** virtual scroll обеспечивает 60 FPS

---

### User Story 6 - Load Family to Project (Priority: P1)

Как Проектировщик, я хочу загрузить семейство из библиотеки в текущий проект, чтобы использовать его в модели.

**Why this priority**: Конечная цель системы — использование семейств в проектах.

**Independent Test**: Найти семейство → нажать Load → семейство загружено в проект с оригинальным именем.

**Acceptance Scenarios**:

**Load without TXT**

1. **Given** Проектировщик на странице семейства без TXT, **When** нажимает "Load to Project", **Then** RFA скачан из Blob, переименован в OriginalFileName, загружен в Revit через LoadFamily()
2. **Given** семейство загружено, **When** загрузка завершена, **Then** сообщение "Family 'X' loaded with N type(s)."

**Load with TXT (Type Catalog)**

3. **Given** семейство с TXT файлом, **When** нажимает "Load to Project", **Then** RFA и TXT скачаны, переименованы в OriginalFileName/OriginalCatalogName
4. **Given** файлы скачаны, **When** TXT парсится, **Then** открывается диалог выбора типов (TypeSelectionWindow)
5. **Given** диалог выбора типов, **When** видит список, **Then** колонки созданы из заголовков TXT, динамически
6. **Given** диалог выбора типов, **When** использует поиск, **Then** фильтрация по TypeName и значениям параметров
7. **Given** диалог выбора типов, **When** использует фильтры по параметрам (ComboBox), **Then** фильтрация по выбранным значениям
8. **Given** диалог выбора типов, **When** выбирает 3 типа из 10 и подтверждает, **Then** загружены только выбранные типы через LoadFamilySymbol()
9. **Given** пользователь отменил выбор типов, **When** нажимает Cancel, **Then** операция aborted, временные файлы удалены

**Status Display in Library**

10. **Given** семейство НЕ в проекте, **When** открывает библиотеку, **Then** статус "Not in Project", кнопка "Load" доступна
11. **Given** семейство уже в проекте (та же версия), **When** открывает библиотеку, **Then** статус "Up to date", кнопка Load недоступна
12. **Given** семейство в проекте устарело (Hash отличается), **When** открывает библиотеку, **Then** статус "Update Available", кнопка "Update"
13. **Given** семейство без клейма но найдено по правилам распознавания, **When** открывает библиотеку, **Then** статус "Legacy Match", кнопка "Update"

**File Naming**

14. **Given** скачивание RFA, **When** файл сохранён, **Then** имя файла = OriginalFileName из FamilyVersion
15. **Given** скачивание TXT, **When** файл сохранён, **Then** имя файла = OriginalCatalogName, совпадает с именем RFA (без расширения)

---

### User Story 7 - Stamp and Publish from Family Editor (Priority: P2)

Как БИМ-менеджер, я хочу опубликовать семейство из Family Editor, не открывая основной шаблон.

**Why this priority**: Ускоряет рабочий процесс при редактировании семейств.

**Independent Test**: Открыть семейство в Family Editor → команда Publish → UI показывает только Tab 2 → Publish.

**Acceptance Scenarios**:

1. **Given** БИМ-менеджер в Family Editor, **When** выполняет команду "Publish to Library", **Then** открывается UI только с Tab 2 (Library Queue)
2. **Given** текущее семейство без клейма, **When** UI открыт, **Then** семейство автоматически добавлено в Queue, Статус = "New"
3. **Given** семейство с клеймом, хеш изменился, **When** UI открыт, **Then** Статус = "Stamped" / "Local Modified", готов к Publish
4. **Given** семейство уже опубликовано, хеш совпадает, **When** UI открыт, **Then** статус "Published", warning "Already up to date"
5. **Given** Tab 1 и Tab 3 в UI Family Editor, **When** открывается UI, **Then** Tab 1 и Tab 3 скрыты (не нужны)

---

### User Story 8 - Type Catalogs Management (Priority: P2)

Как БИМ-менеджер, я хочу прикреплять TXT файлы с каталогами типоразмеров к Loadable Families.

**Why this priority**: Type Catalogs — важная часть работы с большими семействами (много типов).

**Independent Test**: Publish семейство с TXT → проверить что TXT сохранён → загрузить в проект → диалог выбора типов.

**Acceptance Scenarios**:

**Publish with TXT**

1. **Given** БИМ-менеджер публикует семейство, **When** явно выбирает TXT файл для прикрепления, **Then** TXT сохранён рядом с RFA в Blob (системное имя catalog.txt), OriginalCatalogName сохранён
2. **Given** семейство с TXT публикуется, **When** хеш вычисляется, **Then** TotalHash = SHA256(RfaHash + TxtHash)
3. **Given** TXT обновляется для существующего семейства, **When** Publish, **Then** новая версия создаётся

**TXT Parsing**

4. **Given** TXT файл парсится, **When** заголовки содержат ParameterName##type##units, **Then** значения корректно парсятся в типы данных (LENGTH, INTEGER, TEXT)
5. **Given** TXT содержит quoted значения с запятыми, **When** парсится, **Then** значения корректно извлекаются (например, "Metal, Steel")
6. **Given** TXT файл, **When** парсится, **Then** автоопределение типа значения: double → int → string

**Download with TXT**

7. **Given** Проектировщик загружает семейство с TXT, **When** файлы скачаны, **Then** TXT переименован в OriginalCatalogName, имя совпадает с RFA (без расширения)

**UI Selection**

8. **Given** диалог выбора типов, **When** видит параметры, **Then** параметры "Comment", "GPM", "Legacy Part Number" скрыты
9. **Given** диалог выбора типов с 3 параметрами, **When** открывается, **Then** ширина окна 640px
10. **Given** диалог выбора типов с 10 параметрами, **When** открывается, **Then** ширина окна = paramCount * 180 (max 1600px)

---

### Edge Cases

**Worksharing**

- **Given** Worksharing проект, **When** Stamp на элемент занятый другим пользователем, **Then** warning "Element is being edited by [User]. Try again later."
- **Given** Worksharing проект, **When** Stamp на свободный элемент, **Then** Revit borrow элемент автоматически, Stamp выполняется

**Extensible Storage Loss**

- **Given** ES-данные потеряны (upgrade Revit, schema conflict), **When** сканирование, **Then** fallback cascade: серверный маппинг FamilyNameMapping → Legacy Recognition → ручной выбор
- **Given** Legacy Recognition нашёл совпадение, **When** пользователь подтверждает, **Then** авто-Stamp с найденной ролью

**Materials**

- **Given** Pull Update для WallType, материал не найден, **When** пользователь выбрал "Создать новый", **Then** пустой материал с нужным именем создан
- **Given** пользователь выбрал "Использовать Default", **When** CompoundStructure применяется, **Then** слой использует материал по умолчанию для категории

**File Operations**

- **Given** Publish файла > 50MB, **When** загрузка, **Then** прогресс-бар, timeout 60 сек
- **Given** сетевая ошибка при Publish, **When** retry, **Then** 3 попытки с экспоненциальным backoff

**Hash Issues**

- **Given** PartAtom XML не детерминирован (разный при каждом экспорте), **When** пользователь знает об изменениях, **Then** кнопка "Force Publish" позволяет создать версию
- **Given** хеш не изменился но содержимое изменилось (edge case), **When** пользователь подтверждает, **Then** Force Publish создаёт новую версию

**Concurrent Operations**

- **Given** два пользователя Publish одну роль одновременно, **When** оптимистичная блокировка, **Then** второй получает ошибку → retry
- **Given** конфликт версий, **When** retry, **Then** операция выполняется с новыми данными

---

## Requirements *(mandatory)*

### Functional Requirements

**Модуль 1: Управление ролями семейств**

- **FR-001**: Система ДОЛЖНА позволять создавать роли с Name и Description
- **FR-002**: Name роли ДОЛЖНО быть read-only после создания
- **FR-003**: Система ДОЛЖНА поддерживать пакетное создание ролей через текстовое поле (по имени на строку)
- **FR-004**: Система ДОЛЖНА поддерживать импорт ролей из Excel с колонками Name, Type, Description, Category, Tags
- **FR-005**: При импорте Excel новые категории/теги ДОЛЖНЫ создаваться с подтверждением
- **FR-006**: Дубликаты при создании ДОЛЖНЫ пропускаться без ошибки
- **FR-007**: Type роли (Loadable/System) ДОЛЖЕН быть read-only после создания
- **FR-008**: Роль ДОЛЖНА быть неудаляемой если к ней привязаны семейства
- **FR-009**: UI управления ролями ДОЛЖЕН иметь две вкладки: "Loadable Roles" и "System Roles"

**Модуль 2: Движок распознавания**

- **FR-010**: Система ДОЛЖНА поддерживать операции Contains и Not Contains для правил распознавания
- **FR-011**: Система ДОЛЖНА поддерживать сложную вложенность групп с условиями AND/OR
- **FR-012**: UI редактора правил ДОЛЖЕН иметь два режима: Visual (древовидный конструктор) и Formula (текстовое поле)
- **FR-013**: Режимы Visual и Formula ДОЛЖНЫ быть синхронизированы (изменения в одном отражаются в другом)
- **FR-014**: Система ДОЛЖНА поддерживать импорт правил из Excel с колонками RoleName, RuleFormula
- **FR-015**: При импорте правил ДОЛЖНА выполняться валидация синтаксиса формул
- **FR-016**: При импорте правил ДОЛЖНА выполняться проверка конфликтов
- **FR-017**: Кнопка "Test" ДОЛЖНА проверять правило на тестовом имени семейства
- **FR-018**: Кнопка "Check Conflicts" ДОЛЖНА проверять пересечения с правилами других ролей
- **FR-019**: Конфликты ДОЛЖНЫ показываться как warning (не блокировка)

**Модуль 3: Клеймление (Stamp)**

- **FR-020**: Stamp ДОЛЖЕН записывать в Extensible Storage: RoleName, ContentHash
- **FR-021**: Stamp ДОЛЖЕН быть локальной операцией (не требует сети)
- **FR-022**: При Stamp ДОЛЖЕН вычисляться ContentHash = SHA256(NormalizedPartAtomXML + BinaryStreamsHash)
- **FR-023**: FamilyName НЕ ДОЛЖЕН входить в ContentHash
- **FR-024**: При Stamp ДОЛЖЕН проверяться дубликат Hash по базе (опционально, если сеть доступна)
- **FR-025**: При обнаружении дубликата Hash ДОЛЖЕН показываться warning "Это дубликат семейства X"
- **FR-026**: ES схема ДОЛЖНА использовать стабильный GUID на весь жизненный цикл плагина
- **FR-027**: При breaking changes ДОЛЖНА создаваться новая схема с новым GUID, миграция из старой
- **FR-028**: При Worksharing ДОЛЖЕН проверяться ownership элемента перед записью ES через GetCheckoutStatus()
- **FR-029**: Stamp НЕ ДОЛЖЕН записывать Version (версия только при Publish)
- **FR-030**: Stamp НЕ ДОЛЖЕН создавать запись в базе данных (только локально в семействе)

**Модуль 4: Версионирование**

- **FR-031**: Система ДОЛЖНА вычислять ContentHash как гибридный хеш (PartAtom XML + OLE Streams)
- **FR-032**: Нормализация XML ДОЛЖНА включать: удаление timestamp элементов, сортировка элементов, унификация чисел
- **FR-033**: При Publish ДОЛЖНА создаваться новая версия если Hash изменился
- **FR-034**: Все версии ДОЛЖНЫ сохраняться в Azure Blob (история доступна для просмотра)
- **FR-035**: FamilyVersion ДОЛЖНА содержать: Id, FamilyId, Version, Hash, PreviousHash, BlobPath, CatalogBlobPath, OriginalFileName, OriginalCatalogName, CommitMessage, SnapshotJSON, PublishedAt, PublishedBy
- **FR-036**: При сканировании Hash из ES ДОЛЖЕН сравниваться с Hash последней версии в библиотеке
- **FR-037**: Для System Types ContentHash ДОЛЖЕН вычисляться как SHA256(NormalizedJSON)
- **FR-038**: Нормализация JSON ДОЛЖНА включать: сортировка ключей, числа без trailing zeros, материалы по имени

**Модуль 5: Type Catalog**

- **FR-039**: TXT файл ДОЛЖЕН парситься с поддержкой CSV синтаксиса (comma separator, quotes for values with commas)
- **FR-040**: Заголовки ДОЛЖНЫ парситься в формате ParameterName##DataType##Units
- **FR-041**: UI выбора типов ДОЛЖЕН показывать динамические колонки из TXT заголовков
- **FR-042**: UI выбора типов ДОЛЖЕН поддерживать: поиск по имени и параметрам, фильтры по параметрам, множественный выбор (Ctrl/Shift)
- **FR-043**: UI выбора типов ДОЛЖЕН показывать счётчик выбранных типов в реальном времени
- **FR-044**: Параметры с "Comment", "GPM", "Legacy Part Number" в имени ДОЛЖНЫ скрываться в UI
- **FR-045**: При загрузке с TXT ДОЛЖЕН вызываться LoadFamily() затем Delete() для невыбранных типов
- **FR-046**: TXT ДОЛЖЕН храниться рядом с RFA в Blob с системным именем catalog.txt
- **FR-047**: Оригинальное имя TXT ДОЛЖНО сохраняться в OriginalCatalogName
- **FR-048**: Адаптивный размер окна: 3 параметра → 640px, >3 параметров → paramCount * 180 (max 1600px)

**Модуль: System Families (MVP - группы A, E)**

- **FR-049**: Группа A (CompoundStructure: Walls, Floors, Roofs, Ceilings, Foundations) ДОЛЖНА сериализоваться в JSON со слоями
- **FR-050**: Группа E (простые: Levels, Grids, Ramps, Building Pads) ДОЛЖНА сериализоваться в JSON только с параметрами
- **FR-051**: При Publish System Type JSON ДОЛЖЕН сохраняться в базе (нет blob-файлов для System Types)
- **FR-052**: ES для System Types ДОЛЖЕН записываться на сам ElementType (не DataStorage)
- **FR-053**: При Pull Update материал ДОЛЖЕН искаться по имени в проекте, не по ElementId
- **FR-054**: Если материал не найден ДОЛЖЕН показываться warning с вариантами: выбрать существующий/создать/использовать default/пропустить
- **FR-055**: НЕ ДОЛЖНО создаваться автоматических заглушек материалов
- **FR-056**: Если System Type не существует в проекте ДОЛЖЕН создаваться через Duplicate от существующего типа той же категории
- **FR-057**: Если в категории нет типов для Duplicate ДОЛЖНА показываться ошибка "Невозможно создать тип"
- **FR-058**: При локальных изменениях System Type ДОЛЖЕН показываться warning с diff структуры
- **FR-059**: Stacked Wall (Kind.Stacked) и Curtain Wall (Kind.Curtain) НЕ поддерживаются в MVP

**Модуль: Библиотека и поиск**

- **FR-060**: Библиотека ДОЛЖНА поддерживать два вида отображения: Карточки и Таблица
- **FR-061**: Фильтры ДОЛЖНЫ включать: поиск по имени (free text), категория (dropdown), теги (multi-select), тип Loadable/System
- **FR-062**: Страница семейства ДОЛЖНА показывать: превью, описание, роль, категорию, теги, таблицу версий, таблицу типов
- **FR-063**: При скачивании файлы ДОЛЖНЫ переименовываться в OriginalFileName/OriginalCatalogName
- **FR-064**: Virtual scroll ДОЛЖЕН обеспечивать 60 FPS для таблиц с 5000+ строк

**Модуль: Интеграция Revit**

- **FR-065**: UI ДОЛЖЕН работать в WebView2 внутри Revit (встроенный режим)
- **FR-066**: UI ДОЛЖЕН работать в браузере (автономный режим, ограниченный функционал без Revit API)
- **FR-067**: Плагин ДОЛЖЕН поддерживать Revit 2020-2024 (.NET Framework 4.8) и Revit 2025-2026 (.NET 8)
- **FR-068**: WebView2 ДОЛЖЕН обмениваться событиями: revit:ready, revit:families:list, revit:system-types:list, ui:stamp, ui:publish, ui:load-family, ui:update-family, revit:operation-complete

**Модуль: Drafts и Queue**

- **FR-069**: Draft ДОЛЖЕН создаваться при Add to Queue с полями: FamilyName, FamilyUniqueId, SelectedRoleId, TemplateId, Status
- **FR-070**: Draft ДОЛЖЕН связываться с семейством по FamilyUniqueId (Element.UniqueId), не по имени
- **FR-071**: Draft ДОЛЖЕН удаляться после успешного Publish
- **FR-072**: Статусы Draft: New → Role Selected → Stamped → Published

**Модуль: Fallback и Recovery**

- **FR-073**: При потере ES ДОЛЖЕН срабатывать fallback: серверный FamilyNameMapping → Legacy Recognition
- **FR-074**: FamilyNameMapping ДОЛЖЕН обновляться при каждом сканировании проекта
- **FR-075**: При авто-восстановлении через Legacy Recognition ДОЛЖЕН показываться диалог подтверждения

### Key Entities

- **FamilyRole**: Id (GUID), Name (string, read-only after creation), Description (string?), Type (enum: Loadable/System, read-only), CategoryId (GUID?), Tags (List<Tag>)
- **Category**: Id (GUID), Name (string), Description (string?), SortOrder (int)
- **Tag**: Id (GUID), Name (string), Color (string)
- **RecognitionRule**: Id (GUID), RoleId (GUID), RootNode (JSON tree), Formula (string)
- **RecognitionNode**: Type (Group/Condition), Operator (AND/OR для Group, Contains/NotContains для Condition), Children (List<Node>), Value (string для Condition)
- **Family (Loadable)**: Id (GUID), RoleId (GUID), FamilyName (string), CurrentVersion (int), CreatedAt, UpdatedAt
- **FamilyVersion**: Id (GUID), FamilyId (GUID), Version (int), Hash (string), PreviousHash (string?), BlobPath (string), CatalogBlobPath (string?), OriginalFileName (string), OriginalCatalogName (string?), CommitMessage (string?), SnapshotJSON (string), PublishedAt, PublishedBy (string)
- **SystemType**: Id (GUID), RoleId (GUID), TypeName (string), Category (string), SystemFamily (string), Group (enum: A/B/C/D/E), JSON (string), CurrentVersion (int), Hash (string)
- **Draft**: Id (GUID), FamilyName (string), FamilyUniqueId (string), SelectedRoleId (GUID?), TemplateId (GUID), Status (enum: New/RoleSelected/Stamped/Published), CreatedAt, LastSeen
- **FamilyNameMapping**: Id (GUID), FamilyName (string), RoleName (string), ProjectId (GUID), LastSeenAt

---

## Success Criteria *(mandatory)*

### Measurable Outcomes

**Performance**

- **SC-001**: Администратор может создать 100 ролей через Excel импорт за 30 секунд
- **SC-002**: БИМ-менеджер может выполнить Stamp одного семейства за 1 секунду (локально)
- **SC-003**: БИМ-менеджер может опубликовать семейство (до 50MB) за 10 секунд
- **SC-004**: Проектировщик может найти семейство по имени за 2 секунды (пагинация 50 элементов)
- **SC-005**: Проектировщик может загрузить семейство в проект за 8 секунд
- **SC-006**: Сканирование проекта на 1000 семейств выполняется за 5 секунд
- **SC-007**: Batch проверка статусов для 500 семейств выполняется за 3 секунды
- **SC-008**: Virtual scroll обеспечивает 60 FPS для 5000+ строк

**Quality**

- **SC-009**: Legacy Recognition автоматически распознаёт ≥90% семейств из старых проектов
- **SC-010**: Точность правил распознавания: false positives <5%
- **SC-011**: Stamp成功率 ≥99%
- **SC-012**: Publish成功率 ≥98%

**Business**

- **SC-013**: Uptime API ≥99.5%
- **SC-014**: Время онбординга нового пользователя ≤1 день
- **SC-015**: Adoption rate ≥80% БИМ-менеджеров за 3 месяца

---

## Technical Context *(optional but recommended for MVP)*

### Architecture Overview

| Компонент | Технология |
|-----------|------------|
| Revit Plugin | C#, Revit API 2020-2026, WebView2 |
| Backend | .NET 10, EF Core 9, SQL Server 2025 |
| Frontend | Angular 21, PrimeNG, Tailwind CSS |
| Storage | Azure Blob (Azurite для MVP) |
| Local Storage | Revit Extensible Storage |

### API Endpoints (Backend)

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/api/roles` | GET | Получить все роли (фильтр по Type: Loadable/System) |
| `/api/roles` | POST | Создать роль (одиночно или пакетно) |
| `/api/roles/{id}` | PUT | Обновить роль (Description, Category, Tags) |
| `/api/roles/{id}` | DELETE | Удалить роль (только если нет привязанных семейств) |
| `/api/roles/import` | POST | Импорт ролей из Excel |
| `/api/categories` | GET/POST/PUT/DELETE | CRUD категорий |
| `/api/tags` | GET/POST/PUT/DELETE | CRUD тегов |
| `/api/recognition-rules` | GET/POST/PUT/DELETE | CRUD правил |
| `/api/recognition-rules/validate` | POST | Валидация формулы + проверка конфликтов |
| `/api/recognition-rules/test` | POST | Тест правила на имени семейства |
| `/api/families` | GET | Поиск семейств (фильтры, пагинация) |
| `/api/families/{id}` | GET | Детали семейства + история версий |
| `/api/families/{id}/versions` | GET | Список версий |
| `/api/families/{id}/download/{version}` | GET | Скачать RFA (+ TXT если есть) |
| `/api/families/publish` | POST | Загрузить семейство в библиотеку |
| `/api/families/validate-hash` | POST | Проверить Hash на дубликаты |
| `/api/families/batch-check` | POST | Массовая проверка статусов (RoleName + Hash) |
| `/api/system-types` | GET/POST | CRUD System Types (JSON-данные) |
| `/api/drafts` | GET/POST/DELETE | Управление Drafts |

### WebView2 ↔ Revit Plugin Events

| Событие | Направление | Описание |
|---------|-------------|----------|
| `revit:ready` | Plugin → UI | Плагин готов, контекст (projectId, userRole) |
| `revit:families:list` | Plugin → UI | Список семейств из проекта |
| `revit:system-types:list` | Plugin → UI | Список System Types из проекта |
| `ui:stamp` | UI → Plugin | Запрос на Stamp (familyId, roleName) |
| `ui:publish` | UI → Plugin | Запрос на Publish (familyId) |
| `ui:load-family` | UI → Plugin | Загрузить семейство в проект |
| `ui:update-family` | UI → Plugin | Обновить семейство из библиотеки |
| `revit:operation-complete` | Plugin → UI | Результат операции (success/error) |

### Blob Storage Structure

```
container: family-library/
├── families/
│   ├── {roleName}/
│   │   ├── v1/
│   │   │   ├── family.rfa              ← системное имя
│   │   │   └── catalog.txt (optional)  ← системное имя
│   │   ├── v2/
│   │   │   └── ...
├── previews/
│   ├── {familyId}_v{version}.png
```

### Spikes / Research (перед реализацией)

| Spike | Цель | Приоритет |
|-------|------|-----------|
| PartAtom детерминизм | Экспортировать RFA 10 раз → сравнить XML → подтвердить стабильность хеша | Высокий |
| OLE Streams чтение | Прочитать geometry streams из RFA → хешировать | Высокий |
| ES + Transfer Project Standards | Создать WallType с ES → Transfer в другой проект → проверить ES | Средний |
| ES upgrade Revit | Открыть проект 2024 в 2025 → проверить ES | Средний |

---

## Assumptions

- Edge WebView2 Runtime установлен на машинах пользователей (встроен в Windows 11)
- Для MVP используется Azurite вместо реального Azure Blob Storage
- Аутентификация MVP: mock/local, Production: Azure AD
- БИМ-менеджер выполняет Stamp/Publish из non-workshared шаблона (рекомендация для избежания conflicts)
- In-Place Families игнорируются системой
- IdentityGUID НЕ используется (ненадёжен при копировании семейств)
- Семейства с одинаковыми именами в разных категориях идентифицируются по паре (FamilyName, Category)

---

## Out of Scope (Phase 2-3)

**System Families:**
- Группа B (MEP: Pipes, Ducts, Cable Trays, Conduits)
- Группа C (Railings & Stairs)
- Группа D (Curtain Systems)
- Stacked Wall, Curtain Wall типы

**Modules:**
- Модуль 6: Сканер проектов (US-B3 "Update from Library" для любых проектов)
- Модуль 7: Change Tracking (Changelog, Local Changes stash, Pre-Update Preview)
- Модуль 8: Nested Families (зависимости, Shared вложенные)

**Features:**
- Серверный маппинг материалов MaterialMapping (Phase 2)
- Material mapping автоматический (Phase 2)

---

## Dependencies

| Компонент | Требование |
|-----------|------------|
| Revit | 2020, 2021, 2022, 2023, 2024, 2025, 2026 |
| .NET Framework | 4.8 (Revit 2020-2024) |
| .NET Runtime | 8 (Revit 2025-2026) |
| SQL Server | 2019+ или Azure SQL |
| WebView2 | Edge Runtime |
| Docker | Для Azurite в контейнере (опционально) |

### NuGet Packages (Plugin)

| Пакет | Версия | Назначение |
|-------|--------|------------|
| `Revit_API` | 2020-2026 | Revit API |
| `Microsoft.Web.WebView2` | 1.0.+ | WebView2 |
| `Newtonsoft.Json` | 13.0.+ | JSON |
| `Azure.Storage.Blobs` | 12.+ | Azure Blob |

### NuGet Packages (Backend)

| Пакет | Версия | Назначение |
|-------|--------|------------|
| `Microsoft.EntityFrameworkCore` | 9.+ | ORM |
| `Microsoft.EntityFrameworkCore.SqlServer` | 9.+ | SQL Server |
| `Azure.Storage.Blobs` | 12.+ | Blob Storage |
| `Swashbuckle.AspNetCore` | 6.+ | Swagger |

### NPM Packages (Frontend)

| Пакет | Версия | Назначение |
|-------|--------|------------|
| `@angular/core` | 21+ | Angular |
| `primeng` | 19+ | UI компоненты |
| `tailwindcss` | 4+ | CSS |
| `@tanstack/virtual` | 3+ | Virtual scroll |
