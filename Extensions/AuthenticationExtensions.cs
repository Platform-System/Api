using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Api.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var authOptions = configuration.GetAuthenticationOptions();

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authOptions.Authority;
                options.MetadataAddress = authOptions.MetadataAddress;
                options.RequireHttpsMetadata = authOptions.RequireHttpsMetadata;

                // Lấy metadata/JWKS qua URL nội bộ, nhưng vẫn ràng buộc token phải mang issuer hợp lệ.
                options.TokenValidationParameters.ValidateIssuer = true;
                options.TokenValidationParameters.ValidIssuers = authOptions.ValidIssuers.Distinct(StringComparer.OrdinalIgnoreCase);
                options.TokenValidationParameters.ValidateAudience = authOptions.VerifyTokenAudience;
                options.TokenValidationParameters.ValidAudience = authOptions.Resource;
                options.TokenValidationParameters.NameClaimType = AuthenticationConstants.PreferredUserNameClaim;
                options.TokenValidationParameters.RoleClaimType = ClaimTypes.Role;

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        if (context.Principal is not null)
                            KeycloakRoleClaimsMapper.Map(context.Principal);
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();
        services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, Api.Authorization.PermissionPolicyProvider>();

        return services;
    }

    public static WebApplication UseAuthenticationAndAuthorization(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        return app;
    }

    private static AuthenticationOptions GetAuthenticationOptions(this IConfiguration configuration)
    {
        var keycloakSection = configuration.GetSection(AuthenticationConstants.KeycloakSectionName);

        return new AuthenticationOptions
        {
            AuthServerUrl = GetRequiredValue(keycloakSection, AuthenticationConstants.AuthServerUrlKey).TrimEnd('/'),
            PublicAuthServerUrl = GetOptionalValue(keycloakSection, AuthenticationConstants.PublicAuthServerUrlKey)?.TrimEnd('/'),
            Realm = GetRequiredValue(keycloakSection, AuthenticationConstants.RealmKey),
            Resource = GetRequiredValue(keycloakSection, AuthenticationConstants.ResourceKey),
            VerifyTokenAudience = bool.TryParse(keycloakSection[AuthenticationConstants.VerifyTokenAudienceKey], out var parsedVerifyAudience) && parsedVerifyAudience,
            SslRequired = keycloakSection[AuthenticationConstants.SslRequiredKey] ?? AuthenticationConstants.SslRequiredNone
        };
    }

    private static string GetRequiredValue(IConfiguration section, string key)
    {
        var value = section[key];
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException($"{AuthenticationConstants.KeycloakSectionName}:{key} is not configured.");
    }

    private static string? GetOptionalValue(IConfiguration section, string key)
    {
        var value = section[key];
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
