# Data Model: Family Library MVP

**Date**: 2026-02-17
**Branch**: 001-family-library-mvp

---

## Entity Overview

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│    Category     │     │     FamilyRole  │     │      Tag        │
├─────────────────┤     ├─────────────────┤     ├─────────────────┤
│ Id (PK)         │←────│ CategoryId (FK) │     │ Id (PK)         │
│ Name            │     │ Id (PK)         │     │ Name            │
│ Description     │     │ Name (unique)   │     │ Color           │
│ SortOrder       │     │ Type            │     └────────┬────────┘
└─────────────────┘     │ Description     │              │
                        └────────┬────────┘              │
                                 │                       │
                    ┌────────────┴────────────┐          │
                    │                         │          │
              ┌─────▼─────┐           ┌───────▼──────┐   │
              │Recognition│           │    Family    │◄──┘
              │   Rule    │           │  (Loadable)  │
              ├───────────┤           ├──────────────┤
              │ Id (PK)   │           │ Id (PK)      │
              │ RoleId(FK)│           │ RoleId (FK)  │
              │ RootNode  │           │ FamilyName   │
              │ Formula   │           │ CurrentVer   │
              └───────────┘           └──────┬───────┘
                                             │
                                    ┌────────▼────────┐
                                    │  FamilyVersion  │
                                    ├─────────────────┤
                                    │ Id (PK)         │
                                    │ FamilyId (FK)   │
                                    │ Version         │
                                    │ Hash            │
                                    │ PreviousHash    │
                                    │ BlobPath        │
                                    │ CatalogBlobPath │
                                    │ OriginalFileName│
                                    │ CommitMessage   │
                                    │ SnapshotJSON    │
                                    │ PublishedAt     │
                                    │ PublishedBy     │
                                    └─────────────────┘

┌─────────────────┐     ┌─────────────────┐
│   SystemType    │     │     Draft       │
├─────────────────┤     ├─────────────────┤
│ Id (PK)         │     │ Id (PK)         │
│ RoleId (FK)     │     │ FamilyName      │
│ TypeName        │     │ FamilyUniqueId  │
│ Category        │     │ SelectedRoleId  │
│ SystemFamily    │     │ TemplateId      │
│ Group           │     │ Status          │
│ JSON            │     │ CreatedAt       │
│ CurrentVersion  │     │ LastSeen        │
│ Hash            │     └─────────────────┘
└─────────────────┘

