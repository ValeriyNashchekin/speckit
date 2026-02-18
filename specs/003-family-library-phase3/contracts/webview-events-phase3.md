# WebView2 Events: Phase 3 Additions

**Date**: 2026-02-18
**Branch**: 003-family-library-phase3

This document describes WebView2 event **additions** for Phase 3. See `specs/001-family-library-mvp/contracts/webview-events.md` for base events and `specs/002-family-library-phase2/contracts/webview-events-phase2.md` for Phase 2 additions.

---

## Event Overview (Phase 3 Additions)

```
┌─────────────────────────────────────────────────────────────────┐
│                         Revit Plugin                             │
├─────────────────────────────────────────────────────────────────┤
│  OUT: revit:nested:detected      → UI: Show dependencies        │
│  OUT: revit:load:preview         → UI: Pre-load summary         │
│  OUT: revit:material:fallback    → UI: Material not found       │
│  IN:  ui:load-with-nested        ← UI: Load with versions       │
│  IN:  ui:material-mapping:save   ← UI: Save mapping decision    │
│  IN:  ui:nested:view-changes     ← UI: View nested diff         │
└─────────────────────────────────────────────────────────────────┘
```

---

## Plugin → UI Events

### revit:nested:detected

Emitted when nested families are detected during Publish workflow.

**When**: After analyzing family via EditFamily() before Publish

**Payload**:
```typescript
interface NestedDetectedEvent {
  type: 'revit:nested:detected';
  parentFamilyId: string;
  parentFamilyName: string;
  nestedFamilies: NestedFamilyInfo[];
}

interface NestedFamilyInfo {
  familyName: string;
  isShared: boolean;
  hasRole: boolean;
  roleName?: string;
  inLibrary: boolean;
  libraryVersion?: number;
  status: 'ready' | 'not_published' | 'no_role';
}
```

**UI Response**:
- Show dependencies list in Queue (Tab 2)
- Display warning icon if Shared nested not published
- Recommend publishing nested before parent

**Example**:
```json
{
  "type": "revit:nested:detected",
  "parentFamilyId": "guid-123",
  "parentFamilyName": "FreeAxez_Table",
  "nestedFamilies": [
    {
      "familyName": "FreeAxez_Table_Leg",
      "isShared": true,
      "hasRole": true,
      "roleName": "FreeAxez_Table_Leg",
      "inLibrary": true,
      "libraryVersion": 3,
      "status": "ready"
    },
    {
      "familyName": "FreeAxez_Handle",
      "isShared": true,
      "hasRole": false,
      "inLibrary": false,
      "status": "not_published"
    },
    {
      "familyName": "InternalBracket",
      "isShared": false,
      "hasRole": false,
      "inLibrary": false,
      "status": "no_role"
    }
  ]
}
```

---

### revit:load:preview

Emitted when showing pre-load summary before loading family to project.

**When**: After user clicks "Load to Project" on a family with nested dependencies

**Payload**:
```typescript
interface LoadPreviewEvent {
  type: 'revit:load:preview';
  parentFamily: ParentFamilyInfo;
  nestedFamilies: NestedLoadInfo[];
  summary: LoadSummary;
}

interface ParentFamilyInfo {
  familyId: string;
  familyName: string;
  roleName: string;
  version: number;
}

interface NestedLoadInfo {
  familyName: string;
  roleName?: string;
  rfaVersion: number;          // Version in parent RFA
  libraryVersion?: number;     // Latest in library
  projectVersion?: number;     // Currently in project
  recommendedAction: 'load_from_rfa' | 'update_from_library' | 'keep_project' | 'no_action';
  hasConflict: boolean;
}

interface LoadSummary {
  totalToLoad: number;
  newCount: number;
  updateCount: number;
  conflictCount: number;
}
```

**UI Response**:
- Show Pre-Load Summary modal
- Display each nested with version comparison
- Allow user to choose action per nested family
- Enable/disable "Load All" based on conflicts

