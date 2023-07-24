using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MR.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CloudinaryFileIssueType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CloudinaryFileIssueType",
                schema: "MRBasics",
                table: "CloudinaryFileIssues",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloudinaryFileIssueType",
                schema: "MRBasics",
                table: "CloudinaryFileIssues");
        }
    }
}
