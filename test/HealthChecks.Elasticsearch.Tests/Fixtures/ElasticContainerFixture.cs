using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;

namespace HealthChecks.Elasticsearch.Tests.Fixtures;

public class ElasticContainerFixture : IAsyncLifetime
{
    private static readonly TimeSpan StartTimeout = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(5);
    private static readonly Uri ElasticsearchServerUri = new("https://localhost:9200");

    public const string ELASTIC_PASSWORD = "abcDEF123!";
    private readonly ICompositeService _compositeService;

    public string? ApiKey { get; set; }

    public ElasticContainerFixture()
    {
        var composeFilePath = ResolveComposeFilePath();

        _compositeService = new Builder()
            .UseContainer()
            .UseCompose()
            .FromFile(composeFilePath)
            .ForceRecreate()
            .Build()
            .Start();
    }

    public string GetConnectionString() => ElasticsearchServerUri.ToString();

    public async Task InitializeAsync()
    {
        using var httpClient = CreateHttpClient();
        ApiKey = await SetApiKeyInElasticSearchAsync(httpClient).ConfigureAwait(false);
    }

    public Task DisposeAsync()
    {
        Dispose();
        return Task.CompletedTask;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _compositeService.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private static string ResolveComposeFilePath()
    {
        string[] candidatePaths =
        [
            Path.Combine(AppContext.BaseDirectory, "Resources", "docker-compose.yml"),
            Path.Combine(Directory.GetCurrentDirectory(), "Resources", "docker-compose.yml"),
            Path.Combine(Directory.GetCurrentDirectory(), "test", "HealthChecks.Elasticsearch.Tests", "Resources", "docker-compose.yml")
        ];

        return candidatePaths.FirstOrDefault(File.Exists)
            ?? throw new FileNotFoundException("Could not locate the Elasticsearch docker-compose.yml test resource.");
    }

    private static HttpClient CreateHttpClient()
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = delegate
            {
                return true;
            }
        };

        return new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    private async Task<string> SetApiKeyInElasticSearchAsync(HttpClient httpClient)
    {
        var elasticsearchApiKeyUri = new Uri(ElasticsearchServerUri, "/_security/api_key?pretty");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(
            Encoding.ASCII.GetBytes($"elastic:{ELASTIC_PASSWORD}")));

        var deadline = DateTime.UtcNow.Add(StartTimeout);
        HttpStatusCode? lastStatusCode = null;
        string? lastResponseBody = null;
        Exception? lastException = null;

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var response = await httpClient.PostAsJsonAsync(elasticsearchApiKeyUri,
                    new { name = "new-api-key", role_descriptors = new { } }).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var apiKeyResponse = await response.Content.ReadFromJsonAsync<ApiKeyResponse>().ConfigureAwait(false)
                        ?? throw new JsonException();

                    return apiKeyResponse.Encoded;
                }

                lastStatusCode = response.StatusCode;
                lastResponseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                lastException = null;
            }
            catch (HttpRequestException exception)
            {
                lastException = exception;
            }
            catch (TaskCanceledException exception)
            {
                lastException = exception;
            }

            await Task.Delay(RetryInterval).ConfigureAwait(false);
        }

        throw new InvalidOperationException(
            $"Timed out waiting for Elasticsearch API key creation after {StartTimeout}. Last status code: {lastStatusCode}, last response: {lastResponseBody}",
            lastException);
    }

    private record ApiKeyResponse(string Encoded);
}
