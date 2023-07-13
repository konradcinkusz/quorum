using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MR.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class quarter_winner_resolved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "QuarterResolved",
                schema: "MRBasics",
                table: "Quarters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "QuarterWinner",
                schema: "MRBasics",
                table: "QuarterIssues",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuarterResolved",
                schema: "MRBasics",
                table: "Quarters");

            migrationBuilder.DropColumn(
                name: "QuarterWinner",
                schema: "MRBasics",
                table: "QuarterIssues");
        }
    }
}
