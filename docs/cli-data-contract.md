# Local telemetry data contract

The native application exposes recorded data as JSON through its CLI. This is the supported boundary for the bundled skills, custom skills, scripts, and future adapters. Do not read the SQLite file as an integration contract.

All commands emit JSON; `--json` is accepted for compatibility with agent command conventions.

## Hierarchy

```text
session -> laps -> lap summary -> lap telemetry trace
```

| Level | Command | Purpose |
| --- | --- | --- |
| Sessions | `telemetry-tracker sessions list --json` | Browse available sessions. |
| Session | `telemetry-tracker sessions show <session-id> --json` | Read session metadata and its lightweight lap list. |
| Laps | `telemetry-tracker laps list --session <session-id> --json` | Browse the session's lap summaries. |
| Lap | `telemetry-tracker laps show <lap-id> --json` | Read a single lap summary. |
| Telemetry | `telemetry-tracker telemetry show --lap <lap-id> --json` | Read that lap's sampled telemetry trace. |

`laps compare <lap-a> <lap-b> --json` returns compact comparison evidence. `telemetry status --json` is live connection state; it is not recorded lap telemetry.

Lap summaries are the default evidence for agents. Fetch a trace only when the user asks for detailed driving analysis. Trace samples use the documented compact fields `t`, `speed`, `throttle`, `brake`, `steering`, `gear`, `rpm`, `x`, `y`, and `z`.

Errors are JSON objects with an `error` field and a non-zero exit code. Consumers must treat unavailable data as unavailable rather than guessing values.

## Setup baseline commands

`setup files list --root <settings-root> --json` discovers `.svm` files below an LMU Settings directory and returns each file's track-relative directory, `VehicleClassSetting`, fingerprint, and parsed setting count.

`setup import --session <session-id> --file <path> --json` imports an immutable baseline for a real setup file. The source text is stored losslessly with its SHA-256 fingerprint and is versioned against the latest baseline with the same exact `VehicleClassSetting` in that session.

These commands do not generate or modify LMU setup files. A setup must not be generated until its exact car has validated setting definitions and a safe write/LMU verification workflow.
