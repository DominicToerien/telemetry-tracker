# Repository Rules

Universal engineering constraints live in [AGENTS.md](../AGENTS.md). This file owns repository workflow and naming rules.

## Change isolation

- Use a dedicated git worktree for every chat that changes repository files.
- Create new implementation branches and worktrees from `main`, not from another unfinished feature branch.
- If work has a Linear issue, use that issue's branch and include its key in the branch name.
- Keep a branch-agnostic PM chat focused on planning and coordination; move implementation to an issue worktree.
- Merge `main` into a long-running task branch when necessary instead of starting a replacement branch from unfinished work.
- Before transferring active implementation between sessions, agents, or computers, use [the session handoff skill](skills/session-handoff/SKILL.md).
- Preserve essential cross-session work in signed, pushed commits and a current draft-PR handoff. Do not rely on a local transcript or uncommitted diff as its only copy.

## Ticket and branch names

Start every ticket title and branch name with one of:

- `Feature/`
- `Bug/`
- `Spike/`
- `Chore/`
- `Refactor/`
- `Docs/`
- `Test/`

Ticket format: `<Type>/<Concise outcome>`

Branch format:

- `<Type>/<linear-key>-<kebab-title>` when an issue key exists
- `<Type>/<kebab-title>` otherwise

Keep titles specific and branch slugs short. Examples:

- `Feature/Live Console Telemetry Output`
- `Feature/RE-12-live-console-telemetry-output`
- `Bug/RE-14-fix-disconnected-status`

If the user does not specify a type, select the best fit and state the choice.

## Repository hygiene

- Keep task-specific handoff state in the draft PR, not in `agent/current.md`.
- Update current state or durable decisions only when their owned information actually changes.
- Keep references aligned with real repository paths and use relative Markdown links.
- Do not claim documentation automation exists beyond checks actually present in the repository.
