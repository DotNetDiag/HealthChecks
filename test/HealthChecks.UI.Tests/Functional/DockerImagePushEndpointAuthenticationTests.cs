#if NET10_0_OR_GREATER
using HealthChecks.UI.Image.Configuration;
using HealthChecks.UI.Image.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace HealthChecks.UI.Tests;

public class docker_image_push_endpoint_authentication_should
{
    [Fact]
    public void accept_the_push_token_header()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[PushServiceKeys.AUTH_HEADER] = "secret";

        context.Request.IsAuthenticated("secret").ShouldBeTrue();
    }

    [Fact]
    public void accept_the_push_bearer_token()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[HeaderNames.Authorization] = "Bearer secret";

        context.Request.IsAuthenticated("secret").ShouldBeTrue();
    }

    [Fact]
    public void keep_query_string_push_auth_compatible()
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?key=secret");

        context.Request.IsAuthenticated("secret").ShouldBeTrue();
    }

    [Fact]
    public void reject_invalid_push_tokens()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[PushServiceKeys.AUTH_HEADER] = "wrong";
        context.Request.QueryString = new QueryString("?key=also-wrong");

        context.Request.IsAuthenticated("secret").ShouldBeFalse();
    }
}
#endif
