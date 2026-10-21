using SentinelOps.Domain.Incidents;

namespace SentinelOps.Domain.Tests.Incidents;

public sealed class IncidentStateMachineTests
{
    private const string ResourceId =
        "/subscriptions/00000000-0000-0000-0000-000000000000" +
        "/resourceGroups/rg-sentinelops-dev" +
        "/providers/Microsoft.App/containerApps/order-api";

    private static Incident CreateIncident()
    {
        return new Incident(
            "Elevated HTTP 5xx errors",
            "Order API error rate increased.",
            IncidentSeverity.Sev2,
            "order-api",
            ResourceId);
    }

    [Theory]
    [InlineData(IncidentStatus.Detected, IncidentStatus.Correlating)]
    [InlineData(IncidentStatus.Correlating, IncidentStatus.Investigating)]
    [InlineData(IncidentStatus.Investigating, IncidentStatus.Diagnosing)]
    [InlineData(IncidentStatus.Diagnosing, IncidentStatus.RemediationProposed)]
    [InlineData(IncidentStatus.RemediationProposed, IncidentStatus.AwaitingApproval)]
    [InlineData(IncidentStatus.AwaitingApproval, IncidentStatus.Executing)]
    [InlineData(IncidentStatus.Executing, IncidentStatus.Verifying)]
    [InlineData(IncidentStatus.Verifying, IncidentStatus.Resolved)]
    [InlineData(IncidentStatus.Resolved, IncidentStatus.Closed)]
    [InlineData(IncidentStatus.Verifying, IncidentStatus.RollingBack)]
    [InlineData(IncidentStatus.Verifying, IncidentStatus.Escalated)]
    [InlineData(IncidentStatus.RemediationProposed, IncidentStatus.PolicyBlocked)]
    [InlineData(IncidentStatus.AwaitingApproval, IncidentStatus.Rejected)]
    [InlineData(IncidentStatus.AwaitingApproval, IncidentStatus.Expired)]
    [InlineData(IncidentStatus.AwaitingApproval, IncidentStatus.Cancelled)]
    public void CanTransition_WithAllowedTransition_ReturnsTrue(
        IncidentStatus current,
        IncidentStatus next)
    {
        Assert.True(
            IncidentStateMachine.CanTransition(current, next));
    }

    [Theory]
    [InlineData(IncidentStatus.Detected, IncidentStatus.Resolved)]
    [InlineData(IncidentStatus.Investigating, IncidentStatus.Executing)]
    [InlineData(IncidentStatus.Resolved, IncidentStatus.Executing)]
    [InlineData(IncidentStatus.Closed, IncidentStatus.Investigating)]
    [InlineData(IncidentStatus.PolicyBlocked, IncidentStatus.Executing)]
    [InlineData(IncidentStatus.Rejected, IncidentStatus.Executing)]
    [InlineData(IncidentStatus.Expired, IncidentStatus.Executing)]
    [InlineData(IncidentStatus.Cancelled, IncidentStatus.Executing)]
    [InlineData(IncidentStatus.Detected, IncidentStatus.Detected)]
    public void CanTransition_WithInvalidTransition_ReturnsFalse(
        IncidentStatus current,
        IncidentStatus next)
    {
        Assert.False(
            IncidentStateMachine.CanTransition(current, next));
    }

    [Fact]
    public void TransitionTo_WithValidTransition_UpdatesIncident()
    {
        var incident = CreateIncident();

        incident.TransitionTo(
            IncidentStatus.Correlating,
            "IncidentCommander",
            "Starting correlation");

        Assert.Equal(IncidentStatus.Correlating, incident.Status);
        Assert.Single(incident.Transitions);

        var transition = incident.Transitions[0];

        Assert.Equal(IncidentStatus.Detected, transition.From);
        Assert.Equal(IncidentStatus.Correlating, transition.To);
        Assert.Equal("IncidentCommander", transition.Actor);
        Assert.Equal("Starting correlation", transition.Reason);
        Assert.Equal(transition.OccurredAtUtc, incident.UpdatedAtUtc);
    }

    [Fact]
    public void TransitionTo_WithInvalidTransition_DoesNotMutateIncident()
    {
        var incident = CreateIncident();
        var originalUpdatedAt = incident.UpdatedAtUtc;

        Assert.Throws<InvalidOperationException>(() =>
            incident.TransitionTo(
                IncidentStatus.Executing,
                "IncidentCommander",
                "Attempting invalid transition"));

        Assert.Equal(IncidentStatus.Detected, incident.Status);
        Assert.Equal(originalUpdatedAt, incident.UpdatedAtUtc);
        Assert.Empty(incident.Transitions);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void TransitionTo_WithInvalidActor_ThrowsArgumentException(
        string? actor)
    {
        var incident = CreateIncident();

        Assert.Throws<ArgumentException>(() =>
            incident.TransitionTo(
                IncidentStatus.Correlating,
                actor!,
                "Starting correlation"));

        Assert.Equal(IncidentStatus.Detected, incident.Status);
        Assert.Empty(incident.Transitions);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void TransitionTo_WithInvalidReason_ThrowsArgumentException(
        string? reason)
    {
        var incident = CreateIncident();

        Assert.Throws<ArgumentException>(() =>
            incident.TransitionTo(
                IncidentStatus.Correlating,
                "IncidentCommander",
                reason!));

        Assert.Equal(IncidentStatus.Detected, incident.Status);
        Assert.Empty(incident.Transitions);
    }

    [Fact]
    public void TransitionTo_RecordsMultipleTransitionsInOrder()
    {
        var incident = CreateIncident();

        incident.TransitionTo(
            IncidentStatus.Correlating,
            "IncidentCommander",
            "Starting correlation");

        incident.TransitionTo(
            IncidentStatus.Investigating,
            "IncidentCommander",
            "Correlation completed");

        incident.TransitionTo(
            IncidentStatus.Diagnosing,
            "IncidentCommander",
            "Evidence collection completed");

        Assert.Equal(IncidentStatus.Diagnosing, incident.Status);
        Assert.Equal(3, incident.Transitions.Count);

        Assert.Equal(
            IncidentStatus.Correlating,
            incident.Transitions[0].To);

        Assert.Equal(
            IncidentStatus.Investigating,
            incident.Transitions[1].To);

        Assert.Equal(
            IncidentStatus.Diagnosing,
            incident.Transitions[2].To);
    }

    [Fact]
    public void TerminalStates_CannotTransitionToAnyOtherState()
    {
        var terminalStates = new[]
        {
            IncidentStatus.Closed,
            IncidentStatus.Rejected,
            IncidentStatus.Expired,
            IncidentStatus.Cancelled,
            IncidentStatus.PolicyBlocked
        };

        foreach (var terminalState in terminalStates)
        {
            foreach (var nextStatus in Enum.GetValues<IncidentStatus>())
            {
                Assert.False(
                    IncidentStateMachine.CanTransition(
                        terminalState,
                        nextStatus));
            }
        }
    }
}