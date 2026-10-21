using SentinelOps.Domain.Autonomy;

namespace SentinelOps.Domain.Safety;

/// <summary>
/// Resource metadata supplied by a trusted inventory boundary.
/// </summary>
public sealed class TrustedResourceContext
{
    public string ResourceId { get; }

    public DeploymentEnvironment Environment { get; }

    public bool IsAllowlisted { get; }

    public int MaximumReplicas { get; }

    public int MaximumDeadLetterMessages { get; }

    public IReadOnlySet<string> AllowedRevisionNames { get; }

    public IReadOnlySet<string> AllowedFeatureFlagKeys { get; }

    public TrustedResourceContext(
        string resourceId,
        DeploymentEnvironment environment,
        bool isAllowlisted,
        int maximumReplicas,
        int maximumDeadLetterMessages,
        IEnumerable<string>? allowedRevisionNames = null,
        IEnumerable<string>? allowedFeatureFlagKeys = null)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
            throw new ArgumentException(
                "Resource ID is required.",
                nameof(resourceId));

        if (!Enum.IsDefined(environment))
            throw new ArgumentOutOfRangeException(nameof(environment));

        if (maximumReplicas < 0)
            throw new ArgumentOutOfRangeException(nameof(maximumReplicas));

        if (maximumDeadLetterMessages < 0)
            throw new ArgumentOutOfRangeException(
                nameof(maximumDeadLetterMessages));

        ResourceId = resourceId.Trim();
        Environment = environment;
        IsAllowlisted = isAllowlisted;
        MaximumReplicas = maximumReplicas;
        MaximumDeadLetterMessages = maximumDeadLetterMessages;

        AllowedRevisionNames = new HashSet<string>(
            allowedRevisionNames ?? [],
            StringComparer.Ordinal);

        AllowedFeatureFlagKeys = new HashSet<string>(
            allowedFeatureFlagKeys ?? [],
            StringComparer.Ordinal);
    }
}