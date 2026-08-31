# Current Project State

Last verified against commit: `6f500d3` (2026-08-30)

## Current phase

Telemetry Tracker is a runnable standalone .NET Generic Host console application. The LMU shared-memory foundation works on Windows and the project is moving toward its first complete local tracking slice.

## Verified capabilities

- Reads LMU shared memory and retains the latest scoring and telemetry snapshot.
- Reports unsupported or disconnected state without crashing when LMU is unavailable.
- Displays live player telemetry in an updating console status block that wraps to the terminal width.
- Exposes telemetry status and debug query handlers in `Features/TelemetryStatus`.
- Validates and, when configured, normalizes the LMU shared-memory plugin configuration.
- Includes focused tests for interop layout and telemetry-provider behavior.

## Active slice

Add explicit tracking state, lap buffering, lap-boundary detection, summaries, and sampled traces over the existing LMU feed. Keep the work local-first and expose reusable application handlers rather than HTTP endpoints.

Acceptance criteria and deliberately deferred work live in [the current plan](plans/current.md).

## Known constraints

- Live LMU integration is supported only on Windows; SDK headers are authoritative.
- The native client must remain usable offline and must not contain hosted database credentials.
- The hosted API, synchronization, web frontend, and remote MCP adapters are later work in a separate server project.
- The target architecture is ahead of the implementation and must be reached incrementally.

## Blockers

No repository blockers are recorded. Live integration can be revalidated only on a supported Windows installation with LMU available.
