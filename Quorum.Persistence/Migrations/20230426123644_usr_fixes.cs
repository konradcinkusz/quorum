using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quorum.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class usr_fixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "B2BED4FF-47C0-47A1-9AE0-7AEF44CC14BB",
                columns: new[] { "ConcurrencyStamp", "NormalizedUserName", "SecurityStamp", "UserName" },
                values: new object[] { "A63E25A3-1237-4BD0-A5FC-5C8359885E9E", "BASICUSER@GMAIL.COM", "33135628-31F0-4995-861D-CE867EBA0FBE", "basicuser@gmail.com" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "E97336F4-CF5A-4C72-8C61-997E5C621143",
                columns: new[] { "ConcurrencyStamp", "NormalizedUserName", "SecurityStamp", "UserName" },
                values: new object[] { "B87AD966-7EA1-4696-8107-B190AEAB837D", "SUPERADMIN@GMAIL.COM", "A94F51C0-E605-43E6-819D-95C43ACE65D3", "superadmin@gmail.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "B2BED4FF-47C0-47A1-9AE0-7AEF44CC14BB",
                columns: new[] { "ConcurrencyStamp", "NormalizedUserName", "SecurityStamp", "UserName" },
                values: new object[] { "74a28857-0e9f-488f-bb11-22212ca42468", "BASICUSER", "28a72d0e-7818-4a21-b9d9-c9a7dd3d01ac", "basicuser" });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "E97336F4-CF5A-4C72-8C61-997E5C621143",
                columns: new[] { "ConcurrencyStamp", "NormalizedUserName", "SecurityStamp", "UserName" },
                values: new object[] { "ca8aa5be-cb65-4877-8978-56f123152b29", "SUPERADMIN", "532972bb-c278-424d-8dc4-b4aad02c7091", "superadmin" });
        }
    }
}
