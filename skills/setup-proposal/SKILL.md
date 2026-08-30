---
name: setup-proposal
description: Create bounded, versioned Telemetry Tracker setup proposals from driver feedback and available lap evidence.
---

# Setup Proposal

The quality of a generated LMU car setup is more important than producing a quick answer. Never present a generic suggestion, an empty record, or a setting from another car as an LMU setup.

1. Resolve the session and inspect relevant laps with the local JSON CLI.
2. Ask the driver for the handling problem and when it occurs.
3. Obtain the current setup file for that exact car and a validated description of the settings it supports. Preserve unknown fields, comments, and ordering.
4. Explain the telemetry evidence, proposed setting changes, expected effect, and trade-offs before producing an LMU setup.
5. The current CLI intentionally refuses `setup propose`: it has no validated car-specific baseline or setting definitions, so it must not output an empty or invented LMU setup. State this clearly and ask for the representative setup files needed to implement safe generation.

Do not invent LMU setup fields, ranges, units, or apply changes automatically.
