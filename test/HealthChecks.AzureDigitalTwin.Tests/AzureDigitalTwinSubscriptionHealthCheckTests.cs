using System.Net;
using System.Text;
using Azure;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.ResourceManager;

namespace HealthChecks.AzureDigitalTwin.Tests;

public class azure_digital_twin_subscription_healthcheck_should
{
    private const string RESOURCE_ID = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/test/providers/Microsoft.DigitalTwins/digitalTwinsInstances/test-twin";
    private const string OTHER_RESOURCE_ID = "/subscriptions/00000000-0000-0000-0000-000000000000/resourceGroups/other/providers/Microsoft.DigitalTwins/digitalTwinsInstances/other-twin";

    [Fact]
    public async Task return_healthy_after_reading_the_configured_resource()
    {
        using var fixture = new ManagementClientFixture();

        var result = await fixture.HealthCheck.CheckHealthAsync(CreateContext(fixture.HealthCheck));

        result.Status.ShouldBe(HealthStatus.Healthy);
        result.Exception.ShouldBeNull();
        var request = fixture.Handler.Requests.ShouldHaveSingleItem();
        request.Host.ShouldBe("management.azure.com");
        request.AbsolutePath.ShouldBe(RESOURCE_ID);
        fixture.Handler.Methods.ShouldHaveSingleItem().ShouldBe(HttpMethod.Get);
        fixture.Handler.AuthorizationHeaders.ShouldHaveSingleItem().ShouldBe("Bearer offline-test-token");
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound, HealthStatus.Unhealthy)]
    [InlineData(HttpStatusCode.NotFound, HealthStatus.Degraded)]
    [InlineData(HttpStatusCode.Unauthorized, HealthStatus.Degraded)]
    [InlineData(HttpStatusCode.Forbidden, HealthStatus.Unhealthy)]
    public async Task return_configured_failure_status_when_resource_read_fails(HttpStatusCode responseStatus, HealthStatus failureStatus)
    {
        using var fixture = new ManagementClientFixture(responseStatus);

        var result = await fixture.HealthCheck.CheckHealthAsync(CreateContext(fixture.HealthCheck, failureStatus));

        result.Status.ShouldBe(failureStatus);
        result.Exception.ShouldBeOfType<RequestFailedException>().Status.ShouldBe((int)responseStatus);
        fixture.Handler.Requests.ShouldHaveSingleItem().AbsolutePath.ShouldBe(RESOURCE_ID);
    }

    [Fact]
    public async Task probe_the_resource_on_every_invocation()
    {
        using var fixture = new ManagementClientFixture();
        var context = CreateContext(fixture.HealthCheck);

        var firstResult = await fixture.HealthCheck.CheckHealthAsync(context);
        fixture.Handler.ResponseStatus = HttpStatusCode.NotFound;
        var secondResult = await fixture.HealthCheck.CheckHealthAsync(context);

        firstResult.Status.ShouldBe(HealthStatus.Healthy);
        secondResult.Status.ShouldBe(HealthStatus.Unhealthy);
        secondResult.Exception.ShouldBeOfType<RequestFailedException>().Status.ShouldBe((int)HttpStatusCode.NotFound);
        fixture.Handler.Requests.Select(request => request.AbsolutePath).ShouldBe([RESOURCE_ID, RESOURCE_ID]);
    }

    [Fact]
    public async Task probe_each_target_when_the_management_client_is_shared()
    {
        using var fixture = new ManagementClientFixture();
        var otherHealthCheck = fixture.CreateHealthCheck(OTHER_RESOURCE_ID);

        var firstResult = await fixture.HealthCheck.CheckHealthAsync(CreateContext(fixture.HealthCheck));
        var otherResult = await otherHealthCheck.CheckHealthAsync(CreateContext(otherHealthCheck));

        firstResult.Status.ShouldBe(HealthStatus.Healthy);
        otherResult.Status.ShouldBe(HealthStatus.Healthy);
        fixture.Handler.Requests.Select(request => request.AbsolutePath).ShouldBe([RESOURCE_ID, OTHER_RESOURCE_ID]);
    }

    [Fact]
    public async Task forward_cancellation_to_the_resource_request()
    {
        using var fixture = new ManagementClientFixture();
        using var tokenSource = new CancellationTokenSource();
        fixture.Handler.WaitForCancellation = true;

        var pendingResult = fixture.HealthCheck.CheckHealthAsync(CreateContext(fixture.HealthCheck), tokenSource.Token);
        await fixture.Handler.RequestStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
        tokenSource.Cancel();
        var result = await pendingResult.WaitAsync(TimeSpan.FromSeconds(10));

        fixture.Handler.RequestCancellationToken.IsCancellationRequested.ShouldBeTrue();
        result.Status.ShouldBe(HealthStatus.Unhealthy);
        result.Exception.ShouldBeAssignableTo<OperationCanceledException>();
    }

    private static HealthCheckContext CreateContext(IHealthCheck healthCheck, HealthStatus failureStatus = HealthStatus.Unhealthy)
        => new()
        {
            Registration = new HealthCheckRegistration("target-twin", healthCheck, failureStatus, null)
        };

    private sealed class ManagementClientFixture : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly TokenCredential _credential = new OfflineTokenCredential();
        private readonly ArmClient _managementClient;

        public ManagementClientFixture(HttpStatusCode responseStatus = HttpStatusCode.OK)
        {
            Handler = new RecordingManagementHandler { ResponseStatus = responseStatus };
            _httpClient = new HttpClient(Handler);
            var options = new ArmClientOptions
            {
                Transport = new HttpClientTransport(_httpClient)
            };
            options.Retry.MaxRetries = 0;
            _managementClient = new ArmClient(_credential, defaultSubscriptionId: null, options: options);
            HealthCheck = CreateHealthCheck(RESOURCE_ID);
        }

        public RecordingManagementHandler Handler { get; }

        public TestHealthCheck HealthCheck { get; }

        public TestHealthCheck CreateHealthCheck(string resourceId)
            => new(_credential, resourceId, _managementClient);

        public void Dispose()
        {
            HealthCheck.RemoveCachedClient();
            _httpClient.Dispose();
        }
    }

    private sealed class TestHealthCheck : AzureDigitalTwinSubscriptionHealthCheck
    {
        private readonly string _connectionKey;

        public TestHealthCheck(TokenCredential credential, string resourceId, ArmClient managementClient)
            : base(credential, resourceId)
        {
            _connectionKey = credential.GetHashCode().ToString();
            ManagementClientConnections[_connectionKey] = managementClient;
        }

        public void RemoveCachedClient()
            => ManagementClientConnections.TryRemove(_connectionKey, out _);
    }

    private sealed class OfflineTokenCredential : TokenCredential
    {
        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
            => new("offline-test-token", DateTimeOffset.UtcNow.AddHours(1));

        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
            => ValueTask.FromResult(GetToken(requestContext, cancellationToken));
    }

    private sealed class RecordingManagementHandler : HttpMessageHandler
    {
        public List<Uri> Requests { get; } = [];

        public List<HttpMethod> Methods { get; } = [];

        public List<string?> AuthorizationHeaders { get; } = [];

        public HttpStatusCode ResponseStatus { get; set; } = HttpStatusCode.OK;

        public bool WaitForCancellation { get; set; }

        public CancellationToken RequestCancellationToken { get; private set; }

        public TaskCompletionSource RequestStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request.RequestUri!);
            Methods.Add(request.Method);
            AuthorizationHeaders.Add(request.Headers.Authorization?.ToString());
            RequestCancellationToken = cancellationToken;
            RequestStarted.TrySetResult();

            if (WaitForCancellation)
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }

            string content = ResponseStatus == HttpStatusCode.OK
                ? $$$"""
                    {"id":"{{{request.RequestUri!.AbsolutePath}}}","name":"test-twin","type":"Microsoft.DigitalTwins/digitalTwinsInstances","location":"westus2","properties":{"hostName":"test-twin.api.wus2.digitaltwins.azure.net","provisioningState":"Succeeded"}}
                    """
                : """{"error":{"code":"ResourceNotFound","message":"Resource missing or unauthorized."}}""";

            return new HttpResponseMessage(ResponseStatus)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };
        }
    }
}
