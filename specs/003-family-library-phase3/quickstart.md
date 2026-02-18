# Quickstart: Family Library Phase 3

**Date**: 2026-02-18
**Branch**: 003-family-library-phase3
**Depends On**: `002-family-library-phase2`

This guide covers Phase 3 features: Nested Families, Complex System Families, and Material Mapping.

---

## Prerequisites

- MVP and Phase 2 implemented
- Database with Families, SystemTypes, FamilyVersions tables
- Azure Blob storage configured
- Revit plugin with Stamp/Publish commands

---

## Feature 1: Nested Families

### Backend Setup

1. **Run migration**:
```bash
cd src/FamilyLibrary.Api
dotnet ef migrations add AddPhase3Entities
dotnet ef database update
```

2. **Add FamilyDependency entity**:
```csharp
// FamilyLibrary.Domain/Entities/FamilyDependency.cs
public class FamilyDependency
{
    public Guid Id { get; init; }
    public Guid ParentFamilyId { get; init; }
    public string NestedFamilyName { get; init; } = string.Empty;
    public string? NestedRoleName { get; init; }
    public bool IsShared { get; init; }
    public bool InLibrary { get; init; }
    public int? LibraryVersion { get; init; }
    public DateTime DetectedAt { get; init; }
}
```

3. **Add API endpoints**:
```csharp
// FamiliesController.cs
[HttpGet("{id}/dependencies")]
public async Task<ActionResult<List<FamilyDependencyDto>>> GetDependencies(Guid id)
    => Ok(await _nestedService.GetDependenciesAsync(id));

[HttpGet("{id}/used-in")]
public async Task<ActionResult<UsedInDto>> GetUsedIn(Guid id)
    => Ok(await _nestedService.GetUsedInAsync(id));
```

### Plugin Setup

1. **Create NestedDetectionService**:
```csharp
// Services/NestedDetectionService.cs
public class NestedDetectionService
{
    public List<FamilyDependency> Detect(Document doc, Family parent)
    {
        using var familyDoc = doc.EditFamily(parent);
        return new FilteredElementCollector(familyDoc)
            .OfClass(typeof(Family))
            .Cast<Family>()
            .Select(n => new FamilyDependency
            {
                NestedFamilyName = n.Name,
                IsShared = IsSharedFamily(n),
                // ...
            })
            .ToList();
    }
}
```

2. **Create NestedFamilyLoadOptions**:
```csharp
// Services/NestedFamilyLoadOptions.cs
public class NestedFamilyLoadOptions : IFamilyLoadOptions
{
    public bool OnSharedFamilyFound(Family shared, bool inUse,
        out FamilySource source, out bool overwrite)
    {
        source = _choices.GetValueOrDefault(shared.Name, FamilySource.Family);
        overwrite = true;
        return true;
    }
}
```

### Frontend Setup

1. **Add nested-family.service.ts**:
```typescript
// core/services/nested-family.service.ts
@Injectable({ providedIn: 'root' })
export class NestedFamilyService {
  private api = inject(ApiService);

  getDependencies(familyId: string) {
    return this.api.get<NestedFamilyDto[]>(`families/${familyId}/dependencies`);
  }

  getUsedIn(familyId: string) {
    return this.api.get<UsedInDto>(`families/${familyId}/used-in`);
  }
}
```

2. **Add dependencies-list component**:
```typescript
// features/library/components/dependencies-list/
@Component({
  selector: 'app-dependencies-list',
  standalone: true,
  imports: [TableModule, CommonModule],
  template: `
    <p-table [value]="dependencies()" styleClass="w-full">
      <ng-template pTemplate="header">
        <tr>
          <th>Family</th>
          <th>Shared</th>
          <th>In Library</th>
          <th>Version</th>
        </tr>
      </ng-template>
      <ng-template pTemplate="body" let-dep>
        <tr>
          <td>{{ dep.familyName }}</td>
          <td>{{ dep.isShared ? '✓' : '—' }}</td>
          <td>{{ dep.inLibrary ? '✓ v' + dep.libraryVersion : '✗' }}</td>
          <td>{{ dep.libraryVersion ?? '—' }}</td>
        </tr>
      </ng-template>
    </p-table>
  `
})
export class DependenciesListComponent {
  dependencies = input.required<NestedFamilyDto[]>();
}
```

