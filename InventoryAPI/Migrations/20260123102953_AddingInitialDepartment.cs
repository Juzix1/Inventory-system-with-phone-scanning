using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddingInitialDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.InsertData(
                table: "Departments",
                columns: new[] { "Id", "DepartmentLocation", "DepartmentName" },
                values: new object[] { 1, "", "" });

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "Key", "Value" },
                values: new object[] { 3, "CompanyName", "My Company" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Departments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Settings",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.InsertData(
                table: "Settings",
                columns: new[] { "Id", "Key", "Value" },
                values: new object[] { 4, "CompanyName", "My Company" });
        }
    }
}