┌─────────────────┐
│FamilyNameMapping│
├─────────────────┤
│ Id (PK)         │
│ FamilyName      │
│ RoleName        │
│ ProjectId       │
│ LastSeenAt      │
└─────────────────┘
```

---

## Entities

### FamilyRole

Represents a functional role for a family (immutable name after creation).

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | `uniqueidentifier` | PK, NOT NULL | Auto-generated GUID |
| Name | `nvarchar(100)` | UNIQUE, NOT NULL, READ-ONLY | System name, e.g., "FreeAxez_Table" |
| Type | `int` | NOT NULL, READ-ONLY | 0 = Loadable, 1 = System |
| Description | `nvarchar(500)` | NULLABLE | Human-readable description |
| CategoryId | `uniqueidentifier` | FK → Category, NULLABLE | Optional categorization |
| CreatedAt | `datetime2` | NOT NULL | Creation timestamp |
| UpdatedAt | `datetime2` | NOT NULL | Last update timestamp |

**Validation Rules**:
- Name must match pattern `[A-Za-z][A-Za-z0-9_]*`
- Name cannot be changed after creation
- Type cannot be changed after creation
- Cannot delete role if families are associated

---

### Category

Organizational grouping for roles.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | `uniqueidentifier` | PK, NOT NULL | Auto-generated GUID |
| Name | `nvarchar(100)` | UNIQUE, NOT NULL | Display name |
| Description | `nvarchar(500)` | NULLABLE | Optional description |
| SortOrder | `int` | NOT NULL, DEFAULT 0 | UI display order |

---

### Tag

Labels for filtering and categorization.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | `uniqueidentifier` | PK, NOT NULL | Auto-generated GUID |
| Name | `nvarchar(50)` | UNIQUE, NOT NULL | Tag name |
| Color | `nvarchar(20)` | NULLABLE | Hex color for UI |

**Many-to-Many**: FamilyRole ↔ Tag via `FamilyRoleTag` junction table

---

### RecognitionRule

Defines how to match family names to roles.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | `uniqueidentifier` | PK, NOT NULL | Auto-generated GUID |
| RoleId | `uniqueidentifier` | FK → FamilyRole, NOT NULL, UNIQUE | One rule per role |
| RootNode | `nvarchar(max)` | NOT NULL | JSON tree of RecognitionNode |
| Formula | `nvarchar(500)` | NOT NULL | Human-readable formula string |

**RecognitionNode Structure** (stored in RootNode as JSON):
```json
{
  "type": "group",
  "operator": "AND",
  "children": [
    {
      "type": "condition",
      "operator": "Contains",
      "value": "Table"
    },
    {
      "type": "condition",
      "operator": "Contains",
      "value": "FreeAxez"
    }
  ]
}
```

---

### Family (Loadable)

Represents a loadable family in the library.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | `uniqueidentifier` | PK, NOT NULL | Auto-generated GUID |
| RoleId | `uniqueidentifier` | FK → FamilyRole, NOT NULL | Functional role |
| FamilyName | `nvarchar(200)` | NOT NULL | Current family name |
| CurrentVersion | `int` | NOT NULL, DEFAULT 1 | Latest version number |
| CreatedAt | `datetime2` | NOT NULL | First publish date |
| UpdatedAt | `datetime2` | NOT NULL | Last publish date |

**Indexes**:
- `IX_Family_RoleId` on RoleId
- `IX_Family_FamilyName` on FamilyName

---

### FamilyVersion

Represents a specific version of a family.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | `uniqueidentifier` | PK, NOT NULL | Auto-generated GUID |
| FamilyId | `uniqueidentifier` | FK → Family, NOT NULL | Parent family |
| Version | `int` | NOT NULL | Version number (1, 2, 3...) |
| Hash | `nvarchar(64)` | NOT NULL | SHA256 content hash |
| PreviousHash | `nvarchar(64)` | NULLABLE | Hash of previous version |
| BlobPath | `nvarchar(500)` | NOT NULL | Azure Blob path to RFA |
| CatalogBlobPath | `nvarchar(500)` | NULLABLE | Azure Blob path to TXT catalog |
| OriginalFileName | `nvarchar(200)` | NOT NULL | Original RFA filename |
| OriginalCatalogName | `nvarchar(200)` | NULLABLE | Original TXT filename |
| CommitMessage | `nvarchar(500)` | NULLABLE | User-provided change description |
| SnapshotJSON | `nvarchar(max)` | NOT NULL | JSON snapshot of family metadata |
| PublishedAt | `datetime2` | NOT NULL | Publish timestamp |
| PublishedBy | `nvarchar(200)` | NOT NULL | User who published |

**Indexes**:
- `IX_FamilyVersion_FamilyId_Version` UNIQUE on (FamilyId, Version)
- `IX_FamilyVersion_Hash` on Hash (for duplicate detection)

---

### SystemType

Represents a system family type (WallType, FloorType, etc.).

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | `uniqueidentifier` | PK, NOT NULL | Auto-generated GUID |
| RoleId | `uniqueidentifier` | FK → FamilyRole, NOT NULL | Functional role |
| TypeName | `nvarchar(200)` | NOT NULL | Type name in Revit |
| Category | `nvarchar(100)` | NOT NULL | Revit category (Walls, Floors, etc.) |
| SystemFamily | `nvarchar(100)` | NOT NULL | System family name |
| Group | `int` | NOT NULL | 0=A, 1=B, 2=C, 3=D, 4=E |
| JSON | `nvarchar(max)` | NOT NULL | Serialized type data |
| CurrentVersion | `int` | NOT NULL, DEFAULT 1 | Latest version |
| Hash | `nvarchar(64)` | NOT NULL | SHA256 of normalized JSON |
| CreatedAt | `datetime2` | NOT NULL | First publish date |
| UpdatedAt | `datetime2` | NOT NULL | Last publish date |

**Indexes**:
- `IX_SystemType_RoleId` on RoleId
- `IX_SystemType_Category_TypeName` on (Category, TypeName)

---

### Draft

Tracks families being prepared for publish.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | `uniqueidentifier` | PK, NOT NULL | Auto-generated GUID |
| FamilyName | `nvarchar(200)` | NOT NULL | Family name from template |
| FamilyUniqueId | `nvarchar(100)` | NOT NULL | Revit Element.UniqueId |
| SelectedRoleId | `uniqueidentifier` | FK → FamilyRole, NULLABLE | Selected role |
| TemplateId | `uniqueidentifier` | NULLABLE | Reference to template project |
| Status | `int` | NOT NULL, DEFAULT 0 | 0=New, 1=RoleSelected, 2=Stamped, 3=Published |
| CreatedAt | `datetime2` | NOT NULL | Creation timestamp |
| LastSeen | `datetime2` | NOT NULL | Last scan timestamp |

**State Transitions**:
```
New (0) → RoleSelected (1) → Stamped (2) → Published (3)
                            ↓
                     Deleted (on Publish)
