namespace SentinelOps.Domain.Autonomy;

/// <summary>
/// Defines the permitted degree of operational autonomy.
/// </summary>
public enum AutonomyLevel
{
    /// <summary>
    /// Read-only observation and evidence collection.
    /// </summary>
    L0Observe = 0,

    /// <summary>
    /// Diagnosis and recommendations without remediation execution.
    /// </summary>
    L1Recommend = 1,

    /// <summary>
    /// Bounded remediation requiring human approval.
    /// </summary>
    L2HumanApproved = 2,

    /// <summary>
    /// Bounded automatic remediation in eligible nonproduction environments.
    /// </summary>
    L3BoundedAutomatic = 3
}