# SentinelOps AI — P0 Architecture Foundation

## 1. Purpose

SentinelOps AI is a human-governed agentic AIOps and safe
self-healing platform designed for Microsoft Azure.

The platform aims to:

- Detect and correlate operational incidents.
- Collect structured diagnostic evidence.
- Generate and evaluate root-cause hypotheses.
- Recommend bounded remediation actions.
- Enforce deterministic safety policies.
- Require human approval for production remediation.
- Verify recovery against service-level objectives.

The fundamental principle is:

**AI may propose operational actions, but AI cannot authorize itself
to execute those actions.**

## 2. Architecture Overview

The intended operational workflow is:

1. Incident detection
2. Incident correlation
3. Evidence collection
4. Root-cause hypothesis generation
5. Remediation proposal
6. Deterministic safety evaluation
7. Human approval when required
8. Controlled remediation execution
9. Post-remediation verification
10. Incident resolution or escalation

P0 implements domain models and deterministic evaluation logic.

It does not implement live Azure integrations or execution.

## 3. Domain Components

### 3.1 Incident Ontology

Defines incident identity, severity, status, timestamps,
affected services, and Azure resource references.

### 3.2 Incident State Machine

Defines valid incident lifecycle transitions.

Invalid transitions are rejected.

Transition history records the previous state, next state,
timestamp, actor, and reason.

### 3.3 Evidence Model

Represents diagnostic evidence including:

- Evidence type
- Incident association
- Azure resource reference
- Observation timestamp
- Query time window
- Evidence summary
- Raw artifact reference
- Content hash
- Collector version
- Correlation identifiers

Evidence metadata alone does not establish cryptographic
integrity. Verification belongs to the ingestion boundary.

### 3.4 Hypothesis Model

Represents diagnostic hypotheses with:

- Hypothesis statement
- Supporting evidence references
- Contradicting evidence references
- Confidence score
- Verification status

### 3.5 Autonomy Policy

Defines four autonomy levels:

| Level | Meaning                     |
| ----- | --------------------------- |
| L0    | Observe                     |
| L1    | Recommend                   |
| L2    | Human-approved execution    |
| L3    | Bounded automatic execution |

L3 is prohibited in production.

Production remediation requires human approval.

### 3.6 Typed Remediation Catalog

Defines an explicit action allowlist:

1. Collect diagnostics
2. Re-drive selected Service Bus dead-letter messages
3. Scale Azure Container App
4. Switch to a known-good Container App revision
5. Restart a development workload
6. Disable an allowlisted feature flag

Unsupported actions are denied.

### 3.7 Deterministic Safety Evaluator

Evaluates:

- Resource allowlisting
- Deployment environment
- Autonomy policy
- Action catalog eligibility
- Target resource matching
- Resource-specific limits
- Approval metadata

The result is an eligibility decision.

**Eligibility does not constitute execution authorization.**

### 3.8 Recovery Verification

Evaluates post-remediation telemetry against configurable
recovery thresholds.

The initial default policy checks:

- HTTP 5xx error rate
- HTTP p95 latency
- Database connection utilization
- Queue oldest-message age
- New critical alerts

Possible outcomes:

- Passed
- Failed
- InsufficientData

Missing or invalid telemetry cannot establish recovery.

## 4. Trust Boundaries

The following inputs must be treated as untrusted until
validated by an authoritative service:

- AI-generated remediation proposals
- Log messages and trace attributes
- Incident descriptions
- External runbooks
- Caller-provided resource identifiers
- Caller-provided environment labels
- Caller-provided approval records

The future application layer must retrieve trusted resource
metadata and authenticated approvals independently.

## 5. P0 Implementation Boundary

P0 contains:

- Domain models
- Deterministic policy evaluators
- Typed remediation contracts
- Recovery verification contracts
- Automated unit tests
- Architecture and security documentation

P0 does not contain:

- Azure Monitor ingestion
- Azure Resource Manager execution
- Authenticated approval workflows
- Durable approval storage
- Approval replay protection
- Distributed execution locks
- Tamper-evident audit persistence
- Production-ready authorization

These capabilities are required before production remediation.

## 6. Next Phase

The next phase should introduce application services and
trusted infrastructure boundaries while preserving the
domain-level safety invariants established in P0.
