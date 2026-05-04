# Project Progress

This file is the current snapshot of where the project stands.

It should be updated whenever implementation meaningfully changes delivery status, current focus, completed phases, or key blockers.

## Status Summary

- Current date: 2026-05-04
- Overall phase: foundation moving toward the broader telemetry-tracking vertical slice
- App state: runnable
- Telemetry source state: LMU shared-memory foundation implemented, broader mock-plus-tracking pipeline not yet implemented

## Completed

- ASP.NET Core API scaffold is in place
- Console/debug-friendly logging setup is in place
- LMU shared-memory reader foundation is implemented
- Telemetry status and debug endpoints are implemented as a `Features/TelemetryStatus` vertical slice
- LMU disconnected startup behaviour is handled without crashing
- Initial unit and integration-style tests for LMU status/interoperability are in place
- consolidated `agent/`, `agent/specs/`, and `agent/skills/` documentation structure is in place
- `agent/skills/` index and entry-point-driven document loading flow are in place
- Supabase EF Core package, DbContext shell, and startup registration hook are in place
- Root `.env` loading is in place for local secret-based configuration

## In Progress

- Expanding the project from LMU status verification into the broader lap-tracking and persistence workflow defined in `agent/plan.md`
- Establishing the telemetry status feature as the architecture template for future slices
- Establishing the persistence foundation so later lap-storage slices can target Supabase cleanly

## Not Started

- `MockTelemetrySource`
- explicit tracking control endpoints and state machine
- lap buffering and lap boundary detection
- lap summary calculation
- lap trace generation
- Supabase persistence beyond connection setup and DbContext registration
- saved lap query endpoints
- `/ask` AI analysis endpoint
- broader vertical-slice refactor beyond the telemetry status slice

## Current Focus

- Keep the project runnable while evolving from telemetry-status foundation to the first complete telemetry-tracking slice.
- Preserve LMU correctness while introducing source abstraction and tracking behaviour.
- Keep the agent context structure stable so `read agent/entry-point.md` remains a dependable workflow starter.
- Add infrastructure only when it unlocks the next slice cleanly and without widening scope.

## Known Constraints

- Live LMU telemetry is Windows-only
- LMU interop must stay faithful to the SDK headers
- The broader target architecture in the plan is ahead of the current codebase and should be reached incrementally

## Next Recommended Slice

- Implement `MockTelemetrySource` plus once-per-second live console telemetry output.
- Then add explicit tracking state and tracking endpoints.

## Update Rule

When a task changes architecture, feature completion, active phase, or blockers:
- update this file in the same change
- keep it concise and factual
- prefer current snapshot over narrative detail
