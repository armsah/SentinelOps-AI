namespace SentinelOps.Domain.Hypotheses;

/// <summary>
/// Evidence-linked diagnostic hypothesis for an incident.
/// </summary>
public sealed class IncidentHypothesis
{
    public Guid HypothesisId { get; }

    public string Statement { get; }

    public IReadOnlyList<Guid> SupportingEvidenceIds { get; }

    public IReadOnlyList<Guid> ContradictingEvidenceIds { get; }

    public double Confidence { get; }

    public HypothesisStatus Status { get; }

    public IncidentHypothesis(
        string statement,
        IEnumerable<Guid> supportingEvidenceIds,
        IEnumerable<Guid> contradictingEvidenceIds,
        double confidence,
        HypothesisStatus status = HypothesisStatus.Proposed)
    {
        if (string.IsNullOrWhiteSpace(statement))
        {
            throw new ArgumentException(
                "Hypothesis statement is required.",
                nameof(statement));
        }

        if (double.IsNaN(confidence) ||
            double.IsInfinity(confidence) ||
            confidence < 0 ||
            confidence > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(confidence),
                "Confidence must be between 0 and 1.");
        }

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        ArgumentNullException.ThrowIfNull(supportingEvidenceIds);
        ArgumentNullException.ThrowIfNull(contradictingEvidenceIds);

        var supporting = supportingEvidenceIds.Distinct().ToArray();
        var contradicting = contradictingEvidenceIds.Distinct().ToArray();

        if (supporting.Contains(Guid.Empty) ||
            contradicting.Contains(Guid.Empty))
        {
            throw new ArgumentException(
                "Evidence IDs cannot contain an empty GUID.");
        }

        if (supporting.Intersect(contradicting).Any())
        {
            throw new ArgumentException(
                "The same evidence cannot simultaneously support " +
                "and contradict a hypothesis.");
        }

        HypothesisId = Guid.NewGuid();
        Statement = statement.Trim();

        SupportingEvidenceIds = Array.AsReadOnly(supporting);
        ContradictingEvidenceIds = Array.AsReadOnly(contradicting);

        Confidence = confidence;
        Status = status;
    }
}