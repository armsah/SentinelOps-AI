namespace SentinelOps.Domain.Safety;

public enum SafetyDecisionCode
{
    Eligible = 0,
    InvalidRequest = 1,
    UnknownResource = 2,
    EnvironmentMismatch = 3,
    AutonomyDenied = 4,
    ActionDenied = 5,
    ApprovalRequired = 6,
    ApprovalInvalid = 7,
    ResourceLimitExceeded = 8
}

/// <summary>
/// Represents a deterministic eligibility decision.
/// Eligibility is not permission to invoke an Azure operation.
/// </summary>
public sealed record SafetyDecision(
    bool IsEligible,
    SafetyDecisionCode Code,
    string Reason)
{
    public static SafetyDecision Allow() =>
        new(
            true,
            SafetyDecisionCode.Eligible,
            "The proposal satisfies the evaluated safety constraints.");

    public static SafetyDecision Deny(
        SafetyDecisionCode code,
        string reason) =>
        new(false, code, reason);
}