namespace SentinelOps.Domain.Safety;

/// <summary>
/// Approval metadata obtained from a trusted approval service.
/// </summary>
public sealed record RemediationApproval(
    Guid ApprovalId,
    Guid IncidentId,
    string ActionFingerprint,
    string ApprovedBy,
    DateTimeOffset ApprovedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    bool IsRevoked);