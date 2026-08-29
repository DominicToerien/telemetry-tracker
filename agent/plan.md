# Telemetry Tracker Plan

## Summary

Build a local-first telemetry workspace for Le Mans Ultimate using C#, Vertical Slice Architecture, and CQRS. The primary MVP is a Codex/Claude-style terminal application. It continuously reads telemetry, captures sessions and completed laps, presents sessions as directory-like contexts and laps as file-like items, and exposes stable CLI operations that skills use for coaching, comparison, and setup creation.

The goal is a clean working vertical slice with strong foundations and deliberately limited scope.

Evolution model for this plan:
- MVP: local telemetry host, local persistence, CLI/TUI, and skills
- next: native bring-your-own-key terminal chat and MCP
- later: hosted API, synchronization, Supabase persistence, and graphical frontend
- all interfaces reuse the same application commands and queries

## Product Goal

Deliver a running terminal product where:
- telemetry is continuously visible in the console
- tracking is explicitly controlled by the user
- sessions and laps can be browsed and selected as workspace context
- each completed tracked lap saves locally:
  - one relational lap summary
  - one JSONB lap trace
- machine-readable CLI commands expose saved lap data
- skills provide the initial AI analysis and setup workflows
- live telemetry can be displayed in the main TUI or followed from another terminal

## Technology and Constraints

- Platform: .NET 8 local CLI/TUI with reusable application handlers
- Language: C#
- Architecture: Vertical Slice Architecture + CQRS
- Runtime: one local telemetry owner with one or more terminal clients
- Background processing: `HostedService` / `BackgroundService`
- MVP storage: local-first persistence; Supabase is deferred to hosted synchronization
- Interface style: thin CLI/TUI commands first; MCP and HTTP adapters later
- Logging: console logging
- Testing: unit tests where they add clear value

Do not introduce in the MVP:
- microservices
- message brokers
- complex infrastructure
- graphical frontend
- authentication/authorization unless explicitly requested

## Architecture Principles

### 1. Vertical Slice Architecture

- Organise code by feature, not by technical layer.
- Each feature should own its request, handler, result, validation, and data access where practical.
- Avoid large generic service layers.

### 2. CQRS

- Commands mutate state.
- Queries return data.
- Keep command/query handlers explicit, narrow, and easy to follow.

Future implementation direction for CQRS in this project:
- treat write-side persistence as the source of truth
- allow command handlers to persist write-side data and return without waiting for read-model materialization
- defer read-model creation or refresh to a background worker when projection work could slow an interactive command
- keep projection execution in-process at first using `HostedService` / `BackgroundService` friendly patterns
- evolve to more durable projection coordination only when real reliability or scale needs justify it
- treat Redis, if later added, as a read-side cache layered on top of projected read models rather than as the source of truth

### 3. Limited Scope

- Build the smallest complete version that works end-to-end.
- Prefer clarity over cleverness.
- Avoid unused abstractions.
- Do not optimise prematurely.

### 4. Testability

- Add unit tests around logic-heavy components.
- Prioritise tests for lap summary calculation, lap trace generation, tracking state transitions, prompt building, and telemetry-source behaviour.

## Core System Design

The application provides a local telemetry host and terminal clients that:

- runs continuously
- starts a telemetry background service at boot
- continuously reads telemetry for live verification
- logs compact telemetry status to the console
- only saves telemetry when tracking is explicitly active
- saves one summary and one detailed trace per completed lap
- expose commands for tracking, status, sessions, laps, comparisons, and setup revisions
- allow multiple terminals to observe the same telemetry owner

Later phases add collector/server roles, ingestion, synchronization, hosted querying, and a frontend without removing the local or skills-based paths.

## Terminal Workspace Model

