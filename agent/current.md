# Current Project State

Last verified: 2026-09-05

## Current phase

Telemetry Tracker is a runnable standalone .NET Generic Host console application with local tracking, SQLite persistence, machine-readable CLI queries, terminal navigation, and lossless LMU setup-baseline import. The next bounded slice is safe, car-specific setup modification.

## Verified capabilities

- Reads LMU shared memory and retains the latest scoring and telemetry snapshot.
- Reports unsupported or disconnected state without crashing when LMU is unavailable.
- Displays live player telemetry in an updating console status block that wraps to the terminal width.
- Exposes telemetry status and debug query handlers in `Features/TelemetryStatus`.
- Validates and, when configured, normalizes the LMU shared-memory plugin configuration.
- Tracks sessions and complete sequential laps, discarding the initial partial lap and retaining bounded sampled traces in local SQLite storage.
- Retains completed laps in an ordered in-memory retry queue until local persistence succeeds.
- Exposes the recorded session/lap/summary/trace hierarchy through stable JSON CLI queries.
- Provides terminal workspace navigation and initial telemetry-coach and setup-proposal skills.
- Validates terminal session/lap navigation and accepts quoted paths containing spaces.
- Discovers, losslessly imports, versions, browses, and compares LMU `.svm` setup baselines by exact car identifier.
- Refuses setup generation when no validated car-specific modification contract is available.
- Includes focused tests for interop, tracking, persistence, CLI, and setup-baseline behavior.

## Active slice

Validate concrete setting changes and lossless LMU round trips for explicitly supported cars before enabling setup output. Preserve source bytes, comments, unknown fields, and exact car identity.

Acceptance criteria and deliberately deferred work live in [the current plan](plans/current.md).

## Known constraints

- Live LMU integration is supported only on Windows; SDK headers are authoritative.
- The native client must remain usable offline and must not contain hosted database credentials.
- The hosted API, synchronization, web frontend, and remote MCP adapters are later work in a separate server project.
- Setup output must remain disabled until supported fields and round-trip behavior are validated for the exact car.

## Blockers

No repository blockers are recorded. Live integration can be revalidated only on a supported Windows installation with LMU available.
