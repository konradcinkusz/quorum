#nullable disable

namespace Quorum.Persistence.Migrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    /// <summary>
    /// Renames <c>MRBasics.MrUsers</c> to <c>MRBasics.QuorumUsers</c>, following the project's
    /// rename from MR to Quorum.
    /// <para>
    /// The table is renamed here rather than by editing the migration that created it one
    /// commit earlier. That migration has already been merged and may already have been
    /// applied, and EF tracks applied migrations by id: rewriting it would leave any existing
    /// database with a table called <c>MrUsers</c> that no later migration ever touches,
    /// while the model looked for <c>QuorumUsers</c>. Silent divergence, discovered at the
    /// first query.
    /// </para>
    /// <para>
    /// The <c>MRBasics</c> / <c>MRPayments</c> / <c>MRDicts</c> <b>schemas</b> keep their
    /// names deliberately. They are referenced by seventeen historical migrations, they are
    /// invisible outside the database, and renaming them would buy nothing for the risk of a
    /// multi-table <c>ALTER SCHEMA … TRANSFER</c>.
    /// </para>
    /// </summary>
    public partial class RenameMrUsersToQuorumUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "MrUsers",
                schema: "MRBasics",
                newName: "QuorumUsers",
                newSchema: "MRBasics");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "QuorumUsers",
                schema: "MRBasics",
                newName: "MrUsers",
                newSchema: "MRBasics");
        }
    }
}
