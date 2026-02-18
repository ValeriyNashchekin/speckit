# Data Model: Family Library Phase 3

**Date**: 2026-02-18
**Branch**: 003-family-library-phase3

This document describes data model **additions** for Phase 3. See `specs/001-family-library-mvp/data-model.md` for base entities and `specs/002-family-library-phase2/data-model.md` for Phase 2 additions.

---

## Entity Overview (Additions)

```
┌─────────────────┐
│     Family      │
│  (existing)     │
└────────┬────────┘
         │
         │ 1:N
         ▼
┌─────────────────┐
│FamilyDependency │ ◄── NEW
├─────────────────┤
│ Id (PK)         │
│ ParentFamilyId  │
│ NestedFamilyName│
│ NestedRoleName  │
│ IsShared        │
│ InLibrary       │
│ LibraryVersion  │
│ DetectedAt      │
└─────────────────┘

┌─────────────────┐
│ MaterialMapping │ ◄── NEW
├─────────────────┤
│ Id (PK)         │
│ ProjectId       │
│ TemplateMaterial│
│ ProjectMaterial │
│ CreatedAt       │
│ LastUsedAt      │
└─────────────────┘
```

---

## New Entities

### FamilyDependency

Tracks nested family dependencies for loadable families.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | `uniqueidentifier` | PK, NOT NULL | Auto-generated GUID |
| ParentFamilyId | `uniqueidentifier` | FK → Family, NOT NULL | Parent family |
| NestedFamilyName | `nvarchar(200)` | NOT NULL | Nested family name (from RFA) |
| NestedRoleName | `nvarchar(100)` | NULLABLE | Role name if Shared and stamped |
| IsShared | `bit` | NOT NULL | True if Shared nested family |
| InLibrary | `bit` | NOT NULL | True if nested is published to library |
| LibraryVersion | `int` | NULLABLE | Current version in library (if InLibrary) |
| DetectedAt | `datetime2` | NOT NULL | When dependency was detected |

**Validation Rules**:
- Non-Shared nested families always have NestedRoleName = NULL
- InLibrary = false if nested is not Shared
- LibraryVersion only set if InLibrary = true

**Indexes**:
- `IX_FamilyDependency_ParentFamilyId` on ParentFamilyId (for loading dependencies)
- `IX_FamilyDependency_NestedRoleName` on NestedRoleName (for "Used In" queries)

**Relationships**:
- Family (1) → (N) FamilyDependency (cascade delete)

---

### MaterialMapping

Maps template material names to project-specific material names.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | `uniqueidentifier` | PK, NOT NULL | Auto-generated GUID |
| ProjectId | `uniqueidentifier` | NOT NULL | Project identifier (from Revit) |
| TemplateMaterialName | `nvarchar(200)` | NOT NULL | Material name in template/library |
| ProjectMaterialName | `nvarchar(200)` | NOT NULL | Material name in target project |
| CreatedAt | `datetime2` | NOT NULL | Creation timestamp |
| LastUsedAt | `datetime2` | NULLABLE | Last time mapping was applied |

**Validation Rules**:
- TemplateMaterialName and ProjectMaterialName cannot be empty
- Unique per (ProjectId, TemplateMaterialName) combination

**Indexes**:
- `IX_MaterialMapping_ProjectId_TemplateName` UNIQUE on (ProjectId, TemplateMaterialName)
- `IX_MaterialMapping_ProjectId` on ProjectId (for listing)

---

## New DTOs

### NestedFamilyDto

```csharp
public class NestedFamilyDto
{
    public string FamilyName { get; init; } = string.Empty;
    public string? RoleName { get; init; }
    public bool IsShared { get; init; }
    public bool InLibrary { get; init; }
    public int? LibraryVersion { get; init; }
    public int? RfaVersion { get; init; }      // Version embedded in parent RFA
    public int? ProjectVersion { get; init; }  // Version currently in project
}
```

### PreLoadSummaryDto

