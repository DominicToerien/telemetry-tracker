# Agent Specs

This folder contains product-facing specifications that live inside the broader `agent/` workspace.

## Files

- [product.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/specs/product.md)
  The durable product specification and architectural intent.
- [architecture.md](./architecture.md)
  The canonical native Windows, hosted platform, transport, MCP, and Docker boundaries.
- [plan.md](/abs/path/c:/Users/toeri/source/repos/telemetry-tracker/agent/specs/plan.md)
  The stable execution-plan entry path for the agent workflow.

## Usage

- Keep `product.md` focused on product intent, constraints, architecture, and desired outcomes.
- Keep `plan.md` as the stable plan path referenced by the agent entry workflow.
- If the detailed plan lives elsewhere, `agent/specs/plan.md` should point to it clearly.
