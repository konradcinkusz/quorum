using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quorum.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class issue_signature_quarter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Issues",
                schema: "MRBasics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsVerifyByAdmin = table.Column<bool>(type: "bit", nullable: false),
                    IssueStatus = table.Column<int>(type: "int", nullable: false),
                    RatingValue = table.Column<int>(type: "int", nullable: false),
                    InitialPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Issues_AspNetUsers_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Issues_Payments_InitialPaymentId",
                        column: x => x.InitialPaymentId,
                        principalSchema: "MRPayments",
                        principalTable: "Payments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Quarters",
                schema: "MRBasics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quarters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuarterIssues",
                schema: "MRBasics",
                columns: table => new
                {
                    IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuarterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuarterIssues", x => new { x.IssueId, x.QuarterId });
                    table.ForeignKey(
                        name: "FK_QuarterIssues_Issues_IssueId",
                        column: x => x.IssueId,
                        principalSchema: "MRBasics",
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuarterIssues_Quarters_QuarterId",
                        column: x => x.QuarterId,
                        principalSchema: "MRBasics",
                        principalTable: "Quarters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SignaturePools",
                schema: "MRBasics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    QuarterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignaturePools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignaturePools_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SignaturePools_Quarters_QuarterId",
                        column: x => x.QuarterId,
                        principalSchema: "MRBasics",
                        principalTable: "Quarters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Signatures",
                schema: "MRBasics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SignaturePoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Signatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Signatures_Issues_IssueId",
                        column: x => x.IssueId,
                        principalSchema: "MRBasics",
                        principalTable: "Issues",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Signatures_SignaturePools_SignaturePoolId",
                        column: x => x.SignaturePoolId,
                        principalSchema: "MRBasics",
                        principalTable: "SignaturePools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_CreatedById",
                schema: "MRBasics",
                table: "Issues",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_InitialPaymentId",
                schema: "MRBasics",
                table: "Issues",
                column: "InitialPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_QuarterIssues_QuarterId",
                schema: "MRBasics",
                table: "QuarterIssues",
                column: "QuarterId");

            migrationBuilder.CreateIndex(
                name: "IX_SignaturePools_ApplicationUserId",
                schema: "MRBasics",
                table: "SignaturePools",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SignaturePools_QuarterId",
                schema: "MRBasics",
                table: "SignaturePools",
                column: "QuarterId");

            migrationBuilder.CreateIndex(
                name: "IX_Signatures_IssueId",
                schema: "MRBasics",
                table: "Signatures",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_Signatures_SignaturePoolId",
                schema: "MRBasics",
                table: "Signatures",
                column: "SignaturePoolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuarterIssues",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "Signatures",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "Issues",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "SignaturePools",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "Quarters",
                schema: "MRBasics");
        }
    }
}
