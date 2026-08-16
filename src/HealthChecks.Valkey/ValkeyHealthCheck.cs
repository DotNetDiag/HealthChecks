using System.Collections.Concurrent;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace HealthChecks.Valkey;

/// <summary>
/// A health check for Valkey services.
/// </summary>
public sealed class ValkeyHealthCheck : IHealthCheck
{
    private static readonly ConcurrentDictionary<string, IConnectionMultiplexer> _connections = new();

    private readonly string? _valkeyConnectionString;
    private readonly IConnectionMultiplexer? _connectionMultiplexer;
    private readonly Func<IConnectionMultiplexer>? _connectionMultiplexerFactory;

    /// <summary>
    /// Creates an instance of <see cref="ValkeyHealthCheck"/>.
    /// </summary>
    /// <param name="valkeyConnectionString">The Valkey connection string to be used.</param>
    public ValkeyHealthCheck(string valkeyConnectionString)
    {
        _valkeyConnectionString = Guard.ThrowIfNull(valkeyConnectionString, throwOnEmptyString: true);
    }

    /// <summary>
    /// Creates an instance of <see cref="ValkeyHealthCheck"/>.
    /// </summary>
    /// <param name="connectionMultiplexer">The Valkey connection to be used.</param>
    public ValkeyHealthCheck(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = Guard.ThrowIfNull(connectionMultiplexer);
    }

    /// <summary>
    /// Creates an instance of <see cref="ValkeyHealthCheck"/> that calls provided factory when needed for the first time.
    /// </summary>
    /// <param name="connectionMultiplexerFactory">The factory method that connects to Valkey.</param>
    /// <remarks>
    /// A call to <see cref="ConnectionMultiplexer.Connect(ConfigurationOptions, TextWriter?)"/> throws when it is not
    /// possible to connect to the Valkey server(s). The call should not be invoked when <see cref="HealthCheckRegistration"/>
    /// is created, but when <see cref="ValkeyHealthCheck"/> needs the <see cref="IConnectionMultiplexer"/> for the first time,
    /// so exceptions can be handled gracefully.
    /// </remarks>
    internal ValkeyHealthCheck(Func<IConnectionMultiplexer> connectionMultiplexerFactory)
    {
        _connectionMultiplexerFactory = Guard.ThrowIfNull(connectionMultiplexerFactory);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            IConnectionMultiplexer? connection = _connectionMultiplexer ?? _connectionMultiplexerFactory?.Invoke();

            if (_valkeyConnectionString is not null && !_connections.TryGetValue(_valkeyConnectionString, out connection))
            {
                try
                {
                    Task<ConnectionMultiplexer> connectionMultiplexerTask = ConnectionMultiplexer.ConnectAsync(_valkeyConnectionString);
                    connection = await TimeoutAsync(connectionMultiplexerTask, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    return new HealthCheckResult(context.Registration.FailureStatus, description: "Healthcheck timed out");
                }

                if (!_connections.TryAdd(_valkeyConnectionString, connection))
                {
                    connection.Dispose();
                    connection = _connections[_valkeyConnectionString];
                }
            }

            foreach (var endPoint in connection!.GetEndPoints(configuredOnly: true))
            {
                IServer server = connection.GetServer(endPoint);

                if (server.ServerType != ServerType.Cluster)
                {
                    await connection.GetDatabase().PingAsync().ConfigureAwait(false);
                    await server.PingAsync().ConfigureAwait(false);
                }
                else
                {
                    RedisResult clusterInfo = await server.ExecuteAsync("CLUSTER", "INFO").ConfigureAwait(false);

                    if (clusterInfo is object && !clusterInfo.IsNull)
                    {
                        if (!clusterInfo.ToString()!.Contains("cluster_state:ok"))
                        {
                            return new HealthCheckResult(
                                context.Registration.FailureStatus,
                                description: $"INFO CLUSTER is not on OK state for endpoint {endPoint}");
                        }
                    }
                    else
                    {
                        return new HealthCheckResult(
                            context.Registration.FailureStatus,
                            description: $"INFO CLUSTER is null or can't be read for endpoint {endPoint}");
                    }
                }
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            if (_valkeyConnectionString is not null)
            {
                _connections.TryRemove(_valkeyConnectionString, out IConnectionMultiplexer? connection);
#pragma warning disable IDISP007 // Don't dispose injected [false positive here]
                connection?.Dispose();
#pragma warning restore IDISP007 // Don't dispose injected
            }

            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    // Remove when https://github.com/StackExchange/StackExchange.Redis/issues/1039 is done.
    private static async Task<ConnectionMultiplexer> TimeoutAsync(Task<ConnectionMultiplexer> task, CancellationToken cancellationToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task completedTask = await Task
            .WhenAny(task, Task.Delay(Timeout.Infinite, timeoutCts.Token))
            .ConfigureAwait(false);

        if (completedTask == task)
        {
            timeoutCts.Cancel();
            return await task.ConfigureAwait(false);
        }

        cancellationToken.ThrowIfCancellationRequested();
        throw new OperationCanceledException();
    }
}

