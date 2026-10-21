using SentinelOps.Domain.Verification;

namespace SentinelOps.Domain.Tests.Verification;

public sealed class VerificationEvaluatorTests
{
    private static VerificationSnapshot HealthySnapshot()
    {
        var end = DateTimeOffset.UtcNow;

        return new VerificationSnapshot(
            WindowStartUtc: end.AddMinutes(-5),
            WindowEndUtc: end,
            Http5xxRate: 0.005,
            P95LatencyMilliseconds: 300,
            DatabaseConnectionUtilization: 0.40,
            QueueOldestMessageAgeStartSeconds: 120,
            QueueOldestMessageAgeEndSeconds: 60,
            NewCriticalAlerts: 0);
    }

    [Fact]
    public void HealthyTelemetry_Passes()
    {
        var result = VerificationEvaluator.Evaluate(
            VerificationPolicy.CreateDefault(),
            HealthySnapshot());

        Assert.Equal(
            VerificationOutcome.Passed,
            result.Outcome);
    }

    [Fact]
    public void HighErrorRate_Fails()
    {
        var snapshot = HealthySnapshot() with
        {
            Http5xxRate = 0.02
        };

        var result = VerificationEvaluator.Evaluate(
            VerificationPolicy.CreateDefault(),
            snapshot);

        Assert.Equal(
            VerificationOutcome.Failed,
            result.Outcome);
    }

    [Fact]
    public void HighLatency_Fails()
    {
        var snapshot = HealthySnapshot() with
        {
            P95LatencyMilliseconds = 700
        };

        var result = VerificationEvaluator.Evaluate(
            VerificationPolicy.CreateDefault(),
            snapshot);

        Assert.Equal(
            VerificationOutcome.Failed,
            result.Outcome);
    }

    [Fact]
    public void HighDatabaseUtilization_Fails()
    {
        var snapshot = HealthySnapshot() with
        {
            DatabaseConnectionUtilization = 0.85
        };

        var result = VerificationEvaluator.Evaluate(
            VerificationPolicy.CreateDefault(),
            snapshot);

        Assert.Equal(
            VerificationOutcome.Failed,
            result.Outcome);
    }

    [Fact]
    public void IncreasingQueueAge_Fails()
    {
        var snapshot = HealthySnapshot() with
        {
            QueueOldestMessageAgeEndSeconds = 150
        };

        var result = VerificationEvaluator.Evaluate(
            VerificationPolicy.CreateDefault(),
            snapshot);

        Assert.Equal(
            VerificationOutcome.Failed,
            result.Outcome);
    }

    [Fact]
    public void NewCriticalAlert_Fails()
    {
        var snapshot = HealthySnapshot() with
        {
            NewCriticalAlerts = 1
        };

        var result = VerificationEvaluator.Evaluate(
            VerificationPolicy.CreateDefault(),
            snapshot);

        Assert.Equal(
            VerificationOutcome.Failed,
            result.Outcome);
    }

    [Fact]
    public void ShortObservationWindow_IsInsufficient()
    {
        var snapshot = HealthySnapshot() with
        {
            WindowStartUtc =
                DateTimeOffset.UtcNow.AddMinutes(-1)
        };

        var result = VerificationEvaluator.Evaluate(
            VerificationPolicy.CreateDefault(),
            snapshot);

        Assert.Equal(
            VerificationOutcome.InsufficientData,
            result.Outcome);
    }

    [Fact]
    public void MissingSnapshot_IsInsufficient()
    {
        var result = VerificationEvaluator.Evaluate(
            VerificationPolicy.CreateDefault(),
            null);

        Assert.Equal(
            VerificationOutcome.InsufficientData,
            result.Outcome);
    }

    [Fact]
    public void NaNMetric_IsInsufficient()
    {
        var snapshot = HealthySnapshot() with
        {
            Http5xxRate = double.NaN
        };

        var result = VerificationEvaluator.Evaluate(
            VerificationPolicy.CreateDefault(),
            snapshot);

        Assert.Equal(
            VerificationOutcome.InsufficientData,
            result.Outcome);
    }

    [Fact]
    public void DefaultPolicy_HasExpectedThresholds()
    {
        var policy = VerificationPolicy.CreateDefault();

        Assert.Equal(
            TimeSpan.FromMinutes(5),
            policy.ObservationWindow);

        Assert.Equal(
            0.01,
            policy.MaximumHttp5xxRate);

        Assert.Equal(
            500,
            policy.MaximumP95LatencyMilliseconds);

        Assert.Equal(
            0.70,
            policy.MaximumDatabaseConnectionUtilization);

        Assert.Equal(
            0,
            policy.MaximumNewCriticalAlerts);

        Assert.True(
            policy.RequireQueueAgeDecrease);
    }
}