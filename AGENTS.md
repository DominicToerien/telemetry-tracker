# Telemetry Tracker Agent Guide

This file is the canonical starting point for AI-assisted work in this repository. Keep it short: it routes agents to deeper context instead of duplicating that context.

## Instruction precedence

Follow platform and safety instructions first, then the user's explicit request, this file, the nearest directory-specific `AGENTS.md`, and finally linked project documentation. If project documents disagree, report the conflict and prefer the document identified as canonical by [the documentation map](agent/README.md).

## Minimal startup context

Before changing or reviewing the repository, read:

- [agent/current.md](agent/current.md) for the verified implementation state and next bounded slice.
- [agent/rules.md](agent/rules.md) for repository guardrails.

Do not load every file under `agent/`. Use the routing table below and inspect code before assuming documentation is current.

## Task-specific context

| When the task concerns | Read |
|---|---|
| Any repository change | [change workflow](agent/skills/change-workflow/SKILL.md) |
| Product scope, feature behavior, or roadmap | [product spec](agent/specs/product.md), then [roadmap](agent/plans/roadmap.md) when sequencing matters |
| Runtime, deployment, transport, Docker, or MCP | [architecture spec](agent/specs/architecture.md) |
| Implementing or planning the current slice | [current plan](agent/plans/current.md) and [workflow](agent/workflow.md) |
| Historical architectural reasoning | [decision index](agent/decisions/README.md), then only the relevant entry in [the decision archive](agent/decisions.md) |
| LMU telemetry or interop | [LMU integrity guide](agent/skills/lmu-integrity.md) and the relevant files in `references/lmu-sdks/` |
| Tests | [testing guide](agent/skills/testing.md) |
| A feature slice or CQRS structure | [vertical-slice guide](agent/skills/vertical-slice.md) |
| Planning-only project management | [PM chat guide](agent/skills/pm-chat.md) |
| Transferring unfinished implementation | [session handoff skill](agent/skills/session-handoff/SKILL.md) |
| Updating project state or decisions | [documentation map](agent/README.md) and the relevant maintenance guide in `agent/skills/` |

The [skills index](agent/skills/README.md) lists all optional project guides. Read only those relevant to the task.

## Universal engineering rules

- Keep the application runnable and stay within the requested slice.
- Organize new behavior by feature using explicit commands and queries; keep adapters thin.
- Do not invent LMU structures or behavior. Verify LMU work against the checked-in SDK headers.
- Keep Windows interop inside the LMU integration boundary.
- Do not persist raw telemetry continuously; retain bounded lap-level summaries and sampled traces while tracking is active.
- Preserve console visibility for live telemetry verification and use deterministic telemetry sources when tests do not need LMU.
- The native client remains a standalone local Generic Host application. Do not add ASP.NET hosting, hosted persistence, or Docker to it.
- Keep secrets server-side when the separate hosted phase is introduced.
- Prefer small, testable implementations over speculative abstractions or infrastructure.

## Common commands

```bash
dotnet build telemetry-tracker.sln
dotnet test telemetry-tracker.Tests/telemetry-tracker.Tests.csproj
python3 scripts/validate_docs.py
```

Live LMU integration requires Windows. Linux is suitable for builds, unit tests, documentation checks, and mock or recorded telemetry development.

## Documentation maintenance

Update documentation in the same change only when its owned information changes:

| Change | Update |
|---|---|
| Verified implementation state, active slice, or blocker | `agent/current.md` |
| Current slice scope or acceptance criteria | `agent/plans/current.md` |
| Future sequencing | `agent/plans/roadmap.md` |
| Product behavior or durable constraints | `agent/specs/product.md` |
| Runtime or deployment boundary | `agent/specs/architecture.md` and a decision entry |
| Durable architectural choice | `agent/decisions.md` plus its status in `agent/decisions/README.md` |
| Agent workflow or universal rule | this file, `agent/workflow.md`, or `agent/rules.md`, according to ownership |

Do not update status documents for formatting-only changes. Prefer links to canonical information over copying it into multiple files. Run the documentation validator after modifying Markdown.

Do not add process artifacts or request confirmation when the user has already authorized a bounded change.