- a session is a first-class context containing car, circuit, conditions, laps, and setup history
- a lap is selectable within its parent session
- the prompt displays the current context, for example `telemetry-tracker / spa-2026-08-28 / lap-7`
- slash commands perform deterministic operations
- ordinary language is handled by an external skill initially and by optional native chat later
- `/show-telemetry` toggles a live view in the TUI
- `telemetry-tracker telemetry --follow` supports a dedicated telemetry terminal

Initial command families:
- session navigation: `/sessions`, `/open-session`, `/back`
- lap navigation: `/laps`, `/open-lap`, `/compare`
- live operation: `/show-telemetry`, `/tracking start`, `/tracking stop`
- setup workflow: `/setup`, `/setups`, `/create-setup`, `/compare-setup`, `/apply-setup`

## Skills-First Architecture

Skills remain a supported product interface across all phases.

The MVP should first expose deterministic, structured commands such as:

```text
telemetry-tracker sessions list --json
telemetry-tracker laps list --session <id> --json
telemetry-tracker laps show <id> --json
telemetry-tracker laps compare <lap-a> <lap-b> --json
telemetry-tracker setup show --lap <id> --json
```

Skills use these commands to select evidence, ask for driver feedback, explain uncertainty, and produce useful coaching. They must not invent unavailable telemetry or setup fields.

The `/create-setup` skill:
1. resolves the active session, car, reference lap, and baseline setup
2. asks what handling problem the driver experienced
3. inspects relevant telemetry and comparison laps
4. proposes a small, bounded set of setup changes
5. explains evidence, expected effect, and trade-offs
6. saves a versioned proposal
7. requires explicit confirmation before exporting or applying it to LMU

## Tracking Behaviour

Telemetry is always readable, but persistence is controlled.

### While tracking is inactive

- telemetry is still read for console verification
- no buffering for persistence occurs
- no data is saved

### While tracking is active

- telemetry is read continuously
- samples are buffered in memory for the current lap
- samples are downsampled to a controlled rate of 10-20 Hz
- lap changes are detected
- when a lap completes:
  - a `LapSummary` is calculated
  - a `LapTrace` is created from buffered samples
  - both are saved to the database
  - the buffer is cleared

### Tracking control commands

- `/tracking start`
- `/tracking stop`
- `/tracking status`

Equivalent scriptable CLI commands should return structured output. Collector-to-server ingestion is deferred to the hosted expansion phase.

## Live Console Verification

The console must continuously display telemetry status regardless of tracking state.

Expected output once per second as a single updating console status line:

```text
[Telemetry] connected=true | tracking=false | packets/sec=118 | lap=4 | speed=243 | throttle=92% | brake=0% | gear=6 | rpm=8210
```

While tracking:

```text
[Tracking] active=true | lap=4 | samples buffered=932 | speed=243 | throttle=92% | brake=0%
```

When saving:

```text
[Lap Saved] lap=4 | time=1:42.318 | avgSpeed=178 | maxSpeed=286 | samples=1240
```

## Data Storage Model

MVP storage is local-first with four conceptual record types:

1. `sessions`
2. `lap_summaries`
3. `lap_traces`
4. `setup_revisions`

An embedded store such as SQLite is the expected MVP implementation. The precise provider should be confirmed in the persistence slice. Hosted Supabase can later represent the same concepts for synchronization and remote access.

### sessions

Fields should include a stable identifier plus available circuit, car, start/end, conditions, and active setup references. Do not invent values that LMU does not reliably expose.

### lap_summaries

Fields:
- `Id`
- `SessionId`
- `LapNumber`
- `StartedAt`
- `CompletedAt`
- `LapTime`
- `Sector1Time`
- `Sector2Time`
- `Sector3Time`
- `AverageSpeed`
- `MaxSpeed`
- `MinSpeed`
- `AverageThrottle`
- `AverageBrake`
- `MaxBrake`
- `AverageSteering`
- `MaxSteering`
- `GearChanges`
- `TopGear`
- `LowestGear`
- `SampleCount`
- `CreatedAt`

