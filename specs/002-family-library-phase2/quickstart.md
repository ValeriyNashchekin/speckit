# Quickstart: Family Library Phase 2

This guide covers Phase 2 features: Scanner, Change Tracking, and MEP System Families.

## Prerequisites

- MVP completed and deployed
- Backend running on `http://localhost:5000`
- Frontend running on `http://localhost:4200`
- Azurite running on `http://127.0.0.1:10000`

---

## Feature 1: Project Scanner

### Command: Update Families from Library

Available in any Revit project (not just template).

**Access Rights:**
| Page | Roles |
|------|-------|
| Add to Library | БИМ-менеджер, Администратор |
| Update from Library | Все роли |

### API Usage

**Batch Check** (check family statuses):
```http
POST /api/families/batch-check
Content-Type: application/json

{
  "families": [
    {"roleName": "FreeAxez_Table", "hash": "abc123"},
    {"roleName": "FreeAxez_Chair", "hash": "def456"}
  ]
}
```

**Response:**
```json
{
  "results": [
    {
      "roleName": "FreeAxez_Table",
      "status": "UpToDate",
      "libraryVersion": 2
    },
    {
      "roleName": "FreeAxez_Chair",
      "status": "UpdateAvailable",
      "libraryVersion": 3,
      "currentVersion": 1
    }
  ]
}
```

### FamilyScanStatus Values

| Status | Description |
|--------|-------------|
| UpToDate | Family matches library version |
| UpdateAvailable | Newer version exists in library |
| LocalModified | Local changes detected (not in library) |
| NotInLibrary | Family not found in library |
| LegacyMatch | Matched by legacy rules |

### Scanner Flow

```
1. User opens scanner → ui:scan-project
2. Plugin scans document → revit:scan:result
3. User selects families → ui:update-families
4. Plugin updates families → revit:update:progress
5. Update complete → revit:update:complete
6. View changes result → revit:changes:result
```

---

## Feature 2: Change Tracking

### Change Categories

| Icon | Category | Detection |
|------|----------|-----------|
| ✏️ | Name | familyName comparison |
| 📁 | Category | category comparison |
| ➕➖ | Types | types[] set difference |
| 📝 | Parameters | parameters[] by name+value |
| 🔧 | Geometry | hasGeometryChanges flag |
| 📄 | TXT | txtHash comparison |

### API Usage

**Get Changes Between Versions:**
```http
GET /api/families/{id}/changes?fromVersion=2&toVersion=3
```

**Response:**
```json
{
  "items": [
    {
      "category": "Name",
      "previousValue": "FreeAxez_Table_v1",
      "currentValue": "FreeAxez_Table_v2"
    },
    {
      "category": "Types",
      "addedItems": ["Type_D"],
      "removedItems": []
    },
    {
      "category": "Parameters",
      "parameterChanges": [
        {
          "name": "Height",
          "previousValue": "800",
          "currentValue": "900"
        }
      ]
    }
  ],
  "hasChanges": true
}
```

**Pre-Update Preview:**
```http
GET /api/families/{id}/update-preview?currentVersion=1&targetVersion=3
```

**Response:**
```json
{
  "familyId": "abc123",
  "currentVersion": 1,
  "targetVersion": 3,
  "breakingChanges": false,
  "changesSummary": {
    "name": "FreeAxez_Table_v1 → FreeAxez_Table_v3",
    "typesAdded": 2,
    "typesRemoved": 1,
    "parametersChanged": 4,
    "geometryChanged": true
  },
  "warnings": [
    "Type 'Type_Old' will be removed"
  ]
}
```

**Local Changes Detection:**
```http
POST /api/families/{id}/local-changes
Content-Type: application/json

{
  "localSnapshotJson": "{\"familyName\":\"FreeAxez_Table\",\"types\":[\"Type_A\",\"Type_B\"],\"parameters\":[{\"name\":\"Height\",\"value\":\"900\"}]}"
}
```

**Response:**
```json
{
  "hasLocalChanges": true,
  "changes": [
    {
      "category": "Parameters",
      "parameterChanges": [
        {
          "name": "Height",
          "libraryValue": "800",
          "localValue": "900"
        }
      ]
    },
    {
      "category": "Types",
      "addedItems": ["Type_C"],
      "removedItems": []
    }
  ]
}
```

### View Changes Modal

```
Локальные изменения (не опубликованы):

✏️ Name: "FreeAxez_Table" → "FreeAxez_Table_v2"
➕ Type added: "Type_D" (всего типов: 4)
📝 Parameter changed:
   • Height: 800 → 900
   • Material: "Oak" → "Pine"
🔧 Geometry: изменена

[Discard Changes] [Publish]
```

---

## Feature 3: MEP System Families

### Supported Categories

| Group | Categories |
|-------|------------|
| A (full) | RoofType, CeilingType, FoundationType |
| B (MEP) | PipeType, DuctType |

### JSON Structure (PipeType)

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
      {"materialName": "Carbon Steel", "scheduleType": "40"}
    ],
    "fittings": [
      {"familyName": "Elbow", "typeName": "Standard"}
    ]
  }
}
```

### Pull Update for MEP

1. Load JSON from library
2. Find PipeType by role
3. Apply RoutingPreferences from JSON
4. Map fittings by familyName + typeName

---

## Performance Targets

| Operation | Target |
|-----------|--------|
| Scan 1000 families | < 5 seconds |
| Batch check 1000 families | < 500ms |
| Mass update 50 families | < 60 seconds |
| Virtual scroll table | 5000+ families responsive |

---

## Error Handling

### Scanner Errors

```typescript
// Partial failure in batch update
{
  "success": 45,
  "failed": 5,
  "errors": [
    {"familyName": "Broken_Family", "error": "Geometry invalid"}
  ]
}
```

### Legacy Match Conflicts

When multiple rules match same family:
- First rule wins
- Warning logged
- User can manually select different role
