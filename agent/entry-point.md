# Agent Entry Point

Before performing any task:

1. Read the following files:
   - `/agent/README.md`
   - `/agent/specs/product.md`
   - `/agent/specs/plan.md`
   - `/agent/workflow.md`
   - `/agent/rules.md`
   - `/agent/progress.md`
   - `/agent/decisions.md`
   - `/agent/iterations.md`
   - `/agent/skills/README.md`
2. Use these files as the source of truth.
3. Do not proceed if these files have not been read.
4. If any instruction conflicts with these files:
   - follow these files
   - explicitly call out the conflict
5. From `/agent/skills/README.md`, identify any project-specific skills that match the task.
6. Read and use every relevant project-specific skill before or during the task.
7. Do not ignore a relevant skill without a concrete reason.
8. Always align implementation with:
   - Vertical Slice Architecture
   - CQRS
   - project constraints defined in `product.md`

## Practical Interpretation

- `agent/README.md` explains the agent-facing folder structure and how the context files relate to one another.
- `agent/specs/product.md` defines the product and architectural intent.
- `agent/specs/plan.md` defines the stable execution-plan entry path.
- `agent/workflow.md` defines how work should be broken down and validated.
- `agent/rules.md` defines guardrails that should not be casually violated.
- `agent/progress.md` defines the current delivery snapshot.
- `agent/decisions.md` preserves architectural reasoning and prior choices.
- `agent/iterations.md` explains how the project evolved and what earlier approaches got wrong.
- `agent/skills/README.md` is the index for project-specific skills that should be used whenever they materially improve correctness, speed, or consistency.

## Working Expectations

- Prefer incremental, phase-based execution.
- Keep the app runnable after every change.
- Use the LMU SDK headers as the source of truth for LMU integration work.
- When in doubt, reduce scope rather than expand it.
- Use a skill every time it makes sense to do so.
- If a relevant skill is not used, state the reason clearly.
- Follow ticket and branch naming conventions defined in `agent/rules.md` (prefixes like `Feature/`, `Bug/`, `Spike/`, etc).
- Update `agent/decisions.md` when a meaningful decision is made.
- Update `agent/progress.md` when delivery status changes.
- Keep references aligned with the real repository structure; do not point to files or folders that do not exist.
