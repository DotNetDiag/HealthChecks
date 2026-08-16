using Apache.IoTDB.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.IoTDB;

/// <summary>
/// A health check for Apache IoTDB.
/// </summary>
public class IoTDBHealthCheck : IHealthCheck
{
    private readonly IoTDBHealthCheckOptions _options;

    public IoTDBHealthCheck(IoTDBHealthCheckOptions options)
    {
        _options = options;
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var builder = new IoTDBConnectionStringBuilder(_options.ConnectionString);
            var sessionPool = builder.CreateSession();

            await Task.Run(() => sessionPool.Open(_options.EnableRpcCompression, cancellationToken), cancellationToken).WaitAsync(cancellationToken).ConfigureAwait(false);
            var isOpen = sessionPool.IsOpen();
            await sessionPool.Close().ConfigureAwait(false);

            return isOpen
                ? HealthCheckResult.Healthy()
                : new HealthCheckResult(context.Registration.FailureStatus, description: "IoTDB session is not open.");
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, description: ex.Message, exception: ex);
        }
    }
}
