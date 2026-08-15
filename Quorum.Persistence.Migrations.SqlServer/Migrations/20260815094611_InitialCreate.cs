using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quorum.Persistence.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "MRBasics");

            migrationBuilder.EnsureSchema(
                name: "MRPayments");

            migrationBuilder.CreateTable(
                name: "Admin_Logs",
                schema: "MRBasics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Values = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admin_Logs", x => x.Id);
                });

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
                name: "Payments",
                schema: "MRPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentValuePLN = table.Column<decimal>(type: "money", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Quarters",
                schema: "MRBasics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    QuarterNumber = table.Column<int>(type: "int", nullable: false),
                    PrimarySignatureCount = table.Column<int>(type: "int", nullable: false),
                    QuarterResolved = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quarters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuorumUsers",
                schema: "MRBasics",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuorumUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                schema: "MRBasics",
                columns: table => new
                {
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Begin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    End = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.ApplicationUserId);
                });

            migrationBuilder.CreateTable(
                name: "Issues",
                schema: "MRBasics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedByEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsVerifyByAdmin = table.Column<bool>(type: "bit", nullable: false),
                    IssueVisibility = table.Column<int>(type: "int", nullable: false),
                    IssueProcess = table.Column<int>(type: "int", nullable: false),
                    RatingValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InitialPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BackgroundColor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Issues_Payments_InitialPaymentId",
                        column: x => x.InitialPaymentId,
                        principalSchema: "MRPayments",
                        principalTable: "Payments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Payment_Logs",
                schema: "MRPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payment_Logs_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalSchema: "MRPayments",
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentStatusHistories",
                schema: "MRPayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentStatusHistories_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalSchema: "MRPayments",
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SignaturePools",
                schema: "MRBasics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuarterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignaturePools", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignaturePools_Quarters_QuarterId",
                        column: x => x.QuarterId,
                        principalSchema: "MRBasics",
                        principalTable: "Quarters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscription_Logs",
                schema: "MRBasics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscription_Logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscription_Logs_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalSchema: "MRBasics",
                        principalTable: "Subscriptions",
                        principalColumn: "ApplicationUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPayment",
                columns: table => new
                {
                    SubscriptionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPayment", x => new { x.SubscriptionId, x.PaymentId });
                    table.ForeignKey(
                        name: "FK_SubscriptionPayment_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalSchema: "MRPayments",
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriptionPayment_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalSchema: "MRBasics",
                        principalTable: "Subscriptions",
                        principalColumn: "ApplicationUserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CloudinaryFileIssues",
                schema: "MRBasics",
                columns: table => new
                {
                    IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CloudinaryFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CloudinaryFileIssueType = table.Column<int>(type: "int", nullable: false)
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
                name: "IssueRatingHistory",
                schema: "MRBasics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Action = table.Column<int>(type: "int", nullable: false),
                    RelatedObject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueRatingHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueRatingHistory_Issues_IssueId",
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

            migrationBuilder.CreateTable(
                name: "QuarterIssues",
                schema: "MRBasics",
                columns: table => new
                {
                    IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuarterId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuarterWinner = table.Column<bool>(type: "bit", nullable: true)
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
                name: "Signatures",
                schema: "MRBasics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SignaturePoolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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
                name: "IX_CloudinaryFileIssues_CloudinaryFileId",
                schema: "MRBasics",
                table: "CloudinaryFileIssues",
                column: "CloudinaryFileId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueProcessingHistory_IssueId",
                schema: "MRBasics",
                table: "IssueProcessingHistory",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_IssueRatingHistory_IssueId",
                schema: "MRBasics",
                table: "IssueRatingHistory",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_InitialPaymentId",
                schema: "MRBasics",
                table: "Issues",
                column: "InitialPaymentId",
                unique: true,
                filter: "[InitialPaymentId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IssueVisibilityHistory_IssueId",
                schema: "MRBasics",
                table: "IssueVisibilityHistory",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_Logs_PaymentId",
                schema: "MRPayments",
                table: "Payment_Logs",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentStatusHistories_PaymentId",
                schema: "MRPayments",
                table: "PaymentStatusHistories",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_QuarterIssues_QuarterId",
                schema: "MRBasics",
                table: "QuarterIssues",
                column: "QuarterId");

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

            migrationBuilder.CreateIndex(
                name: "IX_Subscription_Logs_SubscriptionId",
                schema: "MRBasics",
                table: "Subscription_Logs",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayment_PaymentId",
                table: "SubscriptionPayment",
                column: "PaymentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Admin_Logs",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "CloudinaryFileIssues",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "IssueProcessingHistory",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "IssueRatingHistory",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "IssueVisibilityHistory",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "Payment_Logs",
                schema: "MRPayments");

            migrationBuilder.DropTable(
                name: "PaymentStatusHistories",
                schema: "MRPayments");

            migrationBuilder.DropTable(
                name: "QuarterIssues",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "QuorumUsers",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "Signatures",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "Subscription_Logs",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "SubscriptionPayment");

            migrationBuilder.DropTable(
                name: "CloudinaryFiles",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "Issues",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "SignaturePools",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "Subscriptions",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "Payments",
                schema: "MRPayments");

            migrationBuilder.DropTable(
                name: "Quarters",
                schema: "MRBasics");
        }
    }
}
