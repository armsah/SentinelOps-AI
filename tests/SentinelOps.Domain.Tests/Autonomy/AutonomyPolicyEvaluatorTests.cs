using SentinelOps.Domain.Autonomy;

namespace SentinelOps.Domain.Tests.Autonomy;

public sealed class AutonomyPolicyEvaluatorTests
{
    [Theory]
    [InlineData(AutonomyLevel.L0Observe)]
    [InlineData(AutonomyLevel.L1Recommend)]
    [InlineData(AutonomyLevel.L2HumanApproved)]
    public void ProductionPolicy_AllowsLevelsUpToL2(
        AutonomyLevel requestedLevel)
    {
        var policy = new AutonomyPolicy(
            DeploymentEnvironment.Production,
            AutonomyLevel.L2HumanApproved);

        Assert.True(
            AutonomyPolicyEvaluator.IsAllowed(
                policy,
                requestedLevel));
    }

    [Fact]
    public void ProductionPolicy_RejectsL3Configuration()
    {
        Assert.Throws<ArgumentException>(() =>
            new AutonomyPolicy(
                DeploymentEnvironment.Production,
                AutonomyLevel.L3BoundedAutomatic));
    }

    [Fact]
    public void ProductionPolicy_DeniesL3Request()
    {
        var policy = new AutonomyPolicy(
            DeploymentEnvironment.Production,
            AutonomyLevel.L2HumanApproved);

        Assert.False(
            AutonomyPolicyEvaluator.IsAllowed(
                policy,
                AutonomyLevel.L3BoundedAutomatic));
    }

    [Fact]
    public void DevelopmentPolicy_AllowsL3WithinCeiling()
    {
        var policy = new AutonomyPolicy(
            DeploymentEnvironment.Development,
            AutonomyLevel.L3BoundedAutomatic);

        Assert.True(
            AutonomyPolicyEvaluator.IsAllowed(
                policy,
                AutonomyLevel.L3BoundedAutomatic));
    }

    [Fact]
    public void StagingPolicy_DeniesRequestAboveCeiling()
    {
        var policy = new AutonomyPolicy(
            DeploymentEnvironment.Staging,
            AutonomyLevel.L1Recommend);

        Assert.False(
            AutonomyPolicyEvaluator.IsAllowed(
                policy,
                AutonomyLevel.L2HumanApproved));
    }

    [Fact]
    public void DisabledPolicy_DeniesEveryLevel()
    {
        var policy = new AutonomyPolicy(
            DeploymentEnvironment.Development,
            AutonomyLevel.L3BoundedAutomatic,
            isEnabled: false);

        foreach (var level in Enum.GetValues<AutonomyLevel>())
        {
            Assert.False(
                AutonomyPolicyEvaluator.IsAllowed(
                    policy,
                    level));
        }
    }

    [Fact]
    public void Evaluator_DeniesUnknownAutonomyLevel()
    {
        var policy = new AutonomyPolicy(
            DeploymentEnvironment.Development,
            AutonomyLevel.L3BoundedAutomatic);

        Assert.False(
            AutonomyPolicyEvaluator.IsAllowed(
                policy,
                (AutonomyLevel)999));
    }

    [Fact]
    public void Policy_RejectsUnknownEnvironment()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new AutonomyPolicy(
                (DeploymentEnvironment)999,
                AutonomyLevel.L1Recommend));
    }

    [Fact]
    public void Policy_RejectsUnknownMaximumLevel()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new AutonomyPolicy(
                DeploymentEnvironment.Development,
                (AutonomyLevel)999));
    }

    [Fact]
    public void EnsureAllowed_ThrowsWhenLevelIsDenied()
    {
        var policy = new AutonomyPolicy(
            DeploymentEnvironment.Production,
            AutonomyLevel.L2HumanApproved);

        Assert.Throws<InvalidOperationException>(() =>
            AutonomyPolicyEvaluator.EnsureAllowed(
                policy,
                AutonomyLevel.L3BoundedAutomatic));
    }

    [Fact]
    public void EnsureAllowed_DoesNotThrowWhenLevelIsPermitted()
    {
        var policy = new AutonomyPolicy(
            DeploymentEnvironment.Development,
            AutonomyLevel.L3BoundedAutomatic);

        AutonomyPolicyEvaluator.EnsureAllowed(
            policy,
            AutonomyLevel.L1Recommend);
    }

    [Fact]
    public void Evaluator_RejectsNullPolicy()
    {
        Assert.Throws<ArgumentNullException>(() =>
            AutonomyPolicyEvaluator.IsAllowed(
                null!,
                AutonomyLevel.L0Observe));
    }
}