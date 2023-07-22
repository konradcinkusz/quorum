using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MR.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class cloudinary_files : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CloudinaryFiles",
                schema: "MRBasics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublicId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SecureUri = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudinaryFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CloudinaryFileIssues",
                schema: "MRBasics",
                columns: table => new
                {
                    IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CloudinaryFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CloudinaryFileIssues", x => new { x.IssueId, x.CloudinaryFileId });
                    table.ForeignKey(
                        name: "FK_CloudinaryFileIssues_CloudinaryFiles_CloudinaryFileId",
                        column: x => x.CloudinaryFileId,
                        principalSchema: "MRBasics",
                        principalTable: "CloudinaryFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CloudinaryFileIssues_Issues_IssueId",
                        column: x => x.IssueId,
                        principalSchema: "MRBasics",
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CloudinaryFileIssues_CloudinaryFileId",
                schema: "MRBasics",
                table: "CloudinaryFileIssues",
                column: "CloudinaryFileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CloudinaryFileIssues",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "CloudinaryFiles",
                schema: "MRBasics");
        }
    }
}
