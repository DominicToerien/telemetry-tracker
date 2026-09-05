# Current Project State

Last verified: 2026-09-05

## Current phase

Telemetry Tracker is a runnable standalone .NET Generic Host console application with local tracking, SQLite persistence, machine-readable CLI queries, terminal navigation, and lossless LMU setup proposal creation for the first validated car.

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
- Creates versioned BMW M4 LMGT3 proposals for six validated setup settings, tied to a source lap and driver feedback.
- Preserves all source bytes outside intended setting lines, including legacy single-byte content, and rejects unsupported cars, fields, values, encodings, and no-op changes.
- Includes focused tests for interop, tracking, persistence, CLI, and setup-baseline behavior.

## Active slice

Validate BMW M4 LMGT3 proposal files in LMU before broadening the supported setup contract.

Acceptance criteria and deliberately deferred work live in [the current plan](plans/current.md).

## Known constraints

- Live LMU integration is supported only on Windows; SDK headers are authoritative.
- The native client must remain usable offline and must not contain hosted database credentials.
- The hosted API, synchronization, web frontend, and remote MCP adapters are later work in a separate server project.
- Setup proposals remain exact-car artifacts derived from an imported baseline, recorded lap, and explicit driver feedback.

## Blockers

No repository blockers are recorded. Live integration can be revalidated only on a supported Windows installation with LMU available.
