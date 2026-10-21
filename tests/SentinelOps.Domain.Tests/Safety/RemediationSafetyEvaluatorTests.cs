using SentinelOps.Domain.Autonomy;
using SentinelOps.Domain.Remediation;
using SentinelOps.Domain.Safety;

namespace SentinelOps.Domain.Tests.Safety;

public sealed class RemediationSafetyEvaluatorTests
{
    private const string ResourceId =
        "/subscriptions/00000000-0000-0000-0000-000000000000" +
        "/resourceGroups/rg-sentinelops-dev" +
        "/providers/Microsoft.App/containerApps/order-api";

    private static RemediationAuthorizationRequest CreateRequest(
        DeploymentEnvironment environment =
            DeploymentEnvironment.Production,
        AutonomyLevel level =
            AutonomyLevel.L2HumanApproved,
        RemediationAction? action = null,
        bool allowlisted = true,
        int maxReplicas = 10,
        RemediationApproval? approval = null)
    {
        action ??= new ScaleContainerAppAction(ResourceId, 5);

        var policy = new AutonomyPolicy(
            environment,
            environment == DeploymentEnvironment.Production
                ? AutonomyLevel.L2HumanApproved
                : AutonomyLevel.L3BoundedAutomatic);

        var resource = new TrustedResourceContext(
            ResourceId,
            environment,
            allowlisted,
            maxReplicas,
            10,
            ["order-api--revision-001"],
            ["EnableNewCheckout"]);

        return new RemediationAuthorizationRequest(
            Guid.NewGuid(),
            action,
            level,
            policy,
            resource,
            approval,
            DateTimeOffset.UtcNow);
    }

    private static RemediationApproval Approve(
        RemediationAuthorizationRequest request)
    {
        return new RemediationApproval(
            Guid.NewGuid(),
            request.IncidentId,
            RemediationActionFingerprint.Compute(request.Action),
            "human-operator",
            request.EvaluatedAtUtc.AddMinutes(-1),
            request.EvaluatedAtUtc.AddMinutes(10),
            false);
    }

    [Fact]
    public void ProductionL2_WithoutApproval_IsDenied()
    {
        var request = CreateRequest();

        var decision = RemediationSafetyEvaluator.Evaluate(request);

        Assert.False(decision.IsEligible);
        Assert.Equal(
            SafetyDecisionCode.ApprovalRequired,
            decision.Code);
    }

    [Fact]
    public void ProductionL2_WithValidApproval_IsEligible()
    {
        var request = CreateRequest();
        request = request with { Approval = Approve(request) };

        var decision = RemediationSafetyEvaluator.Evaluate(request);

        Assert.True(decision.IsEligible);
    }

    [Fact]
    public void ProductionL2_WithExpiredApproval_IsDenied()
    {
        var request = CreateRequest();

        var approval = Approve(request) with
        {
            ExpiresAtUtc = request.EvaluatedAtUtc.AddSeconds(-1)
        };

        request = request with { Approval = approval };

        var decision = RemediationSafetyEvaluator.Evaluate(request);

        Assert.Equal(
            SafetyDecisionCode.ApprovalInvalid,
            decision.Code);
    }

    [Fact]
    public void ProductionL2_WithRevokedApproval_IsDenied()
    {
        var request = CreateRequest();

        request = request with
        {
            Approval = Approve(request) with
            {
                IsRevoked = true
            }
        };

        Assert.Equal(
            SafetyDecisionCode.ApprovalInvalid,
            RemediationSafetyEvaluator.Evaluate(request).Code);
    }

    [Fact]
    public void ProductionL2_WithWrongActionFingerprint_IsDenied()
    {
        var request = CreateRequest();

        request = request with
        {
            Approval = Approve(request) with
            {
                ActionFingerprint = "invalid-fingerprint"
            }
        };

        Assert.Equal(
            SafetyDecisionCode.ApprovalInvalid,
            RemediationSafetyEvaluator.Evaluate(request).Code);
    }

    [Fact]
    public void ProductionL2_WithWrongIncidentApproval_IsDenied()
    {
        var request = CreateRequest();

        request = request with
        {
            Approval = Approve(request) with
            {
                IncidentId = Guid.NewGuid()
            }
        };

        Assert.Equal(
            SafetyDecisionCode.ApprovalInvalid,
            RemediationSafetyEvaluator.Evaluate(request).Code);
    }

    [Fact]
    public void DevelopmentL3_WithAllowlistedAction_IsEligible()
    {
        var request = CreateRequest(
            DeploymentEnvironment.Development,
            AutonomyLevel.L3BoundedAutomatic);

        Assert.True(
            RemediationSafetyEvaluator.Evaluate(request).IsEligible);
    }

    [Fact]
    public void UnknownResource_IsDenied()
    {
        var request = CreateRequest(
            allowlisted: false);

        Assert.Equal(
            SafetyDecisionCode.UnknownResource,
            RemediationSafetyEvaluator.Evaluate(request).Code);
    }

    [Fact]
    public void ResourceReplicaLimit_IsEnforced()
    {
        var request = CreateRequest(
            DeploymentEnvironment.Development,
            AutonomyLevel.L3BoundedAutomatic,
            maxReplicas: 3);

        Assert.Equal(
            SafetyDecisionCode.ResourceLimitExceeded,
            RemediationSafetyEvaluator.Evaluate(request).Code);
    }

    [Fact]
    public void ActionTargetMismatch_IsDenied()
    {
        var request = CreateRequest(
            DeploymentEnvironment.Development,
            AutonomyLevel.L3BoundedAutomatic,
            new ScaleContainerAppAction(
                "/subscriptions/other/resourceGroups/other/providers/Microsoft.App/containerApps/other",
                3));

        Assert.Equal(
            SafetyDecisionCode.UnknownResource,
            RemediationSafetyEvaluator.Evaluate(request).Code);
    }

    [Fact]
    public void ProductionL3_IsDenied()
    {
        var request = CreateRequest(
            DeploymentEnvironment.Production,
            AutonomyLevel.L3BoundedAutomatic);

        Assert.Equal(
            SafetyDecisionCode.AutonomyDenied,
            RemediationSafetyEvaluator.Evaluate(request).Code);
    }

    [Fact]
    public void NullRequest_IsDenied()
    {
        Assert.Equal(
            SafetyDecisionCode.InvalidRequest,
            RemediationSafetyEvaluator.Evaluate(null).Code);
    }

    [Fact]
    public void Fingerprint_ChangesWhenActionParametersChange()
    {
        var first = new ScaleContainerAppAction(ResourceId, 3);
        var second = new ScaleContainerAppAction(ResourceId, 5);

        Assert.NotEqual(
            RemediationActionFingerprint.Compute(first),
            RemediationActionFingerprint.Compute(second));
    }

    [Fact]
    public void Fingerprint_IsDeterministic()
    {
        var action = new ScaleContainerAppAction(ResourceId, 5);

        Assert.Equal(
            RemediationActionFingerprint.Compute(action),
            RemediationActionFingerprint.Compute(action));
    }
}