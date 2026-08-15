using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quorum.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class baseentity_update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "MRBasics",
                table: "Signatures",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "MRBasics",
                table: "Signatures",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "MRBasics",
                table: "SignaturePools",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "MRBasics",
                table: "SignaturePools",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "MRBasics",
                table: "Quarters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "MRBasics",
                table: "Quarters",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "MRPayments",
                table: "PaymentStatusHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "MRPayments",
                table: "PaymentStatusHistories",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "MRPayments",
                table: "Payments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "MRPayments",
                table: "Payments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "MRBasics",
                table: "Issues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "MRBasics",
                table: "Issues",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "MRBasics",
                table: "Admin_Logs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                schema: "MRBasics",
                table: "Admin_Logs",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "MRBasics",
                table: "Signatures");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "MRBasics",
                table: "Signatures");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "MRBasics",
                table: "SignaturePools");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "MRBasics",
                table: "SignaturePools");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "MRBasics",
                table: "Quarters");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "MRBasics",
                table: "Quarters");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "MRPayments",
                table: "PaymentStatusHistories");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "MRPayments",
                table: "PaymentStatusHistories");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "MRPayments",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "MRPayments",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "MRBasics",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "MRBasics",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "MRBasics",
                table: "Admin_Logs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "MRBasics",
                table: "Admin_Logs");
        }
    }
}
