# Agent Rules

- Do not implement beyond the requested phase.
- Do not invent LMU telemetry structures.
- Always inspect `references/lmu-sdks/` before LMU work.
- Keep the app runnable after every change.
- Prefer simple implementations over abstractions.
- Do not introduce new infrastructure unless requested.
- Use `MockTelemetrySource` before LMU integration when building the larger telemetry pipeline.
- Do not persist raw telemetry continuously.
- Follow Vertical Slice Architecture plus CQRS.
- Update `agent/decisions.md` when a meaningful architectural or scope decision is made.
- Update `agent/progress.md` when project status meaningfully changes.

## Additional Working Rules

- Keep endpoints thin.
- Prefer feature-local handlers and data access over large shared service layers.
- Add tests where logic is meaningful, not as ceremony.
- Preserve console visibility for live telemetry verification.
- If a new idea increases complexity without helping the current phase, defer it.
- Do not claim repository automation exists for these agent files unless it has actually been implemented.
