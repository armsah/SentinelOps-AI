# ADR 0004 — Evidence-Based Recovery Verification

**Status:** Accepted

## Context

A successful remediation API response does not prove
that the affected service has recovered.

## Decision

Use configurable post-remediation verification policies.

Initial metrics:

- HTTP 5xx error rate
- HTTP p95 latency
- Database connection utilization
- Queue oldest-message age
- New critical alerts

Verification outcomes:

- Passed
- Failed
- InsufficientData

## Consequences

- Execution success and recovery success are distinct.
- Missing telemetry cannot establish recovery.
- Incident resolution requires evidence-based verification.
- Recovery thresholds must be adapted to each service.
