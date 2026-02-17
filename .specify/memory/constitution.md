# Family Library Constitution

## Core Principles

### I. Clean Architecture (NON-NEGOTIABLE)

**Reference:** `/clean-architecture`

Все компоненты системы следуют принципам Clean Architecture:

- **Layer separation**: Entities → Use Cases → Interface Adapters → Frameworks
- **Dependency rule**: Зависимости направлены внутрь, не наружу
- **Entities**: Чистый C#, без зависимостей на Revit API / EF Core / Angular
- **Use Cases**: Бизнес-логика приложения, оркестрация entities
- **Interface Adapters**: Controllers, ViewModels, Repositories
- **Frameworks**: Revit API, EF Core, Angular, Azure Blob

**Для каждого компонента:**

| Component | Entities | Use Cases | Adapters | Frameworks |
|-----------|----------|-----------|----------|------------|
| Plugin | ✅ | ✅ | ✅ | Revit API |
| Backend | ✅ | ✅ | ✅ | EF Core, ASP.NET |
| Frontend | Services | Components | — | Angular, PrimeNG |

---

### II. .NET Best Practices

**Reference:** `/dotnet-best-practices`, `/ef-core-guidelines`

- **Primary constructors** для dependency injection
- **Nullable reference types** включены везде
- **Async patterns**: async/await, CancellationToken, ConfigureAwait(false) в библиотеках
- **Dependency injection**: constructor injection, scoped lifetimes
- **EF Core**: No tracking queries по умолчанию, explicit tracking только при update
- **Migrations**: одна миграция = одна логическая change

**Code style:**
```csharp
// ✅ Good
public class FamilyService(IFamilyRepository repository) : IFamilyService
{
    public async Task<Family?> GetAsync(Guid id, CancellationToken ct = default)
        => await repository.FindAsync(id, ct);
}

// ❌ Bad
public class FamilyService : IFamilyService
{
    private readonly IFamilyRepository _repository;
    public FamilyService(IFamilyRepository repository) => _repository = repository;
}
```

---

### III. Revit Plugin Architecture

**Reference:** `/revit-api-patterns`, `/revit-command-structure`, `/revit-ui-guidelines`

#### Flat Command Structure
```
Commands/
└── {CommandName}/
    ├── ViewModels/     # MVVM (только если есть UI)
    ├── Views/          # XAML (только если есть UI)
    ├── Models/         # Domain models, DTOs — БЕЗ Revit API
    ├── Services/       # Revit API calls, бизнес-логика
    ├── Utils/          # Helpers, extensions
    └── Enums/          # Enumerations
```

**❌ Запрещено:**
- Вложенные папки (`UI/ViewModels/`, `Core/Services/Implementation/`)
- Пустые папки "на всякий случай"
- Кастомные имена (`Infrastructure/`, `Domain/`, `Application/`)

#### Clean Code Rules
| Правило | Значение |
|---------|----------|
| Функции | 15-25 строк |
| Классы | 250-350 строк |
| Вложенность | Максимум 2-3 уровня |
| Параметры | 0-2 идеал, 3 допустимо, 4+ → рефакторинг |
| Boolean flags | Запрещены в параметрах |

#### Separation of Concerns
- `Models/` — чистый C#, **БЕЗ** Revit API
- `Services/` — **ВЕСЬ** Revit API
- `ViewModels/` — **БЕЗ** Revit API в конструкторе
- `Document` передаётся как параметр метода, не через конструктор

#### XAML UI Requirements
- `Window.Style` элемент ПОСЛЕ `Window.Resources`
- `BasedOn="{StaticResource ThemeWindow}"`
- Grid-based layout (никаких StackPanel для layout)
- `SizeToContent="Height"`, Width фиксированный

#### TOP-3 Revit API Performance Mistakes (AVOID)
1. **Нефильтрованные запросы** — всегда используй `OfCategory()`, `OfClass()`
2. **Transactions без using** — всегда `using (Transaction t = ...)`
3. **Многократный `Element.get_Parameter()`** — кешируй `Parameter` объекты

---

### IV. Angular Frontend Architecture

**Reference:** `/angular-best-practices`, `/angular-guidelines`, `/tailwind-css`

#### Core Angular Patterns
- **Signals** для reactive state (не RxJS BehaviorSubject)
- **Standalone components** (не NgModules)
- **OnPush** change detection везде
- **New control flow**: `@if`, `@for`, `@switch` (не `*ngIf`, `*ngFor`)

#### UI Components (NON-NEGOTIABLE)
- **PrimeNG** используется ВЕЗДЕ где есть подходящий компонент
- Запрещено создавать кастомные компоненты если есть PrimeNG аналог
- Список обязательных PrimeNG компонентов:
  - Tables, DataGrid → `p-table` (с virtual scroll)
  - Forms → `p-inputText`, `p-dropdown`, `p-multiSelect`, `p-calendar`
  - Buttons → `p-button`, `p-splitButton`
  - Dialogs → `p-dialog`, `p-confirmDialog`
  - Overlays → `p-tooltip`, `p-overlayPanel`, `p-menu`
  - Messages → `p-toast`, `p-messages`
  - Cards → `p-card`
  - Tabs → `p-tabView`
  - File upload → `p-fileUpload`

