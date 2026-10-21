namespace SentinelOps.Domain.Autonomy;

/// <summary>
/// Immutable configuration defining the autonomy ceiling for an environment.
/// </summary>
public sealed class AutonomyPolicy
{
    public DeploymentEnvironment Environment { get; }

    public AutonomyLevel MaximumAllowedLevel { get; }

    public bool IsEnabled { get; }

    public AutonomyPolicy(
        DeploymentEnvironment environment,
        AutonomyLevel maximumAllowedLevel,
        bool isEnabled = true)
    {
        if (!Enum.IsDefined(environment))
        {
            throw new ArgumentOutOfRangeException(
                nameof(environment),
                "Unknown deployment environment.");
        }

        if (!Enum.IsDefined(maximumAllowedLevel))
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumAllowedLevel),
                "Unknown autonomy level.");
        }

        if (environment == DeploymentEnvironment.Production &&
            maximumAllowedLevel == AutonomyLevel.L3BoundedAutomatic)
        {
            throw new ArgumentException(
                "L3 automatic remediation is prohibited in production.",
                nameof(maximumAllowedLevel));
        }

        Environment = environment;
        MaximumAllowedLevel = maximumAllowedLevel;
        IsEnabled = isEnabled;
    }
}