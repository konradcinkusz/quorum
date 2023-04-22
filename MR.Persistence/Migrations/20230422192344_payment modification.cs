using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MR.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class paymentmodification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientReferenceId",
                schema: "MRPayments",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentIntentId",
                schema: "MRPayments",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentLink",
                schema: "MRPayments",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "SessionId",
                schema: "MRPayments",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "UserEmail",
                schema: "MRPayments",
                table: "Payments");

            migrationBuilder.AlterColumn<int>(
                name: "PaymentStatus",
                schema: "MRPayments",
                table: "Payments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                schema: "MRPayments",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                schema: "MRPayments",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                schema: "MRPayments",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                schema: "MRPayments",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "PaymentStatus",
                schema: "MRPayments",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ClientReferenceId",
                schema: "MRPayments",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentIntentId",
                schema: "MRPayments",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentLink",
                schema: "MRPayments",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionId",
                schema: "MRPayments",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserEmail",
                schema: "MRPayments",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