---

## Feature 2: Complex System Families

### Group C: Railings

1. **Create RailingSerializer**:
```csharp
// Plugin/Services/RailingSerializer.cs
public class RailingSerializer
{
    public RailingJson Serialize(RailingType railingType)
    {
        var json = new RailingJson
        {
            TypeName = railingType.Name,
            Category = "Railings",
            Parameters = ExtractParameters(railingType),
            RailingStructure = ExtractStructure(railingType),
            Dependencies = ExtractDependencies(railingType)
        };
        return json;
    }

    private List<Dependency> ExtractDependencies(RailingType railingType)
    {
        // Extract baluster family references
        var balusters = GetBalusterFamilies(railingType);
        return balusters.Select(b => new Dependency
        {
            FamilyName = b.FamilyName,
            TypeName = b.TypeName,
            InLibrary = _libraryService.Exists(b.FamilyName)
        }).ToList();
    }
}
```

### Group D: Curtain Walls

1. **Create CurtainSerializer**:
```csharp
// Plugin/Services/CurtainSerializer.cs
public class CurtainSerializer
{
    public CurtainJson Serialize(WallType wallType)
    {
        if (wallType.Kind != WallKind.Curtain)
            throw new ArgumentException("Not a curtain wall type");

        return new CurtainJson
        {
            TypeName = wallType.Name,
            Kind = "Curtain",
            Grid = ExtractGridSettings(wallType),
            Panels = ExtractPanelSettings(wallType),
            Mullions = ExtractMullionSettings(wallType)
        };
    }
}
```

### Stacked Walls

1. **Create StackedWallSerializer**:
```csharp
// Plugin/Services/StackedWallSerializer.cs
public class StackedWallSerializer
{
    public StackedWallJson Serialize(WallType wallType)
    {
        if (wallType.Kind != WallKind.Stacked)
            throw new ArgumentException("Not a stacked wall type");

        var layers = GetStackedLayers(wallType);
        return new StackedWallJson
        {
            TypeName = wallType.Name,
            Kind = "Stacked",
            StackedLayers = layers.Select(l => new StackedLayer
            {
                WallTypeName = l.WallType.Name,
                Height = l.Height
            }).ToList(),
            Dependencies = layers.Select(l => new Dependency
            {
                WallTypeName = l.WallType.Name,
                InLibrary = _libraryService.Exists(l.WallType.Name)
            }).ToList()
        };
    }
}
```

---

## Feature 3: Material Mapping

### Backend Setup

1. **Add MaterialMappingController**:
```csharp
// Controllers/MaterialMappingsController.cs
[ApiController]
[Route("api/material-mappings")]
public class MaterialMappingsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MaterialMappingDto>>> List(
        [FromQuery] Guid projectId)
        => Ok(await _service.ListAsync(projectId));

    [HttpPost]
    public async Task<ActionResult<MaterialMappingDto>> Create(
        [FromBody] CreateMaterialMappingRequest request)
        => Ok(await _service.CreateAsync(request));

    [HttpPut("{id}")]
    public async Task<ActionResult<MaterialMappingDto>> Update(
        Guid id, [FromBody] UpdateMaterialMappingRequest request)
        => Ok(await _service.UpdateAsync(id, request));

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
```

