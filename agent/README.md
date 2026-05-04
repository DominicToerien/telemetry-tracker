# Agent Workspace

This folder is the persistent working context for the AI agent on this project.

Its purpose is to keep product understanding, working rules, workflow guidance, and iteration history in one stable place so implementation stays aligned over time.

## Files

- [entry-point.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/entry-point.md)
  The first file the agent should follow before any task.
- [specs/README.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/specs/README.md)
  Product-facing specifications that the agent should read at startup.
- [skills/README.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/skills/README.md)
  Index of project-specific skills the agent should use when relevant.
- [workflow.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/workflow.md)
  The preferred AI-assisted development workflow for planning and execution.
- [rules.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/rules.md)
  Hard project rules and guardrails.
- [iterations.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/iterations.md)
  Project evolution history and major shifts in approach.
- [decisions.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/decisions.md)
  Major architectural decisions, when they were made, and why.
- [progress.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/progress.md)
  Current delivery snapshot, active phase, and outstanding work.
- [plan.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/plan.md)
  The detailed current implementation plan.

## Usage

- Treat this folder as long-lived context, not scratch notes.
- Update these files when architecture, workflow, or project direction changes.
- Update `decisions.md` when a meaningful architectural or scope decision is made.
- Update `progress.md` when implementation changes the project status, completed work, or current focus.
- Prefer refining these files instead of repeating the same guidance in prompts.
- If instructions elsewhere conflict with this folder, call that out explicitly and resolve the conflict deliberately.

## Auto-Update Expectation

These files are not automatically updated by repository tooling yet.

For now, they are agent-maintained:
- when a change affects decisions, update `decisions.md` in the same task
- when a change affects delivery status, update `progress.md` in the same task

If true automation is wanted later, add a dedicated workflow or script to enforce these updates during development.

## Folder Structure

- `agent/` holds AI-operating context: workflow, rules, iteration history, progress, decisions, and the detailed execution plan.
- `agent/specs/` holds product-facing specifications the agent should also read before work starts.
- `agent/skills/` holds project-specific task skills that should be selected and used whenever they fit.
