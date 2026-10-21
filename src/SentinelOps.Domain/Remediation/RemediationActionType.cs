namespace SentinelOps.Domain.Remediation;

/// <summary>
/// Explicitly allowlisted operational action types.
/// </summary>
public enum RemediationActionType
{
    CollectDiagnostics = 1,
    RedriveServiceBusDeadLetterMessages = 2,
    ScaleContainerApp = 3,
    SwitchContainerAppRevision = 4,
    RestartDevelopmentWorkload = 5,
    DisableFeatureFlag = 6
}