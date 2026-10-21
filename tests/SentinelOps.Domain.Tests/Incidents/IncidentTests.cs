using SentinelOps.Domain.Incidents;

namespace SentinelOps.Domain.Tests.Incidents;

public sealed class IncidentTests
{
    private const string ValidAzureResourceId =
        "/subscriptions/00000000-0000-0000-0000-000000000000" +
        "/resourceGroups/rg-sentinelops-dev" +
        "/providers/Microsoft.App/containerApps/order-api";

    [Fact]
    public void Constructor_WithValidData_CreatesIncident()
    {
        // Arrange
        const string title = "High HTTP 5xx error rate";

        const string description =
            "Order API is returning elevated HTTP 500 errors.";

        // Act
        var incident = new Incident(
            title,
            description,
            IncidentSeverity.Sev2,
            "order-api",
            ValidAzureResourceId);

        // Assert
        Assert.NotEqual(Guid.Empty, incident.Id);

        Assert.Equal(title, incident.Title);

        Assert.Equal(description, incident.Description);

        Assert.Equal(IncidentSeverity.Sev2, incident.Severity);

        Assert.Equal(IncidentStatus.Detected, incident.Status);

        Assert.Equal("order-api", incident.ServiceName);

        Assert.Equal(ValidAzureResourceId, incident.AzureResourceId);

        Assert.True(incident.DetectedAtUtc <= DateTimeOffset.UtcNow);

        Assert.Equal(incident.DetectedAtUtc, incident.UpdatedAtUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidTitle_ThrowsArgumentException(
        string? title)
    {
        Assert.Throws<ArgumentException>(() =>
            new Incident(
                title!,
                "Description",
                IncidentSeverity.Sev2,
                "order-api",
                ValidAzureResourceId));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidServiceName_ThrowsArgumentException(
        string? serviceName)
    {
        Assert.Throws<ArgumentException>(() =>
            new Incident(
                "Test incident",
                "Description",
                IncidentSeverity.Sev2,
                serviceName!,
                ValidAzureResourceId));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_WithInvalidAzureResourceId_ThrowsArgumentException(
        string? azureResourceId)
    {
        Assert.Throws<ArgumentException>(() =>
            new Incident(
                "Test incident",
                "Description",
                IncidentSeverity.Sev2,
                "order-api",
                azureResourceId!));
    }

    [Fact]
    public void Constructor_WithInvalidSeverity_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Incident(
                "Test incident",
                "Description",
                (IncidentSeverity)999,
                "order-api",
                ValidAzureResourceId));
    }

    [Fact]
    public void Constructor_TrimsTextProperties()
    {
        var incident = new Incident(
            "  HTTP 5xx errors  ",
            "  Elevated error rate  ",
            IncidentSeverity.Sev2,
            "  order-api  ",
            ValidAzureResourceId);

        Assert.Equal("HTTP 5xx errors", incident.Title);

        Assert.Equal("Elevated error rate", incident.Description);

        Assert.Equal("order-api", incident.ServiceName);
    }

    [Fact]
    public void Constructor_CreatesUniqueIncidentIdentifiers()
    {
        var first = new Incident(
            "Incident 1",
            "Description",
            IncidentSeverity.Sev2,
            "order-api",
            ValidAzureResourceId);

        var second = new Incident(
            "Incident 2",
            "Description",
            IncidentSeverity.Sev2,
            "order-api",
            ValidAzureResourceId);

        Assert.NotEqual(first.Id, second.Id);
    }
}