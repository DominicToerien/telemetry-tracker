---
name: session-handoff
description: Transfer or resume Telemetry Tracker implementation work between Codex sessions, agents, or computers. Use before pausing active work, changing environments, or continuing an existing branch or draft pull request.
---

# Session Handoff

Make the branch and draft pull request—not a local Codex transcript—the transferable record of implementation work.

## Before Handing Off

1. Inspect the worktree and separate intended changes from unrelated user work.
2. Run validation appropriate to the current state. Record failures honestly.
3. Commit viable work with a verified signature. An explicitly labelled signed `WIP` commit is acceptable when the task is incomplete.
4. Push the task branch.
5. Create or update its draft pull request.
6. Add a PR comment headed `## HANDOFF` using the repository pull-request template fields.
7. Update `agent/progress.md` only when repository-level delivery state changed.
8. Update `agent/decisions.md` only when a durable decision was made.

Do not leave essential work only in an uncommitted diff, local worktree, or chat transcript. Do not claim incomplete or failing work is ready for review.

The handoff must identify:

- task objective, issue, branch, and base branch;
- completed and in-progress work;
- remaining scope and exact next action;
- files or feature slices currently being changed;
- validation commands and their results;
- decisions, assumptions, blockers, and known risks;
- whether the worktree is clean and whether the latest commit is pushed.

## When Resuming

1. Read `agent/entry-point.md` and its required project context.
2. Read the task issue, draft PR description, and latest `## HANDOFF` comment.
3. Fetch the branch and create or attach a dedicated worktree.
4. Inspect the real commit history, diff, and worktree state; do not trust the handoff without verification.
5. Compare the branch with current `main`. Merge `main` into a long-running task branch when needed; do not start a replacement branch from the unfinished branch.
6. Re-run the stated validation before continuing when practical.
7. Continue only the documented remaining scope unless the user explicitly changes it.

If the remote branch or handoff comment does not exist, report that cross-machine recovery is limited. Ask for the other environment to push a signed commit and post the handoff rather than reconstructing unknown work.

## Completion

When the task is ready, replace temporary WIP language with current validation status, move the PR out of draft when appropriate, and remove obsolete handoff state after merge only when it no longer carries useful recovery information.
