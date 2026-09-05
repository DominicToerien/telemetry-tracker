# Agent Skills

This folder contains project-specific agent skills.

These are lightweight operational guides the agent should use when they match the task.

## Available Skills

- [change-workflow/SKILL.md](./change-workflow/SKILL.md)
  Route implementation work through the lightest useful path: direct execution, a compact contract, or a focused grill for material unresolved decisions.
- [context-loading.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/skills/context-loading.md)
  Read the required project context before starting work.
- [decision-logging.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/skills/decision-logging.md)
  Update architectural decisions when meaningful choices are made.
- [lmu-integrity.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/skills/lmu-integrity.md)
  Preserve LMU SDK correctness and avoid invented telemetry assumptions.
- [pm-chat.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/skills/pm-chat.md)
  Run a branch-agnostic planning and project-management workflow using Notion, Linear, and GitHub before implementation begins.
- [phase-execution.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/skills/phase-execution.md)
  Stay within the requested phase or slice.
- [progress-tracking.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/skills/progress-tracking.md)
  Keep project progress current when delivery status changes.
- [session-handoff/SKILL.md](./session-handoff/SKILL.md)
  Transfer or resume implementation work safely across Codex sessions, agents, and computers using pushed branches and draft pull requests.
- [testing.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/skills/testing.md)
  Add focused tests when logic merits them.
- [vertical-slice.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/skills/vertical-slice.md)
  Implement features in a Vertical Slice Architecture style.

## Usage

- Read this folder index from the agent entry point.
- Read `agent/specs/` from the agent entry point before using task skills.
- Select and use relevant skills whenever they clearly fit the task.
- Use `change-workflow` for repository changes; it decides how much planning is actually warranted.
- Do not force every skill on every task; choose the ones that materially improve correctness, consistency, or workflow discipline.
- Always use the session-handoff skill when pausing active implementation for another session, agent, or computer, or when resuming handed-off work.
