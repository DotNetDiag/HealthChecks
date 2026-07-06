using System.Security.Cryptography;
using System.Text;
using HealthChecks.UI.Image.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace HealthChecks.UI.Image.Extensions;

public static class HttpRequestExtensions
{
    public static bool IsAuthenticated(this HttpRequest request, string expectedSecret)
    {
        if (string.IsNullOrEmpty(expectedSecret))
        {
            return false;
        }

        return IsValidSecret(GetHeaderToken(request), expectedSecret) ||
            IsValidSecret(GetBearerToken(request), expectedSecret) ||
            IsValidSecret(request.Query[PushServiceKeys.AUTH_PARAMETER], expectedSecret);
    }

    private static string? GetHeaderToken(HttpRequest request)
    {
        return request.Headers.TryGetValue(PushServiceKeys.AUTH_HEADER, out var values)
            ? values.FirstOrDefault()
            : null;
    }

    private static string? GetBearerToken(HttpRequest request)
    {
        if (!request.Headers.TryGetValue(HeaderNames.Authorization, out var values))
        {
            return null;
        }

        var authorization = values.FirstOrDefault();

        if (authorization?.StartsWith($"{PushServiceKeys.AUTH_SCHEME} ", StringComparison.OrdinalIgnoreCase) != true)
        {
            return null;
        }

        return authorization[PushServiceKeys.AUTH_SCHEME.Length..].Trim();
    }

    private static bool IsValidSecret(StringValues suppliedSecret, string expectedSecret)
    {
        return suppliedSecret.Count == 1 && IsValidSecret(suppliedSecret[0], expectedSecret);
    }

    private static bool IsValidSecret(string? suppliedSecret, string expectedSecret)
    {
        if (string.IsNullOrEmpty(suppliedSecret))
        {
            return false;
        }

        var suppliedBytes = Encoding.UTF8.GetBytes(suppliedSecret);
        var expectedBytes = Encoding.UTF8.GetBytes(expectedSecret);

        return suppliedBytes.Length == expectedBytes.Length &&
            CryptographicOperations.FixedTimeEquals(suppliedBytes, expectedBytes);
    }
}
