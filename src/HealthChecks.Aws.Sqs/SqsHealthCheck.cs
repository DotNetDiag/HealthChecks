using Amazon.SQS;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Aws.Sqs;

public class SqsHealthCheck : IHealthCheck
{
    private readonly SqsOptions _options;

    public SqsHealthCheck(SqsOptions sqsOptions)
    {
        _options = Guard.ThrowIfNull(sqsOptions);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateSqsClient();
            foreach (string queueName in _options.Queues)
            {
                _ = await client.GetQueueUrlAsync(queueName, cancellationToken).ConfigureAwait(false);
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private AmazonSQSClient CreateSqsClient()
    {
        var config = new AmazonSQSConfig();

        // Support custom AWS-compatible endpoints such as LocalStack.
        // PR: https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/pull/2425
        // In AWS SDK v4, setting RegionEndpoint after ServiceURL nullifies ServiceURL.
        // Set RegionEndpoint first so that ServiceURL takes precedence when both are specified.
        if (_options.RegionEndpoint is not null)
        {
            config.RegionEndpoint = _options.RegionEndpoint;
        }

        if (_options.ServiceURL is not null)
        {
            config.ServiceURL = _options.ServiceURL;
        }

        return _options.Credentials is not null
            ? new AmazonSQSClient(_options.Credentials, config)
            : new AmazonSQSClient(config);
    }
}
