using SentinelOps.Domain.Autonomy;
using SentinelOps.Domain.Remediation;

namespace SentinelOps.Domain.Safety;

/// <summary>
/// Deterministic, fail-closed remediation eligibility evaluator.
/// Does not perform operational execution.
/// </summary>
public static class RemediationSafetyEvaluator
{
    public static SafetyDecision Evaluate(
        RemediationAuthorizationRequest? request)
    {
        if (request is null ||
            request.IncidentId == Guid.Empty ||
            request.Action is null ||
            request.Policy is null ||
            request.Resource is null)
        {
            return SafetyDecision.Deny(
                SafetyDecisionCode.InvalidRequest,
                "Required authorization context is missing.");
        }

        if (!request.Resource.IsAllowlisted)
        {
            return SafetyDecision.Deny(
                SafetyDecisionCode.UnknownResource,
                "The target resource is not allowlisted.");
        }

        if (request.Resource.Environment != request.Policy.Environment)
        {
            return SafetyDecision.Deny(
                SafetyDecisionCode.EnvironmentMismatch,
                "The resource environment differs from the policy environment.");
        }

        if (!AutonomyPolicyEvaluator.IsAllowed(
                request.Policy,
                request.RequestedLevel))
        {
            return SafetyDecision.Deny(
                SafetyDecisionCode.AutonomyDenied,
                "The requested autonomy level is not permitted.");
        }

        if (!RemediationCatalogEvaluator.IsEligible(
                request.Action,
                request.Resource.Environment,
                request.RequestedLevel))
        {
            return SafetyDecision.Deny(
                SafetyDecisionCode.ActionDenied,
                "The action is not eligible under the remediation catalog.");
        }

        if (!MatchesResource(
                request.Action,
                request.Resource.ResourceId))
        {
            return SafetyDecision.Deny(
                SafetyDecisionCode.UnknownResource,
                "The action targets a different resource.");
        }

        if (!WithinResourceLimits(
                request.Action,
                request.Resource))
        {
            return SafetyDecision.Deny(
                SafetyDecisionCode.ResourceLimitExceeded,
                "The action exceeds resource-specific constraints.");
        }

        if (request.RequestedLevel == AutonomyLevel.L2HumanApproved)
        {
            if (request.Approval is null)
            {
                return SafetyDecision.Deny(
                    SafetyDecisionCode.ApprovalRequired,
                    "Human approval is required.");
            }

            if (!HasValidApproval(request))
            {
                return SafetyDecision.Deny(
                    SafetyDecisionCode.ApprovalInvalid,
                    "The approval is invalid, expired, revoked, or mismatched.");
            }
        }

        return SafetyDecision.Allow();
    }

    private static bool MatchesResource(
        RemediationAction action,
        string resourceId)
    {
        var actionResourceId = action switch
        {
            CollectDiagnosticsAction a => a.ResourceId,
            RedriveServiceBusDeadLetterMessagesAction a => a.ResourceId,
            ScaleContainerAppAction a => a.ResourceId,
            SwitchContainerAppRevisionAction a => a.ResourceId,
            RestartDevelopmentWorkloadAction a => a.ResourceId,
            DisableFeatureFlagAction a => a.ResourceId,
            _ => null
        };

        return string.Equals(
            actionResourceId,
            resourceId,
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool WithinResourceLimits(
        RemediationAction action,
        TrustedResourceContext resource)
    {
        return action switch
        {
            ScaleContainerAppAction scale =>
                scale.TargetMaxReplicas <= resource.MaximumReplicas,

            RedriveServiceBusDeadLetterMessagesAction redrive =>
                redrive.MessageIds.Count <=
                resource.MaximumDeadLetterMessages,

            SwitchContainerAppRevisionAction revision =>
                resource.AllowedRevisionNames.Contains(
                    revision.KnownGoodRevisionName),

            DisableFeatureFlagAction flag =>
                resource.AllowedFeatureFlagKeys.Contains(
                    flag.FeatureFlagKey),

            _ => true
        };
    }

    private static bool HasValidApproval(
        RemediationAuthorizationRequest request)
    {
        var approval = request.Approval!;

        if (approval.ApprovalId == Guid.Empty ||
            approval.IncidentId != request.IncidentId ||
            approval.IsRevoked ||
            string.IsNullOrWhiteSpace(approval.ApprovedBy) ||
            approval.ApprovedAtUtc > request.EvaluatedAtUtc ||
            approval.ExpiresAtUtc <= request.EvaluatedAtUtc ||
            approval.ExpiresAtUtc <= approval.ApprovedAtUtc)
        {
            return false;
        }

        var expectedFingerprint =
            RemediationActionFingerprint.Compute(request.Action);

        return string.Equals(
            approval.ActionFingerprint,
            expectedFingerprint,
            StringComparison.Ordinal);
    }
}