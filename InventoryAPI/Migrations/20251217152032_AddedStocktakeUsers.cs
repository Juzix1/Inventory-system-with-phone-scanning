using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedStocktakeUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StocktakeId",
                table: "Accounts",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Accounts",
                keyColumn: "Id",
                keyValue: 1,
                column: "StocktakeId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_StocktakeId",
                table: "Accounts",
                column: "StocktakeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Stocktakes_StocktakeId",
                table: "Accounts",
                column: "StocktakeId",
                principalTable: "Stocktakes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Stocktakes_StocktakeId",
                table: "Accounts");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_StocktakeId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "StocktakeId",
                table: "Accounts");
        }
    }
}
