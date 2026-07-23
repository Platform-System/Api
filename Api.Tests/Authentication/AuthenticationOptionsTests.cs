using Api.Authentication;
using Xunit;

namespace Api.Tests.Authentication;

public sealed class AuthenticationOptionsTests
{
    [Fact]
    public void ComputedProperties_WhenSslRequiredIsNone_ReturnExpectedValues()
    {
        var options = new AuthenticationOptions
        {
            AuthServerUrl = "http://localhost:8080/",
            PublicAuthServerUrl = "https://auth.example.com/",
            Realm = "platform",
            Resource = "gateway",
            VerifyTokenAudience = true,
            SslRequired = "none"
        };

        Assert.Equal("http://localhost:8080/realms/platform", options.Authority);
        Assert.Equal("https://auth.example.com/realms/platform", options.IssuerAuthority);
        Assert.Equal("http://localhost:8080/realms/platform/.well-known/openid-configuration", options.MetadataAddress);
        Assert.Equal(
            new[]
            {
                "http://localhost:8080/realms/platform",
                "https://auth.example.com/realms/platform"
            },
            options.ValidIssuers);
        Assert.False(options.RequireHttpsMetadata);
    }

    [Fact]
    public void ComputedProperties_WhenSslRequiredIsExternal_RequiresHttpsMetadata()
    {
        var options = new AuthenticationOptions
        {
            AuthServerUrl = "https://auth.example.com",
            Realm = "platform",
            Resource = "gateway",
            VerifyTokenAudience = false,
            SslRequired = "external"
        };

        Assert.True(options.RequireHttpsMetadata);
    }

    [Fact]
    public void ComputedProperties_WhenPublicAuthServerUrlMissing_FallsBackToInternalAuthority()
    {
        var options = new AuthenticationOptions
        {
            AuthServerUrl = "http://localhost:8080",
            Realm = "platform",
            Resource = "gateway",
            VerifyTokenAudience = false,
            SslRequired = "none"
        };

        Assert.Equal("http://localhost:8080/realms/platform", options.IssuerAuthority);
        Assert.Equal(
            new[]
            {
                "http://localhost:8080/realms/platform",
                "http://localhost:8080/realms/platform"
            },
            options.ValidIssuers);
    }
}
