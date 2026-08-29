# Product Specification

## Product

Build a local-first telemetry workspace for Le Mans Ultimate. The primary MVP experience is an interactive terminal application with a Codex/Claude-style command and chat workflow.

The product should:

- read LMU telemetry
- allow the user to explicitly start and stop telemetry tracking
- treat sessions as directory-like contexts and laps as file-like items within them
- store lap summaries, detailed traces, conditions, and setup snapshots locally
- let users browse sessions and laps, compare laps, and monitor live telemetry from the terminal
- use skills as the initial AI integration for coaching and setup workflows
- retain skills as a supported option when native AI, MCP, hosted APIs, synchronization, and a frontend are added later

The focus is on delivering a clean, working vertical slice of functionality with strong architectural foundations and limited scope.

## Technology

- C#
- Vertical Slice Architecture
- CQRS
- local CLI/TUI as the primary MVP surface
- a local telemetry host that can serve multiple terminal clients
- `HostedService` / `BackgroundService`
- local-first persistence
- console logging
- unit tests where they add clear value

## Constraints

Do not introduce during the initial MVP:

- microservices
- message brokers
- complex infrastructure
- graphical frontend
- authentication or authorization unless explicitly requested
- hosted synchronization or server infrastructure
- automatic, unconfirmed modification of LMU setup files

## Primary User Experience

The terminal behaves like a workspace:

- sessions are navigable directory-like contexts
- laps are selectable file-like items inside a session
- the current session or lap scopes commands and skill context
- slash commands perform deterministic actions
- normal chat can be handled by the active skill or, later, the native AI client

Representative commands include `/sessions`, `/open-session`, `/laps`, `/open-lap`, `/show-telemetry`, `/compare`, `/create-setup`, `/tracking start`, and `/tracking stop`.

`/show-telemetry` can render in the interactive workspace. A scriptable command such as `telemetry-tracker telemetry --follow` should support a dedicated second terminal.

## Skills-First AI Model

Skills are the initial AI experience, not a temporary throwaway implementation.

- stable, machine-readable CLI commands expose session, lap, telemetry, comparison, and setup data
- Codex or Claude skills call those commands and apply domain-specific coaching workflows
- the initial skills-based path can use the AI access already provided by the user's agent environment
- a later native terminal chat can support user-supplied provider API keys
- MCP and HTTP adapters should expose the same application capabilities instead of reimplementing them

`/create-setup` is a guided skill workflow. It combines telemetry, the active setup, setup history, and explicit driver feedback. It creates a versioned setup proposal, explains evidence and trade-offs, and requires confirmation before any export or application to LMU.

## Later Runtime Roles

The later hosted plan remains valid:

- a local collector reads LMU shared memory and owns the immediate terminal experience
- a hosted server may accept ingestion, synchronize data, persist shared history, and serve API/frontend clients
- database and hosted service credentials remain server-side

## Operational Model

The MVP application:

- runs continuously
- continuously reads telemetry for live verification
- only persists telemetry while tracking is explicitly active
- saves one summary and one trace per completed lap in local persistence
- supports multiple terminal views without starting competing telemetry readers
- exposes deterministic CLI operations for skills and automation

## Architectural Direction

- organise code by feature, not by technical layer
- commands mutate state
- queries return data
- keep terminal commands, skill adapters, and later endpoints thin
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

Use the existing summary-plus-trace concept, extended by first-class sessions and setup revisions:

- `sessions`
- `lap_summaries`
- `lap_traces`
- `setup_revisions`

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
- The local app should remain runnable without hosted database configuration; hosted persistence should clearly report when it is disabled.
- Native AI integration is deferred until after the skills-based workflow is useful.
- When native AI is added, users supply their own provider key and it should be stored using an OS credential store where practical.

## Desired Outcome

A runnable terminal product where:

- telemetry is visible in the main TUI or a dedicated terminal
- tracking is user-controlled
- sessions and laps can be browsed using the directory/file mental model
- completed laps save a summary plus a trace locally
- stable CLI commands provide structured context to skills
- a skill can compare laps, coach the driver, and create a versioned setup proposal
- later interfaces expand the product without replacing the skills-based workflow
