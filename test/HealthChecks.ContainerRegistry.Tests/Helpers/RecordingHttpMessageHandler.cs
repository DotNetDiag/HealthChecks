using System.Net;

namespace HealthChecks.ContainerRegistry.Tests.Helpers;

internal sealed class RecordingHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

    public RecordingHttpMessageHandler(HttpStatusCode statusCode)
        : this(_ => new HttpResponseMessage(statusCode))
    {
    }

    public RecordingHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        _responseFactory = responseFactory;
    }

    public Uri? RequestUri { get; private set; }

    public HttpMethod? RequestMethod { get; private set; }

    public string? AuthorizationHeader { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestUri = request.RequestUri;
        RequestMethod = request.Method;
        AuthorizationHeader = request.Headers.Authorization?.ToString();

        return Task.FromResult(_responseFactory(request));
    }
}
