# Agent Rules

- Do not implement beyond the requested phase.
- Do not invent LMU telemetry structures.
- Always inspect `references/lmu-sdks/` before LMU work.
- Keep the app runnable after every change.
- Prefer simple implementations over abstractions.
- Do not introduce new infrastructure unless requested.
- Preserve the working LMU integration as the real telemetry foundation; use `MockTelemetrySource` where deterministic development or tests need it.
- Do not persist raw telemetry continuously.
- Follow Vertical Slice Architecture plus CQRS.
- Keep Supabase/Postgres credentials server-side only; do not require DB secrets in local collector runtime.
- Do not add ASP.NET hosting, HTTP endpoints, hosted persistence migrations, or Docker tooling to the native client; introduce them in a separate server project when the hosted phase begins.
- Update `agent/decisions.md` when a meaningful architectural or scope decision is made.
- Update `agent/progress.md` when project status meaningfully changes.
- Use a dedicated git worktree for every chat that makes repository changes.
- If work is associated with a Linear issue, the worktree must be on that issue's branch.
- Do not implement repository changes from a shared checkout when the task should have its own issue/branch.
- Every new issue branch and implementation worktree must be created from `main`, not from another feature branch.
- Pull or merge `main` forward into long-running branches to stay current instead of branching new work from in-progress branches.
- A branch-agnostic PM chat should default to planning, issue management, and documentation coordination rather than repository edits.
- A PM chat may stay on `main` or another coordination checkout, but implementation work should move to a dedicated issue worktree.
- Before active implementation moves to another session, agent, or computer, use `agent/skills/session-handoff/SKILL.md`.
- Essential cross-session work must be committed with a verified signature, pushed, and described in the task's draft PR; do not rely on an uncommitted diff or local transcript as the only copy.

## Naming Conventions (Tickets and Branches)

These rules apply whenever you ask me to create:
- a Linear ticket
- a git branch

### Allowed Prefixes

Every ticket title and every branch name must start with one of:
- `Feature/`
- `Bug/`
- `Spike/`
- `Chore/`
- `Refactor/`
- `Docs/`
- `Test/`

If you do not specify a type, I will choose the best match and I will call out the choice in the response.

### Ticket Naming Rule (Linear)

Format:
- `<Type>/<ConciseTitle>`

Guidelines:
- Keep `<ConciseTitle>` short and specific (avoid vague titles like "Telemetry work").
- Include the bounded outcome and the relevant area (examples: `Console`, `LMU`, `Tracking`, `Supabase`, `AI`).
- Avoid punctuation that makes searching hard; use words instead of symbols when possible.

Examples:
- `Feature/Live Console Telemetry Output`
- `Spike/Investigate LMU XML Results Logs`
- `Bug/Telemetry Status Shows Connected When LMU Is Closed`

### Branch Naming Rule (Git)

Format:
- `<Type>/<linearKey>-<kebab-title>` when a Linear issue key exists (preferred)
- `<Type>/<kebab-title>` when no Linear key exists

Guidelines:
- `kebab-title` is lowercase, words separated by `-`.
- Keep it short (ideally 3-8 words).
- If a ticket exists, always include its key in the branch name.

Examples:
- `Feature/RE-12-live-console-telemetry-output`
- `Spike/RE-13-investigate-lmu-xml-results-logs`
- `Bug/RE-14-fix-disconnected-status-on-startup`

## Additional Working Rules

- Keep CLI/TUI commands and skill adapters thin; keep later MCP/HTTP adapters in their owning projects.
- Treat stable machine-readable CLI operations as the first AI integration boundary.
- Keep skills supported when native AI, MCP, hosted APIs, synchronization, and a frontend are added later.
- Prefer feature-local handlers and data access over large shared service layers.
- Add tests where logic is meaningful, not as ceremony.
- Preserve console visibility for live telemetry verification.
- If a new idea increases complexity without helping the current phase, defer it.
- Do not claim repository automation exists for these agent files unless it has actually been implemented.
- Before declaring a branch ready, inspect review and discussion comments on every open pull request, assess their applicability, and run an independent diff-based review. Record any material outcome in the draft PR handoff.
