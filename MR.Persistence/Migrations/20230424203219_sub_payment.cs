using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MR.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class sub_payment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Payments_PaymentId",
                schema: "MRBasics",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_PaymentId",
                schema: "MRBasics",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                schema: "MRBasics",
                table: "Subscriptions");

            migrationBuilder.CreateTable(
                name: "Subscription_Logs",
                schema: "MRBasics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPayment",
                columns: table => new
                {
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "Subscription_Logs",
                schema: "MRBasics");

            migrationBuilder.DropTable(
                name: "SubscriptionPayment");

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentId",
                schema: "MRBasics",
                table: "Subscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PaymentId",
                schema: "MRBasics",
                table: "Subscriptions",
                column: "PaymentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Payments_PaymentId",
                schema: "MRBasics",
                table: "Subscriptions",
                column: "PaymentId",
                principalSchema: "MRPayments",
                principalTable: "Payments",
                principalColumn: "Id");
        }
    }
}
