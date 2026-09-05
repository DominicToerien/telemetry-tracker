# Current Plan: Car-Specific Setup Modification

## Outcome

Enable the first safe setup modification only after validating supported settings and lossless LMU `.svm` round trips for a specific car.

## In scope

- Select one exact LMU car identifier with representative baseline fixtures.
- Define and validate the supported setting names, value domains, and encoded representation for that car.
- Modify only explicitly supported values while preserving source bytes, comments, ordering, and unknown fields.
- Parse the emitted setup again and verify a lossless round trip outside the intended changes.
- Add focused tests for validation, rejection, preservation, and deterministic output.

## Out of scope

- ASP.NET endpoints, hosted ingestion, Supabase, Docker, or frontend work.
- Native AI chat or MCP adapters.
- Broad multi-car setup generation.
- Unvalidated recommendations or generic setup values.
- Native AI chat, MCP adapters, synchronization, or frontend work.

## Acceptance criteria

- Unsupported cars, fields, and values are rejected without writing output.
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
