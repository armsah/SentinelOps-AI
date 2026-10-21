namespace SentinelOps.Domain.Incidents;

/// <summary>
/// Represents an operational incident managed by SentinelOps AI.
/// </summary>
public sealed class Incident
{
    private readonly List<IncidentTransition> _transitions = new();

    public Guid Id { get; private set; }

    public string Title { get; private set; }

    public string Description { get; private set; }

    public IncidentSeverity Severity { get; private set; }

    public IncidentStatus Status { get; private set; }

    public string ServiceName { get; private set; }

    public string AzureResourceId { get; private set; }

    public DateTimeOffset DetectedAtUtc { get; private set; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Read-only history of incident lifecycle transitions.
    /// </summary>
    public IReadOnlyList<IncidentTransition> Transitions =>
        _transitions.AsReadOnly();

    public Incident(
        string title,
        string description,
        IncidentSeverity severity,
        string serviceName,
        string azureResourceId)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Incident title cannot be empty.",
                nameof(title));
        }

        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new ArgumentException(
                "Service name cannot be empty.",
                nameof(serviceName));
        }

        if (string.IsNullOrWhiteSpace(azureResourceId))
        {
            throw new ArgumentException(
                "Azure resource ID cannot be empty.",
                nameof(azureResourceId));
        }

        if (!Enum.IsDefined(severity))
        {
            throw new ArgumentOutOfRangeException(
                nameof(severity),
                "Invalid incident severity.");
        }

        Id = Guid.NewGuid();

        Title = title.Trim();

        Description = description?.Trim() ?? string.Empty;

        Severity = severity;

        Status = IncidentStatus.Detected;

        ServiceName = serviceName.Trim();

        AzureResourceId = azureResourceId.Trim();

        DetectedAtUtc = DateTimeOffset.UtcNow;

        UpdatedAtUtc = DetectedAtUtc;
    }

    /// <summary>
    /// Moves the incident to another state after validating the transition.
    /// </summary>
    public void TransitionTo(
        IncidentStatus nextStatus,
        string actor,
        string reason)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            throw new ArgumentException(
                "Transition actor cannot be empty.",
                nameof(actor));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException(
                "Transition reason cannot be empty.",
                nameof(reason));
        }

        IncidentStateMachine.ValidateTransition(
            Status,
            nextStatus);

        var now = DateTimeOffset.UtcNow;

        var transition = new IncidentTransition(
            Status,
            nextStatus,
            now,
            actor.Trim(),
            reason.Trim());

        _transitions.Add(transition);

        Status = nextStatus;

        UpdatedAtUtc = now;
    }
}