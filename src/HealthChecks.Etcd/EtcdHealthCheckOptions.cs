using System.Net.Security;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;

namespace HealthChecks.Etcd;

/// <summary>
/// Represents settings used by <see cref="EtcdHealthCheck"/>.
/// </summary>
public sealed class EtcdHealthCheckOptions
{
    /// <summary>
    /// Gets or sets the etcd client port.
    /// </summary>
    public int Port { get; set; } = 2379;

    /// <summary>
    /// Gets or sets the server name used when the etcd client creates secure channels.
    /// </summary>
    public string ServerName { get; set; } = "my-etcd-server";

    /// <summary>
    /// Gets or sets the optional etcd username used for authenticated connections.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Gets or sets the optional etcd password used for authenticated connections.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets the optional authentication token cache duration.
    /// </summary>
    public TimeSpan? TokenCacheDuration { get; set; }

    /// <summary>
    /// Gets or sets a callback that configures the gRPC channel options.
    /// </summary>
    public Action<GrpcChannelOptions>? ConfigureChannelOptions { get; set; }

    /// <summary>
    /// Gets or sets a callback that configures TLS options for the gRPC channel.
    /// </summary>
    public Action<SslClientAuthenticationOptions>? ConfigureSslOptions { get; set; }

    /// <summary>
    /// Gets or sets optional gRPC interceptors used by the etcd client.
    /// </summary>
    public Interceptor[]? Interceptors { get; set; }
}
