namespace MR.Persistence.Seeds;

public static class ContextSeed
{
    /// <summary>
    /// Seeds the role taxonomy, and nothing else.
    /// <para>
    /// This used to also seed two accounts — <c>superadmin@gmail.com</c> and
    /// <c>basicuser@gmail.com</c> — with a shared, fixed password hash whose plaintext was
    /// written in a comment directly above it, and with <c>EmailConfirmed = true</c> so
    /// they bypassed the <c>RequireConfirmedAccount</c> gate. Anyone reaching the login
    /// page of any environment ever created from these migrations could sign in as
    /// superadmin. Those rows and their role mappings are removed by the
    /// <c>RemoveSeededIdentityAccounts</c> migration.
    /// </para>
    /// <para>
    /// The first administrator is now provisioned out of band. Do not re-add account seed
    /// data here: a credential that ships in a migration is a credential in every
    /// environment built from it, forever.
    /// </para>
    /// </summary>
    public static void Seed(this ModelBuilder modelBuilder)
    {
        CreateRoles(modelBuilder);
    }

    private static void CreateRoles(ModelBuilder modelBuilder)
    {
        List<IdentityRole> roles = DefaultRoles.IdentityRoleList();
        modelBuilder.Entity<IdentityRole>().HasData(roles);
    }
}
