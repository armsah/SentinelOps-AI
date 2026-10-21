namespace SentinelOps.Domain.Hypotheses;

/// <summary>
/// Evaluation state of a diagnostic hypothesis.
/// </summary>
public enum HypothesisStatus
{
    Proposed = 1,
    Verified = 2,
    Rejected = 3
}