```csharp
public class PreLoadSummaryDto
{
    public string ParentFamilyName { get; init; } = string.Empty;
    public int ParentVersion { get; init; }
    public List<NestedFamilySummaryDto> NestedFamilies { get; init; } = [];
}

public class NestedFamilySummaryDto
{
    public string FamilyName { get; init; } = string.Empty;
    public string? RoleName { get; init; }
    public int? RfaVersion { get; init; }
    public int? LibraryVersion { get; init; }
    public int? ProjectVersion { get; init; }
    public NestedLoadAction RecommendedAction { get; init; }
}

public enum NestedLoadAction
{
    LoadFromRfa,        // First time, use RFA version
    UpdateFromLibrary,  // Library has newer
    KeepProjectVersion, // Project already has same or newer
    NoAction            // Non-Shared, embedded in parent
}
```

### UsedInDto

```csharp
public class UsedInDto
{
    public string NestedFamilyName { get; init; } = string.Empty;
    public List<ParentReferenceDto> ParentFamilies { get; init; } = [];
}

public class ParentReferenceDto
{
    public Guid FamilyId { get; init; }
    public string FamilyName { get; init; } = string.Empty;
    public string? RoleName { get; init; }
    public int NestedVersionInParent { get; init; }  // Version embedded in parent
    public int ParentLatestVersion { get; init; }
}
```

### MaterialMappingDto

```csharp
public class MaterialMappingDto
{
    public Guid Id { get; init; }
    public Guid ProjectId { get; init; }
    public string TemplateMaterialName { get; init; } = string.Empty;
    public string ProjectMaterialName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? LastUsedAt { get; init; }
}

public class CreateMaterialMappingRequest
{
    public Guid ProjectId { get; init; }
    public string TemplateMaterialName { get; init; } = string.Empty;
    public string ProjectMaterialName { get; init; } = string.Empty;
}

public class UpdateMaterialMappingRequest
{
    public string ProjectMaterialName { get; init; } = string.Empty;
}
```

---

## Complex System Families JSON Schemas

### Group C: RailingType

```json
{
  "version": 1,
  "typeName": "Railing_Glass_900",
  "category": "Railings",
  "systemFamily": "Railing",
  "parameters": {
    "Height": 900,
    "Offset": 0,
    "Primary Base Method": "Level"
  },
  "railingStructure": {
    "topRailTypeName": "Circular - 50mm",
    "handRailTypeName": null,
    "balusterPlacement": {
      "pattern": [
        {
          "balusterFamilyName": "Baluster-Round",
          "balusterTypeName": "25mm",
          "spacing": 100,
          "offset": 0
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

### Group C: StairType

```json
{
  "version": 1,
  "typeName": "Stair_Concrete_Standard",
  "category": "Stairs",
  "systemFamily": "Stair",
  "parameters": {
    "Base Level": null,
    "Top Level": null,
    "Desired Stair Height": 3000,
    "Tread Depth": 280,
    "Riser Height": 175
  },
  "stairStructure": {
    "runType": "Standard",
    "landingType": "Standard",
    "supportTypeName": null
  },
  "dependencies": []
}
```

### Group D: CurtainWallType

```json
{
  "version": 1,
  "typeName": "Curtain_Wall_Storefront",
  "kind": "Curtain",
  "category": "Walls",
  "systemFamily": "Curtain Wall",
  "parameters": {
    "Unconnected Height": 4000,
    "Wall Location Line": "Finish Face: Exterior"
  },
  "grid": {
    "layout": "Fixed Distance",
    "horizontalSpacing": 1200,
    "verticalSpacing": 2400,
    "horizontalAdjust": 0,
    "verticalAdjust": 0
  },
  "panels": {
    "defaultPanelTypeName": "System Panel: Glazed",
    "isLoadable": false
  },
  "mullions": {
    "horizontalMullionTypeName": "Rectangular Mullion: 50 x 150",
    "verticalMullionTypeName": "Rectangular Mullion: 50 x 150",
    "cornerMullionTypeName": null
  },
  "dependencies": []
}
```

### Group D: StackedWallType

```json
{
  "version": 1,
  "typeName": "Stacked_External",
  "kind": "Stacked",
  "category": "Walls",
  "systemFamily": "Basic Wall",
  "parameters": {
    "Unconnected Height": 0,
    "Wall Location Line": "Core Centerline"
  },
  "stackedLayers": [
    {
      "wallTypeName": "Wall_Lower_Concrete",
      "height": 1200,
      "heightIsVariable": false
    },
    {
      "wallTypeName": "Wall_Upper_Brick",
      "height": 0,
      "heightIsVariable": true
    }
  ],
  "dependencies": [
    {
      "wallTypeName": "Wall_Lower_Concrete",
      "inLibrary": true,
      "groupId": "A"
    },
    {
      "wallTypeName": "Wall_Upper_Brick",
      "inLibrary": true,
      "groupId": "A"
    }
  ]
}
```

### Group B Extended: CableTrayType

```json
{
  "version": 1,
  "typeName": "CableTray_Ladder_100",
  "category": "Cable Trays",
  "systemFamily": "Cable Tray",
  "parameters": {
    "Width": 100,
    "Height": 50,
    "Max Support Spacing": 1500
  },
  "cableTrayFittings": {
    "straightTypeName": "Standard",
    "elbowTypeName": null,
    "teeTypeName": null
  },
  "dependencies": []
}
```

---

## Database Changes

### Migration: AddPhase3Entities

```sql
-- FamilyDependency table
CREATE TABLE FamilyDependencies (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ParentFamilyId UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT FK_FamilyDependency_Family REFERENCES Families(Id) ON DELETE CASCADE,
    NestedFamilyName NVARCHAR(200) NOT NULL,
    NestedRoleName NVARCHAR(100) NULL,
    IsShared BIT NOT NULL DEFAULT 0,
    InLibrary BIT NOT NULL DEFAULT 0,
    LibraryVersion INT NULL,
    DetectedAt DATETIME2 NOT NULL DEFAULT SYSDATETIME()
);

