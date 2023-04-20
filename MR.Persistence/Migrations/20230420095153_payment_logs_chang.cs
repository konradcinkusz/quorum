using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MR.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class payment_logs_chang : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Payment_Logs",
                schema: "MRLogs",
                newName: "Payment_Logs",
                newSchema: "MRPayments");

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                schema: "MRPayments",
                table: "Payment_Logs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "MRLogs");

            migrationBuilder.RenameTable(
                name: "Payment_Logs",
                schema: "MRPayments",
                newName: "Payment_Logs",
                newSchema: "MRLogs");

            migrationBuilder.AlterColumn<int>(
                name: "Action",
                schema: "MRLogs",
                table: "Payment_Logs",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
