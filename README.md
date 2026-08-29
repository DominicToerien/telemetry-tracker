# telemetry-tracker

`telemetry-tracker` is a `net8.0` application for Le Mans Ultimate telemetry collection, terminal-based analysis, and AI-assisted coaching.

The primary MVP direction is a local-first Codex/Claude-style CLI/TUI:
- browse sessions as directory-like contexts and laps as file-like items
- view or follow live telemetry, including from a separate terminal
- compare laps and inspect recorded setup context
- use skills as the initial AI layer for coaching and guided setup proposals

Later hosted phases retain the planned runtime modes from one codebase; they are not the immediate MVP:
- `Collector` mode (user machine): reads LMU shared memory locally and sends telemetry/lap payloads to a hosted API.
- `Server` mode (hosted): accepts telemetry ingestion, persists lap summaries/traces, and serves query/analysis endpoints.

The current implementation is intentionally narrow:
- It reads LMU shared memory on Windows.
- It keeps the latest copied scoring/telemetry snapshot in memory.
- It exposes connection/debug status endpoints for bring-up and validation.
- It renders a live single-line console telemetry status display for local verification while driving.
- It exposes that telemetry status through a dedicated `Features/TelemetryStatus` vertical slice.
- It does not yet include session/lap capture, the interactive TUI, skill-facing CLI operations, hosted ingestion, analytics, or a frontend.

## Project Status

This repository is in the LMU integration foundation phase.

The durable project plan lives in [agent/plan.md](./agent/plan.md). Keep that file current as the project evolves so future work always has a reliable reference point.

The persistent AI working context lives under [agent/](./agent/README.md), product-facing specifications live under [agent/specs/](./agent/specs/README.md), and project-specific task skills live under [agent/skills/](./agent/skills/README.md).

## Requirements

- .NET 8 SDK
- Windows for live LMU shared-memory access
- Le Mans Ultimate running if you want live connected telemetry

Notes:
- The API can still run on non-Windows platforms, but LMU shared memory will be reported as unsupported/disconnected.
- The project currently defaults to a Linux Docker target for container tooling, but live LMU telemetry is a Windows-only runtime feature.
- End users must run the collector locally on the same machine that can access LMU shared memory.

## Quick Start

1. Restore and build:

```powershell
dotnet build telemetry-tracker.csproj
```

2. Run the API:

```powershell
dotnet run --project telemetry-tracker.csproj
```

3. Open Swagger in development:

- `https://localhost:<port>/swagger`

4. Check telemetry status:

- `GET /telemetry/status`
- `GET /telemetry/debug`

Example:

```powershell
Invoke-RestMethod http://127.0.0.1:5099/telemetry/status
```

If LMU is not running, the service should still start and return a disconnected status instead of crashing.

## Configuration

### Supabase connection string

For local server-side persistence configuration, use a root `.env` file with this exact variable name:

```dotenv
ConnectionStrings__Supabase=Host=db.<project-ref>.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=<password>;SSL Mode=Require;Trust Server Certificate=true
```

Notes:
- `ConnectionStrings__Supabase` is the canonical variable name for this project.
- The double underscore maps to ASP.NET configuration key `ConnectionStrings:Supabase`.
- This is what `configuration.GetConnectionString("Supabase")` reads.
- Do not use `SUPABASE_CONNECTION_STRING` for this project anymore.
- Keep this value in `.env` or hosted environment configuration, never committed into `appsettings.json`.
- The local collector role should remain runnable without this value; hosted server environments are where persistence secrets belong.

If `ConnectionStrings__Supabase` is missing, the app still starts and logs that DbContext registration was skipped.

If the DbContext is registered, the app also attempts to run EF Core migrations automatically on startup with `dbContext.Database.Migrate()`.
If migration fails, the failure is logged and the application continues starting so LMU telemetry verification is not blocked by persistence issues.

LMU reader settings live in `appsettings.json` under `LmuTelemetry`:

```json
"LmuTelemetry": {
  "Enabled": true,
  "RetryIntervalSeconds": 5,
  "DebugLogging": false,
  "AutoEnablePluginOnStartup": true,
  "GameInstallPath": "",
  "CustomPluginVariablesPath": "",
  "PluginDllNames": [
    "rFactor2SharedMemoryMapPlugin64.dll",
    "rF2SharedMemoryMapPlugin64.dll",
    "rF2SharedMemeryMapPlugin.dll"
  ]
}
```

