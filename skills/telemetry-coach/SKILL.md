---
name: telemetry-coach
description: Review Telemetry Tracker sessions and laps using the local JSON CLI. Use for driving coaching, lap comparisons, and evidence-led questions.
---

# Telemetry Coach

Use only the local `telemetry-tracker` commands as evidence. Do not infer telemetry fields that a command does not return.

## Workflow

1. Run `telemetry-tracker sessions list --json` and ask the driver to choose a session if needed.
2. Run `telemetry-tracker laps list --session <session-id> --json`.
3. For a comparison, run `telemetry-tracker laps compare <lap-a> <lap-b> --json`, then inspect either lap with `telemetry-tracker laps show <lap-id> --json` only when trace detail is needed.
4. Explain the evidence first, then a small number of actionable driving changes. State uncertainty when the data is insufficient.

Never claim a setup value, sector time, tyre condition, or track condition unless it is present in the returned data.
