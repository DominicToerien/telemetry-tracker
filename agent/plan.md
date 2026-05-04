# Telemetry Tracker Plan

## Summary

Build a lightweight single-process telemetry analysis service for Le Mans Ultimate in ASP.NET Core Web API using C#, Vertical Slice Architecture, and CQRS. The system continuously reads telemetry for live verification, allows explicit start/stop tracking, stores one lap summary plus one detailed lap trace per completed lap in Supabase Postgres, and exposes thin API endpoints for tracking control, telemetry status, lap retrieval, and AI-assisted driving-performance questions.

The goal is a clean working vertical slice with strong foundations and deliberately limited scope.

## Product Goal

Deliver a running backend service where:
- telemetry is continuously visible in the console
- tracking is explicitly controlled by the user
- each completed tracked lap saves:
  - one relational lap summary
  - one JSONB lap trace
- the API exposes saved lap data
- AI insights are based on meaningful telemetry context

## Technology and Constraints

- Platform: ASP.NET Core Web API
- Language: C#
- Architecture: Vertical Slice Architecture + CQRS
- Runtime: single process
- Background processing: `HostedService` / `BackgroundService`
- Storage: Supabase Postgres
- API style: Minimal APIs or thin endpoints
- Logging: console logging
- Testing: unit tests where they add clear value

Do not introduce:
- microservices
- message brokers
- complex infrastructure
- frontend/UI
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

### 3. Limited Scope

- Build the smallest complete version that works end-to-end.
- Prefer clarity over cleverness.
- Avoid unused abstractions.
- Do not optimise prematurely.

### 4. Testability

- Add unit tests around logic-heavy components.
- Prioritise tests for lap summary calculation, lap trace generation, tracking state transitions, prompt building, and telemetry-source behaviour.

## Core System Design

The application is a single ASP.NET Core Web API process that:

- runs continuously
- starts a telemetry background service at boot
- continuously reads telemetry for live verification
- logs compact telemetry status to the console
- only saves telemetry when tracking is explicitly active
- saves one summary and one detailed trace per completed lap
- exposes API endpoints for tracking, status, saved laps, and AI analysis

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

### Tracking control endpoints

- `POST /tracking/start`
- `POST /tracking/stop`
- `GET /tracking/status`

## Live Console Verification

The console must continuously display telemetry status regardless of tracking state.

Expected output once per second:

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

Storage is Supabase Postgres with a two-table model:

1. `lap_summaries`
2. `lap_traces`

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
- `Samples` (JSONB)
- `CreatedAt`

Purpose:
- deeper analysis
- optional input for AI when detailed insight is needed
- not the default shape for general querying

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

Endpoints:
- `GET /telemetry/status`

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

Endpoints:
- `POST /tracking/start`
- `POST /tracking/stop`
- `GET /tracking/status`

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

Endpoints:
- `GET /laps`
- `GET /laps/{id}`

Expected behaviour:
- `GET /laps` returns lightweight lap summary records suitable for browsing and filtering
- `GET /laps/{id}` returns the selected lap summary plus its trace when needed for drill-down

### 5. AI Analysis

Endpoint:
- `POST /ask`

Responsibility:
- build compact structured prompts
- use `LapSummary` as the primary context
- include `LapTrace` only when the question needs deeper analysis
- avoid shipping unnecessary telemetry into the prompt

## API Surface

### Tracking

- `POST /tracking/start`
- `POST /tracking/stop`
- `GET /tracking/status`

### Telemetry

- `GET /telemetry/status`

### Laps

- `GET /laps`
- `GET /laps/{id}`

### AI

- `POST /ask`

Implementation preference:
- thin endpoints
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
10. summary and trace are saved to Supabase Postgres
11. APIs expose saved lap data
12. `/ask` builds a prompt from summary plus optional trace and returns AI-assisted analysis

## Suggested Vertical Slice Structure

Organise by feature rather than technical layer. Likely top-level slices:

- `Features/TelemetryStatus`
- `Features/Tracking`
- `Features/Laps`
- `Features/Ask`
- `Infrastructure/Persistence`
- `Infrastructure/TelemetrySources`

Each feature should keep its own:
- endpoint
- request/response DTOs
- handler
- validation where needed
- feature-local persistence query/command logic where practical

## Testing Priorities

Add unit tests where they provide clear value. Prioritise:

- lap summary calculation
- lap trace generation
- sampling logic
- tracking state transitions
- lap detection
- prompt building
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

### Phase 1. Project setup

- establish folder structure aligned with vertical slices
- keep console logging simple
- wire core app bootstrapping and hosted services

### Phase 2. Mock telemetry + console

- implement `MockTelemetrySource`
- continuously read mock telemetry
- show compact once-per-second console output

### Phase 3. Tracking state

- add start/stop/status endpoints
- implement explicit tracking lifecycle and state transitions

### Phase 4. Lap summary calculation

- detect lap completion
- compute lap summary metrics from buffered samples

### Phase 5. Lap trace generation

- create compact JSONB trace samples
- apply controlled downsampling

### Phase 6. Supabase persistence

- add Postgres persistence for summaries and traces
- keep schema simple and direct

### Phase 7. API endpoints

- add saved laps endpoints
- keep response shapes lightweight and useful

### Phase 8. LLM integration

- add `/ask`
- implement compact prompt building from summary plus optional trace

### Phase 9. LMU integration

- switch from mock-only to configurable mock/LMU source selection
- validate LMU telemetry against the SDK headers

### Phase 10. Demo polish

- improve ergonomics
- tighten logs
- improve error messages
- clean up rough edges in the vertical slice

## Explicit Non-Goals

Do not add in this scope unless explicitly requested:

- frontend or dashboard UI
- auth
- background job systems
- distributed architecture
- event buses
- generic plugin frameworks
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

- single process only
- console logging only
- thin endpoints or Minimal APIs
- Supabase Postgres as the only persistence store
- one current tracked lap buffered in memory
- no persistence when tracking is off
- `LapSummary` is the default input for AI prompts
- `LapTrace` is included only when extra detail is needed
- unit tests only where logic is meaningful enough to justify them

## Expected Outcome

A running service where:

- telemetry is continuously visible in the console
- tracking is explicitly controlled
- each completed lap saves:
  - a summary in Postgres
  - a detailed trace in JSONB
- the API exposes lap data cleanly
- AI insights are based on meaningful telemetry rather than raw packet dumps
