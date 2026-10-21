using SentinelOps.Domain.Autonomy;

namespace SentinelOps.Domain.Remediation;

/// <summary>
/// Evaluates catalog eligibility and bounded action parameters.
/// This does not authorize execution.
/// </summary>
public static class RemediationCatalogEvaluator
{
    public static bool IsEligible(
        RemediationAction? action,
        DeploymentEnvironment environment,
        AutonomyLevel requestedLevel)
    {
        if (action is null ||
            !Enum.IsDefined(environment) ||
            !Enum.IsDefined(requestedLevel))
        {
            return false;
        }

        if (!RemediationCatalog.TryGetDefinition(
                action.ActionType,
                out var definition) ||
            definition is null)
        {
            return false;
        }

        var environmentAllowed = environment switch
        {
            DeploymentEnvironment.Production =>
                definition.ProductionAllowed,

            DeploymentEnvironment.Development =>
                definition.DevelopmentAllowed,

            DeploymentEnvironment.Test =>
                definition.TestAllowed,

            DeploymentEnvironment.Staging =>
                definition.StagingAllowed,

            _ => false
        };

        if (!environmentAllowed)
        {
            return false;
        }

        var requiredLevel =
            environment == DeploymentEnvironment.Production
                ? definition.ProductionLevel
                : definition.NonProductionLevel;

        // Exact level matching avoids silently substituting
        // automatic execution for human-approved execution.
        if (requestedLevel != requiredLevel)
        {
            return false;
        }

        return ValidateParameters(action);
    }

    private static bool ValidateParameters(RemediationAction action)
    {
        return action switch
        {
            CollectDiagnosticsAction diagnostics =>
                IsNonEmpty(diagnostics.ResourceId) &&
                diagnostics.LookbackMinutes is >= 1 and <= 60,

            RedriveServiceBusDeadLetterMessagesAction redrive =>
                IsNonEmpty(redrive.ResourceId) &&
                IsNonEmpty(redrive.QueueName) &&
                IsNonEmpty(redrive.IdempotencyKey) &&
                redrive.MessageIds is not null &&
                redrive.MessageIds.Count is >= 1 and <= 10 &&
                redrive.MessageIds.All(IsNonEmpty) &&
                redrive.MessageIds.Distinct(
                    StringComparer.Ordinal).Count() ==
                    redrive.MessageIds.Count,

            ScaleContainerAppAction scale =>
                IsNonEmpty(scale.ResourceId) &&
                scale.TargetMaxReplicas is >= 1 and <= 10,

            SwitchContainerAppRevisionAction revision =>
                IsNonEmpty(revision.ResourceId) &&
                IsNonEmpty(revision.KnownGoodRevisionName),

            RestartDevelopmentWorkloadAction restart =>
                IsNonEmpty(restart.ResourceId),

            DisableFeatureFlagAction flag =>
                IsNonEmpty(flag.ResourceId) &&
                IsNonEmpty(flag.FeatureFlagKey),

            _ => false
        };
    }

    private static bool IsNonEmpty(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }
}