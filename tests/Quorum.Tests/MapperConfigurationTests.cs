using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Quorum.Domain.Entities;
using Quorum.Shared.DTOs.Issue.Admin;
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

    [Fact]
    public async Task An_issue_carries_its_filer_onto_the_admin_dto()
    {
        await using var factory = new QuorumApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();

        var issue = new Issue
        {
            Title = "Rebuild the bridge",
            Question = "Should the bridge be rebuilt?",
            CreatedById = "user-42",
            CreatedByEmail = "filer@example.test",
        };

        var dto = mapper.Map<IssueAdminCreateDTO>(issue);

        // Neither destination name matches a source property, so these are populated only
        // because IssueProfile says so explicitly. They are what the admin console shows as
        // the person who filed an initiative — and they were the three members at risk from
        // the duplicate CreateMap this test accompanies the removal of, since the second
        // declaration mapped one of them and would have been silent about the other two.
        Assert.Equal("user-42", dto.ApplicationUserId);
        Assert.Equal("filer@example.test", dto.ApplicationUserEmail);
        Assert.Equal("Rebuild the bridge", dto.Title);
    }
}
