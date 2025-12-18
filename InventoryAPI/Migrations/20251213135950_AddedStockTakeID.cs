using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedStockTakeID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Rooms_RoomId",
                table: "InventoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Stocktakes_StocktakeId",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_RoomId",
                table: "InventoryItems");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Rooms_StocktakeId",
                table: "InventoryItems",
                column: "StocktakeId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Rooms_StocktakeId",
                table: "InventoryItems");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_RoomId",
                table: "InventoryItems",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Rooms_RoomId",
                table: "InventoryItems",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Stocktakes_StocktakeId",
                table: "InventoryItems",
                column: "StocktakeId",
                principalTable: "Stocktakes",
                principalColumn: "Id");
        }
    }
}
