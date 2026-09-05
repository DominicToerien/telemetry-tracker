---
name: change-workflow
description: Shape and deliver Telemetry Tracker repository changes with the minimum useful ceremony. Use for feature, bug, refactor, or documentation requests that may lead to implementation; skip for read-only questions and status reports.
---

# Change Workflow

Choose the lightest path that protects the result. Do not create process artifacts merely to prove that a process was followed.

## 1. Inspect Before Asking

Read the required project context and inspect the relevant code, tests, and authoritative references. Resolve factual questions from the repository. Ask the user only for decisions that cannot be safely inferred.

## 2. Route The Change

### Direct

Implement immediately when the requested outcome is bounded, existing patterns settle the design, acceptance can be inferred safely, and a wrong assumption would be cheap to reverse.

Examples include small bug fixes with a clear reproduction, narrow documentation corrections, and mechanical changes.

### Contract

For meaningful but sufficiently clear work, state a compact contract in commentary before implementation:

- outcome;
- important in-scope behaviour;
- important exclusions;
- validation that will demonstrate completion.

An explicit instruction to implement counts as approval when this contract is consistent with the request. Do not ask the user to approve it again. Pause only if the contract exposes a material choice or conflict.

### Grill

Grill before implementation only when unresolved decisions could materially change product behaviour, architecture, data contracts, user workflow, or the size of the change.

- Explain briefly why clarification is warranted.
- Ask one decision at a time.
- Include a recommended answer and its main trade-off.
- Explore the repository for facts instead of asking the user for them.
- Ignore naming, formatting, and other reversible implementation choices the agent can make safely.
- After the material decisions are settled, summarize the contract and wait for approval before implementation.

If a question needs runnable evidence, recommend a bounded spike or prototype instead of extending the interview indefinitely.

## 3. Record Only Durable Information

Keep the working contract in the conversation or task/PR by default.

- Update product specifications only when durable product behaviour changes.
- Update `agent/decisions.md` only for consequential, lasting decisions.
- Update `agent/progress.md` only when delivery status changes.
- Create a dedicated proposal/spec artifact only for work that spans sessions, has several independently testable behaviours, or needs review by someone not in the conversation.

Do not duplicate the same plan across `agent/plan.md`, a task, a PR, and another change document.

## 4. Implement Within The Contract

Use the relevant project skills and implement the smallest complete vertical slice. Make ordinary reversible implementation decisions autonomously. Stop and return to the user only when new evidence invalidates the contract or reveals a material choice outside it.

## 5. Verify On Two Axes

Before reporting completion, verify:

1. **Engineering:** appropriate build/tests pass, repository rules are followed, and the change has no known unintended regression.
2. **Contract:** the requested outcome and stated behaviours are present, exclusions stayed excluded, and any deviation is disclosed.

Report the observable result, validation performed, limitations, and the next useful slice only when one is relevant.
