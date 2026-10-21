namespace SentinelOps.Domain.Evidence;

/// <summary>
/// Immutable operational evidence associated with an incident.
/// </summary>
public sealed class IncidentEvidence
{
    public Guid EvidenceId { get; }

    public Guid IncidentId { get; }

    public EvidenceType EvidenceType { get; }

    public string SourceResourceId { get; }

    public DateTimeOffset ObservedAtUtc { get; }

    public DateTimeOffset QueryWindowStartUtc { get; }

    public DateTimeOffset QueryWindowEndUtc { get; }

    public string Summary { get; }

    public string? RawArtifactUri { get; }

    public string ContentHash { get; }

    public string CollectorVersion { get; }

    public IReadOnlyList<string> CorrelationIds { get; }

    public IncidentEvidence(
        Guid incidentId,
        EvidenceType evidenceType,
        string sourceResourceId,
        DateTimeOffset observedAtUtc,
        DateTimeOffset queryWindowStartUtc,
        DateTimeOffset queryWindowEndUtc,
        string summary,
        string? rawArtifactUri,
        string contentHash,
        string collectorVersion,
        IEnumerable<string>? correlationIds = null)
    {
        if (incidentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Incident ID cannot be empty.",
                nameof(incidentId));
        }

        if (!Enum.IsDefined(evidenceType))
        {
            throw new ArgumentOutOfRangeException(nameof(evidenceType));
        }

        if (string.IsNullOrWhiteSpace(sourceResourceId))
        {
            throw new ArgumentException(
                "Source resource ID is required.",
                nameof(sourceResourceId));
        }

        if (queryWindowStartUtc > queryWindowEndUtc)
        {
            throw new ArgumentException(
                "Query window start cannot be after its end.");
        }

        if (string.IsNullOrWhiteSpace(summary))
        {
            throw new ArgumentException(
                "Evidence summary is required.",
                nameof(summary));
        }

        if (string.IsNullOrWhiteSpace(contentHash))
        {
            throw new ArgumentException(
                "Content hash is required.",
                nameof(contentHash));
        }

        if (string.IsNullOrWhiteSpace(collectorVersion))
        {
            throw new ArgumentException(
                "Collector version is required.",
                nameof(collectorVersion));
        }

        EvidenceId = Guid.NewGuid();
        IncidentId = incidentId;
        EvidenceType = evidenceType;
        SourceResourceId = sourceResourceId.Trim();

        ObservedAtUtc = observedAtUtc.ToUniversalTime();
        QueryWindowStartUtc = queryWindowStartUtc.ToUniversalTime();
        QueryWindowEndUtc = queryWindowEndUtc.ToUniversalTime();

        Summary = summary.Trim();
        RawArtifactUri = string.IsNullOrWhiteSpace(rawArtifactUri)
            ? null
            : rawArtifactUri.Trim();

        ContentHash = contentHash.Trim();
        CollectorVersion = collectorVersion.Trim();

        CorrelationIds = Array.AsReadOnly(
            (correlationIds ?? [])
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(id => id.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray());
    }
}