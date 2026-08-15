using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quorum.Infrastructure.Extension;
using Xunit;

namespace Quorum.Tests;

/// <summary>
/// The token-validation wiring must fail at startup, naming the missing setting, rather
/// than at the first request with a token nothing can validate. These pin the three
/// required settings and the happy path.
/// </summary>
public class AuthenticationExtensionsTests
{
    private static IConfiguration Config(params (string Key, string Value)[] values)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(values.ToDictionary(v => v.Key, v => (string?)v.Value))
            .Build();

    [Fact]
    public void A_missing_metadata_address_fails_startup_naming_the_setting()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            services.AddExternalJwtAuthentication(Config(("Auth:Issuer", "Quorum"), ("Auth:Audience", "Quorum"))));

        Assert.Contains("Auth:MetadataAddress", ex.Message);
    }

    [Fact]
    public void A_missing_issuer_fails_startup_naming_the_setting()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            services.AddExternalJwtAuthentication(Config(
                ("Auth:MetadataAddress", "https://auth.example/.well-known/openid-configuration"),
                ("Auth:Audience", "Quorum"))));

        Assert.Contains("Auth:Issuer", ex.Message);
    }

    [Fact]
    public void A_missing_audience_fails_startup_naming_the_setting()
    {
        var services = new ServiceCollection();

        var ex = Assert.Throws<InvalidOperationException>(() =>
            services.AddExternalJwtAuthentication(Config(
                ("Auth:MetadataAddress", "https://auth.example/.well-known/openid-configuration"),
                ("Auth:Issuer", "Quorum"))));

        Assert.Contains("Auth:Audience", ex.Message);
    }

    [Fact]
    public void A_complete_configuration_registers_authentication()
    {
        var services = new ServiceCollection();

        services.AddExternalJwtAuthentication(Config(
            ("Auth:MetadataAddress", "https://auth.example/.well-known/openid-configuration"),
            ("Auth:Issuer", "Quorum"),
            ("Auth:Audience", "Quorum")));

        Assert.Contains(services, d => d.ServiceType.FullName == "Microsoft.AspNetCore.Authentication.IAuthenticationService");
    }
}
