namespace SentinelOps.Domain.Evidence;

/// <summary>
/// Categories of operational evidence supported by SentinelOps AI.
/// </summary>
public enum EvidenceType
{
    Metric = 1,
    Log = 2,
    Trace = 3,
    Change = 4,
    Configuration = 5,
    Health = 6,
    Runbook = 7
}