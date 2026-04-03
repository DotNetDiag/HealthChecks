using HealthChecks.UI.Image.Configuration;

namespace HealthChecks.UI.Image.Extensions;

public static class HttpRequestExtensions
{
    public static bool IsAuthenticated(this HttpRequest request)
    {
        return request.Query.ContainsKey(PushServiceKeys.AUTH_PARAMETER) &&
            request.Query[PushServiceKeys.AUTH_PARAMETER] == Environment.GetEnvironmentVariable(PushServiceKeys.PUSH_ENDPOINT_SECRET);
    }
}
