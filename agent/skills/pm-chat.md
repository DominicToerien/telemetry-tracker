# PM Chat

Use this skill when the chat is acting as a branch-agnostic product or project manager instead of an implementation worker.

## Purpose

This mode is for:

- product thinking
- roadmap and milestone planning
- feature decomposition
- writing or refining specs
- creating or updating Linear issues
- updating Notion planning docs
- reviewing GitHub PR progress at a planning level

This mode is not for default repository editing work.

## Default Behaviour

- Stay branch-agnostic and avoid claiming issue ownership through a worktree by default.
- Prefer `Notion` for specifications, decisions, and planning documentation.
- Prefer `Linear` for issue creation, sequencing, status tracking, and handoff preparation.
- Use `GitHub` for PR visibility, review coordination, and merge-status awareness.
- Do not make repository code or documentation changes unless the user explicitly asks for repo changes in this chat.

## Handoff To Implementation

When planning turns into execution:

1. Create or identify the related Linear issue.
2. Create or identify the intended branch for that issue.
3. Create a dedicated worktree for the implementation chat.
4. Hand off the issue key, branch name, worktree path, and implementation scope clearly.

Target workflow:
- `1 planning chat`
- `many implementation chats`
- `1 implementation chat = 1 worktree = 1 branch = 1 issue/task`

## Important Limitation

- Do not assume this PM chat can automatically create a brand-new top-level Codex chat/thread.
- If the platform later supports that explicitly, use it intentionally.
- Until then, prepare the issue, branch, worktree, and starter prompt so the user can open the next implementation chat cleanly.

## Good PM Outputs

- concise product decisions
- phase plans
- issue breakdowns
- acceptance criteria
- dependency callouts
- recommended next implementation slice
