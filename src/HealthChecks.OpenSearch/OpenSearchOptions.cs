using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace HealthChecks.OpenSearch;

/// <summary>
/// Options for <see cref="OpenSearchHealthCheck"/>.
/// </summary>
public class OpenSearchOptions
{
    public Uri? Uri { get; private set; }

    public string? UserName { get; private set; }

    public string? Password { get; private set; }

    public string? ApiKeyId { get; private set; }

    public string? ApiKey { get; private set; }

    public X509Certificate? Certificate { get; private set; }

    public bool AuthenticateWithBasicCredentials { get; private set; }

    public bool AuthenticateWithApiKey { get; private set; }

    public bool AuthenticateWithCertificate { get; private set; }

    public bool UseClusterHealthApi { get; set; }

    public Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool>? CertificateValidationCallback { get; private set; }

    public TimeSpan? RequestTimeout { get; set; }

    public OpenSearchOptions UseServer(string uri)
    {
        return UseServer(new Uri(Guard.ThrowIfNull(uri, throwOnEmptyString: true)));
    }

    public OpenSearchOptions UseServer(Uri uri)
    {
        Uri = Guard.ThrowIfNull(uri);

        return this;
    }

    public OpenSearchOptions UseBasicAuthentication(string userName, string password)
    {
        UserName = Guard.ThrowIfNull(userName, throwOnEmptyString: true);
        Password = Guard.ThrowIfNull(password, throwOnEmptyString: true);

        ApiKeyId = null;
        ApiKey = null;
        Certificate = null;
        AuthenticateWithApiKey = false;
        AuthenticateWithCertificate = false;
        AuthenticateWithBasicCredentials = true;
        return this;
    }

    public OpenSearchOptions UseApiKey(string id, string apiKey)
    {
        ApiKeyId = Guard.ThrowIfNull(id, throwOnEmptyString: true);
        ApiKey = Guard.ThrowIfNull(apiKey, throwOnEmptyString: true);

        UserName = null;
        Password = null;
        Certificate = null;
        AuthenticateWithBasicCredentials = false;
        AuthenticateWithCertificate = false;
        AuthenticateWithApiKey = true;

        return this;
    }

    public OpenSearchOptions UseCertificate(X509Certificate certificate)
    {
        Certificate = Guard.ThrowIfNull(certificate);

        UserName = null;
        Password = null;
        ApiKeyId = null;
        ApiKey = null;
        AuthenticateWithBasicCredentials = false;
        AuthenticateWithApiKey = false;
        AuthenticateWithCertificate = true;
        return this;
    }

    public OpenSearchOptions UseCertificateValidationCallback(Func<object, X509Certificate, X509Chain, SslPolicyErrors, bool> callback)
    {
        CertificateValidationCallback = Guard.ThrowIfNull(callback);
        return this;
    }
}
