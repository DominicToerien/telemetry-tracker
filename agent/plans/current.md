# Current Plan: Tracking Core

## Outcome

Build the first complete tracking slice on top of the working LMU telemetry feed while keeping the native console application runnable.

## In scope

- Explicit inactive and active tracking states.
- Start and stop commands implemented as reusable application handlers.
- In-memory buffering for the current lap while tracking is active.
- Lap-boundary detection grounded in fields available from the LMU SDK.
- A completed-lap summary and a bounded, sampled lap trace.
- Deterministic tests using a mock or controlled telemetry source where live LMU is unnecessary.
- Continued console visibility for telemetry connection state.

## Out of scope

- ASP.NET endpoints, hosted ingestion, Supabase, Docker, or frontend work.
- Native AI chat or MCP adapters.
- Interactive terminal navigation beyond what is necessary to control and verify tracking.
- Setup export.

## Acceptance criteria

- Tracking can be started and stopped without disrupting telemetry reading.
- Frames are persisted or retained only while tracking is active.
- A lap transition produces one summary and one bounded trace without inventing LMU fields.
- Meaningful state and lap-boundary behavior has focused automated coverage.
- The application builds, tests pass, and disconnected/non-Windows startup behavior remains safe.

## Relevant context

- [Current project state](../current.md)
- [Product specification](../specs/product.md)
- [Architecture specification](../specs/architecture.md)
- [LMU integrity guide](../skills/lmu-integrity.md)
- [Vertical-slice guide](../skills/vertical-slice.md)
- [Detailed design archive](../plan.md)
