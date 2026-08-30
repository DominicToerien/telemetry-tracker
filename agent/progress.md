# Project Progress

This file is the current snapshot of where the project stands.

It should be updated whenever implementation meaningfully changes delivery status, current focus, completed phases, or key blockers.

## Status Summary

- Current date: 2026-08-30
- Overall phase: LMU foundation moving toward the skills-first local terminal MVP
- App state: runnable
- Telemetry source state: LMU shared-memory reader is producing live player telemetry values, broader mock-plus-tracking pipeline not yet implemented
- Runtime direction: standalone native Windows CLI/TUI client; a separate hosted server project may be added later

## Completed

- standalone .NET Generic Host console scaffold is in place
- Console/debug-friendly logging setup is in place
- LMU shared-memory reader foundation is implemented
- Telemetry status and debug query handlers are implemented as a `Features/TelemetryStatus` vertical slice
- Live LMU player telemetry output is implemented for LMU verification (RE-5)
- LMU startup now validates and normalizes local shared-memory plugin configuration
- LMU interop layout was corrected so telemetry block offsets produce live car data
- Live telemetry console output now renders as a single updating status row instead of sequential log spam
- LMU disconnected startup behaviour is handled without crashing
- Initial unit and integration-style tests for LMU status/interoperability are in place
- consolidated `agent/`, `agent/specs/`, and `agent/skills/` documentation structure is in place
- `agent/skills/` index and entry-point-driven document loading flow are in place
- ASP.NET hosting, HTTP endpoints, Swagger, hosted persistence packages/migrations, and Docker tooling have been removed from the native client
- local parallel-agent worktree scripts are in place without obsolete API port allocation
- branch-agnostic PM chat guidance is in place for planning-first workflows
- a non-authoritative LMU `.svm` format fixture is documented for future setup parsing and `/create-setup` work
- a draft-PR-based session handoff skill and pull-request template are in place for transferring work across agents and computers
- the native Windows MVP, optional Dockerized hosted platform, transport, and MCP boundaries are documented as required agent context
- local tracking, SQLite session/lap persistence, terminal navigation, and initial skills are implemented on the current skills branch
- the structured recorded-data contract supports `session -> laps -> lap summary -> telemetry trace` queries without exposing SQLite directly

## In Progress

- Expanding the project from LMU status verification into the broader lap-tracking and persistence workflow defined in `agent/plan.md`
- Establishing the telemetry status feature as the architecture template for future slices
- Tightening the LMU bring-up path now that real in-game telemetry data is flowing
- Reframing the initial product surface around session/lap terminal navigation and stable skill-facing CLI commands

## Not Started

- `MockTelemetrySource`
- explicit tracking control commands and state machine
- lap buffering and lap boundary detection
- lap summary calculation
- lap trace generation
- broader vertical-slice refactor beyond the telemetry status slice
- tracking start/stop state and CLI adapters
- first-class local session model and setup revision history
- scriptable session/lap/comparison/setup CLI commands
- interactive session/lap TUI navigation
- skills for session review, lap comparison, and guided setup creation
- multi-terminal attachment to one local telemetry owner

## Deferred Hosted Phase

- separate ASP.NET Core server project
- authenticated ingestion and synchronization APIs
- Supabase or other hosted persistence
- web frontend and remote MCP adapters

## Current Focus

- Keep the project runnable while evolving from telemetry-status foundation to the first complete telemetry-tracking slice.
- Preserve LMU correctness while introducing source abstraction and tracking behaviour.
- Keep the agent context structure stable so `read agent/entry-point.md` remains a dependable workflow starter.
- Add infrastructure only when it unlocks the next slice cleanly and without widening scope.
- Keep parallel local AI work isolated through worktrees rather than shared branch switching.
- Make dedicated worktrees the default expectation for any chat that edits the repository.
- Keep planning-first conversations branch-agnostic until they intentionally hand work off to an implementation chat.
- Make local CLI/TUI and skills the first complete product loop; retain MCP, native BYOK chat, hosted API/sync, and frontend as later adapters.

## Known Constraints

- Live LMU telemetry is Windows-only
- LMU interop must stay faithful to the SDK headers
- The broader target architecture in the plan is ahead of the current codebase and should be reached incrementally
- Hosted persistence must keep DB secrets server-side; collector runtime should remain secret-light
- Console status rendering is optimized for live readability rather than historical per-frame logging

## Next Recommended Slice

- Build explicit tracking state and lap capture on top of the working LMU feed, shaped as reusable application handlers rather than HTTP-first behaviour.
- Then add first-class local sessions and stable JSON CLI queries so the initial skills have a dependable data interface.

## Update Rule

When a task changes architecture, feature completion, active phase, or blockers:
- update this file in the same change
- keep it concise and factual
- prefer current snapshot over narrative detail
