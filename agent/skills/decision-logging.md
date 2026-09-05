# Decision Logger

When making a durable architectural, design, or scope decision, update:

- `agent/decisions.md`
- `agent/decisions/README.md`

Record:

- Decision
- Reason
- Alternatives considered
- Tradeoffs

Log only non-trivial decisions such as:

- Introducing or rejecting an abstraction
- Choosing a persistence or integration approach
- Changing feature boundaries
- Revising a previously documented architectural direction

Do not log:

- Minor code-style choices
- Obvious implementation details
- Routine renames or formatting changes

If a decision conflicts with existing documented guidance, call out the conflict explicitly and document why the newer decision was taken.

Mark replaced decisions as superseded in the full record and the index.
