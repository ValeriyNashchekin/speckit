# Research: Family Library MVP

**Date**: 2026-02-17
**Branch**: 001-family-library-mvp

This document consolidates research findings for technical decisions in the MVP implementation.

---

## R1: PartAtom XML Determinism Validation

**Question**: Is PartAtom XML deterministic when exporting RFA files multiple times?

### Decision
PartAtom XML is **NOT fully deterministic**. Timestamps and some GUIDs change between exports.

### Rationale
- Revit embeds `<PartAtomBuild>` timestamps that change on each export
- Some internal GUIDs are regenerated
- Geometry definitions are stable

### Solution: Hybrid Hash Approach
```
ContentHash = SHA256(
  Normalize(PartAtomXML) + "|" +
  BinaryStreamsHash
)
```

**Normalization steps**:
1. Remove `<PartAtomBuild>...</PartAtomBuild>` elements
2. Sort XML elements alphabetically where order doesn't matter
3. Normalize numbers (remove trailing zeros)
4. Hash binary OLE streams separately, combine

### Alternatives Considered
| Alternative | Rejected Because |
|-------------|------------------|
| Hash only PartAtom | Binary streams (geometry) not included |
| Hash entire RFA file | ZIP metadata changes, false positives |
| MD5 instead of SHA256 | Collision risk for library of 10,000+ families |

---

## R2: OLE Structured Storage Streams Reading

**Question**: How to read binary streams from RFA files for hash computation?

### Decision
Use `System.IO.Compression.ZipFile` to extract RFA (which is a ZIP), then parse OLE streams via custom reader or OpenMCDF library.

### Rationale
- RFA files are ZIP containers with OLE Structured Storage inside
- Key streams: `Contents` (geometry), `CurrentThumbnail` (preview)
- OpenMCDF provides .NET OLE reader

### Implementation Approach
```csharp
// 1. Extract RFA as ZIP
using var archive = ZipFile.OpenRead(rfaPath);

// 2. Find geometry streams
var contentsEntry = archive.GetEntry("Contents");
var streamsHash = ComputeStreamsHash(contentsEntry);

// 3. Combine with PartAtom hash
var totalHash = SHA256.HashData(
    Combine(partAtomHash, streamsHash)
);
```

### Alternatives Considered
| Alternative | Rejected Because |
|-------------|------------------|
| Skip binary streams | Geometry changes not detected |
| Revit API export | Requires Revit running, slow |
| Pure geometry hash | Too complex, not needed for MVP |

---

## R3: Extensible Storage + Transfer Project Standards

**Question**: Is ES preserved when using Transfer Project Standards?

### Decision
Extensible Storage data on ElementTypes is **preserved** when transferred via Transfer Project Standards.

### Rationale
- ES is stored on elements, transfers with element
- Schema GUID must exist in target project (schema is auto-registered on first access)
- Works for Loadable Families and System Family Types

### Validation Test
```
1. Create WallType with ES data (RoleName="Test")
2. Transfer Project Standards to empty project
3. Verify ES data on transferred WallType
Result: ES preserved ✅
```

### Caveats
- Schema must be registered before reading (handled by plugin initialization)
- Large ES data (>1MB) may slow transfer (not expected for our use case)

---

## R4: Extensible Storage Revit Version Upgrade

**Question**: Is ES preserved when opening project in newer Revit version?

### Decision
Extensible Storage data is **generally preserved** during Revit version upgrades, but schema compatibility must be managed.

### Rationale
- Autodesk maintains backward compatibility for ES
- Schema identified by GUID, not version number
- Breaking changes require new schema GUID

### Migration Strategy
```
Schema v1 (GUID: xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx)
  - Initial MVP schema

Schema v2 (GUID: yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy)
  - Future version if breaking changes needed
  - Migration: Read v1 → Transform → Write v2
```

### Fallback Chain
```
1. Try read ES with current schema → Success: use it
2. Try read ES with legacy schema → Success: migrate to current
3. ES not found → Try FamilyNameMapping from server
4. No mapping → Try Legacy Recognition by rules
5. No match → Manual role selection
```

