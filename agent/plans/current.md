# Current Plan: BMW M4 LMGT3 Setup Modification

## Outcome

Enable safe, versioned setup proposals for the exact LMU car identifier `BMW_M4_LMGT3 GT3 WEC2025`.

## In scope

- Retain representative BMW M4 LMGT3 baseline fixtures with source provenance.
- Validate the observed encoded pairs for rear wing, rear anti-roll bar, brake bias, and the three traction-control maps.
- Modify only explicitly supported values while preserving source bytes, comments, ordering, and unknown fields.
- Parse the emitted setup again and verify a lossless round trip outside the intended changes.
- Tie every proposal to its immutable baseline, source lap, and driver feedback.
- Add focused tests for validation, rejection, preservation, and deterministic output.

## Out of scope

- ASP.NET endpoints, hosted ingestion, Supabase, Docker, or frontend work.
- Native AI chat or MCP adapters.
- Broad multi-car setup generation.
- Unvalidated recommendations or generic setup values.
- Native AI chat, MCP adapters, synchronization, or frontend work.

## Acceptance criteria

- Unsupported cars, fields, values, source encodings, and no-op changes are rejected without creating a proposal.
- A supported change preserves every byte or parsed element outside the intended setting edits.
- Emitted setups parse successfully and retain the exact car identifier and source provenance.
- The application builds and focused setup tests pass.

## Relevant context

- [Current project state](../current.md)
- [Product specification](../specs/product.md)
- [Architecture specification](../specs/architecture.md)
- [LMU integrity guide](../skills/lmu-integrity.md)
- [Vertical-slice guide](../skills/vertical-slice.md)
- [Detailed design archive](../plan.md)
