namespace SentinelOps.Domain.Remediation;

/// <summary>
/// Risk classification for a remediation action.
/// </summary>
public enum RemediationRisk
{
    ReadOnly = 0,
    Low = 1,
    Medium = 2,
    High = 3
}