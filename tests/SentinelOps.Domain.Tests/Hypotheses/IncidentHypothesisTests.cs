using SentinelOps.Domain.Hypotheses;

namespace SentinelOps.Domain.Tests.Hypotheses;

public sealed class IncidentHypothesisTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesHypothesis()
    {
        var evidenceId = Guid.NewGuid();

        var hypothesis = new IncidentHypothesis(
            "Recent deployment caused increased HTTP 5xx errors.",
            [evidenceId],
            [],
            0.85);

        Assert.NotEqual(Guid.Empty, hypothesis.HypothesisId);
        Assert.Equal(HypothesisStatus.Proposed, hypothesis.Status);
        Assert.Equal(0.85, hypothesis.Confidence);
        Assert.Contains(evidenceId, hypothesis.SupportingEvidenceIds);
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(1.1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_WithInvalidConfidence_Throws(double confidence)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new IncidentHypothesis(
                "Test hypothesis",
                [],
                [],
                confidence));
    }

    [Fact]
    public void Constructor_WithEmptyStatement_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new IncidentHypothesis(
                " ",
                [],
                [],
                0.5));
    }

    [Fact]
    public void Constructor_WithOverlappingEvidence_Throws()
    {
        var evidenceId = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() =>
            new IncidentHypothesis(
                "Test hypothesis",
                [evidenceId],
                [evidenceId],
                0.7));
    }

    [Fact]
    public void Constructor_DeduplicatesEvidenceIds()
    {
        var evidenceId = Guid.NewGuid();

        var hypothesis = new IncidentHypothesis(
            "Database saturation is causing latency.",
            [evidenceId, evidenceId],
            [],
            0.7);

        Assert.Single(hypothesis.SupportingEvidenceIds);
    }

    [Fact]
    public void Constructor_DefaultsToProposed()
    {
        var hypothesis = new IncidentHypothesis(
            "Queue backlog is increasing processing latency.",
            [],
            [],
            0.6);

        Assert.Equal(HypothesisStatus.Proposed, hypothesis.Status);
    }
}