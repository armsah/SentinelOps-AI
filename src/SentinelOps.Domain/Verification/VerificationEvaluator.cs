namespace SentinelOps.Domain.Verification;

/// <summary>
/// Possible outcomes of post-remediation verification.
/// </summary>
public enum VerificationOutcome
{
    Passed = 1,

    Failed = 2,

    InsufficientData = 3
}

/// <summary>
/// Contains the verification outcome and explanatory reasons.
/// </summary>
public sealed record VerificationResult(
    VerificationOutcome Outcome,
    IReadOnlyList<string> Reasons);

/// <summary>
/// Evaluates observed recovery against deterministic
/// service-level verification thresholds.
/// </summary>
public static class VerificationEvaluator
{
    public static VerificationResult Evaluate(
        VerificationPolicy policy,
        VerificationSnapshot? snapshot)
    {
        ArgumentNullException.ThrowIfNull(policy);

        // Missing, invalid, or incomplete telemetry must
        // never be interpreted as successful recovery.
        if (snapshot is null ||
            snapshot.WindowEndUtc <= snapshot.WindowStartUtc ||
            snapshot.WindowEndUtc - snapshot.WindowStartUtc <
                policy.ObservationWindow ||
            !IsFraction(snapshot.Http5xxRate) ||
            !IsNonNegative(snapshot.P95LatencyMilliseconds) ||
            !IsFraction(snapshot.DatabaseConnectionUtilization) ||
            !IsNonNegative(
                snapshot.QueueOldestMessageAgeStartSeconds) ||
            !IsNonNegative(
                snapshot.QueueOldestMessageAgeEndSeconds) ||
            snapshot.NewCriticalAlerts < 0)
        {
            return new VerificationResult(
                VerificationOutcome.InsufficientData,
                ["Telemetry is missing, invalid, or incomplete."]);
        }

        var failures = new List<string>();

        // HTTP error rate verification.
        if (snapshot.Http5xxRate >= policy.MaximumHttp5xxRate)
        {
            failures.Add(
                "HTTP 5xx rate exceeds the recovery target.");
        }

        // HTTP p95 latency verification.
        if (snapshot.P95LatencyMilliseconds >=
            policy.MaximumP95LatencyMilliseconds)
        {
            failures.Add(
                "HTTP p95 latency exceeds the recovery target.");
        }

        // Database connection utilization verification.
        if (snapshot.DatabaseConnectionUtilization >=
            policy.MaximumDatabaseConnectionUtilization)
        {
            failures.Add(
                "Database connection utilization exceeds the target.");
        }

        // Critical alert verification.
        if (snapshot.NewCriticalAlerts >
            policy.MaximumNewCriticalAlerts)
        {
            failures.Add(
                "New critical alerts were detected.");
        }

        // Queue recovery verification.
        if (policy.RequireQueueAgeDecrease &&
            snapshot.QueueOldestMessageAgeEndSeconds >=
            snapshot.QueueOldestMessageAgeStartSeconds)
        {
            failures.Add(
                "Queue oldest-message age is not decreasing.");
        }

        // All required verification conditions passed.
        if (failures.Count == 0)
        {
            return new VerificationResult(
                VerificationOutcome.Passed,
                Array.Empty<string>());
        }

        return new VerificationResult(
            VerificationOutcome.Failed,
            failures.AsReadOnly());
    }

    private static bool IsFraction(double value)
    {
        return double.IsFinite(value) &&
               value >= 0 &&
               value <= 1;
    }

    private static bool IsNonNegative(double value)
    {
        return double.IsFinite(value) &&
               value >= 0;
    }
}