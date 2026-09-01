using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Quorum.Tests;

/// <summary>
/// Cover for the AutoMapper profiles, which nothing else reaches.
/// <para>
/// <c>AddAutoMapper</c> registers the configuration provider as a lazily-constructed
/// singleton: the <c>MapperConfiguration</c> is built the first time <c>IMapper</c> is
/// resolved, not during <c>builder.Build()</c>. So a broken profile does not fail the
/// build, does not fail startup — #48 boots the application without touching the profiles —
/// and surfaces at the first request that maps the type involved, as a configuration
/// exception in whatever endpoint that happens to be.
/// </para>
/// <para>
/// Resolving the mapper is what forces that construction, which is all this needs to do.
/// </para>
/// </summary>
public sealed class MapperConfigurationTests
{
    [Fact]
    public async Task The_mapper_configuration_the_application_registers_can_be_built()
    {
        await using var factory = new QuorumApplicationFactory();

        // Resolving through the running application, rather than newing up a
        // MapperConfiguration here, so this covers the profiles the application actually
        // discovers — AddAutoMapper scans AppDomain.CurrentDomain.GetAssemblies(), and a
        // hand-built list here would be a second opinion about which profiles exist.
        using var scope = factory.Services.CreateScope();

        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        Assert.NotNull(mapper.ConfigurationProvider);
    }
}
