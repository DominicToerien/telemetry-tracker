# Agent Specs

This folder contains product-facing specifications that live inside the broader `agent/` workspace.

## Files

- [product.md](product.md)
  The durable product specification and architectural intent.
- [architecture.md](./architecture.md)
  The canonical native Windows, hosted platform, transport, MCP, and Docker boundaries.
- [plan.md](plan.md)
  The stable execution-plan entry path for the agent workflow.

## Usage

- Keep `product.md` focused on product intent, constraints, architecture, and desired outcomes.
- Keep `plan.md` as a compatibility pointer to the current plan and roadmap.
- Load specifications by task through the root `AGENTS.md`; they are not mandatory startup context.