Fields:
- `Enabled`: turns the LMU background reader on or off
- `RetryIntervalSeconds`: retry delay when LMU shared memory is unavailable
- `DebugLogging`: enables extra reader logging
- `AutoEnablePluginOnStartup`: when true, attempts to set shared-memory plugin `Enabled` to `1` in `CustomPluginVariables.JSON` if it is disabled or missing
- `GameInstallPath`: optional LMU install root override for startup prerequisite checks
- `CustomPluginVariablesPath`: optional explicit path override for `CustomPluginVariables.JSON`
- `PluginDllNames`: plugin DLL names accepted by the startup prerequisite checker

On startup, the LMU background service now validates:
- at least one expected shared-memory plugin DLL exists under `Plugins` or `Bin64/Plugins`
- `CustomPluginVariables.JSON` exists at the configured or detected LMU path
- shared-memory plugin entries are enabled
- `UnsubscribedBuffersMask` is normalized to `0`
- `EnableDirectMemoryAccess` is normalized to `1`

These startup checks can also normalize the local plugin JSON when `AutoEnablePluginOnStartup` is enabled. Failures are logged as warnings and do not crash the API.

## API Endpoints

### `GET /telemetry/status`

Returns the current LMU provider state, including:
- whether the provider is enabled
- whether the current platform supports LMU shared memory
- whether the service is currently connected
- timestamps for the last successful scoring/telemetry reads
- the last known shared-memory event
- the current disconnected/warning message, if any

### `GET /telemetry/debug`

Returns a lightweight debug view of the latest copied snapshot metadata, such as:
- current track name
- player name
- session id
- active vehicle count
- player vehicle availability

This endpoint is for bring-up/debugging and should not be treated as a stable public telemetry contract yet.

## Later Hosted Deployment Direction

Deferred deployment model after the local terminal and skills workflow is useful:
- user runs local collector build on Windows
- collector reads LMU shared memory and posts tracked lap payloads to hosted API
- hosted API owns Supabase credentials and persistence

Security boundary:
- do not ship real DB connection strings to end users
- collector should not require direct database credentials
- server-only secrets stay in hosted environment configuration

Configuration expectation:
- app should run in a degraded mode when persistence configuration is missing
- telemetry status and local verification should still function without DB access

## Architecture Overview

Main flow:

1. `LmuTelemetryBackgroundService` starts with the API.
2. On Windows, it attempts to open the LMU event, file mapping, and shared lock objects.
3. When LMU signals an update, the service copies the shared-memory layout into managed structs.
4. `LmuTelemetryProvider` stores the latest thread-safe in-memory status/snapshot.
5. The background service renders a live single-row console status line for local verification.
6. The `Features/TelemetryStatus` slice exposes thin query endpoints over that state.

Important files:
- [agent/README.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/README.md)
- [agent/entry-point.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/entry-point.md)
- [agent/specs/README.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/specs/README.md)
- [agent/specs/product.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/specs/product.md)
- [agent/specs/plan.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/specs/plan.md)
- [agent/skills/README.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/skills/README.md)
- [agent/plan.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/plan.md)
- [Features/TelemetryStatus/Endpoint.cs](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/Features/TelemetryStatus/Endpoint.cs)
- [Features/TelemetryStatus/GetTelemetryStatusHandler.cs](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/Features/TelemetryStatus/GetTelemetryStatusHandler.cs)
- [Features/TelemetryStatus/GetTelemetryDebugHandler.cs](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/Features/TelemetryStatus/GetTelemetryDebugHandler.cs)
- [Program.cs](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/Program.cs)
- [Telemetry/Lmu/LmuTelemetryBackgroundService.cs](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/Telemetry/Lmu/LmuTelemetryBackgroundService.cs)
- [Telemetry/Lmu/LmuTelemetryProvider.cs](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/Telemetry/Lmu/LmuTelemetryProvider.cs)
- [Telemetry/Lmu/Interop/LmuInteropModels.cs](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/Telemetry/Lmu/Interop/LmuInteropModels.cs)

## LMU SDK Rules

The LMU headers under `references/lmu-sdks/` are the single source of truth for integration details.

Do not:
- invent telemetry fields
- rename SDK fields in the interop layer
- change memory layout assumptions casually
- assume pointer-bearing scoring fields can be used directly after mapping

Always:
- verify field names and types against the headers
- preserve 4-byte packing
- treat the shared-memory read/copy flow in the SDK as authoritative
- keep Windows interop isolated to the LMU integration boundary

