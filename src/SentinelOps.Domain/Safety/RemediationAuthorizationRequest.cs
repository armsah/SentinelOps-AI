using SentinelOps.Domain.Autonomy;
using SentinelOps.Domain.Remediation;

namespace SentinelOps.Domain.Safety;

public sealed record RemediationAuthorizationRequest(
    Guid IncidentId,
    RemediationAction Action,
    AutonomyLevel RequestedLevel,
    AutonomyPolicy Policy,
    TrustedResourceContext Resource,
    RemediationApproval? Approval,
    DateTimeOffset EvaluatedAtUtc);