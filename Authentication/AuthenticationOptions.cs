namespace Api.Authentication;

public sealed class AuthenticationOptions
{
    public required string AuthServerUrl { get; init; }
    public string? PublicAuthServerUrl { get; init; }
    public required string Realm { get; init; }
    public required string Resource { get; init; }
    public required bool VerifyTokenAudience { get; init; }
    public required string SslRequired { get; init; }

    public string Authority => $"{AuthServerUrl.TrimEnd('/')}/realms/{Realm}";
    public string IssuerAuthority => $"{(PublicAuthServerUrl ?? AuthServerUrl).TrimEnd('/')}/realms/{Realm}";
    public string MetadataAddress => $"{Authority}/.well-known/openid-configuration";
    public string[] ValidIssuers => [Authority, IssuerAuthority];
    public bool RequireHttpsMetadata => !string.Equals(SslRequired, AuthenticationConstants.SslRequiredNone, StringComparison.OrdinalIgnoreCase);
}
