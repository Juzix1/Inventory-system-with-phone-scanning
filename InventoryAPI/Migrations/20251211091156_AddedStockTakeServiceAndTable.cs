using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedStockTakeServiceAndTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StocktakeId",
                table: "InventoryItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StocktakeId1",
                table: "InventoryItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Stocktakes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AllItems = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocktakes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_StocktakeId",
                table: "InventoryItems",
                column: "StocktakeId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_StocktakeId1",
                table: "InventoryItems",
                column: "StocktakeId1");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Stocktakes_StocktakeId",
                table: "InventoryItems",
                column: "StocktakeId",
                principalTable: "Stocktakes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Stocktakes_StocktakeId1",
                table: "InventoryItems",
                column: "StocktakeId1",
                principalTable: "Stocktakes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Stocktakes_StocktakeId",
                table: "InventoryItems");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Stocktakes_StocktakeId1",
                table: "InventoryItems");

            migrationBuilder.DropTable(
                name: "Stocktakes");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_StocktakeId",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_StocktakeId1",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "StocktakeId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "StocktakeId1",
                table: "InventoryItems");
        }
    }
}