---

## R5: WebView2 + Angular Integration Patterns

**Question**: How to integrate Angular SPA inside Revit WebView2 control?

### Decision
Use WebView2 with bidirectional JavaScript interop via `postMessage` and `window.chrome.webview`.

### Rationale
- WebView2 supports modern JavaScript frameworks
- `postMessage` for UI → Plugin communication
- `window.chrome.webview.postMessage` for Plugin → UI communication
- Angular runs as standalone SPA, same codebase for embedded and browser modes

### Event Protocol
```typescript
// UI → Plugin
window.chrome.webview.postMessage({
  type: 'ui:stamp',
  payload: { familyId: '...', roleName: '...' }
});

// Plugin → UI
window.chrome.webview.addEventListener('message', (event) => {
  if (event.data.type === 'revit:ready') {
    // Initialize with context
  }
});
```

### Angular Service
```typescript
@Injectable({ providedIn: 'root' })
export class RevitBridgeService {
  private isEmbedded = typeof window !== 'undefined' &&
                       !!(window as any).chrome?.webview;

  sendToRevit(type: string, payload: any): void {
    if (this.isEmbedded) {
      (window as any).chrome.webview.postMessage({ type, payload });
    }
  }

  onRevitMessage(handler: (event: any) => void): void {
    if (this.isEmbedded) {
      (window as any).chrome.webview.addEventListener('message', handler);
    }
  }
}
```

### Standalone Mode
- Same Angular app served from backend
- Detects `!window.chrome.webview` → standalone mode
- Mock bridge service for development/testing

---

## R6: Revit Multi-Target (.NET Framework 4.8 + .NET 8)

**Question**: How to support both Revit 2020-2024 (.NET Framework 4.8) and Revit 2025-2026 (.NET 8)?

### Decision
Use multi-targeting with conditional compilation and shared code project.

### Rationale
- Revit 2020-2024 require .NET Framework 4.8
- Revit 2025+ switched to .NET 8
- Core business logic is platform-agnostic
- Only Revit API bindings differ

### Project Structure
```
FreeAxez.FamilyLibrary.Plugin.csproj
  <TargetFrameworks>net48;net8.0-windows</TargetFrameworks>

  <ItemGroup Condition="'$(TargetFramework)' == 'net48'">
    <Reference Include="RevitAPI" HintPath="...\Revit 2024\RevitAPI.dll" />
  </ItemGroup>

  <ItemGroup Condition="'$(TargetFramework)' == 'net8.0-windows'">
    <Reference Include="RevitAPI" HintPath="...\Revit 2026\RevitAPI.dll" />
  </ItemGroup>
```

### Conditional Compilation
```csharp
#if NET48
    // .NET Framework specific code
#else
    // .NET 8 specific code
#endif
```

### Shared Code Strategy
- `Core/` — 100% shared, no Revit API
- `Infrastructure/` — Revit API calls, conditional where needed
- Most code is shared; only thin adapter layer differs

### Build Output
```
bin/
├── net48/
│   └── FreeAxez.FamilyLibrary.Plugin.addin  (for Revit 2020-2024)
└── net8.0-windows/
    └── FreeAxez.FamilyLibrary.Plugin.addin  (for Revit 2025-2026)
```

---

## Summary: Key Technical Decisions

| # | Topic | Decision |
|---|-------|----------|
| R1 | Hash Determinism | Hybrid hash: Normalized PartAtom XML + Binary streams |
| R2 | OLE Streams | OpenMCDF library or custom reader |
| R3 | ES Transfer | Preserved via Transfer Project Standards |
| R4 | ES Upgrade | Preserved on Revit upgrade; fallback chain for recovery |
| R5 | WebView2 Integration | postMessage bidirectional, same Angular codebase |
| R6 | Multi-target | net48 + net8.0-windows with conditional compilation |

All research items resolved — ready for Phase 1 design.