Purpose:
- filtering
- sorting
- comparisons
- primary input for AI prompts

### lap_traces

Fields:
- `Id`
- `LapSummaryId` (FK)
- `SampleRateHz`
- `TraceFormatVersion`
- `Samples` (structured JSON; JSONB when hosted in Postgres)
- `CreatedAt`

Purpose:
- deeper analysis
- optional input for AI when detailed insight is needed
- not the default shape for general querying

### setup_revisions

Fields should include:
- `Id`
- `SessionId`
- optional `SourceLapId`
- optional parent revision identifier
- car and setup-format identifiers
- human-readable name
- structured setup values or a safely preserved source artifact
- proposal rationale and expected trade-offs
- status such as baseline, proposed, confirmed, exported, or discarded
- creation timestamp

Rules:
- never overwrite the baseline setup when creating a proposal
- distinguish what the game supplied from what a skill inferred or proposed
- retain enough provenance to compare a setup against the laps driven with it
- require explicit user confirmation before export or application

Setup-file reference rules:
- `references/lmu-setups/992s-pc-moddev-example.svm` is a non-authoritative format fixture from a ModDev pace-car path
- preserve unknown fields, comments, and ordering until real LMU round-trip behaviour proves normalization is safe
- do not generalize setting availability, indices, ranges, or units across cars
- collect representative, provenance-documented fixtures for supported cars before implementing setup export
- validate parser and writer behaviour separately from the AI skill that proposes changes

## Telemetry Sample Format

Each lap trace sample should stay compact and consistent:

```json
{
  "t": 12.35,
  "speed": 241.2,
  "throttle": 0.94,
  "brake": 0.0,
  "steering": -0.12,
  "gear": 6,
  "rpm": 8120,
  "x": 123.4,
  "y": 0.0,
  "z": 456.7
}
```

Rules:
- use short field names to reduce size
- use numeric values where possible
- keep the shape consistent
- `t` is relative to lap start

## Sampling Strategy

- do not store every raw packet
- downsample to 10-20 Hz
- keep sampling consistent
- preserve enough fidelity for:
  - braking zones
  - throttle application
  - gear changes
  - steering inputs

## Telemetry Source Design

The telemetry source is configurable:

- `Mock`
- `LMU`

### MockTelemetrySource

- emits realistic telemetry
- simulates lap changes
- matches the real source data shape closely enough to exercise the full pipeline

### LmuSharedMemoryTelemetrySource

- reads real LMU telemetry
- uses the LMU SDK headers as the source of truth

Implementation direction:
- define a small source interface for polling the latest telemetry frame
- keep source-specific details inside the source implementation
- make the rest of the tracking pipeline source-agnostic

## LMU Shared Memory Integration

Reference files are the source of truth:

- `references/lmu-sdks/InternalsPlugin.hpp`
- `references/lmu-sdks/PluginObjects.hpp`
- `references/lmu-sdks/SharedMemoryInterface.hpp`

If these files are later moved under `docs/lmu-sdk/`, the plan remains the same: those headers are authoritative.

Requirements:
- do not invent telemetry fields
- do not invent struct layouts
- respect struct alignment and 4-byte packing
- follow the shared-memory read/copy pattern defined in the headers
- handle missing telemetry gracefully
- do not crash if LMU is not running

Interop expectations:
- keep Win32 interop isolated
- translate C++ headers into interop-safe C# structs only where needed
- treat pointer-bearing fields carefully
- avoid using raw mapped pointers as long-lived managed state

## Feature Slices

### 1. Telemetry Status

Responsibility:
- always-on live read verification
- connectivity status
- packets/sec and latest live values for console and API

MVP interfaces:
- `/show-telemetry`
- `telemetry-tracker telemetry status --json`

Expected shape:
- source name
- connected/disconnected
- supported platform
- tracking active/inactive
- last read time
- packets/sec
- concise disconnected message when unavailable

### 2. Tracking Control

