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
