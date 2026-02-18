# Data Model: Family Library Phase 2

**Date**: 2026-02-18
**Branch**: 002-family-library-phase2

This document describes data model **additions** for Phase 2. See `specs/001-family-library-mvp/data-model.md` for base entities.

---

## New Enumerations

### ChangeCategory

```csharp
public enum ChangeCategory
{
    Name = 0,       // ✏️ Family renamed
    Category = 1,   // 📁 Revit category changed
    Types = 2,      // ➕➖ Types added/removed
    Parameters = 3, // 📝 Parameters changed
    Geometry = 4,   // 🔧 Geometry changed (hash-based)
    Txt = 5         // 📄 Type catalog changed
}
```

### FamilyScanStatus

```csharp
public enum FamilyScanStatus
{
    UpToDate = 0,        // 🟢 Hash matches library
    UpdateAvailable = 1, // 🟡 Library has newer version
    LegacyMatch = 2,     // 🔵 Matched by recognition rules
    Unmatched = 3,       // ⚪ No stamp, no rule match
    LocalModified = 4    // 📝 Local changes detected
}
```

---

## Existing Entity Extensions

### FamilyVersion

**New field usage** (entity already exists from MVP):

| Field | Phase 2 Usage |
|-------|---------------|
| `SnapshotJSON` | Standardized schema for change detection |
| `OriginalCatalogName` | TXT file name for display |

**SnapshotJSON Schema** (standardized):

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

---

## New DTOs (Not Entities)

### BatchCheckRequest

```csharp
public class BatchCheckRequest
{
    public List<FamilyCheckItem> Families { get; init; } = [];
}

public class FamilyCheckItem
{
    public string RoleName { get; init; } = string.Empty;
    public string Hash { get; init; } = string.Empty;
}
```

### BatchCheckResponse

```csharp
public class BatchCheckResponse
{
    public List<FamilyCheckResult> Results { get; init; } = [];
}

public class FamilyCheckResult
{
    public string RoleName { get; init; } = string.Empty;
    public FamilyScanStatus Status { get; init; }
    public int? LibraryVersion { get; init; }
    public int? CurrentVersion { get; init; }
    public string? LibraryHash { get; init; }
}
```

### ChangeSet DTO

```csharp
public class ChangeSetDto
{
    public List<ChangeItemDto> Items { get; init; } = [];
    public bool HasChanges => Items.Count > 0;
}

public class ChangeItemDto
{
    public ChangeCategory Category { get; init; }
    public string? PreviousValue { get; init; }
    public string? CurrentValue { get; init; }
    public List<string>? AddedItems { get; init; }
    public List<string>? RemovedItems { get; init; }
    public List<ParameterChangeDto>? ParameterChanges { get; init; }
}

public class ParameterChangeDto
{
    public string Name { get; init; } = string.Empty;
    public string? PreviousValue { get; init; }
    public string? CurrentValue { get; init; }
}
```

### ScanResult DTO

```csharp
public class ScanResultDto
{
    public List<ScannedFamilyDto> Families { get; init; } = [];
    public int TotalCount { get; init; }
    public int UpToDateCount { get; init; }
    public int UpdateAvailableCount { get; init; }
    public int LegacyMatchCount { get; init; }
    public int UnmatchedCount { get; init; }
}

public class ScannedFamilyDto
{
    public string UniqueId { get; init; } = string.Empty;
    public string FamilyName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string? RoleName { get; init; }
    public bool IsAutoRole { get; init; }
    public FamilyScanStatus Status { get; init; }
    public int? LocalVersion { get; init; }
    public int? LibraryVersion { get; init; }
}
```

---

## MEP System Types JSON Structure

### Group A (Roofs, Ceilings, Foundations)

Uses same CompoundStructure JSON as Walls/Floors (MVP Phase 6).

```json
{
  "typeName": "Roof_Concrete_200",
  "category": "Roofs",
  "systemFamily": "Roof Types",
  "compoundStructure": {
    "layers": [
      {"materialName": "Concrete", "thickness": 200, "function": "Structure"},
      {"materialName": "Insulation", "thickness": 50, "function": "Insulation"}
    ]
  },
  "parameters": { ... }
}
```

### Group B (Pipes, Ducts)

```json
{
  "typeName": "Standard_DN50",
  "category": "Pipes",
  "systemFamily": "Pipe Types",
  "parameters": {
    "Routing Preference": "Standard"
  },
  "routingPreferences": {
    "segments": [
      {
        "materialName": "Carbon Steel",
        "scheduleType": "40"
      }
    ],
    "fittings": [
      {
        "familyName": "Elbow",
        "typeName": "Standard",
        "angleRange": "0-90"
      }
    ]
  }
}
```

---

## Database Changes

**No migrations required** for Phase 2. All necessary fields exist from MVP:
- `FamilyVersion.SnapshotJSON` — Used for change detection
- `FamilyVersion.OriginalCatalogName` — Added in MVP Phase 10

**New indexes** (optional optimization):

```sql
-- Index for batch check performance (if needed)
CREATE INDEX IX_Families_RoleId_Include
ON Families(RoleId)
INCLUDE (CurrentVersion);
```

---

## Relationships Summary (Phase 2 Additions)

No new relationships. Phase 2 extends existing entities with:
- New enumeration types
- New DTOs for API contracts
- Standardized JSON schema for SnapshotJSON
