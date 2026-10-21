using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using SentinelOps.Domain.Remediation;

namespace SentinelOps.Domain.Safety;

/// <summary>
/// Creates deterministic fingerprints for supported action requests.
/// </summary>
public static class RemediationActionFingerprint
{
    public static string Compute(RemediationAction action)
    {
        ArgumentNullException.ThrowIfNull(action);

        // The explicit type discriminator prevents unrelated action
        // records from sharing the same serialized representation.
        var payload = action switch
        {
            CollectDiagnosticsAction a => JsonSerializer.Serialize(
                new
                {
                    Type = nameof(CollectDiagnosticsAction),
                    a.ResourceId,
                    a.LookbackMinutes
                }),

            RedriveServiceBusDeadLetterMessagesAction a =>
                JsonSerializer.Serialize(
                    new
                    {
                        Type = nameof(RedriveServiceBusDeadLetterMessagesAction),
                        a.ResourceId,
                        a.QueueName,
                        a.MessageIds,
                        a.IdempotencyKey
                    }),

            ScaleContainerAppAction a => JsonSerializer.Serialize(
                new
                {
                    Type = nameof(ScaleContainerAppAction),
                    a.ResourceId,
                    a.TargetMaxReplicas
                }),

            SwitchContainerAppRevisionAction a =>
                JsonSerializer.Serialize(
                    new
                    {
                        Type = nameof(SwitchContainerAppRevisionAction),
                        a.ResourceId,
                        a.KnownGoodRevisionName
                    }),

            RestartDevelopmentWorkloadAction a =>
                JsonSerializer.Serialize(
                    new
                    {
                        Type = nameof(RestartDevelopmentWorkloadAction),
                        a.ResourceId
                    }),

            DisableFeatureFlagAction a => JsonSerializer.Serialize(
                new
                {
                    Type = nameof(DisableFeatureFlagAction),
                    a.ResourceId,
                    a.FeatureFlagKey
                }),

            _ => throw new NotSupportedException(
                "Unsupported remediation action.")
        };

        var bytes = SHA256.HashData(
            Encoding.UTF8.GetBytes(payload));

        return Convert.ToHexString(bytes);
    }
}