# Decision Index

Use this index to find the relevant architectural decision without loading the entire historical log in [../decisions.md](../decisions.md).

## Current decisions

| Date | Decision | Status |
|---|---|---|
| 2026-05-04 | Vertical Slice Architecture plus CQRS | Accepted |
| 2026-05-04 | Lap-based persistence instead of raw continuous storage | Accepted |
| 2026-05-04 | LMU SDK headers are the source of truth | Accepted |
| 2026-05-04 | Consolidated agent documentation structure | Revised by this progressive-disclosure structure |
| 2026-05-04 | Telemetry status refactored into a vertical slice | Revised; handlers remain and HTTP surface is superseded |
| 2026-05-04 | LMU plugin prerequisites are normalized at startup | Accepted |
| 2026-05-04 | LMU interop layout preserves telemetry offsets exactly | Accepted |
| 2026-05-04 | Live telemetry uses an updating console display | Revised to allow width-aware wrapping |
| 2026-05-06 | Read models may be projected asynchronously behind hosted commands | Deferred hosted guidance |
| 2026-05-06 | Parallel agent work uses Git worktrees | Revised; isolation remains and API port allocation is superseded |
| 2026-05-06 | Product planning uses a branch-agnostic PM chat | Accepted |
| 2026-08-28 | Skills-first local terminal MVP | Accepted |
| 2026-08-29 | LMU setup examples are non-authoritative fixtures | Accepted |
| 2026-08-29 | Cross-session work uses branch and draft-PR handoffs | Accepted |
| 2026-08-30 | Native Windows MVP and separate hosted platform | Accepted |
| 2026-08-30 | Native client is a standalone Generic Host application | Accepted |
| 2026-08-30 | Stable CLI is the recorded telemetry integration boundary | Accepted |
| 2026-08-30 | Abstain from LMU setup generation without car-specific validation | Accepted |
| 2026-08-30 | LMU baselines are lossless, car-identified source artifacts | Accepted |
| 2026-08-31 | Progressive disclosure for agent context | Accepted |

## Superseded decisions

These entries are retained only for historical reasoning:

- Single Process ASP.NET Core API
- Supabase DbContext using local `.env` configuration
- Standardized Supabase connection-string loading
- Automatic EF Core migrations in the native process
- Single codebase with collector and hosted-server roles

They are superseded by **Native Client Is A Standalone Generic Host Application**. Do not use them as current implementation guidance.

## Adding a decision

Add the full entry to `agent/decisions.md`, then add or update its row here. Mark replaced entries `superseded` in both places and link the replacement by title. Keep implementation details out of this index.
