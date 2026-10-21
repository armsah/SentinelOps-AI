namespace SentinelOps.Domain.Incidents;

/// <summary>
/// Defines valid state transitions for SentinelOps AI incidents.
/// </summary>
public static class IncidentStateMachine
{
    private static readonly IReadOnlyDictionary<
        IncidentStatus,
        IReadOnlySet<IncidentStatus>> AllowedTransitions =
        new Dictionary<IncidentStatus, IReadOnlySet<IncidentStatus>>
        {
            [IncidentStatus.Detected] =
                new HashSet<IncidentStatus>
                {
                    IncidentStatus.Correlating
                },

            [IncidentStatus.Correlating] =
                new HashSet<IncidentStatus>
                {
                    IncidentStatus.Investigating
                },

            [IncidentStatus.Investigating] =
                new HashSet<IncidentStatus>
                {
                    IncidentStatus.Diagnosing,
                    IncidentStatus.Escalated
                },

            [IncidentStatus.Diagnosing] =
                new HashSet<IncidentStatus>
                {
                    IncidentStatus.RemediationProposed,
                    IncidentStatus.Escalated
                },

            [IncidentStatus.RemediationProposed] =
                new HashSet<IncidentStatus>
                {
                    IncidentStatus.AwaitingApproval,
                    IncidentStatus.Escalated,
                    IncidentStatus.PolicyBlocked
                },

            [IncidentStatus.AwaitingApproval] =
                new HashSet<IncidentStatus>
                {
                    IncidentStatus.Executing,
                    IncidentStatus.Rejected,
                    IncidentStatus.Expired,
                    IncidentStatus.Cancelled
                },

            [IncidentStatus.Executing] =
                new HashSet<IncidentStatus>
                {
                    IncidentStatus.Verifying,
                    IncidentStatus.Escalated
                },

            [IncidentStatus.Verifying] =
                new HashSet<IncidentStatus>
                {
                    IncidentStatus.Resolved,
                    IncidentStatus.RollingBack,
                    IncidentStatus.Escalated
                },

            [IncidentStatus.RollingBack] =
                new HashSet<IncidentStatus>
                {
                    IncidentStatus.Escalated
                },

            [IncidentStatus.Resolved] =
                new HashSet<IncidentStatus>
                {
                    IncidentStatus.Closed
                },

            [IncidentStatus.Escalated] =
                new HashSet<IncidentStatus>
                {
                    IncidentStatus.Closed
                }
        };

    public static bool CanTransition(
        IncidentStatus current,
        IncidentStatus next)
    {
        return AllowedTransitions.TryGetValue(
                   current,
                   out var allowed)
               && allowed.Contains(next);
    }

    public static void ValidateTransition(
        IncidentStatus current,
        IncidentStatus next)
    {
        if (!CanTransition(current, next))
        {
            throw new InvalidOperationException(
                $"Invalid incident transition: {current} -> {next}.");
        }
    }
}