using dotnet_etcd;
using Etcdserverpb;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Etcd;

/// <summary>
/// A health check for etcd services.
/// </summary>
public sealed class EtcdHealthCheck : IHealthCheck
{
    private readonly EtcdClient? _client;
    private readonly string? _connectionString;
    private readonly EtcdHealthCheckOptions _options;

    /// <summary>
    /// Creates a new instance of the etcd health check.
    /// </summary>
    /// <param name="client">The etcd client.</param>
    public EtcdHealthCheck(EtcdClient client)
    {
        _client = Guard.ThrowIfNull(client);
        _options = new EtcdHealthCheckOptions();
    }

    /// <summary>
    /// Creates a new instance of the etcd health check.
    /// </summary>
    /// <param name="connectionString">The etcd connection string.</param>
    /// <param name="options">Optional settings used by the health check.</param>
    public EtcdHealthCheck(string connectionString, EtcdHealthCheckOptions? options = default)
    {
        _connectionString = Guard.ThrowIfNull(connectionString);
        _options = options ?? new EtcdHealthCheckOptions();
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (_client is not null)
            {
                return await CheckClientAsync(_client, context, cancellationToken).ConfigureAwait(false);
            }

            ValidateOptions();

            using var client = CreateClient();
            return await CheckClientAsync(client, context, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private static async Task<HealthCheckResult> CheckClientAsync(
        EtcdClient client,
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        StatusResponse response = await client.StatusAsync(new StatusRequest(), cancellationToken: cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Empty response from etcd client.");

        if (response.Errors.Count > 0)
        {
            return new HealthCheckResult(
                context.Registration.FailureStatus,
                description: "etcd status reported errors.",
                data: new Dictionary<string, object>
                {
                    { "version", response.Version },
                    { "leader", response.Leader },
                    { "errors", response.Errors.ToArray() }
                });
        }

        return HealthCheckResult.Healthy();
    }

    private EtcdClient CreateClient()
    {
        string connectionString = _connectionString!;
        string? userName = _options.UserName;

        if (!string.IsNullOrEmpty(userName))
        {
            return new EtcdClient(
                connectionString,
                userName,
                _options.Password!,
                _options.Port,
                _options.ServerName,
                _options.ConfigureChannelOptions,
                _options.TokenCacheDuration);
        }

        if (_options.ConfigureSslOptions is not null)
        {
            return new EtcdClient(
                connectionString,
                _options.ConfigureSslOptions,
                _options.Port,
                _options.ServerName,
                _options.ConfigureChannelOptions,
                _options.Interceptors);
        }

        return new EtcdClient(
            connectionString,
            _options.Port,
            _options.ServerName,
            _options.ConfigureChannelOptions,
            _options.Interceptors);
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException($"{nameof(_connectionString)} must be configured.");
        }

        if (_options.Port <= 0 || _options.Port > 65535)
        {
            throw new InvalidOperationException($"{nameof(EtcdHealthCheckOptions.Port)} must be between 1 and 65535.");
        }

        if (string.IsNullOrWhiteSpace(_options.ServerName))
        {
            throw new InvalidOperationException($"{nameof(EtcdHealthCheckOptions.ServerName)} must be configured.");
        }

        bool hasUserName = !string.IsNullOrEmpty(_options.UserName);
        bool hasPassword = !string.IsNullOrEmpty(_options.Password);

        if (hasUserName != hasPassword)
        {
            throw new InvalidOperationException($"{nameof(EtcdHealthCheckOptions.UserName)} and {nameof(EtcdHealthCheckOptions.Password)} must be configured together.");
        }

        if (hasUserName && (_options.ConfigureSslOptions is not null || _options.Interceptors is not null))
        {
            throw new InvalidOperationException(
                $"{nameof(EtcdHealthCheckOptions.ConfigureSslOptions)} and {nameof(EtcdHealthCheckOptions.Interceptors)} are not supported with username/password options. Register an EtcdClient instance for advanced authenticated connections.");
        }
    }
}