Reference headers:
- `references/lmu-sdks/SharedMemoryInterface.hpp`
- `references/lmu-sdks/InternalsPlugin.hpp`
- `references/lmu-sdks/PluginObjects.hpp`

Setup-format context:
- `references/lmu-setups/README.md`
- `references/lmu-setups/992s-pc-moddev-example.svm`

The `.svm` file is a non-authoritative ModDev pace-car example. Use it to understand and preserve the file shape, not as proof that settings, ranges, or units apply to other LMU cars.

## Failure Handling Expectations

If LMU shared memory is unavailable:
- the API must still start
- the process must not crash
- the service must log a clear warning
- `/telemetry/status` must report disconnected state

If the app is running on a non-Windows OS:
- the API must still start
- LMU telemetry must report unsupported/disconnected

## Tests

Build the app:

```powershell
dotnet build telemetry-tracker.csproj
```

Build tests:

```powershell
dotnet build telemetry-tracker.Tests/telemetry-tracker.Tests.csproj
```

Run tests:

```powershell
dotnet test telemetry-tracker.Tests/telemetry-tracker.Tests.csproj --no-build
```

Current tests cover:
- key interop layout sizes
- disconnected default provider behavior
- scoring-only update handling
- telemetry-only update handling
- status endpoint response shape

## Local .env example

See [.env.example](./.env.example) for the expected local configuration shape.

## Parallel Agent Work

If you want multiple Codex chats or agents working in parallel on different branches, use git worktrees instead of repeatedly switching one checkout.

The repo now includes helper scripts for that workflow:

- `scripts/New-AgentWorkspace.ps1`
- `scripts/Invoke-AgentWorkspace.ps1`
- `scripts/Remove-AgentWorkspace.ps1`

These scripts create a separate worktree per task, assign workspace-specific localhost ports, and optionally copy your local `.env` into the new worktree.

Start here:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\New-AgentWorkspace.ps1 `
  -Name tracking `
  -BranchName Spike/tracking-control `
  -BaseBranch main
```

Then open that worktree in a separate chat/editor window and run:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Invoke-AgentWorkspace.ps1 -Command run
```

There is a fuller walkthrough in [docs/parallel-agent-worktrees.md](./docs/parallel-agent-worktrees.md).

## Planning Chat Pattern

It is also useful to keep one branch-agnostic planning chat that acts like a product or project manager for the repository.

Recommended split:

- planning chat
  - stays branch-agnostic by default
  - works mainly with Notion, Linear, and GitHub
  - defines scope, sequencing, and issue breakdown
- implementation chats
  - each use a dedicated worktree
  - each map to a dedicated branch
- each own a single issue or bounded task

If planning work turns into implementation, the planning chat should prepare the issue, branch, worktree, and handoff prompt for a separate implementation chat.

When implementation pauses or moves to another computer, use [the session handoff skill](./agent/skills/session-handoff/SKILL.md). Push a signed commit, keep a draft pull request open, and update its `## HANDOFF` section so the receiving session can verify and continue the work without depending on a local transcript.

Branching rule for implementation chats:

- start every new issue branch from `main`
- create the worktree from `main`
- merge `main` into longer-running branches as needed
- merge completed work back to `main`
## Current LMU Bring-Up State

- Real LMU shared memory connection is working on Windows.
- The project can now display live player-car values such as lap, speed, throttle, brake, steering, gear, RPM, fuel, and brake pressure in the console.
- LMU plugin configuration is normalized on startup to reduce local setup drift.
- Remaining work is to build tracking, persistence, and analysis on top of this working live telemetry foundation.

## Developer Notes

- Keep `agent/plan.md` updated when scope or architecture changes.
- Keep `agent/` and `agent/specs/` aligned when the project direction changes.
- New user-facing behaviour should prefer `Features/{FeatureName}` slices with explicit handlers and thin CLI/TUI adapters.
- Stable JSON CLI operations are the initial integration boundary for skills; later MCP and HTTP adapters should reuse the same handlers.
- Prefer adding new behavior behind the existing provider/service boundary instead of pushing Win32 concerns into controllers.
- Avoid exposing raw LMU structs directly as a long-term public API unless that choice is intentional and documented.
- If you touch interop models, update tests first or alongside the change.

## Next Likely Steps

- Add explicit tracking state, lap buffering, lap-boundary detection, summaries, and traces over the working LMU feed.
- Add first-class local sessions and setup associations.
- Add stable JSON CLI queries for sessions, laps, comparisons, telemetry, and setups.
- Build the interactive terminal workspace and initial skills on those deterministic operations.
