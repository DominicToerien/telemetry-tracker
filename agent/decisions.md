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

## 2026-05-05 - Standardize Supabase .env Loading On ASP.NET ConnectionStrings Convention

Status:
- accepted

Decision:
- Replace the custom `.env` parser with `DotNetEnv` and standardize local database configuration on `ConnectionStrings__Supabase`.

Why:
- Keeps startup simpler and closer to common ASP.NET configuration patterns.
- Removes ambiguity between multiple possible connection-string variable names.
- Makes `.env` behavior line up with `configuration.GetConnectionString("Supabase")`.

Consequences:
- Local `.env` setup for persistence should use `ConnectionStrings__Supabase` only.
- The previous custom-only fallback key `SUPABASE_CONNECTION_STRING` is no longer part of the supported configuration path.
- LMU and telemetry startup behavior remain independent of persistence because DbContext registration is still conditional.

## 2026-05-05 - EF Core Migrations Run Automatically On Startup When Persistence Is Configured

Status:
- accepted

Decision:
- Resolve `TelemetryTrackerDbContext` during startup and call `dbContext.Database.Migrate()` automatically when the DbContext is registered.

Why:
- Removes manual migration steps during development and deployment.
- Keeps persistence setup closer to zero-touch once a valid connection string is present.
- Still preserves the current telemetry bring-up path because migration only runs when the DbContext exists.

Consequences:
- Hosted/server environments can apply pending EF Core migrations on boot automatically.
- Local collector or misconfigured environments still start because missing DbContext registration skips the migration path entirely.
- Migration failures are logged and do not block application startup, which favors telemetry availability over hard-failing on persistence startup problems at this stage.

## 2026-05-06 - Read Models Are Projected Asynchronously Behind Commands

Status:
- accepted

Decision:
- Future CQRS write flows should persist write-side data first, then hand read-model creation or refresh work to a background worker instead of blocking the API response.
- Redis, if later introduced, should sit on top of the read side as a cache and should not replace projected read models or write-side persistence.

Why:
- Preserves fast and predictable write endpoint latency.
- Fits the existing single-process ASP.NET Core architecture that already allows hosted background services.
- Keeps the future CQRS path explicit so contributors do not accidentally couple command completion to read-model materialization.

Consequences:
- Future command handlers should be able to complete successfully before projection work finishes.
- Read-side freshness becomes eventually consistent once projected views are introduced.
- The first implementation should favor simple in-process coordination before any heavier durability pattern is justified.

## 2026-05-06 - Parallel Agent Work Uses Git Worktrees With Workspace-Specific Runtime Settings

Status:
- accepted

Decision:
- Support parallel local AI chats or agents by using git worktrees instead of sharing one branch checkout.
- Give each worktree its own generated local runtime settings such as `ASPNETCORE_URLS` so multiple local instances can run at the same time without port collisions.

Why:
- Multiple agents working in one checkout would constantly fight over branch switches and local edits.
- The project is intended for incremental AI-assisted development, so safe parallel task isolation improves throughput without changing application architecture.
- Keeping the setup local and script-driven avoids introducing heavier infrastructure.

Consequences:
- Local developer workflow now includes worktree bootstrap and teardown scripts.
- Parallel local API runs should use workspace-specific runner scripts instead of shared launch-profile defaults.
- Local `.env` values can be copied into new worktrees for convenience, while remaining gitignored.

## 2026-05-06 - Product Planning Uses A Branch-Agnostic PM Chat

Status:
- accepted

Decision:
- Treat product and project planning as a separate branch-agnostic chat mode that prefers Notion, Linear, and GitHub over repository edits.
- Use dedicated issue worktrees only when work transitions from planning into implementation.

Why:
- Planning and implementation have different needs and different risk profiles.
- Keeping the PM chat branch-agnostic reduces accidental code changes in the wrong place while preserving a long-lived planning context.
- This fits the new worktree-first implementation model without forcing every planning conversation into a branch.

Consequences:
- The repository should document a clear planning-chat pattern alongside the implementation-chat worktree pattern.
- Planning chats should prepare implementation handoffs, but should not assume they can automatically create new top-level Codex chats.

## 2026-08-28 - Skills-First Local Terminal MVP

Status:
- accepted

Decision:
- Make a Codex/Claude-style local CLI/TUI the primary MVP surface.
- Treat sessions as directory-like contexts and laps as file-like items within them.
- Use stable machine-readable CLI operations plus Codex/Claude skills as the initial AI experience.
- Keep skills supported when native bring-your-own-key chat, MCP, hosted APIs, synchronization, and a graphical frontend are added later.
- Make setup creation a guided, versioned proposal workflow that requires driver feedback and explicit confirmation before export or application.

Why:
- The terminal workspace creates an immediately usable product instead of requiring users to call backend endpoints.
- Skills provide coaching and setup workflows sooner by using an existing agent experience.
- Stable application commands prevent the skills MVP from becoming throwaway work and provide a foundation for later adapters.
- Local-first operation matches LMU shared-memory constraints and avoids premature hosted infrastructure.

Alternatives considered:
- Continue with an HTTP API and Supabase as the primary MVP surface.
- Build native AI chat before exposing deterministic CLI operations.
- Make MCP the first integration layer.

Tradeoffs:
- Terminal interaction and local multi-client coordination become earlier engineering priorities.
- Hosted ingestion, Supabase-first persistence, `/ask`, and a graphical frontend move to later phases.
- Skills depend on a stable structured CLI and cannot replace deterministic access to telemetry data.

