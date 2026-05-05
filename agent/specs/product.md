# Product Specification

## Product

Build a lightweight telemetry analysis service for Le Mans Ultimate.

The service should:

- read LMU telemetry
- allow the user to explicitly start and stop telemetry tracking
- store lap-level summaries and detailed lap telemetry traces in Supabase Postgres
- expose an API for querying saved laps and asking AI-assisted driving-performance questions
- support both local telemetry collection and hosted ingestion/query capabilities from one codebase

The focus is on delivering a clean, working vertical slice of functionality with strong architectural foundations and limited scope.

## Technology

- ASP.NET Core Web API
- C#
- Vertical Slice Architecture
- CQRS
- single process runtime
- `HostedService` / `BackgroundService`
- Supabase Postgres
- console logging
- unit tests where they add clear value

## Constraints

Do not introduce:

- microservices
- message brokers
- complex infrastructure
- frontend/UI
- authentication or authorization unless explicitly requested

## Runtime Roles

The same project supports two roles:

- Collector role (local user machine):
  - reads LMU shared memory locally
  - performs tracking and lap packaging
  - sends telemetry/lap payloads to hosted endpoints
- Server role (hosted):
  - accepts ingestion payloads
  - persists summaries and traces
  - serves query and AI endpoints

The collector must run locally because LMU shared memory is local-process accessible.

## Operational Model

The application:

- runs continuously
- continuously reads telemetry for live verification
- only persists telemetry while tracking is explicitly active
- saves one summary and one trace per completed lap
- exposes APIs for tracking, telemetry status, laps, and AI analysis
- keeps database credentials server-side only

## Architectural Direction

- organise code by feature, not by technical layer
- commands mutate state
- queries return data
- keep endpoints thin
- avoid large generic service layers
- prefer the smallest complete end-to-end solution

## Tracking Model

While tracking is inactive:

- telemetry is still read for live verification
- no persistence buffering occurs
- nothing is saved

While tracking is active:

- telemetry is read continuously
- the current lap is buffered in memory
- sampling is downsampled to 10-20 Hz
- lap completion triggers summary calculation, trace generation, and persistence

## Persistence Model

Use two tables:

- `lap_summaries`
- `lap_traces`

`lap_summaries` is for filtering, comparisons, and primary AI prompt context.

`lap_traces` is for deeper analysis and optional AI detail when needed.

## LMU Integration Rules

The authoritative LMU SDK files are:

- `references/lmu-sdks/InternalsPlugin.hpp`
- `references/lmu-sdks/PluginObjects.hpp`
- `references/lmu-sdks/SharedMemoryInterface.hpp`

Requirements:

- do not invent telemetry fields
- respect struct packing and alignment
- follow the shared-memory copy pattern from the headers
- handle LMU absence gracefully
- do not crash if LMU is not running

## Security and Configuration

- Never commit real Supabase/Postgres connection strings.
- Collector runtime should not require direct DB credentials.
- Hosted server runtime owns persistence secrets.
- App should remain runnable without DB config, while clearly reporting persistence-disabled status.

## Desired Outcome

A runnable service where:

- telemetry is visible in the console
- tracking is user-controlled
- completed laps save a summary plus a trace
- saved laps can be queried over HTTP
- AI insights are based on structured telemetry context
