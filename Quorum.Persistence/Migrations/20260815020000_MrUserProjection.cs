#nullable disable

namespace Quorum.Persistence.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <summary>
    /// Creates <c>MRBasics.MrUsers</c> — MR's own projection of the users it serves — and
    /// backfills it from the local identity table.
    /// <para>
    /// Step towards ADR 0001. Like the <c>CreatedByEmail</c> backfill before it, this has to
    /// run while identity is still local: afterwards there is no <c>AspNetUsers</c> to read,
    /// and MR would start with an empty roster, meaning a newly opened quarter would issue
    /// signature pools to nobody until each user next signed in.
    /// </para>
    /// </summary>
    public partial class MrUserProjection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MrUsers",
                schema: "MRBasics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MrUsers", x => x.Id);
                });

            // Seed the roster from the accounts that exist today. CreatedAt is not available
            // on AspNetUsers, so both timestamps are stamped at migration time — these rows
            // record when MR first knew about a user, not when the account was created, and
            // for pre-existing accounts that is the moment of this migration.
            migrationBuilder.Sql(@"
                INSERT INTO [MRBasics].[MrUsers] ([Id], [Email], [FirstSeenAt], [LastSeenAt])
                SELECT u.[Id], u.[Email], SYSUTCDATETIME(), SYSUTCDATETIME()
                FROM [dbo].[AspNetUsers] u
                WHERE NOT EXISTS (
                    SELECT 1 FROM [MRBasics].[MrUsers] m WHERE m.[Id] = u.[Id]);");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "MrUsers", schema: "MRBasics");
        }
    }
}
