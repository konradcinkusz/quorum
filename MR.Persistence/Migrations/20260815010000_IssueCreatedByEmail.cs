#nullable disable

namespace MR.Persistence.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <summary>
    /// Adds <c>Issues.CreatedByEmail</c> and backfills it from <c>AspNetUsers</c>.
    /// <para>
    /// Step 2 of the migration plan in
    /// <c>docs/architecture/0001-identity-via-authservice.md</c>. The backfill has to happen
    /// now, while identity still lives in this database: once <c>authservice</c> owns user
    /// accounts, the join below has nothing to join to, and the email of whoever filed each
    /// existing issue would be lost rather than merely stale.
    /// </para>
    /// </summary>
    public partial class IssueCreatedByEmail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedByEmail",
                schema: "MRBasics",
                table: "Issues",
                type: "nvarchar(max)",
                nullable: true);

            // Backfill from the local identity table. Left NULL where an issue has no creator
            // or the account has already gone — a missing email is honest, and the read paths
            // updated alongside this treat NULL as "unknown" rather than substituting anything.
            migrationBuilder.Sql(@"
                UPDATE i
                SET i.[CreatedByEmail] = u.[Email]
                FROM [MRBasics].[Issues] i
                INNER JOIN [dbo].[AspNetUsers] u ON u.[Id] = i.[CreatedById]
                WHERE i.[CreatedById] IS NOT NULL;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByEmail",
                schema: "MRBasics",
                table: "Issues");
        }
    }
}
