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

## Third Approach

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

## Current Approach - Skills-First Terminal Workspace

- local-first CLI/TUI as the primary MVP product
- sessions presented as directory-like contexts and laps as file-like items
- one local telemetry owner with live views available in multiple terminals
- stable JSON CLI operations for session, lap, comparison, telemetry, and setup data
- Codex/Claude skills as the initial coaching and setup layer
- guided, versioned setup proposals with explicit confirmation before export
- native BYOK chat, MCP, hosted API/sync, Supabase, and frontend retained as later phases

## Why This Evolution

The API-first plan established useful architecture but did not yet provide a compelling user-facing workflow. The terminal workspace turns the existing LMU foundation and planned lap model into an immediately usable product. Skills shorten the path to useful AI coaching while stable CLI commands ensure that later MCP, native AI, API, and frontend work can reuse rather than replace the MVP foundation.
