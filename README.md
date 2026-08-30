# telemetry-tracker

`telemetry-tracker` is a `net8.0` application for Le Mans Ultimate telemetry collection, terminal-based analysis, and AI-assisted coaching.

The primary MVP direction is a local-first Codex/Claude-style CLI/TUI:
- browse sessions as directory-like contexts and laps as file-like items
- view or follow live telemetry, including from a separate terminal
- compare laps and inspect recorded setup context
- use skills as the initial AI layer for coaching and guided setup proposals

The current project is only the standalone native client. A later hosted phase will add a separate ASP.NET Core server project that this client can call over HTTPS.

The current implementation is intentionally narrow:
- It reads LMU shared memory on Windows.
- It keeps the latest copied scoring/telemetry snapshot in memory.
- It renders a live single-line console telemetry status display for local verification while driving.
- It keeps telemetry status/debug queries as application handlers for future CLI commands.
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
- The console app can start on non-Windows platforms, but LMU shared memory will be reported as unsupported/disconnected.
- LMU live integration is officially supported only on Windows 10/11. Windows is the authoritative runtime for shared memory, setup files, plugins, hardware, and integration testing.
- Linux remains suitable for development, builds, unit tests, documentation, and recorded/mock telemetry testing. Proton support is unofficial and must not shape the MVP architecture.
- The human-facing MVP is installed and run natively on the same Windows machine as LMU; it is not a Docker workload.
- This project has no Docker or ASP.NET server runtime. Those concerns belong to a future server project.
- The complete deployment and transport boundary is documented in [agent/specs/architecture.md](./agent/specs/architecture.md).

## Quick Start

1. Restore and build:

```powershell
dotnet build telemetry-tracker.csproj
```

2. Run the native console app:

```powershell
dotnet run --project telemetry-tracker.csproj
```

The app continuously shows live connection/telemetry state. Press `Ctrl+C` to exit. If LMU is not running, the process remains stable and reports a disconnected status.

## Configuration

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

These startup checks can also normalize the local plugin JSON when `AutoEnablePluginOnStartup` is enabled. Failures are logged as warnings and do not crash the app.

## Later Hosted Deployment Direction

Deferred deployment model after the local terminal and skills workflow is useful:
- user runs local collector build on Windows
- native app reads LMU shared memory and optionally posts completed session/lap artifacts to the hosted API over authenticated HTTPS
- hosted API owns Supabase credentials and persistence
- web frontend consumes HTTP/WebSocket APIs; agents may use MCP over mature capabilities

MCP is not the synchronization protocol or the frontend transport. The local application remains fully usable when the hosted platform or network is unavailable.

Security boundary:
- do not ship real DB connection strings to end users
- collector should not require direct database credentials
- server-only secrets stay in hosted environment configuration

The native client will use `HttpClient` when synchronization is implemented. It does not host HTTP endpoints or contain hosted database credentials/migrations.

## Architecture Overview

Main flow:

1. The .NET Generic Host starts `LmuTelemetryBackgroundService`.
2. On Windows, it attempts to open the LMU event, file mapping, and shared lock objects.
3. When LMU signals an update, the service copies the shared-memory layout into managed structs.
4. `LmuTelemetryProvider` stores the latest thread-safe in-memory status/snapshot.
5. The background service renders a live single-row console status line for local verification.
6. The `Features/TelemetryStatus` slice provides reusable queries for future CLI/TUI adapters.

Important files:
- [agent/README.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/README.md)
- [agent/entry-point.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/entry-point.md)
- [agent/specs/README.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/specs/README.md)
- [agent/specs/product.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/specs/product.md)
- [agent/specs/plan.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/specs/plan.md)
- [agent/skills/README.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/skills/README.md)
- [agent/plan.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/plan.md)
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
- the console app must still start
- the process must not crash
- the app must display or log a clear disconnected state

If the app is running on a non-Windows OS:
- the console app must still start
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

## Parallel Agent Work

If you want multiple Codex chats or agents working in parallel on different branches, use git worktrees instead of repeatedly switching one checkout.

The repo now includes helper scripts for that workflow:

- `scripts/New-AgentWorkspace.ps1`
- `scripts/Invoke-AgentWorkspace.ps1`
- `scripts/Remove-AgentWorkspace.ps1`

These scripts create a separate worktree per task and optionally copy a local `.env` if one is introduced for a future task.

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
- Prefer adding new behavior behind the existing provider/service boundary instead of pushing Win32 concerns into CLI/TUI adapters.
- Avoid exposing raw LMU structs directly as a long-term public API unless that choice is intentional and documented.
- If you touch interop models, update tests first or alongside the change.

## Next Likely Steps

- Add explicit tracking state, lap buffering, lap-boundary detection, summaries, and traces over the working LMU feed.
- Add first-class local sessions and setup associations.
- Add stable JSON CLI queries for sessions, laps, comparisons, telemetry, and setups.
- Build the interactive terminal workspace and initial skills on those deterministic operations.