#### Styling (NON-NEGOTIABLE)
- **Tailwind CSS v4** — ТОЛЬКО utility classes
- **❌ Запрещено:**
  - Кастомные CSS файлы (`*.component.css`, `styles.css`)
  - Inline styles в templates (`style="..."`)
  - SCSS/SASS препроцессоры
  - Кастомные CSS классы (кроме Tailwind `@apply` в крайних случаях)
- **✅ Разрешено:**
  - Tailwind utility classes в templates: `class="flex items-center gap-4 p-4"`
  - Tailwind конфигурация для темы (colors, fonts)
  - PrimeNG theming через CSS variables

**Example:**
```html
<!-- ✅ Good: PrimeNG + Tailwind -->
<p-table [value]="families()" styleClass="w-full" [rows]="50" [virtualScroll]="true">
  <ng-template pTemplate="header">
    <tr>
      <th pSortableColumn="name" class="p-4 bg-gray-100">Name</th>
    </tr>
  </ng-template>
  <ng-template pTemplate="body" let-family>
    <tr>
      <td class="p-4">{{ family.name }}</td>
    </tr>
  </ng-template>
</p-table>

<!-- ❌ Bad: Custom CSS -->
<p-table class="family-table">  <!-- family-table defined in .css -->
```

**Structure:**
```
src/
├── app/
│   ├── features/           # Feature modules
│   │   └── library/
│   │       ├── components/
│   │       ├── services/
│   │       └── models/
│   ├── shared/             # Shared components (только если нет PrimeNG аналога)
│   └── core/               # API, interceptors
├── tailwind.config.js      # Tailwind конфигурация
└── styles.css              # ТОЛЬКО Tailwind directives (@tailwind base; @tailwind components; @tailwind utilities;)
```

---

### V. Azure Integration

**Reference:** `/azure-blob-storage`, `/azure-configuration`

- **Azurite для MVP** — локальная разработка без затрат на Azure
- **Managed Identity** для production (не connection strings в коде)
- **SAS tokens** с ограниченным сроком для Blob access
- **Key Vault** для секретов (не appsettings.json)

**Configuration pattern:**
```csharp
// MVP: Azurite
"ConnectionStrings:AzureBlob": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;..."

// Production: Azure
"ConnectionStrings:AzureBlob": "DefaultEndpointsProtocol=https;AccountName=freeaxezfamilylib;..."
```

---

## Code Quality Standards

### Naming Conventions

| Context | Convention | Example |
|---------|------------|---------|
| C# classes | PascalCase | `FamilyService` |
| C# methods | PascalCase | `GetFamilyAsync` |
| C# private fields | _camelCase | `_repository` |
| TypeScript | camelCase | `familyService` |
| CSS classes | kebab-case | `family-card` |
| Revit commands | {Action}{Target} | `StampFamilyCommand` |

### File Naming

| Type | Convention |
|------|------------|
| C# class | `{ClassName}.cs` |
| Interface | `I{InterfaceName}.cs` |
| Angular component | `{name}.component.ts` |
| Angular service | `{name}.service.ts` |
| Spec | `spec.md` |
| Plan | `plan.md` |
| Tasks | `tasks.md` |

---

## Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| Revit Plugin | C#, Revit API | 2020-2026 |
| Backend | .NET, EF Core | 10, 9 |
| Frontend | Angular, PrimeNG, Tailwind | 21, 19, 4 |
| Database | SQL Server | 2025 |
| Storage | Azure Blob | — |
| Local Storage | Revit Extensible Storage | — |
| UI (Plugin) | WPF, WebView2 | — |

---

## Development Workflow

### Branch Strategy
```
master (main)
  └── 001-family-library-mvp
  └── 002-family-library-phase2
  └── 003-family-library-phase3
```

### Commit Convention
```
<type>(<scope>): <description>

Types: feat, fix, refactor, docs, test, chore
```

### Code Review Checklist
- [ ] Clean Architecture layers respected
- [ ] No Revit API in Models/ViewModels
- [ ] Functions < 25 lines
- [ ] Classes < 350 lines
- [ ] No boolean parameters
- [ ] Proper async/await usage
- [ ] Tests cover happy path + edge cases
- [ ] PrimeNG component used if available (no custom components)
- [ ] Only Tailwind utility classes (no custom CSS files)
- [ ] No inline styles in templates

---

## Governance

1. **Constitution supersedes all other practices** — при конфликте следуем конституции
2. **Amendments** — требуют документации, одобрения, migration plan
3. **Skills** — все скиллы являются расширением конституции, не противоречат ей
4. **Complexity justification** — любая сложность должна быть обоснована

---

**Version**: 1.1.0 | **Ratified**: 2026-02-17 | **Last Amended**: 2026-02-17
