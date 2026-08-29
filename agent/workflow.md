# AI-Assisted Development Workflow

## Philosophy

AI is treated as a junior engineer that requires:

- clear specifications
- tight constraints
- incremental guidance
- continuous validation

The goal is not to generate code, but to accelerate structured problem solving.

## Core Workflow

### 1. Define Product Context

- Write or update `agent/specs/product.md`
- Clearly define architecture, constraints, goals, and current scope

If the chat is intended to be planning-only, use the `agent/skills/pm-chat.md` skill and keep the chat branch-agnostic by default.

### 1.5. Confirm Naming Conventions

- Before creating Linear tickets or git branches, follow the naming rules in `agent/rules.md`.
- Tickets and branches must start with `Feature/`, `Bug/`, `Spike/`, etc (see `agent/rules.md` for the allowed set and formats).

### 1.6. Isolate The Workspace

- For any task that will change repository files, create or use a dedicated git worktree for that chat.
- If the task belongs to a Linear issue, use the branch associated with that issue inside the worktree.
- Create new issue branches and worktrees from `main` before implementation begins.
- Prefer merging `main` into a long-running branch over branching new work from another unfinished branch.
- Treat the main checkout as coordination space, not the default place for parallel implementation work.

### 2. Generate Execution Plan

- Ask AI to create or refine `agent/plan.md`
- Mirror a concise execution-facing version in `agent/specs/plan.md` when useful
- Review and refine the plan before implementation

### 2.5. Maintain Decision and Progress Context

- Record meaningful architecture and scope decisions in `agent/decisions.md`
- Keep the current delivery snapshot in `agent/progress.md`
- Update both in the same task when relevant changes are made

### 3. Execute in Vertical Slices

For each phase:

- instruct AI to implement only one phase
- do not allow skipping ahead
- ensure each slice is complete and testable
- keep each active chat isolated to its own worktree and branch while implementing

Example instruction:

> Implement Phase 2 only. Do not proceed further. Summarise changes and how to test.

### 4. Review and Refine

After each step:

- inspect generated code
- simplify where necessary
- remove unnecessary abstractions
- ensure alignment with architecture
- update `agent/progress.md` if the current state changed
- update `agent/decisions.md` if a meaningful decision was made or revised

### 5. Introduce Real Integrations Last

- preserve the working LMU integration as the real telemetry foundation
- use deterministic mocks when they materially improve isolated development and repeatable tests
- prove the local tracking, session, lap, and skill-facing CLI flow before adding native LLM, MCP, hosted synchronization, or frontend integrations
- keep hosted database credentials server-side when hosted persistence is introduced

### 6. Validate with Real Use Cases

- use actual CLI/TUI commands and stable JSON output
- exercise the current HTTP status/debug endpoints only as implementation bring-up surfaces
- run realistic skill workflows such as lap comparison and guided setup creation
- verify outputs match expectations

## PM Chat Pattern

Use a dedicated planning chat when the goal is product management rather than implementation.

- Keep that chat focused on product decisions, specs, sequencing, and issue creation.
- Prefer `Notion`, `Linear`, and `GitHub` over repo edits.
- When work should move into implementation, prepare the issue, branch, worktree, and handoff prompt for a separate implementation chat.
- Do not assume a new top-level Codex chat can be created automatically unless the platform explicitly supports it.

## Key Principles

- small steps beat large generations
- explicit constraints beat vague instructions
- structure first, implementation second
- mock, then validate, then integrate
- always keep the system runnable

## Common Failure Modes

### Over-engineering

Avoid by:

- enforcing vertical slices
- avoiding generic abstractions
- delaying infrastructure until the current slice needs it

### AI hallucination, especially LMU data

Avoid by:

- forcing reference to `references/lmu-sdks/`
- refusing to invent telemetry fields or layouts
- checking headers before changing LMU interop code

### Loss of control over scope

Avoid by:

- phase-based execution
- explicit instructions such as do not proceed further
- keeping plans and slices decision-complete before coding

### Poor demo quality

Avoid by:

- continuous console output
- a usable session/lap terminal workflow
- realistic CLI commands, skill questions, and setup proposals
- preserving a runnable app at all times

## Outcome

This workflow is intended to produce:

- high-quality, structured code
- fast iteration cycles
- strong alignment between design and implementation
- demonstrable engineering thinking, not just tool usage
- preserved reasoning and status context across iterations
