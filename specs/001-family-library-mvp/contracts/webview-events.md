# WebView2 Event Contract

**Purpose**: Define all postMessage events between Revit Plugin and Angular Frontend
**Version**: 1.0.0
**Last Updated**: 2026-02-17

---

## Overview

Plugin and Frontend communicate via WebView2 `postMessage` API. Both sides MUST follow this contract exactly.

**Communication Pattern**:
```
┌─────────────────┐  postMessage   ┌─────────────────┐
│  Revit Plugin   │ ◄────────────► │ Angular Frontend │
│  (C#/WebView2)  │                │ (TypeScript)     │
└─────────────────┘                └─────────────────┘
```

**Key Files**:
- Plugin: `src/FamilyLibrary.Plugin/FamilyLibrary.Plugin/Infrastructure/WebView2/RevitBridge.cs`
- Frontend: `src/FamilyLibrary.Web/src/app/core/services/revit-bridge.service.ts`
- Frontend Types: `src/FamilyLibrary.Web/src/app/core/models/webview-events.model.ts`

---

## Event Format

All events MUST follow this structure:

```typescript
interface WebViewEvent<T = unknown> {
  type: string;           // Event name (e.g., "revit:ready")
  payload: T;             // Event data
  timestamp: number;      // Unix timestamp (optional, for debugging)
  correlationId?: string; // For request/response matching (optional)
}
```

---

## Plugin → Frontend Events

### `revit:ready`

Plugin loaded and ready to receive commands.

```typescript
interface RevitReadyPayload {
  version: string;          // Revit version (e.g., "2024", "2026")
  pluginVersion: string;    // Plugin version (e.g., "1.0.0")
  documentType: "Project" | "Family" | "None";
  documentPath?: string;    // Current document path
}
```

**When**: After WebView2 navigation completes
**Response**: None required

---

### `revit:families:list`

List of families from current document scan.

```typescript
interface FamilyItem {
  id: string;               // Revit Element.UniqueId
  name: string;             // Family name
  categoryName: string;     // Revit category
  isSystemFamily: boolean;
  hasStamp: boolean;        // ES stamp present?
  stampData?: StampData;    // If hasStamp
}

interface StampData {
  roleId: string;
  roleName: string;
  stampedAt: string;        // ISO 8601
  stampedBy: string;
  contentHash: string;
}

interface RevitFamiliesListPayload {
  families: FamilyItem[];
  scanDuration: number;     // milliseconds
  projectId: string;        // Document.UniqueId
}
```

**When**: After `ui:scan-families` request
**Triggered by**: `ui:scan-families`

---

### `revit:scan:complete`

Scan completed with summary (alternative to families:list for large documents).

```typescript
interface RevitScanCompletePayload {
  totalFamilies: number;
  loadableCount: number;
  systemCount: number;
  stampedCount: number;
  scanDuration: number;
}
```

---

### `revit:stamp:result`

Result of stamp operation.

```typescript
interface RevitStampResultPayload {
  success: boolean;
  familyId: string;
  familyName: string;
  roleId: string;
  roleName: string;
  error?: string;
}
```

**When**: After `ui:stamp` request
**Triggered by**: `ui:stamp`

---

### `revit:publish:result`

Result of publish operation.

```typescript
interface RevitPublishResultPayload {
  success: boolean;
  familyId?: string;        // Server family ID
  familyName: string;
  version?: number;
  error?: string;
}
```

**When**: After `ui:publish` request
**Triggered by**: `ui:publish`

---

### `revit:load:result`

Result of load family operation.

```typescript
interface RevitLoadResultPayload {
  success: boolean;
  familyId: string;
  familyName: string;
  loadedSymbols?: string[]; // Symbol names loaded
  error?: string;
}
```

**When**: After `ui:load-family` request
**Triggered by**: `ui:load-family`

---

### `revit:type-catalog:show`

Request to show type selection dialog.

```typescript
interface TypeCatalogEntry {
  typeName: string;
  parameters: Record<string, string | number>; // Parameter name → value
}

interface RevitTypeCatalogShowPayload {
  familyId: string;
  familyName: string;
  types: TypeCatalogEntry[];
  parameterHeaders: string[]; // Column headers
}
```

**When**: Family has type catalog (.txt file)
**Response**: `ui:type-catalog:select`

---

### `revit:error`

Generic error from Plugin.

```typescript
interface RevitErrorPayload {
  code: string;             // Error code (e.g., "TRANSACTION_FAILED")
  message: string;          // User-friendly message
  details?: string;         // Technical details (for logging)
  recoverable: boolean;     // Can user retry?
}
```

---

## Frontend → Plugin Events

### `ui:ready`

Frontend loaded and ready.

```typescript
interface UiReadyPayload {
  version: string;          // Frontend version
  locale: string;           // User locale
}
```

**When**: After Angular bootstrap
**Response**: `revit:ready` (if not already sent)

---

### `ui:scan-families`

Request to scan families in current document.

