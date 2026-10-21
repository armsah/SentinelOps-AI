namespace SentinelOps.Domain.Verification;

/// <summary>
/// Represents aggregated telemetry collected during
/// a post-remediation observation window.
/// </summary>
public sealed record VerificationSnapshot(
    DateTimeOffset WindowStartUtc,
    DateTimeOffset WindowEndUtc,
    double Http5xxRate,
    double P95LatencyMilliseconds,
    double DatabaseConnectionUtilization,
    double QueueOldestMessageAgeStartSeconds,
    double QueueOldestMessageAgeEndSeconds,
    int NewCriticalAlerts);