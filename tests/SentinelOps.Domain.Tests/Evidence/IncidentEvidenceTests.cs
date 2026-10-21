using SentinelOps.Domain.Evidence;

namespace SentinelOps.Domain.Tests.Evidence;

public sealed class IncidentEvidenceTests
{
    private static IncidentEvidence CreateEvidence(
        Guid? incidentId = null,
        DateTimeOffset? start = null,
        DateTimeOffset? end = null,
        IEnumerable<string>? correlationIds = null)
    {
        var now = DateTimeOffset.UtcNow;

        return new IncidentEvidence(
            incidentId ?? Guid.NewGuid(),
            EvidenceType.Metric,
            "/subscriptions/test/resourceGroups/rg/providers/Microsoft.App/containerApps/api",
            now,
            start ?? now.AddMinutes(-5),
            end ?? now,
            "HTTP 5xx error rate exceeded threshold.",
            null,
            "sha256:example-digest",
            "collector-1.0",
            correlationIds);
    }

    [Fact]
    public void Constructor_WithValidData_CreatesEvidence()
    {
        var incidentId = Guid.NewGuid();

        var evidence = CreateEvidence(incidentId);

        Assert.NotEqual(Guid.Empty, evidence.EvidenceId);
        Assert.Equal(incidentId, evidence.IncidentId);
        Assert.Equal(EvidenceType.Metric, evidence.EvidenceType);
        Assert.NotEmpty(evidence.Summary);
    }

    [Fact]
    public void Constructor_WithEmptyIncidentId_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            CreateEvidence(Guid.Empty));
    }

    [Fact]
    public void Constructor_WithInvalidQueryWindow_Throws()
    {
        var now = DateTimeOffset.UtcNow;

        Assert.Throws<ArgumentException>(() =>
            CreateEvidence(
                start: now,
                end: now.AddMinutes(-5)));
    }

    [Fact]
    public void Constructor_DeduplicatesCorrelationIds()
    {
        var evidence = CreateEvidence(
            correlationIds:
            [
                "trace-001",
                "trace-001",
                "trace-002"
            ]);

        Assert.Equal(2, evidence.CorrelationIds.Count);
        Assert.Contains("trace-001", evidence.CorrelationIds);
        Assert.Contains("trace-002", evidence.CorrelationIds);
    }

    [Fact]
    public void Constructor_NormalizesTimestampsToUtc()
    {
        var now = DateTimeOffset.Now;

        var evidence = new IncidentEvidence(
            Guid.NewGuid(),
            EvidenceType.Log,
            "resource-1",
            now,
            now.AddMinutes(-5),
            now,
            "Application exception.",
            null,
            "sha256:example-digest",
            "collector-1.0");

        Assert.Equal(TimeSpan.Zero, evidence.ObservedAtUtc.Offset);
        Assert.Equal(TimeSpan.Zero, evidence.QueryWindowStartUtc.Offset);
        Assert.Equal(TimeSpan.Zero, evidence.QueryWindowEndUtc.Offset);
    }
}