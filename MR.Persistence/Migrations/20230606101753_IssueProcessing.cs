using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MR.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IssueProcessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueStatusHistories",
                schema: "MRBasics");

            migrationBuilder.RenameColumn(
                name: "IssueStatus",
                schema: "MRBasics",
                table: "Issues",
                newName: "IssueVisibility");

            migrationBuilder.AddColumn<int>(
                name: "IssueProcess",
                schema: "MRBasics",
                table: "Issues",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "IssueProcessingHistory",
                schema: "MRBasics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueProcess = table.Column<int>(type: "int", nullable: false),
                    IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueProcessingHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueProcessingHistory_Issues_IssueId",
                        column: x => x.IssueId,
                        principalSchema: "MRBasics",
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IssueVisibilityHistory",
                schema: "MRBasics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueVisibility = table.Column<int>(type: "int", nullable: false),
                    IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueVisibilityHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueVisibilityHistory_Issues_IssueId",
                        column: x => x.IssueId,
                        principalSchema: "MRBasics",
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssueProcessingHistory_IssueId",
                schema: "MRBasics",
                table: "IssueProcessingHistory",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueVisibilityHistory_IssueId",
                schema: "MRBasics",
                table: "IssueVisibilityHistory",
                column: "IssueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueProcessingHistory",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "IssueVisibilityHistory",
                schema: "MRBasics");

            migrationBuilder.DropColumn(
                name: "IssueProcess",
                schema: "MRBasics",
                table: "Issues");

            migrationBuilder.RenameColumn(
                name: "IssueVisibility",
                schema: "MRBasics",
                table: "Issues",
                newName: "IssueStatus");

            migrationBuilder.CreateTable(
                name: "IssueStatusHistories",
                schema: "MRBasics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IssueStatus = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueStatusHistories_Issues_IssueId",
                        column: x => x.IssueId,
                        principalSchema: "MRBasics",
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IssueStatusHistories_IssueId",
                schema: "MRBasics",
                table: "IssueStatusHistories",
                column: "IssueId");
        }
    }
}
