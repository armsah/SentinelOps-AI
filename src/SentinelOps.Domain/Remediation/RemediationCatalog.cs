using SentinelOps.Domain.Autonomy;

namespace SentinelOps.Domain.Remediation;

/// <summary>
/// Fixed allowlist of supported remediation actions.
/// </summary>
public static class RemediationCatalog
{
    private static readonly IReadOnlyDictionary<
        RemediationActionType,
        RemediationActionDefinition> Definitions =
        new Dictionary<
            RemediationActionType,
            RemediationActionDefinition>
        {
            [RemediationActionType.CollectDiagnostics] = new(
                RemediationActionType.CollectDiagnostics,
                RemediationRisk.ReadOnly,
                AutonomyLevel.L0Observe,
                AutonomyLevel.L0Observe,
                ProductionAllowed: true,
                DevelopmentAllowed: true,
                TestAllowed: true,
                StagingAllowed: true),

            [RemediationActionType.RedriveServiceBusDeadLetterMessages] = new(
                RemediationActionType.RedriveServiceBusDeadLetterMessages,
                RemediationRisk.Medium,
                AutonomyLevel.L2HumanApproved,
                AutonomyLevel.L3BoundedAutomatic,
                ProductionAllowed: true,
                DevelopmentAllowed: true,
                TestAllowed: false,
                StagingAllowed: false),

            [RemediationActionType.ScaleContainerApp] = new(
                RemediationActionType.ScaleContainerApp,
                RemediationRisk.Medium,
                AutonomyLevel.L2HumanApproved,
                AutonomyLevel.L3BoundedAutomatic,
                ProductionAllowed: true,
                DevelopmentAllowed: true,
                TestAllowed: false,
                StagingAllowed: false),

            [RemediationActionType.SwitchContainerAppRevision] = new(
                RemediationActionType.SwitchContainerAppRevision,
                RemediationRisk.High,
                AutonomyLevel.L2HumanApproved,
                AutonomyLevel.L2HumanApproved,
                ProductionAllowed: true,
                DevelopmentAllowed: true,
                TestAllowed: false,
                StagingAllowed: false),

            [RemediationActionType.RestartDevelopmentWorkload] = new(
                RemediationActionType.RestartDevelopmentWorkload,
                RemediationRisk.Low,
                AutonomyLevel.L3BoundedAutomatic,
                AutonomyLevel.L3BoundedAutomatic,
                ProductionAllowed: false,
                DevelopmentAllowed: true,
                TestAllowed: false,
                StagingAllowed: false),

            [RemediationActionType.DisableFeatureFlag] = new(
                RemediationActionType.DisableFeatureFlag,
                RemediationRisk.Medium,
                AutonomyLevel.L2HumanApproved,
                AutonomyLevel.L2HumanApproved,
                ProductionAllowed: true,
                DevelopmentAllowed: true,
                TestAllowed: false,
                StagingAllowed: false)
        };

    public static bool TryGetDefinition(
        RemediationActionType actionType,
        out RemediationActionDefinition? definition)
    {
        return Definitions.TryGetValue(
            actionType,
            out definition);
    }

    public static IReadOnlyCollection<RemediationActionDefinition>
        GetDefinitions()
    {
        return Array.AsReadOnly(
            Definitions.Values.ToArray());
    }
}