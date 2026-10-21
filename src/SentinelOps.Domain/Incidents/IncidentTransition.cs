namespace SentinelOps.Domain.Incidents;

/// <summary>
/// Immutable record describing an incident lifecycle transition.
/// </summary>
public sealed record IncidentTransition(
    IncidentStatus From,
    IncidentStatus To,
    DateTimeOffset OccurredAtUtc,
    string Actor,
    string Reason);