using System.Net;

namespace HealthChecks.Minio.Tests.Helpers;

internal sealed class RecordingHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string? _reasonPhrase;

    public RecordingHttpMessageHandler(HttpStatusCode statusCode, string? reasonPhrase = null)
    {
        _statusCode = statusCode;
        _reasonPhrase = reasonPhrase;
    }

    public Uri? RequestUri { get; private set; }

    public HttpMethod? RequestMethod { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        RequestUri = request.RequestUri;
        RequestMethod = request.Method;

        return Task.FromResult(new HttpResponseMessage(_statusCode)
        {
            ReasonPhrase = _reasonPhrase
        });
    }
}