**Example**:
```json
{
  "type": "revit:load:preview",
  "parentFamily": {
    "familyId": "guid-456",
    "familyName": "FreeAxez_Table",
    "roleName": "FreeAxez_Table",
    "version": 4
  },
  "nestedFamilies": [
    {
      "familyName": "FreeAxez_Table_Leg",
      "roleName": "FreeAxez_Table_Leg",
      "rfaVersion": 3,
      "libraryVersion": 5,
      "projectVersion": 2,
      "recommendedAction": "update_from_library",
      "hasConflict": true
    },
    {
      "familyName": "FreeAxez_Handle",
      "roleName": "FreeAxez_Handle",
      "rfaVersion": 1,
      "libraryVersion": 2,
      "projectVersion": null,
      "recommendedAction": "load_from_rfa",
      "hasConflict": false
    }
  ],
  "summary": {
    "totalToLoad": 3,
    "newCount": 1,
    "updateCount": 2,
    "conflictCount": 1
  }
}
```

---

### revit:material:fallback

Emitted when material is not found during Pull Update and no mapping exists.

**When**: During System Family Pull Update, material lookup fails

**Payload**:
```typescript
interface MaterialFallbackEvent {
  type: 'revit:material:fallback';
  systemTypeId: string;
  systemTypeName: string;
  missingMaterial: MissingMaterialInfo;
  availableOptions: MaterialOption[];
}

interface MissingMaterialInfo {
  templateMaterialName: string;
  category?: string;
  layerIndex?: number;
}

interface MaterialOption {
  id: string;
  name: string;
  type: 'existing' | 'create' | 'default' | 'skip';
}
```

**UI Response**:
- Show Material Fallback dialog
- List existing materials in category
- Offer Create New / Use Default / Skip options
- Allow saving as mapping for future

**Example**:
```json
{
  "type": "revit:material:fallback",
  "systemTypeId": "guid-789",
  "systemTypeName": "Wall_External_200",
  "missingMaterial": {
    "templateMaterialName": "Brick, Common",
    "category": "Masonry",
    "layerIndex": 0
  },
  "availableOptions": [
    {"id": "mat-1", "name": "Кирпич красный", "type": "existing"},
    {"id": "mat-2", "name": "Кирпич белый", "type": "existing"},
    {"id": "create", "name": "Create 'Brick, Common'", "type": "create"},
    {"id": "default", "name": "Use Default", "type": "default"},
    {"id": "skip", "name": "Skip layer", "type": "skip"}
  ]
}
```

---

## UI → Plugin Events

### ui:load-with-nested

Sent when user confirms load with selected nested family versions.

**When**: User clicks "Load All" or "Load Selected" in Pre-Load Summary

**Payload**:
```typescript
interface LoadWithNestedEvent {
  type: 'ui:load-with-nested';
  parentFamilyId: string;
  nestedChoices: NestedLoadChoice[];
}

interface NestedLoadChoice {
  familyName: string;
  source: 'rfa' | 'library';
  targetVersion?: number;  // If library source
}
```

**Plugin Response**:
1. Build IFamilyLoadOptions with choices
2. Load parent family
3. For each nested with source='library':
   - Download from library
   - LoadFamily to override RFA version
4. Report completion

**Example**:
```json
{
  "type": "ui:load-with-nested",
  "parentFamilyId": "guid-456",
  "nestedChoices": [
    {
      "familyName": "FreeAxez_Table_Leg",
      "source": "library",
      "targetVersion": 5
    },
    {
      "familyName": "FreeAxez_Handle",
      "source": "rfa"
    }
  ]
}
```

---

### ui:material-mapping:save

Sent when user saves a material mapping decision.

**When**: User selects material and checks "Remember for this project"

