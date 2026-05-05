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

### 1.5. Confirm Naming Conventions

- Before creating Linear tickets or git branches, follow the naming rules in `agent/rules.md`.
- Tickets and branches must start with `Feature/`, `Bug/`, `Spike/`, etc (see `agent/rules.md` for the allowed set and formats).

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

- always build against mocks first
- only integrate external systems such as LMU, database, and LLM once the flow works end-to-end
- when integrating real persistence, keep DB access on hosted server side and use collector-to-server ingest from local clients

### 6. Validate with Real Use Cases

- use actual API calls
- ask realistic questions
- verify outputs match expectations

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
- API-first design
- realistic example queries
- preserving a runnable app at all times

## Outcome

This workflow is intended to produce:

- high-quality, structured code
- fast iteration cycles
- strong alignment between design and implementation
- demonstrable engineering thinking, not just tool usage
- preserved reasoning and status context across iterations
