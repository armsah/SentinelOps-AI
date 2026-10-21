# ADR 0001 — Human-Governed Autonomy

**Status:** Accepted

## Context

Agentic AIOps can accelerate incident response.

However, unrestricted autonomous remediation creates
operational and security risks.

## Decision

Adopt four autonomy levels:

- L0: Observe
- L1: Recommend
- L2: Human-approved execution
- L3: Bounded automatic execution

Production environments must not permit L3.

Production mutating actions require L2 human approval.

## Consequences

- AI-generated recommendations are not authorization.
- Production remediation requires human governance.
- Automatic remediation is restricted to eligible
  nonproduction operations.
- Authorization must be enforced independently of the AI model.
