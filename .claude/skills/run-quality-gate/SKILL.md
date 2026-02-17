---
name: run-quality-gate
description: Execute quality gate validation with configurable blocking behavior for Angular 19 + .NET 10 stack. Use when running type-check, build, tests, lint, or custom validation commands in orchestrators or workers to enforce quality standards.
allowed-tools: Bash, Read
---

# Run Quality Gate

Execute validation commands as quality gates with structured error reporting for Angular 19 + .NET 10 projects.

## When to Use

- Type-check, build, test, lint validation
- Orchestrator phase validation
- Worker self-validation
- EF Core migration validation

## Input

```json
{
  "gate": "type-check|build|tests|lint|migrations|custom",
  "stack": "angular|dotnet|auto",
  "blocking": true,
  "custom_command": "npm run custom-validate || dotnet custom-tool"
}
```

## Gate Commands

### Angular Gates

| Gate | Command |
|------|---------|
| type-check | `npm run type-check` or `ng build --aot` |
| build | `ng build --configuration production` |
| tests (unit) | `ng test --code-coverage --watch=false --browsers=ChromeHeadless` |
| tests (e2e) | `ng e2e --headed=false` |
| lint | `ng lint` |
| custom | `custom_command` value |

### .NET Gates

| Gate | Command |
|------|---------|
| type-check | `dotnet build --no-restore` |
| build | `dotnet publish -c Release --no-restore` |
| tests | `dotnet test --no-build --configuration Release` |
| lint | `dotnet format --verify-no-changes --folder` |
| migrations | `dotnet ef migrations add __Temp__ && dotnet ef migrations remove --force` |
| custom | `custom_command` value |

## Auto-Detection

When `stack="auto"`, the skill detects the project type:

**Angular indicators**:
- `angular.json` exists
- `package.json` contains `@angular/core`
- `tsconfig.json` exists

**.NET indicators**:
- `*.sln` file exists
- `*.csproj` file exists
- `project.json` exists

**Monorepo indicators**:
- `pnpm-workspace.yaml` exists
- `nx.json` exists
- `turbo.json` exists

## Process

1. **Detect stack** - Use stack parameter or auto-detect
2. **Map gate to command** - Load from gate-mappings.json
3. **Validate custom_command** - If gate="custom"
4. **Execute via Bash** - Timeout: 5 minutes, capture stdout/stderr
5. **Parse result** - Exit code 0 = passed, non-zero = failed
6. **Extract errors** - Lines with "error", "failed", TS####, CS#### codes
7. **Determine action**:
   - Passed → action="continue"
   - Failed + blocking → action="stop"
   - Failed + non-blocking → action="warn"

## Output

```json
{
  "gate": "type-check",
  "stack": "angular",
  "detected_stack": "angular",
  "passed": true,
  "blocking": true,
  "action": "continue",
  "errors": [],
  "exit_code": 0,
  "duration_ms": 2345,
  "command": "ng build --aot --configuration production",
  "timestamp": "2025-01-30T14:30:00Z"
}
```

## Examples

### Example 1: Angular Type Check Passes

**Input**:
```json
{
  "gate": "type-check",
  "stack": "angular",
  "blocking": true
}
```

**Output**:
```json
{
  "gate": "type-check",
  "stack": "angular",
  "detected_stack": "angular",
  "passed": true,
  "blocking": true,
  "action": "continue",
  "errors": [],
  "exit_code": 0,
  "duration_ms": 4521,
  "command": "ng build --aot --configuration production",
  "timestamp": "2025-01-30T14:30:00Z"
}
```

### Example 2: .NET Build Fails (Blocking)

**Input**:
```json
{
  "gate": "build",
  "stack": "dotnet",
  "blocking": true
}
```

**Output**:
```json
{
  "gate": "build",
  "stack": "dotnet",
  "detected_stack": "dotnet",
  "passed": false,
  "blocking": true,
  "action": "stop",
  "errors": [
    "Program.cs(10,25): error CS0246: The type or namespace name 'MissingType' could not be found",
    "CourseService.cs(42,15): error CS0029: Cannot implicitly convert type 'string' to 'int'"
  ],
  "exit_code": 1,
  "duration_ms": 12345,
  "command": "dotnet publish -c Release --no-restore",
  "timestamp": "2025-01-30T14:32:00Z"
}
```

### Example 3: Angular Lint Fails (Non-Blocking)

**Input**:
```json
{
  "gate": "lint",
  "stack": "angular",
  "blocking": false
}
```

**Output**:
```json
{
  "gate": "lint",
  "stack": "angular",
  "detected_stack": "angular",
  "passed": false,
  "blocking": false,
  "action": "warn",
  "errors": [
    "error - Missing semicolon (src/app/app.component.ts:15:10)",
    "warning - Unused variable 'tmp' (src/app/utils.ts:42:5)"
  ],
  "exit_code": 1,
  "duration_ms": 8765,
  "command": "ng lint",
  "timestamp": "2025-01-30T14:35:00Z"
}
```

### Example 4: Auto-Detect .NET Stack

**Input**:
```json
{
  "gate": "tests",
  "stack": "auto",
  "blocking": true
}
```

**Output**:
```json
{
  "gate": "tests",
  "stack": "auto",
  "detected_stack": "dotnet",
  "passed": true,
  "blocking": true,
  "action": "continue",
  "errors": [],
  "exit_code": 0,
  "duration_ms": 25678,
  "command": "dotnet test --no-build --configuration Release --verbosity normal",
  "timestamp": "2025-01-30T14:40:00Z"
}
```

### Example 5: EF Core Migration Validation

**Input**:
```json
{
  "gate": "migrations",
  "stack": "dotnet",
  "blocking": true
}
```

**Output**:
```json
{
  "gate": "migrations",
  "stack": "dotnet",
  "detected_stack": "dotnet",
  "passed": true,
  "blocking": true,
  "action": "continue",
  "errors": [],
  "exit_code": 0,
  "duration_ms": 12456,
  "command": "dotnet ef migrations add __Temp__ --startup-project ../Web && dotnet ef migrations remove --force --startup-project ../Web",
  "timestamp": "2025-01-30T14:45:00Z",
  "note": "Temp migration created and removed successfully - EF Core model is valid"
}
```

### Example 6: Custom Command

**Input**:
```json
{
  "gate": "custom",
  "stack": "angular",
  "blocking": true,
  "custom_command": "npm run check-deps"
}
```

**Output**:
```json
{
  "gate": "custom",
  "stack": "angular",
  "detected_stack": "angular",
  "passed": false,
  "blocking": true,
  "action": "stop",
  "errors": [
    "Found 2 outdated dependencies:",
    "- @angular/core 17.0.0 → 19.0.0",
    "- rxjs 7.8.0 → 7.8.1"
  ],
  "exit_code": 1,
  "duration_ms": 5432,
  "command": "npm run check-deps",
  "timestamp": "2025-01-30T14:50:00Z"
}
```

## Error Handling

- **Timeout (5 min)**: Return failed with timeout error
- **Missing custom_command**: Return error
- **Command not found**: Return failed with exit_code=127
- **Stack detection fails**: Default to first available indicator
- **Ambiguous stack (both Angular and .NET)**: Use both, run gates sequentially

## Notes

- Exit code 0 always = success regardless of output
- Blocking flag only affects action, not passed status
- Error extraction uses regex patterns from gate-mappings.json
- For monorepos, run gates for each detected stack
- EF Core migration gate validates model without applying to database
- Angular type-check falls back to `ng build --aot` if no type-check script exists
