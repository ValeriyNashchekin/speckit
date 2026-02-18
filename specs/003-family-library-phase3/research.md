# Research: Family Library Phase 3

**Date**: 2026-02-18
**Branch**: 003-family-library-phase3

This document consolidates research findings for Phase 3 implementation decisions.

---

## R13: Nested Family Detection via EditFamily

**Question**: How to efficiently detect Shared nested families without performance impact?

### Decision

Detect at Publish time, cache in FamilyDependency table. Do NOT detect at scan time.

### Rationale

- EditFamily() is expensive (1-3 seconds per family)
- Running at scan time for 5000+ families would be prohibitively slow
- Dependencies are stable — only change when family is modified
- Caching in database enables "Used In" queries without EditFamily

### Algorithm

```
At Publish time:
1. Open family document via EditFamily(parentFamily)
2. Collect all nested families via FilteredElementCollector
3. For each nested:
   a. Check FAMILY_SHARED parameter → IsShared
   b. If Shared and has ES stamp → get NestedRoleName
   c. Check library → InLibrary, LibraryVersion
4. Store in FamilyDependency table
5. Close family document without saving
```

### Implementation

```csharp
public class NestedDetectionService
{
    public List<FamilyDependency> DetectDependencies(Document doc, Family parentFamily)
    {
        var dependencies = new List<FamilyDependency>();

        using var familyDoc = doc.EditFamily(parentFamily);

        var nestedFamilies = new FilteredElementCollector(familyDoc)
            .OfClass(typeof(Family))
            .Cast<Family>()
            .ToList();

        foreach (var nested in nestedFamilies)
        {
            var isShared = nested.get_Parameter(BuiltInParameter.FAMILY_SHARED)?.AsInteger() == 1;

            dependencies.Add(new FamilyDependency
            {
                ParentFamilyId = parentFamilyId,
                NestedFamilyName = nested.Name,
                IsShared = isShared,
                NestedRoleName = isShared ? GetRoleFromES(nested) : null,
                InLibrary = isShared && CheckInLibrary(nested.Name),
                LibraryVersion = GetLibraryVersion(nested.Name)
            });
        }

        return dependencies;
    }
}
```

### Alternatives Considered

| Alternative | Rejected Because |
|-------------|------------------|
| Detect at scan time | Too slow (1-3 sec × 5000 families) |
| Store in ES | Limited space (4KB), already used for stamp |
| Real-time detection | Unnecessary, dependencies change rarely |

---

## R14: IFamilyLoadOptions for Version Control

**Question**: How to control which version of nested family loads with parent?

### Decision

Implement custom IFamilyLoadOptions with user-selected sources. Use two-phase load: parent first, then override nested from library if needed.

### Rationale

