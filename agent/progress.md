# Project Progress

This file is the current snapshot of where the project stands.

It should be updated whenever implementation meaningfully changes delivery status, current focus, completed phases, or key blockers.

## Status Summary

- Current date: 2026-05-04
- Overall phase: foundation moving toward the broader telemetry-tracking vertical slice
- App state: runnable
- Telemetry source state: LMU shared-memory reader is producing live player telemetry values, broader mock-plus-tracking pipeline not yet implemented
- Runtime direction: single codebase, planned dual-role operation (`Collector` local + hosted `Server`)

## Completed

- ASP.NET Core API scaffold is in place
- Console/debug-friendly logging setup is in place
- LMU shared-memory reader foundation is implemented
- Telemetry status and debug endpoints are implemented as a `Features/TelemetryStatus` vertical slice
- Live LMU player telemetry output is implemented for LMU verification (RE-5)
- LMU startup now validates and normalizes local shared-memory plugin configuration
- LMU interop layout was corrected so telemetry block offsets produce live car data
- Live telemetry console output now renders as a single updating status row instead of sequential log spam
- LMU disconnected startup behaviour is handled without crashing
- Initial unit and integration-style tests for LMU status/interoperability are in place
- consolidated `agent/`, `agent/specs/`, and `agent/skills/` documentation structure is in place
- `agent/skills/` index and entry-point-driven document loading flow are in place

## In Progress

- Expanding the project from LMU status verification into the broader lap-tracking and persistence workflow defined in `agent/plan.md`
- Establishing the telemetry status feature as the architecture template for future slices
- Tightening the LMU bring-up path now that real in-game telemetry data is flowing
- Defining collector-to-hosted ingestion direction so local clients do not need direct DB credentials

## Not Started

- `MockTelemetrySource`
- explicit tracking control endpoints and state machine
- lap buffering and lap boundary detection
- lap summary calculation
- lap trace generation
- Supabase persistence
- saved lap query endpoints
- `/ask` AI analysis endpoint
- broader vertical-slice refactor beyond the telemetry status slice
- tracking start/stop state and endpoints
- collector ingestion endpoints and runtime role mode switch

## Current Focus

- Keep the project runnable while evolving from telemetry-status foundation to the first complete telemetry-tracking slice.
- Preserve LMU correctness while introducing source abstraction and tracking behaviour.
- Keep the agent context structure stable so `read agent/entry-point.md` remains a dependable workflow starter.

## Known Constraints

- Live LMU telemetry is Windows-only
- LMU interop must stay faithful to the SDK headers
- The broader target architecture in the plan is ahead of the current codebase and should be reached incrementally
- Hosted persistence must keep DB secrets server-side; collector runtime should remain secret-light
- Console status rendering is optimized for live readability rather than historical per-frame logging

## Next Recommended Slice

- Build explicit tracking state and tracking endpoints on top of the now-working LMU live telemetry feed.
- Then add lap buffering and downsampled lap capture.

## Update Rule

When a task changes architecture, feature completion, active phase, or blockers:
- update this file in the same change
- keep it concise and factual
- prefer current snapshot over narrative detail
