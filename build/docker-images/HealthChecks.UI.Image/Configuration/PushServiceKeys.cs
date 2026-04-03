namespace HealthChecks.UI.Image.Configuration;

public static class PushServiceKeys
{
    public const string ENABLED = "enable_push_endpoint";
    public const string PUSH_ENDPOINT_SECRET = "push_endpoint_secret";
    public const int SERVICE_ADDED = 0;
    public const int SERVICE_UPDATED = 1;
    public const int SERVICE_REMOVED = 2;
    public const string AUTH_PARAMETER = "key";
}
