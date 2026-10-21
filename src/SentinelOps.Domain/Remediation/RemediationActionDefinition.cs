using SentinelOps.Domain.Autonomy;

namespace SentinelOps.Domain.Remediation;

/// <summary>
/// Immutable policy metadata for an allowlisted action.
/// </summary>
public sealed record RemediationActionDefinition(
    RemediationActionType ActionType,
    RemediationRisk Risk,
    AutonomyLevel ProductionLevel,
    AutonomyLevel NonProductionLevel,
    bool ProductionAllowed,
    bool DevelopmentAllowed,
    bool TestAllowed,
    bool StagingAllowed);