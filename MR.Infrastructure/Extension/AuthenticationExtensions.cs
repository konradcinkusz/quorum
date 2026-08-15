using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace MR.Infrastructure.Extension;

/// <summary>
/// Token validation against an external identity service, per P5: exactly one service holds a
/// signing key, and every other service validates against that service's published JWKS.
/// <para>
/// MR holds <b>no key material at all</b> — not a signing key, not a shared secret. It can
/// verify a token and cannot mint one. That is the whole point of the principle, and it is the
/// property MR did not have while it ran its own IdentityServer.
/// </para>
/// <para>
/// Deliberately configuration-driven and free of any identity-provider-specific detail. The
/// only thing this needs is a service that publishes an OpenID discovery document; the signing
/// keys are then fetched from its JWKS and refreshed automatically on rotation, so a key roll
/// upstream needs no change or redeploy here. See
/// <c>docs/architecture/0001-identity-via-authservice.md</c>.
/// </para>
/// </summary>
public static class AuthenticationExtensions
{
    public const string SectionName = "Auth";

    public static IServiceCollection AddExternalJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);

        var metadataAddress = section["MetadataAddress"];
        if (string.IsNullOrWhiteSpace(metadataAddress))
        {
            // Fail at startup naming the setting, rather than at the first request with a
            // token that cannot be validated because there is nothing to validate against.
            throw new InvalidOperationException(
                $"{SectionName}:MetadataAddress is not configured. It must be the identity " +
                "service's OpenID discovery document, e.g. " +
                "https://<authservice-host>/.well-known/openid-configuration — supplied via " +
                $"user-secrets in development or {SectionName}__MetadataAddress in a deployed " +
                "environment.");
        }

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MetadataAddress = metadataAddress;

                // The metadata document is fetched over the network at startup and on refresh.
                // Permitting HTTP is a development affordance only, so a locally-run identity
                // service works without a certificate; it defaults to requiring HTTPS.
                options.RequireHttpsMetadata = !section.GetValue("AllowHttpMetadata", false);

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Both are required rather than merely validated-if-present: a token
                    // accepted without checking who issued it and who it was for is a token
                    // any other service's identity provider can mint.
                    ValidateIssuer = true,
                    ValidIssuer = Require(section, "Issuer"),

                    ValidateAudience = true,
                    ValidAudience = Require(section, "Audience"),

                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,

                    // The default is five minutes, which lets an expired token keep working
                    // for five more. Thirty seconds absorbs ordinary clock drift and no more.
                    ClockSkew = TimeSpan.FromSeconds(30),

                    // MR's authorization reads ClaimTypes.Role (the RequireAdminRole policy)
                    // and ClaimTypes.NameIdentifier (every ownership check added for F2).
                    // Stating the mapping here means a provider that spells its claims
                    // differently is a configuration change, not a hunt through the codebase.
                    RoleClaimType = section["RoleClaimType"] ?? ClaimTypes.Role,
                    NameClaimType = section["NameClaimType"] ?? ClaimTypes.Name,
                };
            });

        return services;
    }

    private static string Require(IConfigurationSection section, string key)
        => section[key] is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException(
                $"{SectionName}:{key} is not configured. It must match the value the identity " +
                "service puts in the tokens it issues.");
}
