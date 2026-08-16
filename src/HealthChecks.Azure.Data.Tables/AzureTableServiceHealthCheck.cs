using Azure.Data.Tables;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Azure.Data.Tables;

/// <summary>
/// Azure Tables health check.
/// </summary>
public sealed class AzureTableServiceHealthCheck : IHealthCheck
{
    private const string PROBE_PARTITION_KEY = "__dotnetdiag_healthchecks_probe_partition__";
    private const string PROBE_ROW_KEY = "__dotnetdiag_healthchecks_probe_row__";
    private const string PROBE_TABLE_NAME = "__dotnetdiag_healthchecks_probe_table__";
    private static readonly string _tableServiceProbeFilter = TableServiceClient.CreateQueryFilter(item => item.Name == PROBE_TABLE_NAME);
    private static readonly string _tableEntityProbeFilter = TableClient.CreateQueryFilter<TableEntity>(entity =>
        entity.PartitionKey == PROBE_PARTITION_KEY && entity.RowKey == PROBE_ROW_KEY);
    private static readonly string[] _tableEntityProbeSelectColumns = [nameof(TableEntity.PartitionKey), nameof(TableEntity.RowKey)];

    private readonly TableServiceClient _tableServiceClient;
    private readonly AzureTableServiceHealthCheckOptions _options;

    /// <summary>
    /// Creates new instance of Azure Tables health check.
    /// </summary>
    /// <param name="tableServiceClient">
    /// The <see cref="TableServiceClient"/> used to perform Azure Tables operations.
    /// Azure SDK recommends treating clients as singletons <see href="https://devblogs.microsoft.com/azure-sdk/lifetime-management-and-thread-safety-guarantees-of-azure-sdk-net-clients/"/>,
    /// so this should be the exact same instance used by other parts of the application.
    /// </param>
    /// <param name="options">Optional settings used by the health check.</param>
    public AzureTableServiceHealthCheck(TableServiceClient tableServiceClient, AzureTableServiceHealthCheckOptions? options)
    {
        _tableServiceClient = Guard.ThrowIfNull(tableServiceClient);
        _options = options ?? new();
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!string.IsNullOrEmpty(_options.TableName))
            {
                // Note: PoLP (Principle of least privilege)
                // This can be used having at least the role assignment "Storage Table Data Reader" at table level.
                // A raw filter like "false" can be rejected by Azure Tables, so probe with a valid sentinel key filter instead.
                // Issue: https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/2445
                var tableClient = _tableServiceClient.GetTableClient(_options.TableName);
                await tableClient
                    .QueryAsync<TableEntity>(
                        filter: _tableEntityProbeFilter,
                        maxPerPage: 1,
                        select: _tableEntityProbeSelectColumns,
                        cancellationToken: cancellationToken)
                    .GetAsyncEnumerator(cancellationToken)
                    .MoveNextAsync()
                    .ConfigureAwait(false);
            }
            else
            {
                // Note: TableServiceClient.GetPropertiesAsync() cannot be used with only the role assignment
                // "Storage Table Data Contributor," so TableServiceClient.QueryAsync() and
                // TableClient.QueryAsync<T>() are used instead to probe service health.
                // Note: PoLP (Principle of least privilege)
                // This can can be used with only the role assignment "Storage Table Data Reader" at storage account level.
                // A raw filter like "false" can be rejected by Azure Tables, so probe with a valid sentinel table-name filter instead.
                // Issue: https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/2445
                await _tableServiceClient
                    .QueryAsync(
                        filter: _tableServiceProbeFilter,
                        maxPerPage: 1,
                        cancellationToken: cancellationToken)
                    .GetAsyncEnumerator(cancellationToken)
                    .MoveNextAsync()
                    .ConfigureAwait(false);
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }
}