Responsibility:
- explicit tracking lifecycle
- state transitions
- current tracking session metadata

MVP interfaces:
- `/tracking start`
- `/tracking stop`
- `/tracking status`

Rules:
- starting tracking begins in-memory buffering for the active lap
- stopping tracking immediately stops persistence buffering
- stopping tracking should not save a partial lap unless that behaviour is explicitly added later
- duplicate start/stop requests should be handled cleanly and predictably

### 3. Lap Capture and Persistence

Responsibility:
- detect lap boundaries
- build lap summary
- build lap trace
- persist both atomically enough for the vertical slice

Rules:
- only tracked laps are persisted
- save exactly one summary and one trace per completed lap
- clear lap buffer after successful persistence
- keep session-level identifiers stable enough to group laps

### 4. Saved Laps Querying

Expected behaviour:
- `/sessions` and `/laps` return lightweight records suitable for browsing and filtering
- `/open-lap` returns the selected lap summary plus its trace when needed for drill-down
- scriptable equivalents return stable JSON for skills
- future read-heavy views may be served from dedicated projected read models instead of directly from write-side tables

### 5. Skill Operations

Responsibility:
- provide stable JSON CLI output for sessions, laps, comparisons, telemetry, and setups
- keep analysis evidence compact and structured
- allow Codex/Claude skills to coach without direct database access
- support guided, versioned setup proposals

## MVP Command Surface

### Tracking

- `/tracking start`
- `/tracking stop`
- `/tracking status`

### Telemetry

- `/show-telemetry`
- `telemetry-tracker telemetry --follow`
- `telemetry-tracker telemetry status --json`

### Laps

- `/sessions`, `/open-session`
- `/laps`, `/open-lap`, `/compare`
- equivalent scriptable commands with `--json`

### Setups

- `/setup`, `/setups`
- `/create-setup`
- `/compare-setup`
- `/apply-setup` with explicit confirmation

### Later Adapters

- MCP tools over the same operations
- native BYOK terminal chat
- HTTP ingestion, query, and AI endpoints
- hosted synchronization and frontend

Implementation preference:
- thin interface adapters
- request/handler per feature
- no large controller/service orchestration layer

## Data Flow

End-to-end flow:

1. application starts
2. telemetry background service starts
3. telemetry source emits live frames continuously
4. console logs compact live status once per second
5. if tracking is inactive, frames are observed but not persisted
6. if tracking is active, frames are sampled and buffered for the current lap
7. lap change is detected
8. lap summary is calculated
9. lap trace JSON payload is created
10. session, summary, trace, and setup association are saved locally
11. CLI/TUI queries expose navigable session and lap context
12. skills consume structured commands for coaching and setup workflows
13. later MCP, native AI, HTTP, synchronization, and frontend adapters reuse the same handlers

## Suggested Vertical Slice Structure

Organise by feature rather than technical layer. Likely top-level slices:

- `Features/TelemetryStatus`
- `Features/Tracking`
- `Features/Sessions`
- `Features/Laps`
- `Features/Setups`
- `Features/SkillOperations`
- later: `Features/Ingestion` and `Features/Ask`
- `Infrastructure/Persistence`
- `Infrastructure/TelemetrySources`

Each feature should keep its own:
- command/query handler and thin interface adapter
- request/response DTOs
- handler
- validation where needed
- feature-local persistence query/command logic where practical

When future read models are introduced:
- keep write handlers in the owning feature slice
- place projection handlers or background projection workers close to the feature they serve when practical
- keep query handlers reading from read models or cache-friendly shapes, not from command orchestration code

## Testing Priorities

Add unit tests where they provide clear value. Prioritise:

- lap summary calculation
- lap trace generation
- sampling logic
- tracking state transitions
- lap detection
- structured skill-facing output and comparison evidence selection
- setup proposal validation and confirmation boundaries
- telemetry source behaviour

Also keep focused tests around:

