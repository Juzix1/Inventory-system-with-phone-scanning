using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangedChkItemsList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CheckedItemIdList",
                table: "Stocktakes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckedItemIdList",
                table: "Stocktakes");
        }
    }
}
