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

## Local baseline discovery

The native client can inspect a user-selected LMU Settings root using `setup files list --root <path> --json`, then import a chosen file with `setup import --session <id> --file <path> --json`.

The `VehicleClassSetting` inside the file is the car identity for baseline storage. File names and track folders are useful provenance, but user-defined names and series/year variants must not be used as a universal vehicle schema. The importer stores the original `.svm` bytes and a fingerprint without rewriting it.