```typescript
interface UiScanFamiliesPayload {
  includeSystemFamilies?: boolean;  // Default: false
  groupFilter?: SystemFamilyGroup[]; // For system families
}

type SystemFamilyGroup = "A" | "B" | "C" | "D" | "E";
```

**When**: User opens Queue tab
**Response**: `revit:families:list` or `revit:scan:complete`

---

### `ui:stamp`

Stamp a family with role.

```typescript
interface UiStampPayload {
  familyId: string;         // Revit Element.UniqueId
  familyName: string;
  roleId: string;           // Selected role ID
  roleName: string;
}
```

**When**: User clicks "Stamp" button
**Response**: `revit:stamp:result`

---

### `ui:publish`

Publish family to library.

```typescript
interface UiPublishPayload {
  familyId: string;         // Revit Element.UniqueId
  familyName: string;
  commitMessage?: string;   // Version comment
  catalogFile?: string;     // Base64 encoded .txt file (optional)
}
```

**When**: User clicks "Publish" button
**Response**: `revit:publish:result`

---

### `ui:load-family`

Load family from library to project.

```typescript
interface UiLoadFamilyPayload {
  serverFamilyId: string;   // Server Family.ID
  familyName: string;
  version?: number;         // Specific version (default: latest)
  targetSymbolNames?: string[]; // For type catalog selection
}
```

**When**: User clicks "Load" button
**Response**: `revit:load:result`

---

### `ui:type-catalog:select`

User selected types from catalog.

```typescript
interface UiTypeCatalogSelectPayload {
  familyId: string;
  selectedTypes: string[];  // Type names to load
}
```

**When**: After type selection dialog
**Response**: `revit:load:result`

---

### `ui:navigate`

Navigate to URL in WebView2.

```typescript
interface UiNavigatePayload {
  url: string;              // Relative URL (e.g., "/library", "/roles")
}
```

**When**: User navigates within Angular app (for logging/tracking)

---

### `ui:log`

Log message from Frontend (for debugging).

```typescript
interface UiLogPayload {
  level: "debug" | "info" | "warn" | "error";
  message: string;
  data?: unknown;
}
```

---

## TypeScript Interfaces

All interfaces MUST be generated in:
`src/FamilyLibrary.Web/src/app/core/models/webview-events.model.ts`

```typescript
// Auto-generated from contracts/webview-events.md
// DO NOT EDIT MANUALLY

export interface WebViewEvent<T = unknown> {
  type: string;
  payload: T;
  timestamp?: number;
  correlationId?: string;
}

// ... all other interfaces
```

---

## Plugin Implementation

### Sending Events (C#)

```csharp
// In RevitBridge.cs
public void SendEvent<T>(string type, T payload)
{
    var event = new
    {
        type = type,
        payload = payload,
        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    };

    var json = JsonSerializer.Serialize(event);
    WebView.ExecuteScriptAsync($"window.dispatchEvent(new CustomEvent('revit-message', {{ detail: {json} }}))");
}
```

### Receiving Events (C#)

```csharp
// In RevitBridge.cs
public void Initialize()
{
    WebView.WebMessageReceived += (sender, args) =>
    {
        var json = args.WebMessageAsJson;
        var @event = JsonSerializer.Deserialize<WebViewEvent>(json);
        HandleEvent(@event);
    };
}
```

---

## Frontend Implementation

### Sending Events (TypeScript)

```typescript
// In revit-bridge.service.ts
@Injectable({ providedIn: 'root' })
export class RevitBridgeService {
  private isRevit = typeof window !== 'undefined' && 'chrome' in window && 'webview' in (window as any).chrome;

  send<T>(type: string, payload: T): void {
    if (!this.isRevit) {
      console.log('[RevitBridge] Not in Revit, skipping:', type);
      return;
    }

    const event: WebViewEvent<T> = {
      type,
      payload,
      timestamp: Date.now()
    };

    (window as any).chrome.webview.postMessage(event);
  }
}
```

### Receiving Events (TypeScript)

```typescript
// In revit-bridge.service.ts
on<T>(eventType: string): Observable<T> {
  return fromEvent<CustomEvent>(window, 'revit-message').pipe(
    filter(e => e.detail?.type === eventType),
    map(e => e.detail.payload as T)
  );
}
```

---

## Testing

### Mock Mode (Browser without Revit)

When running in browser (not in Revit), Frontend should:
1. Detect `!window.chrome?.webview`
2. Use mock data from `assets/mock/webview-events.json`
3. Log all events to console

### Integration Test Checklist

- [ ] Plugin sends `revit:ready` on load
- [ ] Frontend sends `ui:ready` on bootstrap
- [ ] `ui:scan-families` → `revit:families:list` roundtrip works
- [ ] `ui:stamp` → `revit:stamp:result` roundtrip works
- [ ] `ui:publish` → `revit:publish:result` roundtrip works
- [ ] `ui:load-family` → `revit:load:result` roundtrip works
- [ ] Error events handled gracefully
- [ ] All payloads match TypeScript interfaces

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-02-17 | Initial contract |