**Payload**:
```typescript
interface MaterialMappingSaveEvent {
  type: 'ui:material-mapping:save';
  projectId: string;
  templateMaterialName: string;
  projectMaterialName: string;
  applyToCurrent: boolean;
}
```

**Plugin Response**:
1. Send to API: POST /api/material-mappings
2. If applyToCurrent=true, apply to current Pull Update
3. Store in local cache for session

**Example**:
```json
{
  "type": "ui:material-mapping:save",
  "projectId": "proj-guid-123",
  "templateMaterialName": "Brick, Common",
  "projectMaterialName": "Кирпич красный",
  "applyToCurrent": true
}
```

---

### ui:nested:view-changes

Sent when user wants to see diff between nested versions.

**When**: User clicks "Show Details" or "View Changes" on nested family in Pre-Load Summary

**Payload**:
```typescript
interface NestedViewChangesEvent {
  type: 'ui:nested:view-changes';
  familyName: string;
  fromVersion: number;
  toVersion: number;
}
```

**Plugin Response**:
- Fetch change set from API
- Display in change viewer modal

**Example**:
```json
{
  "type": "ui:nested:view-changes",
  "familyName": "FreeAxez_Table_Leg",
  "fromVersion": 2,
  "toVersion": 5
}
```

---

## Complete Event Flow Examples

### Flow 1: Publish Family with Nested Dependencies

```
User: Select family → Click "Publish"
       │
       ▼
Plugin: EditFamily → Detect nested
       │
       ▼
Event: revit:nested:detected
       │
       ▼
UI: Show dependencies modal
    "FreeAxez_Handle not published. Continue?"
       │
       ▼
User: Click "Publish Anyway"
       │
       ▼
Plugin: Upload RFA, save dependencies to DB
       │
       ▼
Event: revit:operation-complete { success: true }
```

### Flow 2: Load Family with Nested Version Selection

```
User: Click "Load to Project" on FreeAxez_Table
       │
       ▼
Plugin: Fetch dependencies, compare versions
       │
       ▼
Event: revit:load:preview
       │
       ▼
UI: Show Pre-Load Summary
    - Leg: RFA=v3, Library=v5 → Recommend update
    - Handle: RFA=v1, Library=v2 → OK
       │
       ▼
User: Select "Update Leg from Library", Click "Load"
       │
       ▼
Event: ui:load-with-nested
       │
       ▼
Plugin: Load parent, then override Leg from library
       │
       ▼
Event: revit:operation-complete { success: true, loaded: [...] }
```

### Flow 3: Material Fallback During Pull Update

```
User: Click "Pull Update" on Wall_External_200
       │
       ▼
Plugin: Parse JSON, lookup materials
       │
       ▼
Plugin: "Brick, Common" not found, no mapping
       │
       ▼
Event: revit:material:fallback
       │
       ▼
UI: Show dialog
    - Select existing: "Кирпич красный"
    - [✓] Remember for this project
       │
       ▼
User: Select, check Remember, Click Apply
       │
       ▼
Event: ui:material-mapping:save + selection
       │
       ▼
Plugin: Save mapping, apply to current update
       │
       ▼
Event: revit:operation-complete { success: true }
```

---

## TypeScript Interfaces Summary

```typescript
// Phase 3 Event Types
export type Phase3PluginToUIEvent =
  | NestedDetectedEvent
  | LoadPreviewEvent
  | MaterialFallbackEvent;

export type Phase3UIToPluginEvent =
  | LoadWithNestedEvent
  | MaterialMappingSaveEvent
  | NestedViewChangesEvent;

// All event types (including MVP and Phase 2)
export type AllPluginToUIEvent =
  | Phase3PluginToUIEvent
  | Phase2PluginToUIEvent  // From webview-events-phase2.md
  | MVPPluginToUIEvent;    // From webview-events.md

export type AllUIToPluginEvent =
  | Phase3UIToPluginEvent
  | Phase2UIToPluginEvent
  | MVPUIToPluginEvent;
```
