---
name: setup-proposal
description: Create bounded, versioned Telemetry Tracker setup proposals from driver feedback and available lap evidence.
---

# Setup Proposal

Setup proposals are advisory records. They are not LMU setup-file writes.

1. Resolve the session and inspect relevant laps with the local JSON CLI.
2. Ask the driver for the handling problem and when it occurs.
3. Explain evidence, expected effect, and trade-offs before proposing a change.
4. Create the proposal with `telemetry-tracker setup propose --session <id> --name <name> --feedback <feedback> --json`.
5. Make clear that export/application is unavailable until car-specific setup-file validation is implemented.

Do not invent LMU setup fields or apply changes automatically.