Supersedes:
- Refines `Single Process ASP.NET Core API` and `Single Codebase With Collector And Hosted Server Roles` by moving them out of the primary MVP experience; their reusable foundations and later hosted direction remain valid.

## 2026-08-29 - In-Memory Tracking Core Before Local Persistence

Status:
- accepted

Decision:
- Implement the Phase 1 tracking lifecycle, sampling, lap-boundary detection, trace construction, and summary calculation entirely in memory.
- Keep capture input source-agnostic through a compact telemetry-frame contract, with the existing LMU reader acting only as an adapter.

Why:
- The CLI/TUI and local persistence phases both need stable capture behaviour, but neither should determine how telemetry is sampled or when a lap is complete.

Consequences:
- Stopping tracking deliberately discards a partial lap until persistence semantics are introduced.

## 2026-08-29 - LMU Setup Examples Are Non-Authoritative Format Fixtures

Status:
- accepted

Decision:
- Keep the salvaged `.svm` example as a renamed, documented format fixture.
- Treat setup files as car-specific source artifacts whose unknown fields, comments, and ordering should be preserved until real round-trip behaviour is validated.
- Require representative fixtures and LMU validation before implementing setup export.

Why:
- The available example comes from a ModDev pace-car path, contains mostly commented defaults, and cannot define the setup contract for every LMU car.
- `/create-setup` needs concrete file-shape context, but unsafe generalization would produce invalid or misleading setup changes.

Alternatives considered:
- Merge the old RE-9 documentation unchanged.
- Discard the setup example entirely.
- Treat the example as a universal setup schema.

Tradeoffs:
- Parser and writer development needs more representative fixtures before setup export is safe.
- The current fixture remains useful for lossless parsing, storage, and versioning experiments.

## 2026-08-29 - Cross-Session Work Uses Branch And Draft PR Handoffs

Status:
- accepted

Decision:
- Make signed, pushed branch commits plus a draft pull request and structured `## HANDOFF` state the canonical transfer mechanism between Codex sessions, agents, and computers.
- Keep project-wide progress and decisions in the existing durable agent documents while keeping temporary task state in the draft PR.

Why:
- Local transcripts, worktrees, and uncommitted changes are not reliably available on another computer.
- A branch and draft PR provide inspectable code, task context, validation status, and a durable recovery point without polluting long-lived project documentation with ephemeral notes.

Alternatives considered:
- Depend on Codex session history alone.
- Store one shared handoff file in the repository root.
- Require work to be complete before it can move between environments.

Tradeoffs:
- Incomplete but valuable work may use explicitly labelled signed WIP commits.
- Contributors must keep the draft PR handoff current when work pauses.
- A receiving agent must still verify the branch and validation results rather than trusting the handoff text blindly.

## 2026-08-30 - Native Windows MVP And Separate Hosted Platform

Status:
- accepted

Decision:
- Install and run the human-facing LMU collector, local telemetry host, persistence, setup management, CLI/TUI, and skills natively on the Windows machine running LMU.
- Do not containerize the live LMU-facing MVP.
- Add Docker only beside a concrete optional later hosted API/platform; keep the current native client free of container tooling.
- Use authenticated HTTPS for native-to-hosted synchronization, HTTP/WebSocket APIs for the web frontend, and MCP for agent-facing tools.

Why:
- LMU integration depends on host Windows shared-memory objects and local setup files.
- The driving loop must work offline and remain independent of Docker, hosted services, and network availability.
- Explicit transport responsibilities prevent MCP, frontend APIs, and synchronization from becoming conflated.

Alternatives considered:
- Run the local collector in the existing Linux Docker image.
- Use a Windows container with mounted setup directories.
- Require the hosted platform for live telemetry and local product use.

Tradeoffs:
- The project needs a native Windows packaging and update path.
- The later server role should gain an explicit entry point or project before its Docker deployment matures.
- Shared application contracts remain useful, but native and hosted secrets, adapters, and runtime dependencies must stay separated.

## 2026-08-30 - Native Client Is A Standalone Generic Host Application

Status:
- accepted

Decision:
- Convert the current executable from an ASP.NET Core Web application to a standalone .NET Generic Host console application.
- Remove HTTP endpoints, Swagger, authorization middleware, Docker tooling, Supabase/Npgsql/EF migrations, and server-oriented `.env` configuration from the native client.
- Preserve application query handlers and LMU hosted-service behavior for future CLI/TUI adapters.
- Introduce ASP.NET Core, hosted persistence, and a server-specific Dockerfile only in a separate server project when the hosted phase is active.

Why:
- The native client reads LMU shared memory and local setup files and must present an unambiguous offline human-facing CLI/TUI product.
- Calling a future hosted API requires `HttpClient`, not an embedded HTTP server.
- Dormant server infrastructure in the client obscures security, deployment, and ownership boundaries.

Alternatives considered:
- Keep one ASP.NET executable with collector/server runtime modes.
- Retain unused Docker and Supabase scaffolding until the hosted phase.
- Keep HTTP status endpoints as local bring-up interfaces.

Tradeoffs:
- The existing HTTP status endpoint and its integration test are removed; equivalent CLI commands will be implemented in the CLI phase.
- Hosted server scaffolding will need to be created later in its own project.
- The current live console remains a bring-up experience rather than the final interactive TUI.

Supersedes:
- `Single Process ASP.NET Core API`
- `Single Codebase With Collector And Hosted Server Roles`
- `Supabase DbContext Uses .env Configuration With Conditional Registration`
- `Standardize Supabase .env Loading On ASP.NET ConnectionStrings Convention`
- `EF Core Migrations Run Automatically On Startup When Persistence Is Configured`
