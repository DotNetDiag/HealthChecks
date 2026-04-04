using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.Aws.Sns;

public class SnsTopicAndSubscriptionHealthCheck : IHealthCheck
{
    private readonly SnsOptions _options;

    public SnsTopicAndSubscriptionHealthCheck(SnsOptions snsOptions)
    {
        _options = Guard.ThrowIfNull(snsOptions);
    }

    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = CreateSnsClient();

            foreach (var (topicName, subscriptions) in _options.TopicsAndSubscriptions.Select(x => (x.Key, x.Value)))
            {
                var topic = await FindTopicByNameAsync(client, topicName, cancellationToken).ConfigureAwait(false)
                    ?? throw new NotFoundException($"Topic {topicName} does not exist.");

                if (subscriptions.Count == 0)
                {
                    continue;
                }

                var subscriptionsArn = await GetSubscriptionArnsAsync(client, topic.TopicArn, cancellationToken).ConfigureAwait(false);

                foreach (string subscription in subscriptions)
                {
                    if (!subscriptionsArn.Contains(subscription))
                    {
                        throw new NotFoundException($"Subscription {subscription} in Topic {topicName} does not exist.");
                    }
                }
            }

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, exception: ex);
        }
    }

    private static async Task<Topic?> FindTopicByNameAsync(
        IAmazonSimpleNotificationService client,
        string topicName,
        CancellationToken cancellationToken)
    {
        string? nextToken = null;

        do
        {
            var response = await client.ListTopicsAsync(new ListTopicsRequest
            {
                NextToken = nextToken
            }, cancellationToken).ConfigureAwait(false);

            var topic = response.Topics.FirstOrDefault(item => TopicMatches(item.TopicArn, topicName));
            if (topic is not null)
            {
                return topic;
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrEmpty(nextToken));

        return null;
    }

    private static async Task<HashSet<string>> GetSubscriptionArnsAsync(
        IAmazonSimpleNotificationService client,
        string topicArn,
        CancellationToken cancellationToken)
    {
        string? nextToken = null;
        var subscriptionArns = new HashSet<string>(StringComparer.Ordinal);

        do
        {
            var response = await client.ListSubscriptionsByTopicAsync(new ListSubscriptionsByTopicRequest
            {
                TopicArn = topicArn,
                NextToken = nextToken
            }, cancellationToken).ConfigureAwait(false);

            foreach (var subscription in response.Subscriptions)
            {
                if (!string.IsNullOrEmpty(subscription.SubscriptionArn))
                {
                    subscriptionArns.Add(subscription.SubscriptionArn);
                }
            }

            nextToken = response.NextToken;
        }
        while (!string.IsNullOrEmpty(nextToken));

        return subscriptionArns;
    }

    private static bool TopicMatches(string topicArn, string topicName)
    {
        if (string.Equals(topicArn, topicName, StringComparison.Ordinal))
        {
            return true;
        }

        var topicNameSeparatorIndex = topicArn.LastIndexOf(':');
        if (topicNameSeparatorIndex < 0 || topicNameSeparatorIndex == topicArn.Length - 1)
        {
            return false;
        }

        return string.Equals(topicArn.Substring(topicNameSeparatorIndex + 1), topicName, StringComparison.Ordinal);
    }

    private AmazonSimpleNotificationServiceClient CreateSnsClient()
    {
        var config = new AmazonSimpleNotificationServiceConfig();

        // Support custom AWS-compatible endpoints such as LocalStack.
        // PR: https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/pull/2438
        if (_options.ServiceURL is not null)
        {
            config.ServiceURL = _options.ServiceURL;
        }

        if (_options.RegionEndpoint is not null)
        {
            config.RegionEndpoint = _options.RegionEndpoint;
        }

        return _options.Credentials is not null
            ? new AmazonSimpleNotificationServiceClient(_options.Credentials, config)
            : new AmazonSimpleNotificationServiceClient(config);
    }
}
