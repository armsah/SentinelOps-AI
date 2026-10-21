namespace SentinelOps.Domain.Verification;

/// <summary>
/// Defines immutable recovery verification thresholds for a service.
/// </summary>
public sealed class VerificationPolicy
{
    public TimeSpan ObservationWindow { get; }

    public double MaximumHttp5xxRate { get; }

    public double MaximumP95LatencyMilliseconds { get; }

    public double MaximumDatabaseConnectionUtilization { get; }

    public int MaximumNewCriticalAlerts { get; }

    public bool RequireQueueAgeDecrease { get; }

    public VerificationPolicy(
        TimeSpan observationWindow,
        double maximumHttp5xxRate,
        double maximumP95LatencyMilliseconds,
        double maximumDatabaseConnectionUtilization,
        int maximumNewCriticalAlerts,
        bool requireQueueAgeDecrease)
    {
        if (observationWindow <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(observationWindow),
                "Observation window must be greater than zero.");
        }

        if (!IsFraction(maximumHttp5xxRate))
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumHttp5xxRate),
                "HTTP 5xx rate threshold must be between 0 and 1.");
        }

        if (!double.IsFinite(maximumP95LatencyMilliseconds) ||
            maximumP95LatencyMilliseconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumP95LatencyMilliseconds),
                "P95 latency threshold must be positive and finite.");
        }

        if (!IsFraction(maximumDatabaseConnectionUtilization))
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumDatabaseConnectionUtilization),
                "Database utilization threshold must be between 0 and 1.");
        }

        if (maximumNewCriticalAlerts < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumNewCriticalAlerts),
                "Maximum critical alerts cannot be negative.");
        }

        ObservationWindow = observationWindow;

        MaximumHttp5xxRate = maximumHttp5xxRate;

        MaximumP95LatencyMilliseconds =
            maximumP95LatencyMilliseconds;

        MaximumDatabaseConnectionUtilization =
            maximumDatabaseConnectionUtilization;

        MaximumNewCriticalAlerts = maximumNewCriticalAlerts;

        RequireQueueAgeDecrease = requireQueueAgeDecrease;
    }

    /// <summary>
    /// Creates the initial SentinelOps AI verification policy.
    /// These defaults must be adapted to service-specific SLOs.
    /// </summary>
    public static VerificationPolicy CreateDefault()
    {
        return new VerificationPolicy(
            observationWindow: TimeSpan.FromMinutes(5),
            maximumHttp5xxRate: 0.01,
            maximumP95LatencyMilliseconds: 500,
            maximumDatabaseConnectionUtilization: 0.70,
            maximumNewCriticalAlerts: 0,
            requireQueueAgeDecrease: true);
    }

    private static bool IsFraction(double value)
    {
        return double.IsFinite(value) &&
               value >= 0 &&
               value <= 1;
    }
}