# Telemetry Integrity Guard

When working with LMU telemetry, always inspect:

- `/references/lmu-sdks/`

Treat the SDK headers in that directory as authoritative for:

- Available telemetry fields
- Struct layouts
- Packing and alignment expectations
- Shared-memory interaction patterns

Do not:

- Invent telemetry fields
- Guess struct layouts
- Assume names, offsets, or enum values
- Implement LMU interop from memory

If something is unclear:

1. Document the assumption being considered
2. Ask for clarification when the assumption could affect correctness
3. Prefer deferring the change over introducing fabricated telemetry details

Use this skill for any LMU telemetry parsing, mapping, persistence, or API exposure work.
