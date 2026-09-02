# Agent Skills

This folder contains project-specific agent skills.

These are lightweight operational guides the agent should use when they match the task.

## Available Skills

- [context-loading.md](context-loading.md)
  Load the minimal baseline and route to task-specific context.
- [decision-logging.md](decision-logging.md)
  Update architectural decisions when meaningful choices are made.
- [lmu-integrity.md](lmu-integrity.md)
  Preserve LMU SDK correctness and avoid invented telemetry assumptions.
- [pm-chat.md](pm-chat.md)
  Run a branch-agnostic planning and project-management workflow using Notion, Linear, and GitHub before implementation begins.
- [phase-execution.md](phase-execution.md)
  Stay within the requested phase or slice.
- [progress-tracking.md](progress-tracking.md)
  Keep project progress current when delivery status changes.
- [session-handoff/SKILL.md](./session-handoff/SKILL.md)
  Transfer or resume implementation work safely across Codex sessions, agents, and computers using pushed branches and draft pull requests.
- [testing.md](testing.md)
  Add focused tests when logic merits them.
- [vertical-slice.md](vertical-slice.md)
  Implement features in a Vertical Slice Architecture style.

## Usage

- Use the routing table in the root `AGENTS.md` to select a skill.
- Select and use relevant skills whenever they clearly fit the task.
- Do not force every skill on every task; choose the ones that materially improve correctness, consistency, or workflow discipline.
- Use the session-handoff skill when pausing active implementation for another session, agent, or computer, or when resuming handed-off work.
