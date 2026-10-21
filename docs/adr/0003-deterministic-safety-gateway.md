# ADR 0003 — Deterministic Safety Gateway

**Status:** Accepted

## Context

AI model confidence cannot establish operational authorization.

## Decision

Use deterministic evaluation of remediation proposals.

The safety decision must consider:

- Trusted resource context
- Deployment environment
- Autonomy ceiling
- Typed remediation catalog
- Action parameter bounds
- Resource-specific constraints
- Human approval metadata

## Consequences

- Unsupported actions fail closed.
- Invalid approval metadata is rejected.
- Resource mismatches are rejected.
- AI cannot override policy decisions.
- Production execution requires additional authenticated
  authorization controls beyond the P0 domain evaluator.
