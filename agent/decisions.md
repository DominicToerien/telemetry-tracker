# Architectural Decisions

This file records major architectural and product-shaping decisions so the project keeps its reasoning, not just its end state.

## How To Use This File

- Add an entry when a decision meaningfully affects architecture, scope, persistence, telemetry modelling, AI usage, or development workflow.
- Capture what was decided, when, and why.
- If a decision is later reversed or refined, add a new entry instead of rewriting history.
- Keep entries concise but specific enough that future work can follow the reasoning.

## Template

```text
## YYYY-MM-DD - Decision Title

Status:
- accepted | superseded | revised

Decision:
- What was chosen

Why:
- Why it was chosen

Consequences:
- What this enables
- What this constrains

Supersedes:
- Optional prior decision title
```

## 2026-05-04 - Single Process ASP.NET Core API

Status:
- accepted

Decision:
- Build the application as a single ASP.NET Core Web API process using hosted background services.

Why:
- Keeps the architecture lightweight.
- Fits the limited-scope vertical slice goal.
- Reduces operational complexity while the product is still being shaped.

Consequences:
- Background telemetry reading, tracking, persistence, and API serving all live in one deployable unit.
- Any future decomposition must be intentional and justified, not assumed.

## 2026-05-04 - Vertical Slice Architecture Plus CQRS

Status:
- accepted

Decision:
- Organise the application by feature slices and use explicit commands and queries.

Why:
- Keeps features easier to reason about and evolve.
- Avoids large generic service layers.
- Matches the desired delivery style for incremental AI-assisted implementation.

Consequences:
- New work should be added by feature, not by technical layer.
- Shared abstractions should be introduced sparingly.

## 2026-05-04 - Lap-Based Persistence Instead Of Raw Continuous Storage

Status:
- accepted

Decision:
- Persist one `LapSummary` and one `LapTrace` per completed tracked lap instead of storing raw telemetry continuously.

Why:
- Improves demo value.
- Reduces storage volume.
- Produces cleaner context for analysis and AI prompting.

Consequences:
- Tracking state and lap boundary detection are first-class concerns.
- Trace sampling quality matters more than raw packet retention.

## 2026-05-04 - LMU SDK Headers Are The Source Of Truth

Status:
- accepted

Decision:
- Treat `references/lmu-sdks/` as authoritative for LMU structs, events, names, and memory layout.

Why:
- Prevents hallucinated telemetry fields and invalid interop assumptions.
- Keeps LMU integration grounded in the vendor contract.

Consequences:
- LMU-related changes must be verified against the headers.
- Interop code should stay narrow and explicit.

## 2026-05-04 - Consolidated Agent Documentation Structure

Status:
- accepted

Decision:
- Use `agent/` for AI-operating context, `agent/skills/` for project-specific skills, and `agent/specs/` for product-facing specifications.

Why:
- Makes `read agent/entry-point.md` a reliable single starting instruction.
- Prevents documentation from referencing invented or drifting paths.
- Separates product intent from agent workflow and task-specific guidance.

Consequences:
- `agent/entry-point.md` must stay aligned with the real repository structure.
- New agent-facing markdown files should be added under `agent/`, `agent/skills/`, or `agent/specs/` intentionally, not ad hoc.

## 2026-05-04 - Telemetry Status Refactored Into A Vertical Slice

Status:
- accepted

Decision:
- Move the telemetry status HTTP surface from a controller-first shape into `Features/TelemetryStatus` with explicit query handlers and minimal endpoints.

Why:
- The target project architecture is Vertical Slice Architecture plus CQRS.
- The earlier controller plus shared DTO structure was functional but still organized more by technical layer than by feature.
- This creates a concrete implementation pattern for future slices such as tracking, laps, and ask.

Consequences:
- New user-facing behaviour should be added under `Features/{FeatureName}` first.
- Infrastructure such as the LMU provider can remain outside the feature folder, but feature contracts and handlers should live with the feature.

## 2026-05-04 - Supabase DbContext Uses .env Configuration With Conditional Registration

Status:
- accepted

Decision:
- Load local environment variables from a root `.env` file at startup and register `TelemetryTrackerDbContext` only when a Supabase connection string is actually configured.

Why:
- Keeps the `RE-6` slice tightly focused on connection setup instead of wider persistence work.
- Preserves a runnable application even when local database credentials are not present yet.
- Supports the intended developer workflow where secrets are supplied through `.env` instead of being committed into config files.

Consequences:
- The app can start without Supabase and will log that DbContext registration was skipped.
- Future persistence slices can depend on a real EF Core DbContext without reworking startup configuration.
- Features that require database access must handle the fact that local environments may not have Supabase configured yet.

## 2026-05-04 - Single Codebase With Collector And Hosted Server Roles

Status:
- accepted

Decision:
- Keep local telemetry collection and hosted ingestion/query capabilities in the same project, with role-specific runtime behavior.

Why:
- Preserves implementation speed and shared contracts in one codebase.
- Matches the requirement that LMU shared memory must be read locally on the user machine.
- Avoids shipping database credentials to end users.

Consequences:
- Add explicit runtime mode configuration (collector vs server) as implementation proceeds.
- Add ingestion endpoints for collector-to-server lap submission.
- Persistence secrets remain server-only; local runtime should work without DB credentials.

## 2026-05-04 - LMU Plugin Prerequisites Are Normalized At Startup

Status:
- accepted

Decision:
- On local LMU collector startup, validate and normalize the shared-memory plugin configuration by checking plugin presence, enabling the plugin entry, forcing `UnsubscribedBuffersMask = 0`, and forcing `EnableDirectMemoryAccess = 1`.

Why:
- LMU telemetry troubleshooting showed that local machine configuration drift was a real source of false-negative telemetry failures.
- The collector should be resilient to common plugin misconfiguration instead of relying on manual edits every time.

Consequences:
- Startup now mutates `CustomPluginVariables.JSON` when required settings are missing or incorrect.
- The project assumes this local file is safe to manage automatically on user machines running the collector role.
- Future plugin-related troubleshooting should inspect startup normalization logs first.

## 2026-05-04 - LMU Interop Layout Must Preserve Telemetry Offsets Exactly

Status:
- accepted

Decision:
- Treat the LMU scoring-to-telemetry memory boundary as offset-sensitive and preserve the effective 12-byte scoring stream size region in the interop layout to avoid telemetry block drift.

Why:
- We reached a state where LMU was clearly connected and in realtime, but `activeVehicles` remained `0` until the scoring layout was corrected.
- This indicated a real interop alignment problem rather than only a plugin configuration problem.

Consequences:
- Shared-memory layout fixes must be validated not only by struct-size tests but by verifying downstream telemetry fields stay sane.
- Changes near `SharedMemoryScoringData` and `SharedMemoryTelemetryData` should be treated as high-risk LMU interop work.

## 2026-05-04 - Live Telemetry Console Uses A Single Updating Status Row

Status:
- accepted

Decision:
- Render live telemetry verification as a single updating console row instead of sequential log lines.

Why:
- Sequential once-per-second telemetry lines quickly became unreadable during live driving verification.
- The main purpose of this console output is live operator feedback, not long-term log retention.

Consequences:
- Important startup and warning messages should remain normal logs, while the live telemetry feed behaves like a status display.
- Console rendering must account for terminal width so the status row does not wrap into multiple physical lines.