```

---

### FamilyNameMapping

Server-side fallback for ES data recovery.

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| Id | `uniqueidentifier` | PK, NOT NULL | Auto-generated GUID |
| FamilyName | `nvarchar(200)` | NOT NULL | Family name in project |
| RoleName | `nvarchar(100)` | NOT NULL | Associated role name |
| ProjectId | `uniqueidentifier` | NOT NULL | Project identifier |
| LastSeenAt | `datetime2` | NOT NULL | Last scan timestamp |

**Purpose**: When ES is lost, lookup by (FamilyName, ProjectId) to recover role association.

**Indexes**:
- `IX_FamilyNameMapping_FamilyName_ProjectId` UNIQUE on (FamilyName, ProjectId)

---

## Enumerations

### RoleType
```csharp
public enum RoleType
{
    Loadable = 0,
    System = 1
}
```

### DraftStatus
```csharp
public enum DraftStatus
{
    New = 0,
    RoleSelected = 1,
    Stamped = 2,
    Published = 3
}
```

### SystemFamilyGroup
```csharp
public enum SystemFamilyGroup
{
    GroupA = 0,  // CompoundStructure: Walls, Floors, Roofs, Ceilings, Foundations
    GroupB = 1,  // MEP: Pipes, Ducts (Phase 2)
    GroupC = 2,  // Railings, Stairs (Phase 3)
    GroupD = 3,  // Curtain Systems (Phase 3)
    GroupE = 4   // Simple: Levels, Grids, Ramps, Building Pads
}
```

### RecognitionOperator
```csharp
public enum RecognitionOperator
{
    Contains = 0,
    NotContains = 1
}

public enum LogicalOperator
{
    And = 0,
    Or = 1
}
```

---

## Relationships Summary

| From | To | Relationship | Notes |
|------|-----|--------------|-------|
| FamilyRole | Category | N:1 | Optional |
| FamilyRole | Tag | N:N | Via junction table |
| FamilyRole | RecognitionRule | 1:1 | One rule per role |
| Family | FamilyRole | N:1 | Required |
| Family | FamilyVersion | 1:N | Version history |
| SystemType | FamilyRole | N:1 | Required |
| Draft | FamilyRole | N:1 | Optional |

---

## Database Constraints

### Foreign Keys
- `FK_FamilyRole_Category` → ON DELETE SET NULL
- `FK_RecognitionRule_FamilyRole` → ON DELETE CASCADE
- `FK_Family_FamilyRole` → ON DELETE RESTRICT
- `FK_FamilyVersion_Family` → ON DELETE CASCADE
- `FK_SystemType_FamilyRole` → ON DELETE RESTRICT
- `FK_Draft_FamilyRole` → ON DELETE SET NULL

### Unique Constraints
- `UQ_FamilyRole_Name` on FamilyRole.Name
- `UQ_RecognitionRule_RoleId` on RecognitionRule.RoleId
- `UQ_Category_Name` on Category.Name
- `UQ_Tag_Name` on Tag.Name
- `UQ_FamilyVersion_FamilyId_Version` on (FamilyVersion.FamilyId, FamilyVersion.Version)
