namespace SentinelOps.Domain.Autonomy;

/// <summary>
/// Identifies the deployment environment governed by an autonomy policy.
/// </summary>
public enum DeploymentEnvironment
{
    Development = 1,

    Test = 2,

    Staging = 3,

    Production = 4
}