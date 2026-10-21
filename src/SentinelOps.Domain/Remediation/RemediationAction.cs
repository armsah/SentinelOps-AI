namespace SentinelOps.Domain.Remediation;

/// <summary>
/// Base type for all supported remediation requests.
/// </summary>
public abstract record RemediationAction
{
    public abstract RemediationActionType ActionType { get; }
}

/// <summary>
/// Collects read-only operational diagnostics.
/// </summary>
public sealed record CollectDiagnosticsAction(
    string ResourceId,
    int LookbackMinutes) : RemediationAction
{
    public override RemediationActionType ActionType =>
        RemediationActionType.CollectDiagnostics;
}

/// <summary>
/// Re-drives explicitly selected Service Bus dead-letter messages.
/// </summary>
public sealed record RedriveServiceBusDeadLetterMessagesAction(
    string ResourceId,
    string QueueName,
    IReadOnlyList<string> MessageIds,
    string IdempotencyKey) : RemediationAction
{
    public override RemediationActionType ActionType =>
        RemediationActionType.RedriveServiceBusDeadLetterMessages;
}

/// <summary>
/// Changes a Container App replica ceiling within policy-defined limits.
/// </summary>
public sealed record ScaleContainerAppAction(
    string ResourceId,
    int TargetMaxReplicas) : RemediationAction
{
    public override RemediationActionType ActionType =>
        RemediationActionType.ScaleContainerApp;
}

/// <summary>
/// Routes Container App traffic to a previously validated revision.
/// </summary>
public sealed record SwitchContainerAppRevisionAction(
    string ResourceId,
    string KnownGoodRevisionName) : RemediationAction
{
    public override RemediationActionType ActionType =>
        RemediationActionType.SwitchContainerAppRevision;
}

/// <summary>
/// Restarts an allowlisted development workload.
/// </summary>
public sealed record RestartDevelopmentWorkloadAction(
    string ResourceId) : RemediationAction
{
    public override RemediationActionType ActionType =>
        RemediationActionType.RestartDevelopmentWorkload;
}

/// <summary>
/// Disables a feature flag that has been explicitly allowlisted.
/// </summary>
public sealed record DisableFeatureFlagAction(
    string ResourceId,
    string FeatureFlagKey) : RemediationAction
{
    public override RemediationActionType ActionType =>
        RemediationActionType.DisableFeatureFlag;
}