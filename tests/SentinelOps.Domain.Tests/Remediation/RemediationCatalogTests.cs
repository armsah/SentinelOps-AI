using SentinelOps.Domain.Autonomy;
using SentinelOps.Domain.Remediation;

namespace SentinelOps.Domain.Tests.Remediation;

public sealed class RemediationCatalogTests
{
    private const string ResourceId =
        "/subscriptions/00000000-0000-0000-0000-000000000000" +
        "/resourceGroups/rg-sentinelops-dev" +
        "/providers/Microsoft.App/containerApps/order-api";

    [Fact]
    public void Catalog_ContainsSixAllowlistedActions()
    {
        Assert.Equal(
            6,
            RemediationCatalog.GetDefinitions().Count);
    }

    [Fact]
    public void Catalog_RejectsUnknownActionType()
    {
        Assert.False(
            RemediationCatalog.TryGetDefinition(
                (RemediationActionType)999,
                out _));
    }

    [Fact]
    public void Diagnostics_AreEligibleAtL0InProduction()
    {
        var action = new CollectDiagnosticsAction(
            ResourceId,
            LookbackMinutes: 15);

        Assert.True(
            RemediationCatalogEvaluator.IsEligible(
                action,
                DeploymentEnvironment.Production,
                AutonomyLevel.L0Observe));
    }

    [Fact]
    public void Diagnostics_RejectExcessiveLookback()
    {
        var action = new CollectDiagnosticsAction(
            ResourceId,
            LookbackMinutes: 120);

        Assert.False(
            RemediationCatalogEvaluator.IsEligible(
                action,
                DeploymentEnvironment.Production,
                AutonomyLevel.L0Observe));
    }

    [Fact]
    public void ScaleContainerApp_RequiresL2InProduction()
    {
        var action = new ScaleContainerAppAction(
            ResourceId,
            TargetMaxReplicas: 5);

        Assert.True(
            RemediationCatalogEvaluator.IsEligible(
                action,
                DeploymentEnvironment.Production,
                AutonomyLevel.L2HumanApproved));

        Assert.False(
            RemediationCatalogEvaluator.IsEligible(
                action,
                DeploymentEnvironment.Production,
                AutonomyLevel.L3BoundedAutomatic));
    }

    [Fact]
    public void ScaleContainerApp_AllowsL3InDevelopment()
    {
        var action = new ScaleContainerAppAction(
            ResourceId,
            TargetMaxReplicas: 5);

        Assert.True(
            RemediationCatalogEvaluator.IsEligible(
                action,
                DeploymentEnvironment.Development,
                AutonomyLevel.L3BoundedAutomatic));
    }

    [Fact]
    public void ScaleContainerApp_RejectsReplicaLimitAboveTen()
    {
        var action = new ScaleContainerAppAction(
            ResourceId,
            TargetMaxReplicas: 11);

        Assert.False(
            RemediationCatalogEvaluator.IsEligible(
                action,
                DeploymentEnvironment.Development,
                AutonomyLevel.L3BoundedAutomatic));
    }

    [Fact]
    public void Redrive_RejectsMoreThanTenMessages()
    {
        var action = new RedriveServiceBusDeadLetterMessagesAction(
            ResourceId,
            "orders",
            Enumerable.Range(1, 11)
                .Select(i => $"message-{i}")
                .ToArray(),
            "idempotency-key-1");

        Assert.False(
            RemediationCatalogEvaluator.IsEligible(
                action,
                DeploymentEnvironment.Development,
                AutonomyLevel.L3BoundedAutomatic));
    }

    [Fact]
    public void Redrive_RejectsDuplicateMessageIds()
    {
        var action = new RedriveServiceBusDeadLetterMessagesAction(
            ResourceId,
            "orders",
            ["message-1", "message-1"],
            "idempotency-key-1");

        Assert.False(
            RemediationCatalogEvaluator.IsEligible(
                action,
                DeploymentEnvironment.Development,
                AutonomyLevel.L3BoundedAutomatic));
    }

    [Fact]
    public void Restart_IsProhibitedInProduction()
    {
        var action = new RestartDevelopmentWorkloadAction(ResourceId);

        Assert.False(
            RemediationCatalogEvaluator.IsEligible(
                action,
                DeploymentEnvironment.Production,
                AutonomyLevel.L3BoundedAutomatic));
    }

    [Fact]
    public void Restart_IsEligibleAtL3InDevelopment()
    {
        var action = new RestartDevelopmentWorkloadAction(ResourceId);

        Assert.True(
            RemediationCatalogEvaluator.IsEligible(
                action,
                DeploymentEnvironment.Development,
                AutonomyLevel.L3BoundedAutomatic));
    }

    [Fact]
    public void RevisionSwitch_RequiresL2InProduction()
    {
        var action = new SwitchContainerAppRevisionAction(
            ResourceId,
            "order-api--revision-001");

        Assert.True(
            RemediationCatalogEvaluator.IsEligible(
                action,
                DeploymentEnvironment.Production,
                AutonomyLevel.L2HumanApproved));
    }

    [Fact]
    public void FeatureFlagDisable_RequiresL2InProduction()
    {
        var action = new DisableFeatureFlagAction(
            ResourceId,
            "EnableNewCheckout");

        Assert.True(
            RemediationCatalogEvaluator.IsEligible(
                action,
                DeploymentEnvironment.Production,
                AutonomyLevel.L2HumanApproved));
    }

    [Fact]
    public void Catalog_DeniesNullAction()
    {
        Assert.False(
            RemediationCatalogEvaluator.IsEligible(
                null,
                DeploymentEnvironment.Development,
                AutonomyLevel.L0Observe));
    }

    [Fact]
    public void Catalog_DeniesUnknownEnvironment()
    {
        var action = new CollectDiagnosticsAction(
            ResourceId,
            10);

        Assert.False(
            RemediationCatalogEvaluator.IsEligible(
                action,
                (DeploymentEnvironment)999,
                AutonomyLevel.L0Observe));
    }

    [Fact]
    public void Catalog_DeniesUnknownAutonomyLevel()
    {
        var action = new CollectDiagnosticsAction(
            ResourceId,
            10);

        Assert.False(
            RemediationCatalogEvaluator.IsEligible(
                action,
                DeploymentEnvironment.Development,
                (AutonomyLevel)999));
    }

    [Fact]
    public void Catalog_DeniesUnsupportedEnvironmentForRedrive()
    {
        var action = new RedriveServiceBusDeadLetterMessagesAction(
            ResourceId,
            "orders",
            ["message-1"],
            "idempotency-key-1");

        Assert.False(
            RemediationCatalogEvaluator.IsEligible(
                action,
                DeploymentEnvironment.Staging,
                AutonomyLevel.L3BoundedAutomatic));
    }
}