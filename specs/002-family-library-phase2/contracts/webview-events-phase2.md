# WebView2 Events Contract: Phase 2

**Date**: 2026-02-18
**Branch**: 002-family-library-phase2

This document extends `contracts/webview-events.md` with Phase 2 specific events.

---

## Event Summary

| Event | Direction | Purpose |
|-------|-----------|---------|
| `revit:scan:result` | Plugin → UI | Project scan completed |
| `revit:scan:progress` | Plugin → UI | Scan progress update |
| `revit:update:progress` | Plugin → UI | Update progress |
| `revit:update:complete` | Plugin → UI | Batch update completed |
| `revit:changes:result` | Plugin → UI | Change diff computed |
| `ui:scan-project` | UI → Plugin | Request project scan |
| `ui:update-families` | UI → Plugin | Update selected families |
| `ui:stamp-legacy` | UI → Plugin | Stamp legacy families |
| `ui:get-changes` | UI → Plugin | Request change diff |

---

## Plugin → UI Events

### `revit:scan:result`

Emitted when project scan completes.

```typescript
interface ScanResultEvent {
  type: 'revit:scan:result';
  payload: {
    families: ScannedFamily[];
    totalCount: number;
    summary: {
      upToDate: number;
      updateAvailable: number;
      legacyMatch: number;
      unmatched: number;
      localModified: number;
    };
  };
}

interface ScannedFamily {
  uniqueId: string;
  familyName: string;
  category: string;
  roleName?: string;
  isAutoRole: boolean;  // true if matched by recognition rules
  status: 'UpToDate' | 'UpdateAvailable' | 'LegacyMatch' | 'Unmatched' | 'LocalModified';
  localVersion?: number;
  libraryVersion?: number;
  localHash?: string;
  libraryHash?: string;
}
```

### `revit:scan:progress`

Emitted during long scans.

```typescript
interface ScanProgressEvent {
  type: 'revit:scan:progress';
  payload: {
    scanned: number;
    total: number;
    currentFamily: string;
  };
}
```

### `revit:update:progress`

Emitted during batch update.

```typescript
interface UpdateProgressEvent {
  type: 'revit:update:progress';
  payload: {
    completed: number;
    total: number;
    currentFamily: string;
    success: number;
    failed: number;
  };
}
```

### `revit:update:complete`

Emitted when batch update finishes.

```typescript
interface UpdateCompleteEvent {
  type: 'revit:update:complete';
  payload: {
    total: number;
    success: number;
    failed: number;
    errors: Array<{
      familyName: string;
      error: string;
    }>;
  };
}
```

### `revit:changes:result`

Emitted when change diff is computed.

```typescript
interface ChangesResultEvent {
  type: 'revit:changes:result';
  payload: {
    familyUniqueId: string;
    changes: ChangeSet;
  };
}

interface ChangeSet {
  items: ChangeItem[];
  hasChanges: boolean;
}

interface ChangeItem {
  category: 'Name' | 'Category' | 'Types' | 'Parameters' | 'Geometry' | 'Txt';
  previousValue?: string;
  currentValue?: string;
  addedItems?: string[];
  removedItems?: string[];
  parameterChanges?: ParameterChange[];
}

interface ParameterChange {
  name: string;
  previousValue?: string;
  currentValue?: string;
}
```

---

## UI → Plugin Events

### `ui:scan-project`

Request project scan.

```typescript
interface ScanProjectEvent {
  type: 'ui:scan-project';
  payload: {
    includeSystemFamilies: boolean;
  };
}
```

### `ui:update-families`

Request batch update.

```typescript
interface UpdateFamiliesEvent {
  type: 'ui:update-families';
  payload: {
    families: Array<{
      uniqueId: string;
      roleName?: string;
    }>;
    showPreview: boolean;  // Show pre-update preview
  };
}
```

### `ui:stamp-legacy`

Stamp families matched by legacy recognition.

```typescript
interface StampLegacyEvent {
  type: 'ui:stamp-legacy';
  payload: {
    families: Array<{
      uniqueId: string;
      roleName: string;
    }>;
  };
}
```

### `ui:get-changes`

Request change diff for local modifications.

```typescript
interface GetChangesEvent {
  type: 'ui:get-changes';
  payload: {
    uniqueId: string;
  };
}
```

---

## TypeScript Interfaces

Add to `src/FamilyLibrary.Web/src/app/core/models/webview-events.model.ts`:

```typescript
// Phase 2 additions

export interface ScanResultEvent {
  type: 'revit:scan:result';
  payload: ScanResult;
}

export interface ScanResult {
  families: ScannedFamily[];
  totalCount: number;
  summary: ScanSummary;
}

export interface ScannedFamily {
  uniqueId: string;
  familyName: string;
  category: string;
  roleName?: string;
  isAutoRole: boolean;
  status: FamilyScanStatus;
  localVersion?: number;
  libraryVersion?: number;
}

export type FamilyScanStatus =
  | 'UpToDate'
  | 'UpdateAvailable'
  | 'LegacyMatch'
  | 'Unmatched'
  | 'LocalModified';

export interface ChangeSet {
  items: ChangeItem[];
  hasChanges: boolean;
}

export interface ChangeItem {
  category: ChangeCategory;
  previousValue?: string;
  currentValue?: string;
  addedItems?: string[];
  removedItems?: string[];
  parameterChanges?: ParameterChange[];
}

export type ChangeCategory =
  | 'Name'
  | 'Category'
  | 'Types'
  | 'Parameters'
  | 'Geometry'
  | 'Txt';

export interface ParameterChange {
  name: string;
  previousValue?: string;
  currentValue?: string;
}
```

---

## C# Event Classes

Add to `FamilyLibrary.Plugin/Core/Models/WebViewEvents.cs`:

```csharp
// Phase 2 additions

public class ScanResultEvent
{
    public const string Type = "revit:scan:result";
    public ScanResultPayload Payload { get; init; } = new();
}

public class ScanResultPayload
{
    public List<ScannedFamily> Families { get; init; } = [];
    public int TotalCount { get; init; }
    public ScanSummary Summary { get; init; } = new();
}

public class ScannedFamily
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

public enum FamilyScanStatus
{
    UpToDate,
    UpdateAvailable,
    LegacyMatch,
    Unmatched,
    LocalModified
}

public class ChangeSet
{
    public List<ChangeItem> Items { get; init; } = [];
    public bool HasChanges => Items.Count > 0;
}

public class ChangeItem
{
    public ChangeCategory Category { get; init; }
    public string? PreviousValue { get; init; }
    public string? CurrentValue { get; init; }
    public List<string>? AddedItems { get; init; }
    public List<string>? RemovedItems { get; init; }
    public List<ParameterChange>? ParameterChanges { get; init; }
}

public enum ChangeCategory
{
    Name,
    Category,
    Types,
    Parameters,
    Geometry,
    Txt
}
```

---

## Integration Notes

1. **Frontend Agent**: Reads this contract, extends existing `webview-events.model.ts`
2. **Plugin Agent**: Reads this contract, extends existing `WebViewEvents.cs`
3. **Both agents**: Must update `RevitBridgeService` / `RevitBridge` to handle new events
