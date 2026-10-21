# ADR 0002 — Typed Remediation Catalog

**Status:** Accepted

## Context

Allowing AI agents to execute arbitrary commands creates
unacceptable operational risk.

## Decision

Use a fixed catalog of strongly typed remediation actions.

Initial supported actions:

1. Collect diagnostics
2. Re-drive selected Service Bus DLQ messages
3. Scale Azure Container App
4. Switch Container App revision
5. Restart development workload
6. Disable allowlisted feature flag

Every action must satisfy parameter and environment constraints.

## Consequences

- Arbitrary shell commands are unsupported.
- Arbitrary SQL execution is unsupported.
- Unrestricted infrastructure modification is unsupported.
- Resource deletion is unsupported.
- New remediation actions require explicit implementation,
  review, and testing.
