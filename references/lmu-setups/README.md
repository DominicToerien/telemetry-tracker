# LMU Setup References

This folder contains setup-file examples used to understand LMU's `.svm` shape and to plan future setup snapshot, comparison, and creation workflows.

## Current fixture

- `992s-pc-moddev-example.svm` is a renamed copy of a generated `TempModFile.svm` example.
- Its embedded vehicle path identifies `MODDEV/VEHICLES/PACECAR/992S_PC`.
- Most adjustable values are represented as commented defaults; only a small subset is active.

## Limitations

This fixture is not an authoritative setup schema and is not representative evidence for every retail LMU car. Setup availability, index ranges, units, and valid combinations can vary by vehicle and upgrades.

Use it to:

- recognize the section-and-setting structure of an `.svm` file;
- design lossless source-artifact storage and versioning;
- develop parser experiments that preserve unknown and commented fields.

Do not use it to:

- invent supported settings for another car;
- assume numeric indices have universal meanings;
- generate or apply a setup without car-specific validation;
- treat comments or the `[BASIC]` values as an authoritative tuning model.

Before `/create-setup` writes LMU files, add representative fixtures from supported cars, document their provenance, and validate generated output against LMU. Setup proposals must remain versioned and require explicit user confirmation before export or application.
