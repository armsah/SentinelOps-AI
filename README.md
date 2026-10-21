# SentinelOps-AI

Human-Governed Agentic AIOps &amp; Safe Self-Healing Platform for Azure. Human-Governed Agentic AIOps &amp; Safe Self-Healing Platform for Azure.

## P0 — Architecture Foundation

The P0 foundation establishes the domain architecture and
human-governed safety principles for SentinelOps AI.

### Implemented

- Incident ontology and severity classification
- Incident lifecycle state machine
- Evidence and diagnostic hypothesis models
- L0–L3 autonomy policy
- Environment-specific autonomy restrictions
- Typed remediation action catalog
- Deterministic safety evaluator
- Scoped approval metadata and action fingerprints
- Resource-specific remediation constraints
- SLO-based recovery verification
- Architecture documentation
- Security threat model
- Architecture Decision Records

### Safety Principles

- AI-generated proposals are not authorization.
- Production L3 automatic remediation is prohibited.
- Production mutating actions require human approval.
- Only allowlisted actions are eligible for remediation.
- Missing or invalid telemetry cannot establish recovery.

### Technology

- .NET 10
- C#
- xUnit
- Microsoft Azure-oriented architecture

### Build

```powershell
dotnet restore SentinelOps.sln
dotnet build SentinelOps.sln
```

### Test

```powershell
dotnet test SentinelOps.sln
```

### Documentation

- `docs/architecture/p0-foundation.md`
- `docs/security/threat-model.md`
- `docs/adr/0001-human-governed-autonomy.md`
- `docs/adr/0002-typed-remediation-catalog.md`
- `docs/adr/0003-deterministic-safety-gateway.md`
- `docs/adr/0004-evidence-based-verification.md`

### Current Scope

P0 contains domain models, policy evaluation, automated tests,
and architecture documentation.

Azure execution, authenticated approval services, durable
audit storage, and production authorization are future work.
