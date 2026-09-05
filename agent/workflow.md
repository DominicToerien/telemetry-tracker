# AI-Assisted Development Workflow

## Philosophy

Use the least ceremony that prevents expensive misunderstanding. The agent should investigate facts and make reversible implementation choices autonomously; the user should decide product and architectural trade-offs that materially affect the result.

## Everyday Change Loop

Use `agent/skills/change-workflow/SKILL.md` for repository changes:

1. Inspect the relevant context, code, tests, and authoritative references.
2. Route the request to direct implementation, a compact contract, or a focused grill.
3. Implement the smallest complete vertical slice after the scope is settled.
4. Verify both engineering correctness and conformance to the agreed contract.
5. Report the working outcome, validation, limitations, and any useful next slice.

An explicit request to implement is enough to proceed when the outcome is bounded. Do not introduce an approval ceremony for routine work. Grill one material decision at a time only when a wrong assumption could significantly change behaviour, architecture, data contracts, user workflow, or scope.

Keep ordinary working contracts in the conversation, issue, or pull request. Create additional specification artifacts only when work spans sessions, contains several independently reviewable behaviours, or needs durable asynchronous review.

### How To Invoke It

- `Implement <bounded change>`: route automatically and proceed without redundant approval when the scope is clear.
- `Grill me about <idea>`: force the grill path; ask one material decision at a time and do not implement until the resulting contract is approved.
- `Explore <idea>`: investigate options and trade-offs without creating files or implementing.
- `Implement the approved contract`: execute the settled scope and verify against it.

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

### 1.7. Make Active Work Transferable

- Push the task branch early enough that the work is not known only to one computer.
- Open a draft pull request when implementation begins or as soon as the first viable commit exists.
- Before pausing, switching sessions, changing agents, or changing computers, use `agent/skills/session-handoff/SKILL.md`.
- Keep the latest `## HANDOFF` state in the draft PR and keep essential implementation work in signed, pushed commits.
- Treat local Codex transcripts as helpful context, not as the canonical project record.

### 2. Generate Execution Plan

- Create or refine `agent/plans/current.md` when the active slice changes or needs a durable, asynchronously reviewable contract.
- For an ordinary bounded change, use the compact contract from the change workflow instead of creating another plan document.
- Update `agent/plans/roadmap.md` only when future sequencing changes.
- Use `agent/plan.md` only as a detailed historical design reference.
- Do not mirror the same task plan into multiple repository documents.

### 2.5. Maintain Decision and Progress Context

- Record durable architecture and scope decisions in `agent/decisions.md` and update `agent/decisions/README.md`.
- Keep the current delivery snapshot in `agent/current.md`.
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
- update `agent/current.md` if verified repository-level state changed
- update the decision log and index if a durable decision was made or revised
- update the draft PR handoff before transferring incomplete work to another session or environment
- verify the implementation against both repository standards and the agreed change contract

### 5. Introduce Real Integrations Last

- preserve the working LMU integration as the real telemetry foundation
- use deterministic mocks when they materially improve isolated development and repeatable tests
- prove the local tracking, session, lap, and skill-facing CLI flow before adding native LLM, MCP, hosted synchronization, or frontend integrations
- keep hosted database credentials server-side when hosted persistence is introduced

### 6. Validate with Real Use Cases

- use actual CLI/TUI commands and stable JSON output
- run realistic skill workflows such as lap comparison and guided setup creation
- verify outputs match expectations

## PM Chat Pattern

Use a dedicated planning chat when the goal is product management rather than implementation.

- Keep that chat focused on product decisions, specs, sequencing, and issue creation.
- Prefer `Notion`, `Linear`, and `GitHub` over repo edits.
- When work should move into implementation, prepare the issue, branch, worktree, and handoff prompt for a separate implementation chat.
- For an existing implementation, resume from its branch, draft PR, and latest `## HANDOFF` comment instead of creating a replacement task branch.
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
