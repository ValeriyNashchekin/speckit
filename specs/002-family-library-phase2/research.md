# Research: Family Library Phase 2

**Date**: 2026-02-18
**Branch**: 002-family-library-phase2

This document consolidates research findings for Phase 2 implementation decisions.

---

## R8: Snapshot JSON Schema for Change Detection

**Question**: What fields must SnapshotJSON contain for accurate diff detection?

### Decision
SnapshotJSON must contain standardized fields for 6 change categories.

### Rationale
- Diff detection requires consistent data structure across versions
- JSON stored in FamilyVersion.SnapshotJSON column (already exists from MVP)
- Must capture all mutable aspects of family

### Snapshot Schema
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

### Change Category Detection

| Category | Icon | Detection Method |
|----------|------|------------------|
| Name | ✏️ | Compare `familyName` field |
| Category | 📁 | Compare `category` field |
| Types | ➕➖ | Compare `types[]` array (set difference) |
| Parameters | 📝 | Compare `parameters[]` by name + value |
| Geometry | 🔧 | `hasGeometryChanges` flag (Hash-based) |
| TXT | 📄 | Compare `txtHash` field |

### Implementation Notes
- Snapshot created at Publish time
- Previous snapshot retrieved from last FamilyVersion
- Diff computed on-demand (not stored)

### Alternatives Considered
| Alternative | Rejected Because |
|-------------|------------------|
| Store diff in DB | Computed on-demand is sufficient, saves space |
| Omit category | Category changes are rare but visible to users |
| Hash parameters only | Need detailed diff for UI display |

---

## R9: Batch Check API Performance

**Question**: How to efficiently check 1000+ families without N+1 queries?

### Decision
Single batch endpoint with optimized SQL query using IN clause.

### Rationale
- Scanner may check 1000-5000 families per scan
- N+1 queries would be prohibitively slow
- SQL Server handles IN clause efficiently for 1000s of items

### API Contract
```http
POST /api/families/batch-check
Content-Type: application/json

Request:
{
  "families": [
    {"roleName": "FreeAxez_Table", "hash": "abc123..."},
    {"roleName": "FreeAxez_Chair", "hash": "def456..."}
  ]
}

Response:
{
  "results": [
    {
      "roleName": "FreeAxez_Table",
      "status": "UpToDate",
      "libraryVersion": 2,
      "libraryHash": "abc123..."
    },
    {
      "roleName": "FreeAxez_Chair",
      "status": "UpdateAvailable",
      "libraryVersion": 3,
      "currentVersion": 1,
      "libraryHash": "xyz789..."
    },
    {
      "roleName": "FreeAxez_Lamp",
      "status": "NotFound"
    }
  ]
}
```

### SQL Implementation
```sql
-- Single query with JOIN
SELECT
    r.Name as RoleName,
    f.Id as FamilyId,
    f.CurrentVersion,
    fv.Hash as LibraryHash
FROM FamilyRoles r
LEFT JOIN Families f ON r.Id = f.RoleId
LEFT JOIN FamilyVersions fv ON f.Id = fv.FamilyId
    AND f.CurrentVersion = fv.Version
WHERE r.Name IN (@roleNames)
```

### Performance Target
- 1000 families check: < 500ms
- 5000 families check: < 2 seconds

### Alternatives Considered
| Alternative | Rejected Because |
|-------------|------------------|
| Parallel individual requests | Connection overhead, slower |
| Stored procedure | Unnecessary complexity for simple query |
| Cache all families | Stale data, memory overhead |

---

## R10: Legacy Recognition for Scanner

**Question**: How to match unstamped families to roles efficiently?

### Decision
Client-side evaluation with cached recognition rules.

### Rationale
- Recognition rules are relatively few (one per role)
- Rules don't change frequently
- Client-side evaluation avoids network latency per family
- Plugin caches rules for session duration

### Algorithm
```
1. On scanner open: Fetch all recognition rules from /api/recognition-rules
2. Cache rules in memory for session
3. For each unstamped family:
   a. Evaluate each rule against FamilyName
   b. First matching rule → Legacy Match status
   c. No rule matches → Unmatched status
4. If multiple rules match → Conflict warning, use first
```

