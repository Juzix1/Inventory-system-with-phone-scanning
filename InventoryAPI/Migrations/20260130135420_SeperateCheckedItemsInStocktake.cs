using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class SeperateCheckedItemsInStocktake : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckedItemIdList",
                table: "Stocktakes");

            migrationBuilder.CreateTable(
                name: "StocktakeCheckedItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StocktakeId = table.Column<int>(type: "int", nullable: false),
                    InventoryItemId = table.Column<int>(type: "int", nullable: false),
                    CheckedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CheckedByUserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StocktakeCheckedItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StocktakeCheckedItems_Stocktakes_StocktakeId",
                        column: x => x.StocktakeId,
                        principalTable: "Stocktakes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StocktakeCheckedItems_CheckedDate",
                table: "StocktakeCheckedItems",
                column: "CheckedDate");

            migrationBuilder.CreateIndex(
                name: "IX_StocktakeCheckedItems_StocktakeId_InventoryItemId",
                table: "StocktakeCheckedItems",
                columns: new[] { "StocktakeId", "InventoryItemId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StocktakeCheckedItems");

            migrationBuilder.AddColumn<string>(
                name: "CheckedItemIdList",
                table: "Stocktakes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
