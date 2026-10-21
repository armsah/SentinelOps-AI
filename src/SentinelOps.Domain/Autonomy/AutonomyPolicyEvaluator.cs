namespace SentinelOps.Domain.Autonomy;

/// <summary>
/// Evaluates whether an autonomy level is permitted by an environment policy.
/// </summary>
public static class AutonomyPolicyEvaluator
{
    public static bool IsAllowed(
        AutonomyPolicy policy,
        AutonomyLevel requestedLevel)
    {
        ArgumentNullException.ThrowIfNull(policy);

        if (!Enum.IsDefined(requestedLevel))
        {
            return false;
        }

        if (!policy.IsEnabled)
        {
            return false;
        }

        if (policy.Environment == DeploymentEnvironment.Production &&
            requestedLevel == AutonomyLevel.L3BoundedAutomatic)
        {
            return false;
        }

        return requestedLevel <= policy.MaximumAllowedLevel;
    }

    public static void EnsureAllowed(
        AutonomyPolicy policy,
        AutonomyLevel requestedLevel)
    {
        if (!IsAllowed(policy, requestedLevel))
        {
            throw new InvalidOperationException(
                $"Autonomy level '{requestedLevel}' is not permitted " +
                "by the current policy.");
        }
    }
}