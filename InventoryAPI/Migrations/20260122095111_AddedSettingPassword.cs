using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedSettingPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ItemTypes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.AddColumn<bool>(
                name: "resetPasswordOnNextLogin",
                table: "Accounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 1,
                column: "resetPasswordOnNextLogin",
                value: false);

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "Value",
                value: "/home/juzix/Projects/InventoryApp/Images");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "resetPasswordOnNextLogin",
                table: "Accounts");

            migrationBuilder.InsertData(
                table: "ItemTypes",
                columns: new[] { "Id", "TypeName" },
                values: new object[] { 4, "Book" });

            migrationBuilder.UpdateData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 1,
                column: "Value",
                value: "C:\\uploads");

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "Key", "Value" },
                values: new object[] { 3, "EnableNotifications", "true" });
        }
    }
}
