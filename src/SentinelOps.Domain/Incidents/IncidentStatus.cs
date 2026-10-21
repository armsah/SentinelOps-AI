namespace SentinelOps.Domain.Incidents;

/// <summary>
/// Represents the lifecycle states of an operational incident.
/// </summary>
public enum IncidentStatus
{
    Detected = 0,

    Correlating = 1,

    Investigating = 2,

    Diagnosing = 3,

    RemediationProposed = 4,

    AwaitingApproval = 5,

    Executing = 6,

    Verifying = 7,

    Resolved = 8,

    RollingBack = 9,

    Escalated = 10,

    Closed = 11,

    Rejected = 12,

    Expired = 13,

    Cancelled = 14,

    PolicyBlocked = 15
}