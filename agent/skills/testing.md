# Test Generator (Smart)

When implementing meaningful logic, add unit tests for:

- Calculations
- State transitions
- Branching behaviour
- Edge cases
- Failure handling with deterministic outcomes

Do not add tests for:

- Framework wiring
- Thin interface-adapter plumbing
- Simple data mapping with no meaningful logic
- Behaviour already fully covered indirectly unless the new test adds clarity

Prefer tests that are:

- Clear about inputs and expected outputs
- Deterministic
- Small and focused
- Easy to read without understanding the whole system

When deciding whether to add tests:

- Add them when logic could regress silently
- Skip them when the test would mostly duplicate framework behaviour

Use this skill whenever writing or changing logic-heavy code.