2. **Add MaterialMappingService**:
```csharp
// Services/MaterialMappingService.cs
public class MaterialMappingService
{
    public async Task<string?> LookupAsync(Guid projectId, string templateName)
    {
        var mapping = await _db.MaterialMappings
            .Where(m => m.ProjectId == projectId && m.TemplateMaterialName == templateName)
            .FirstOrDefaultAsync();

        if (mapping != null)
        {
            mapping.LastUsedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        return mapping?.ProjectMaterialName;
    }
}
```

### Frontend Setup

1. **Add material-mappings settings page**:
```typescript
// features/settings/material-mappings/
@Component({
  selector: 'app-material-mappings',
  standalone: true,
  imports: [TableModule, ButtonModule, DialogModule, FormsModule],
  template: `
    <p-table [value]="mappings()" styleClass="w-full">
      <ng-template pTemplate="header">
        <tr>
          <th>Template Material</th>
          <th>Project Material</th>
          <th>Actions</th>
        </tr>
      </ng-template>
      <ng-template pTemplate="body" let-mapping>
        <tr>
          <td>{{ mapping.templateMaterialName }}</td>
          <td>{{ mapping.projectMaterialName }}</td>
          <td>
            <p-button icon="pi pi-pencil" (onClick)="edit(mapping)"
                      styleClass="p-button-text" />
            <p-button icon="pi pi-trash" (onClick)="delete(mapping)"
                      styleClass="p-button-text p-button-danger" />
          </td>
        </tr>
      </ng-template>
    </p-table>

    <p-button label="Add Mapping" icon="pi pi-plus"
              (onClick)="showDialog.set(true)" />
  `
})
export class MaterialMappingsComponent {
  private service = inject(MaterialMappingService);
  projectId = input.required<string>();
  mappings = this.service.mappings;
  showDialog = signal(false);
}
```

---

## Testing

### Unit Tests

```csharp
// NestedFamilyServiceTests.cs
public class NestedFamilyServiceTests
{
    [Fact]
    public async Task GetDependencies_ReturnsNestedFamilies()
    {
        // Arrange
        var parentId = Guid.NewGuid();
        _db.FamilyDependencies.Add(new FamilyDependency
        {
            ParentFamilyId = parentId,
            NestedFamilyName = "Leg",
            IsShared = true,
            InLibrary = true,
            LibraryVersion = 3
        });
        await _db.SaveChangesAsync();

        // Act
        var result = await _service.GetDependenciesAsync(parentId);

        // Assert
        result.Should().HaveCount(1);
        result[0].FamilyName.Should().Be("Leg");
    }
}
```

### Integration Tests

```csharp
// MaterialMappingIntegrationTests.cs
public class MaterialMappingIntegrationTests : IClassFixture<TestContainersFixture>
{
    [Fact]
    public async Task PullUpdate_AppliesMaterialMapping()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        await _client.PostAsJsonAsync("/api/material-mappings", new
        {
            ProjectId = projectId,
            TemplateMaterialName = "Brick, Common",
            ProjectMaterialName = "Кирпич красный"
        });

        // Act - Pull Update for wall with "Brick, Common"
        var result = await _pluginService.PullUpdate(systemTypeId, projectId);

        // Assert
        result.AppliedMaterials.Should().Contain("Кирпич красный");
    }
}
```

---

## Verification Checklist

### Nested Families
- [ ] FamilyDependency table created
- [ ] Detection at Publish works
- [ ] Dependencies shown in Queue UI
- [ ] Pre-Load Summary shows version conflicts
- [ ] Load with library version override works
- [ ] "Used In" queries work

### Complex System Families
- [ ] RailingType serializes with dependencies
- [ ] CurtainWallType serializes with grid/panels/mullions
- [ ] StackedWallType serializes with layers
- [ ] Pull Update validates dependencies
- [ ] Error shown if dependencies missing

### Material Mapping
- [ ] MaterialMapping table created
- [ ] CRUD API works
- [ ] Lookup at Pull Update works
- [ ] Fallback dialog shows when mapping not found
- [ ] "Remember" saves mapping
- [ ] Settings UI for managing mappings
