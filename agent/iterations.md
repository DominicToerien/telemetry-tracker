# Iteration History

## Initial Approach

- Console app
- Raw telemetry stored continuously
- No clear architecture
- No separation of concerns

## Issues

- Poor demo experience
- Difficult to query data
- No clear integration path for AI
- High data volume

## Second Approach

- Introduced API
- Introduced background service
- Still storing raw telemetry

## Improvements

- Better structure
- Easier interaction

## Issues

- Storage inefficient
- AI context too noisy

## Current Approach

- ASP.NET Core Web API
- Vertical Slice Architecture plus CQRS
- Explicit tracking control
- Lap-based persistence
- JSONB lap traces
- Supabase integration

## Key Improvements

- Reduced storage complexity
- Better AI signal quality
- Clear architecture boundaries
- Real-time console verification

## Key Insight

The biggest improvement came from:

- moving from raw telemetry storage to lap-based summaries plus traces
- separating ingestion, persistence, and analysis concerns
- structuring AI interaction through controlled prompts instead of raw data dumps

## Current Repository Reality

The repository currently has LMU shared-memory ingestion foundations and telemetry status endpoints in place. The broader lap-tracking, Supabase, mock source, and AI workflow described in the current plan are the next evolution, not completed functionality.