### C# Implementation
```csharp
public class LegacyRecognitionService
{
    private List<RecognitionRule>? _cachedRules;

    public async Task InitializeAsync()
    {
        _cachedRules = await _api.GetRecognitionRulesAsync();
    }

    public string? MatchRole(string familyName)
    {
        foreach (var rule in _cachedRules!)
        {
            if (EvaluateRule(rule.RootNode, familyName))
                return rule.RoleName;
        }
        return null;
    }

    private bool EvaluateRule(RecognitionNode node, string name)
    {
        return node.Type == "group"
            ? EvaluateGroup(node, name)
            : EvaluateCondition(node, name);
    }
}
```

### Performance
- Rule fetch: One-time at scanner open
- Per-family evaluation: < 1ms (in-memory)
- Total 1000 families: < 1 second

### Alternatives Considered
| Alternative | Rejected Because |
|-------------|------------------|
| Server-side matching | Network latency per family |
| Regex compilation | Overkill for Contains/NotContains |
| Pre-computed mappings | Doesn't handle new families |

---

## R11: MEP RoutingPreferences Serialization

**Question**: How to serialize PipeType/DuctType RoutingPreferences to JSON?

### Decision
Custom serializer extracting segments and fittings from RoutingPreferenceManager.

### Rationale
- RoutingPreferenceManager is complex Revit API object
- Need serializable representation for storage and comparison
- Focus on segments (materials) and fittings (elbows, tees, etc.)

### JSON Structure
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

### Revit API Implementation
```csharp
public class RoutingPreferencesSerializer
{
    public RoutingPreferencesJson Serialize(PipeType pipeType)
    {
        var rpm = pipeType.RoutingPreferenceManager;
        var result = new RoutingPreferencesJson();

        // Segments (materials)
        for (int i = 0; i < rpm.GetNumberOfRules(RoutingPreferenceRuleGroupType.Segments); i++)
        {
            var rule = rpm.GetRule(RoutingPreferenceRuleGroupType.Segments, i);
            var segment = rule.MEPPartId;
            // Extract material info...
            result.Segments.Add(new SegmentJson { ... });
        }

        // Fittings (elbows, tees, etc.)
        foreach (var groupType in new[] {
            RoutingPreferenceRuleGroupType.Elbows,
            RoutingPreferenceRuleGroupType.Tees,
            RoutingPreferenceRuleGroupType.Transitions,
            RoutingPreferenceRuleGroupType.Junctions
        })
        {
            for (int i = 0; i < rpm.GetNumberOfRules(groupType); i++)
            {
                var rule = rpm.GetRule(groupType, i);
                // Extract fitting info...
                result.Fittings.Add(new FittingJson { ... });
            }
        }

        return result;
    }
}
```

### Pull Update Logic
```csharp
public void ApplyRoutingPreferences(PipeType pipeType, RoutingPreferencesJson json)
{
    var rpm = pipeType.RoutingPreferenceManager;

    // Clear existing rules
    rpm.Clear(RoutingPreferenceRuleGroupType.Segments);
    // ... clear other groups

    // Add segments from JSON
    foreach (var segment in json.Segments)
    {
        var material = FindMaterial(segment.MaterialName);
        // Create and add rule...
    }

    // Add fittings from JSON (by name lookup)
    foreach (var fitting in json.Fittings)
    {
        var familySymbol = FindFitting(fitting.FamilyName, fitting.TypeName);
        // Create and add rule...
    }
}
```

### Alternatives Considered
| Alternative | Rejected Because |
|-------------|------------------|
| Full object serialization | Too much internal Revit data |
| Store rules only | Need material names for display |
| Skip RoutingPreferences | Critical for MEP workflows |

---

## R12: Change Diff Algorithm

**Question**: How to compute diff between two SnapshotJSON instances?

### Decision
Field-by-field comparison with category-specific logic.

### Rationale
- Simple algorithm, easy to understand and debug
- Each category has specific comparison logic
- Output format matches UI requirements

