#nullable disable

namespace Quorum.Persistence.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <summary>
    /// Removes the two accounts that were seeded through <c>HasData</c> into every migration
    /// from <c>20230426123223_init</c> onward: <c>superadmin@gmail.com</c> and
    /// <c>basicuser@gmail.com</c>, which shared one fixed password hash whose plaintext was
    /// written in a source comment above it, and carried <c>EmailConfirmed = true</c> so
    /// they bypassed the <c>RequireConfirmedAccount</c> sign-in gate.
    /// <para>
    /// Role mappings are deleted before the users, because <c>AspNetUserRoles.UserId</c> is
    /// a foreign key onto <c>AspNetUsers.Id</c>.
    /// </para>
    /// <para>
    /// <b>This migration is deliberately not reversible.</b> <c>Down</c> would have to
    /// re-insert the compromised credentials, and a rollback that restores a known-password
    /// administrator is not a rollback anyone wants to be able to run by accident. Roll back
    /// past this point only by restoring a backup.
    /// </para>
    /// </summary>
    public partial class RemoveSeededIdentityAccounts : Migration
    {
        private const string SuperAdminUserId = "E97336F4-CF5A-4C72-8C61-997E5C621143";
        private const string BasicUserId = "B2BED4FF-47C0-47A1-9AE0-7AEF44CC14BB";

        private const string SuperAdminRoleId = "49FDD1CA-B802-4F9F-AC09-C8619FE90DF5";
        private const string AdminRoleId = "649B8428-A497-47CA-A5C4-953EE395F16E";
        private const string ModeratorRoleId = "91F6299F-59AC-4538-8DFD-4E891200E162";
        private const string BasicRoleId = "D17D4635-9015-422B-AE84-A399601DED81";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var (userId, roleId) in new[]
            {
                (BasicUserId, BasicRoleId),
                (SuperAdminUserId, SuperAdminRoleId),
                (SuperAdminUserId, AdminRoleId),
                (SuperAdminUserId, ModeratorRoleId),
                (SuperAdminUserId, BasicRoleId),
            })
            {
                migrationBuilder.DeleteData(
                    table: "AspNetUserRoles",
                    keyColumns: new[] { "UserId", "RoleId" },
                    keyValues: new object[] { userId, roleId });
            }

            // Rows these accounts may have accumulated in tables that key off the user id.
            // DeleteData targets the seeded primary keys only; anything created by a real
            // user is untouched. AspNetUsers is last so no foreign key is left dangling.
            foreach (var userId in new[] { SuperAdminUserId, BasicUserId })
            {
                migrationBuilder.Sql(
                    $"DELETE FROM [AspNetUserClaims] WHERE [UserId] = '{userId}';");
                migrationBuilder.Sql(
                    $"DELETE FROM [AspNetUserLogins] WHERE [UserId] = '{userId}';");
                migrationBuilder.Sql(
                    $"DELETE FROM [AspNetUserTokens] WHERE [UserId] = '{userId}';");
                migrationBuilder.Sql(
                    $"DELETE FROM [RefreshToken] WHERE [ApplicationUserId] = '{userId}';");
            }

            foreach (var userId in new[] { SuperAdminUserId, BasicUserId })
            {
                migrationBuilder.DeleteData(
                    table: "AspNetUsers",
                    keyColumn: "Id",
                    keyValue: userId);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            throw new NotSupportedException(
                "RemoveSeededIdentityAccounts is not reversible: reverting it would re-create " +
                "administrator accounts with a publicly known password. Restore from a backup " +
                "instead.");
        }
    }
}