CREATE INDEX IX_FamilyDependency_ParentFamilyId
    ON FamilyDependencies(ParentFamilyId);

CREATE INDEX IX_FamilyDependency_NestedRoleName
    ON FamilyDependencies(NestedRoleName)
    WHERE NestedRoleName IS NOT NULL;

-- MaterialMapping table
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

CREATE INDEX IX_MaterialMapping_ProjectId
    ON MaterialMappings(ProjectId);
```

---

## Relationships Summary (Phase 3 Additions)

| From | To | Relationship | Notes |
|------|-----|--------------|-------|
| Family | FamilyDependency | 1:N | Parent has many nested dependencies |
| Family (via Role) | FamilyDependency | 1:N | Nested appears in many parents ("Used In") |
| Project | MaterialMapping | 1:N | Project has many material mappings |

---

## State Transitions

### NestedFamilyStatus (derived, not stored)

```
                    ┌─────────────────────────────────────┐
                    │                                     │
                    ▼                                     │
┌───────────┐   ┌───────────┐   ┌───────────┐   ┌───────┴───┐
│  Not in   │──▶│  In RFA   │──▶│ In Library│──▶│ Published │
│  Library  │   │   Only    │   │   Only    │   │ to Project│
└───────────┘   └───────────┘   └───────────┘   └───────────┘
     │               │               │               │
     └───────────────┴───────────────┴───────────────┘
                           │
                    Warning if not
                    in library at
                    parent publish
```

### MaterialMapping Usage

```
┌─────────────────┐
│    Created      │
│   (LastUsedAt   │
│     = null)     │
└────────┬────────┘
         │ First use at Pull Update
         ▼
┌─────────────────┐
│     Active      │
│  (LastUsedAt    │◄─────────────┐
│    updated)     │              │
└────────┬────────┘              │
         │ Not used for 30+ days │
         ▼                       │
┌─────────────────┐              │
│    Stale        │──────────────┘
│ (Deletion       │  Used again
│  suggested)     │
└─────────────────┘
```
