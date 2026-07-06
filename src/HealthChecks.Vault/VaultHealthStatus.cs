namespace HealthChecks.Vault;

/// <summary>
/// Represents the Vault node status reported by the system health endpoint.
/// </summary>
public enum VaultHealthStatus
{
    /// <summary>
    /// The Vault node is initialized, unsealed, and active.
    /// </summary>
    Active,

    /// <summary>
    /// The Vault node is initialized, unsealed, and in standby mode.
    /// </summary>
    Standby,

    /// <summary>
    /// The Vault node is an Enterprise performance standby node.
    /// </summary>
    PerformanceStandby,

    /// <summary>
    /// The Vault node is an Enterprise disaster recovery secondary node.
    /// </summary>
    DisasterRecoverySecondary,

    /// <summary>
    /// The Vault node is sealed.
    /// </summary>
    Sealed,

    /// <summary>
    /// The Vault node is not initialized.
    /// </summary>
    Uninitialized,

    /// <summary>
    /// The Vault node is in standby mode and cannot connect to the active node.
    /// </summary>
    HighAvailabilityUnhealthy,

    /// <summary>
    /// The Vault node was removed from the cluster.
    /// </summary>
    Removed,

    /// <summary>
    /// The Vault node status could not be identified.
    /// </summary>
    Unknown
}