### ChangeSet Structure
```csharp
public class ChangeSet
{
    public List<ChangeItem> Items { get; } = new();

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

public class ParameterChange
{
    public string Name { get; init; }
    public string? PreviousValue { get; init; }
    public string? CurrentValue { get; init; }
}
```

### Implementation
```csharp
public class ChangeDetectionService
{
    public ChangeSet DetectChanges(FamilySnapshot previous, FamilySnapshot current)
    {
        var changes = new ChangeSet();

        // Name change
        if (previous.FamilyName != current.FamilyName)
        {
            changes.Items.Add(new ChangeItem
            {
                Category = ChangeCategory.Name,
                PreviousValue = previous.FamilyName,
                CurrentValue = current.FamilyName
            });
        }

        // Category change
        if (previous.Category != current.Category)
        {
            changes.Items.Add(new ChangeItem
            {
                Category = ChangeCategory.Category,
                PreviousValue = previous.Category,
                CurrentValue = current.Category
            });
        }

        // Types change
        var addedTypes = current.Types.Except(previous.Types).ToList();
        var removedTypes = previous.Types.Except(current.Types).ToList();
        if (addedTypes.Any() || removedTypes.Any())
        {
            changes.Items.Add(new ChangeItem
            {
                Category = ChangeCategory.Types,
                AddedItems = addedTypes,
                RemovedItems = removedTypes
            });
        }

        // Parameters change
        var paramChanges = CompareParameters(previous.Parameters, current.Parameters);
        if (paramChanges.Any())
        {
            changes.Items.Add(new ChangeItem
            {
                Category = ChangeCategory.Parameters,
                ParameterChanges = paramChanges
            });
        }

        // Geometry change (hash-based flag)
        if (current.HasGeometryChanges)
        {
            changes.Items.Add(new ChangeItem
            {
                Category = ChangeCategory.Geometry
            });
        }

        // TXT change
        if (previous.TxtHash != current.TxtHash)
        {
            changes.Items.Add(new ChangeItem
            {
                Category = ChangeCategory.Txt,
                PreviousValue = previous.TxtHash,
                CurrentValue = current.TxtHash
            });
        }

        return changes;
    }

    private List<ParameterChange> CompareParameters(
        List<ParameterSnapshot> previous,
        List<ParameterSnapshot> current)
    {
        var changes = new List<ParameterChange>();
        var prevDict = previous.ToDictionary(p => p.Name);
        var currDict = current.ToDictionary(p => p.Name);

        foreach (var (name, curr) in currDict)
        {
            if (prevDict.TryGetValue(name, out var prev))
            {
                if (prev.Value != curr.Value)
                {
                    changes.Add(new ParameterChange
                    {
                        Name = name,
                        PreviousValue = prev.Value,
                        CurrentValue = curr.Value
                    });
                }
            }
            else
            {
                changes.Add(new ParameterChange
                {
                    Name = name,
                    PreviousValue = null,
                    CurrentValue = curr.Value
                });
            }
        }

        // Removed parameters
        foreach (var name in prevDict.Keys.Except(currDict.Keys))
        {
            changes.Add(new ParameterChange
            {
                Name = name,
                PreviousValue = prevDict[name].Value,
                CurrentValue = null
            });
        }

        return changes;
    }
}
```

### Alternatives Considered
| Alternative | Rejected Because |
|-------------|------------------|
| JSON diff library | Generic output, harder to map to UI |
| Hash-only change detection | No details for UI display |
| Store diff in DB | Computed on-demand is sufficient |

---

## Summary: Phase 2 Technical Decisions

| # | Topic | Decision |
|---|-------|----------|
| R8 | Snapshot Schema | 6-category JSON structure for diff detection |
| R9 | Batch Check | Single endpoint with SQL IN clause |
| R10 | Legacy Recognition | Client-side cached rules evaluation |
| R11 | MEP Serialization | Custom RoutingPreferences serializer |
| R12 | Diff Algorithm | Field-by-field comparison with ChangeSet |

All research items resolved — ready for Phase 1 design.
