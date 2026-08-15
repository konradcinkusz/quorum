using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quorum.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class primary_SC_to_Quarter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Issues_InitialPaymentId",
                schema: "MRBasics",
                table: "Issues");

            migrationBuilder.AddColumn<int>(
                name: "PrimarySignatureCount",
                schema: "MRBasics",
                table: "Quarters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Issues_InitialPaymentId",
                schema: "MRBasics",
                table: "Issues",
                column: "InitialPaymentId",
                unique: true,
                filter: "[InitialPaymentId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Issues_InitialPaymentId",
                schema: "MRBasics",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "PrimarySignatureCount",
                schema: "MRBasics",
                table: "Quarters");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_InitialPaymentId",
                schema: "MRBasics",
                table: "Issues",
                column: "InitialPaymentId");
        }
    }
}