- Revit loads nested families automatically with parent
- OnSharedFamilyFound callback allows choosing source
- FamilySource.Family = use from RFA file
- FamilySource.Project = use existing in project (don't load from RFA)
- Override after parent load if library version is newer

### Implementation

```csharp
public class NestedFamilyLoadOptions : IFamilyLoadOptions
{
    private readonly Dictionary<string, NestedLoadChoice> _choices;

    public NestedFamilyLoadOptions(Dictionary<string, NestedLoadChoice> choices)
    {
        _choices = choices;
    }

    public bool OnSharedFamilyFound(
        Family sharedFamily,
        bool familyInUse,
        out FamilySource source,
        out bool overwriteParameterValues)
    {
        var name = sharedFamily.Name;

        if (_choices.TryGetValue(name, out var choice))
        {
            source = choice.UseLibraryVersion ? FamilySource.Project : FamilySource.Family;
        }
        else
        {
            source = FamilySource.Family; // Default: use version from RFA
        }

        overwriteParameterValues = true;
        return true; // Continue loading
    }

    public bool OnFamilyFound(
        bool familyInUse,
        out bool overwriteParameterValues)
    {
        overwriteParameterValues = true;
        return true;
    }
}

public record NestedLoadChoice(string FamilyName, bool UseLibraryVersion, int? TargetVersion);
```

### Load Flow

```
1. User clicks "Load to Project" on FreeAxez_Table v4
2. Plugin shows Pre-Load Summary with nested versions:
   - FreeAxez_Table_Leg: RFA=v3, Library=v5, Project=v2
   - User chooses: "Update from Library (v5)"
3. Plugin builds NestedFamilyLoadOptions with choices
4. LoadFamily(parent.rfa, options) → loads parent + nested from RFA
5. If library version chosen: LoadFamily(nested_v5.rfa) → overwrites
6. Result: Parent v4 + Nested v5
```

### Alternatives Considered

| Alternative | Rejected Because |
|-------------|------------------|
| Always use RFA version | Misses library updates |
| Always use library version | May break if RFA has newer geometry |
| Unload/reload nested | Complex, loses element references |

---

## R15: RailingType Serialization (Group C)

**Question**: How to serialize RailingType with baluster dependencies?

### Decision

Custom serializer extracting railingStructure, top rail, and baluster dependencies.

### Rationale

- RailingTypes reference Loadable Families (balusters)
- Must track dependencies for Pull Update validation
- Top rails and handrails may be separate types
- Baluster patterns define which families are used

### JSON Structure

```json
{
  "typeName": "Railing_Glass_900",
  "category": "Railings",
  "systemFamily": "Railing",
  "parameters": {
    "Height": 900,
    "Offset": 0
  },
  "railingStructure": {
    "topRailTypeName": "Circular - 50mm",
    "handRailTypeName": null,
    "balusterPlacement": {
      "pattern": [
        {
          "balusterFamilyName": "Baluster-Round",
          "balusterTypeName": "25mm",
          "spacing": 100
        }
      ]
    }
  },
  "dependencies": [
    {
      "familyName": "Baluster-Round",
      "typeName": "25mm",
      "category": "Structural Columns",
      "inLibrary": true
    }
  ]
}
```

### Revit API Approach

```csharp
public class RailingSerializer
{
    public RailingJson Serialize(RailingType railingType)
    {
        var json = new RailingJson
        {
            TypeName = railingType.Name,
            Category = "Railings",
            SystemFamily = "Railing"
        };

        // Parameters
        json.Parameters["Height"] = railingType.get_Parameter(BuiltInParameter.RAILING_HEIGHT)?.AsDouble();

        // Top rail
        json.RailingStructure.TopRailTypeName = GetTopRailTypeName(railingType);

        // Balusters - requires exploring BalusterInfo
        var balusterInfo = GetBalusterInfo(railingType);
        foreach (var baluster in balusterInfo)
        {
            json.RailingStructure.BalusterPlacement.Pattern.Add(new BalusterPattern
            {
                BalusterFamilyName = baluster.FamilyName,
                BalusterTypeName = baluster.TypeName,
                Spacing = baluster.Spacing
            });

            json.Dependencies.Add(new Dependency
            {
                FamilyName = baluster.FamilyName,
                TypeName = baluster.TypeName,
                InLibrary = CheckInLibrary(baluster.FamilyName)
            });
        }

        return json;
    }
}
```

### Alternatives Considered

| Alternative | Rejected Because |
|-------------|------------------|
| Store element IDs | IDs are project-specific |
| Skip dependencies | Breaks Pull Update |
| Embed full family | Too large, duplication |

---

## R16: CurtainWallType Serialization (Group D)

**Question**: How to serialize CurtainWallType with grid/panels/mullions?

### Decision

Store grid settings, default panel type, and mullion assignments. Track Loadable panel dependencies.

### Rationale

- Curtain walls have grid layout parameters
- Panels can be System (default) or Loadable
- Mullions are usually types from Mullion family
- Stacked walls have references to child WallTypes

### JSON Structure

```json
{
  "typeName": "Curtain_Wall_Storefront",
  "kind": "Curtain",
  "category": "Walls",
  "systemFamily": "Curtain Wall",
  "parameters": {},
  "grid": {
    "horizontalSpacing": 1200,
    "verticalSpacing": 2400,
    "horizontalGridJustification": "Beginning",
    "verticalGridJustification": "Beginning"
  },
  "panels": {
    "defaultPanelTypeName": "System Panel: Glazed",
    "isLoadable": false
  },
  "mullions": {
    "horizontalMullionTypeName": "Rectangular Mullion: 50 x 150",
    "verticalMullionTypeName": "Rectangular Mullion: 50 x 150"
  },
  "dependencies": []
}
```

### Stacked Wall Extension

```json
{
  "typeName": "Stacked_External",
  "kind": "Stacked",
  "category": "Walls",
  "systemFamily": "Basic Wall",
  "stackedLayers": [
    { "wallTypeName": "Wall_Lower_Concrete", "height": 1200 },
    { "wallTypeName": "Wall_Upper_Brick", "height": 0 }
  ],
  "dependencies": [
    { "wallTypeName": "Wall_Lower_Concrete", "inLibrary": true },
    { "wallTypeName": "Wall_Upper_Brick", "inLibrary": true }
  ]
}
```

### Pull Update Validation

```
Before applying Stacked Wall:
1. Check if Wall_Lower_Concrete exists in project
2. Check if Wall_Upper_Brick exists in project
3. If any missing → Error with suggestion to load from library
```

---

## R17: Material Mapping Server-Side Storage

**Question**: How to store and apply material mappings per project?

### Decision

MaterialMapping table with TemplateMaterialName → ProjectMaterialName per ProjectId. Applied automatically at Pull Update.

### Rationale

- Different projects use different material names (localization, standards)
- Mappings are project-specific
- Automatic application reduces manual work
- Fallback to MVP behavior if mapping not found

### Table Design

```sql
CREATE TABLE MaterialMappings (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ProjectId UNIQUEIDENTIFIER NOT NULL,
    TemplateMaterialName NVARCHAR(200) NOT NULL,
    ProjectMaterialName NVARCHAR(200) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
    LastUsedAt DATETIME2 NULL,
    CONSTRAINT IX_MaterialMapping_ProjectId_TemplateName
        UNIQUE (ProjectId, TemplateMaterialName)
);
```

### Usage Flow

```
Pull Update for Wall_External_200:

1. Parse JSON compoundStructure
2. For each layer:
   a. materialName = "Brick, Common"
   b. Lookup MaterialMapping (ProjectId, "Brick, Common")
   c. IF found → use ProjectMaterialName ("Кирпич красный")
   d. IF NOT found → MVP fallback:
      - Warning: "Material 'Brick, Common' not found"
      - Options: [Select existing] [Create new] [Default] [Skip]
```

### UI for Managing Mappings

```
Settings > Material Mappings

Filter by Project: [Dropdown]

| Template Material      | Project Material         | Actions |
|------------------------|--------------------------|---------|
| Brick, Common          | Кирпич красный           | ✏️ 🗑️   |
| Concrete, Cast-in-Place| Бетон литой              | ✏️ 🗑️   |

[+ Add Mapping] [Import from Project]
```

### Alternatives Considered

| Alternative | Rejected Because |
|-------------|------------------|
| Global mappings | Different projects, different standards |
| Store in Revit | Lost on upgrade, no sharing |
| Auto-learn mappings | Risk of incorrect mappings |

---

## R18: StackedWallType Serialization

**Question**: How to serialize StackedWallType with child WallType references?

### Decision

Store wall layer references by TypeName with height. Validate existence at Pull Update.

### Rationale

- Stacked walls combine multiple Basic Wall types
- References are by WallType, not by ID
- Heights define the vertical composition
- Height=0 means "extend to top"

### JSON Structure

```json
{
  "typeName": "Stacked_External",
  "kind": "Stacked",
  "category": "Walls",
  "systemFamily": "Basic Wall",
  "parameters": {},
  "stackedLayers": [
    { "wallTypeName": "Wall_Lower_Concrete", "height": 1200 },
    { "wallTypeName": "Wall_Upper_Brick", "height": 0 }
  ],
  "dependencies": [
    { "wallTypeName": "Wall_Lower_Concrete", "inLibrary": true, "groupId": "A" },
    { "wallTypeName": "Wall_Upper_Brick", "inLibrary": true, "groupId": "A" }
  ]
}
```

### Revit API Approach

```csharp
public class StackedWallSerializer
{
    public StackedWallJson Serialize(WallType wallType)
    {
        if (wallType.Kind != WallKind.Stacked)
            throw new ArgumentException("Not a stacked wall type");

        var json = new StackedWallJson
        {
            TypeName = wallType.Name,
            Kind = "Stacked"
        };

        // Get stacked wall layers
        var stackedLayers = GetStackedWallLayers(wallType);
        foreach (var layer in stackedLayers)
        {
            json.StackedLayers.Add(new StackedLayer
            {
                WallTypeName = layer.WallType.Name,
                Height = layer.Height
            });

            json.Dependencies.Add(new Dependency
            {
                WallTypeName = layer.WallType.Name,
                InLibrary = CheckInLibrary(layer.WallType.Name)
            });
        }

        return json;
    }
}
```

### Pull Update Validation

```
Before applying Stacked_External:

1. Query dependencies: Wall_Lower_Concrete, Wall_Upper_Brick
2. Check project: Wall_Lower_Concrete exists? → Yes
3. Check project: Wall_Upper_Brick exists? → No
4. IF in library → Suggest: "Load Wall_Upper_Brick from library?"
5. IF NOT in library → Error: "Cannot apply: Wall_Upper_Brick not found"
```

---

## Summary: Phase 3 Technical Decisions

| # | Topic | Decision |
|---|-------|----------|
| R13 | Nested Detection | EditFamily at Publish, cache in DB |
| R14 | Load Options | IFamilyLoadOptions with user choices, two-phase load |
| R15 | Railing Serialization | Custom serializer with baluster dependencies |
| R16 | Curtain Serialization | Grid/panels/mullions in JSON |
| R17 | Material Mapping | Server-side table per project |
| R18 | Stacked Wall Serialization | Layer references with validation |

All research items resolved — ready for Phase 1 design.
