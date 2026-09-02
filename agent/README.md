# Agent Documentation Map

The root [AGENTS.md](../AGENTS.md) is the canonical entry point. This directory stores durable context behind that progressive-disclosure router.

## Ownership

| Path | Owns | Load when |
|---|---|---|
| [current.md](current.md) | Verified implementation state, active slice, blockers | Every repository task |
| [rules.md](rules.md) | Repository workflow and naming guardrails | Every repository task |
| [plans/current.md](plans/current.md) | Current slice scope and acceptance criteria | Implementing or planning the active slice |
| [plans/roadmap.md](plans/roadmap.md) | Future sequence | Roadmap or product planning |
| [specs/product.md](specs/product.md) | Durable product behavior and constraints | Product or feature design |
| [specs/architecture.md](specs/architecture.md) | Runtime, deployment, transport, and ownership boundaries | Architectural work |
| [decisions/README.md](decisions/README.md) | Current decision status and historical lookup | A prior decision may affect the task |
| [decisions.md](decisions.md) | Full historical decision records | After selecting relevant entries from the index |
| [workflow.md](workflow.md) | Planning, implementation, and validation process | Implementation or delivery planning |
| [skills/README.md](skills/README.md) | Optional task-specific guides | Selecting a relevant project guide |
| [plan.md](plan.md) | Archived comprehensive design | A task needs design detail absent from current specs |
| [iterations.md](iterations.md) | Product evolution narrative | Historical research only |

Compatibility pointers [entry-point.md](entry-point.md), [progress.md](progress.md), and [specs/plan.md](specs/plan.md) remain for older prompts and direct links. They route to the canonical files and should not grow into parallel sources of truth.

## Maintenance rules

- Update only the document that owns the changed information.
- Link to canonical material instead of repeating it.
- Put temporary task state in the task's draft PR handoff, not long-lived project context.
- Add durable decisions to the historical log and update the compact decision index in the same change.
- Move completed plans to an archive rather than keeping them in startup context.
- Use repository-relative links so documentation works in GitHub, worktrees, Windows, and Linux.
- Run `python3 scripts/validate_docs.py` after changing Markdown.

These files are agent-maintained. Objective structure and link integrity are checked in CI; semantic freshness still requires review against the implementation.