- disconnected LMU startup
- telemetry status endpoint returns disconnected state when LMU is unavailable
- mock source behaviour for deterministic lap completion

## Logging Expectations

Logging stays simple and console-based.

Required logging:
- once-per-second live telemetry status
- tracking state changes
- lap save events
- clear warnings when telemetry is unavailable
- clear warnings when persistence fails

Avoid:
- verbose packet-by-packet logs by default
- noisy framework-heavy logging configuration

## Phased Implementation Plan

### Phase 1. Local telemetry host and tracking core

- preserve and reuse the working LMU reader
- introduce explicit tracking state, lap buffering, boundary detection, summaries, and traces
- ensure one local component owns LMU shared-memory acquisition

### Phase 2. Local session and setup persistence

- make sessions first-class records
- persist laps and traces locally
- capture the setup associated with a session/lap where LMU provides a reliable source
- store versioned setup proposals without overwriting the baseline

### Phase 3. Scriptable CLI

- add deterministic session, lap, telemetry, comparison, and setup commands
- provide stable JSON output for skills and automation
- support `telemetry --follow` from a separate terminal

### Phase 4. Interactive terminal workspace

- add session/lap navigation and context-aware prompt
- add slash-command routing
- add live telemetry display without blocking navigation
- keep presentation thin over application commands and queries

### Phase 5. Initial skills

- provide session review and lap comparison skills
- add a guided `/create-setup` workflow
- require driver feedback and explicit confirmation before setup export
- validate the drive, inspect, compare, ask, adjust loop

### Phase 6. Native AI and MCP

- add optional bring-your-own-key terminal chat
- store credentials securely where practical
- expose mature application operations as MCP tools
- keep skills fully supported

### Phase 7. Hosted expansion

- add collector/server runtime roles
- add ingestion and synchronization
- use Supabase for hosted persistence
- add HTTP query/AI endpoints and authentication when required

### Phase 8. Frontend and broader product polish

- add a graphical frontend over the established application/API contracts
- improve visualization, sharing, and longer-term analytics

### Later Architectural Evolution

- add asynchronous read-model projection when write endpoints would otherwise block on expensive query-shape updates
- start with an in-process queue plus background worker before considering heavier infrastructure
- add Redis only after read models exist and query hot paths justify caching
- invalidate or refresh Redis from the read side, not from ad hoc endpoint logic

## Explicit Non-Goals

Do not add in this scope unless explicitly requested:

- graphical frontend during the MVP
- auth
- background job systems
- distributed architecture
- event buses
- generic plugin frameworks
- hosted synchronization
- automatic setup application without confirmation
- premature analytics pipelines

## Current Baseline

Already in place:
- ASP.NET Core API scaffold
- basic LMU shared-memory reader foundation
- telemetry status endpoints
- console-friendly logging configuration
- initial tests around layout and disconnected behaviour

This baseline should be evolved toward the feature slices above rather than replaced with a heavier architecture.

## Implementation Defaults

Unless requirements change, use these defaults:

- one local telemetry owner with multiple client terminals where needed
- console logging only
- thin CLI/TUI commands; thin MCP and HTTP adapters later
- local-first persistence for the MVP; Supabase later for hosted data
- database credentials only in hosted server configuration
- one current tracked lap buffered in memory
- no persistence when tracking is off
- `LapSummary` is the default evidence supplied to skills
- `LapTrace` is included only when extra detail is needed
- unit tests only where logic is meaningful enough to justify them

## Expected Outcome

A running terminal workspace where:

- telemetry is continuously visible in the console
- tracking is explicitly controlled
- each completed lap saves:
  - a summary in local persistence
  - a detailed trace in JSONB
- sessions and laps are navigable contexts
- live telemetry can be followed from one or more terminal views
- skills use structured CLI data for coaching and versioned setup proposals
- later MCP, native AI, hosted API, synchronization, and frontend work can reuse the same capabilities
