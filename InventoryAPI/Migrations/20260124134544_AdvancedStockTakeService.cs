using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class AdvancedStockTakeService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "StocktakeAuthorizedAccounts",
                columns: table => new
                {
                    AccountId = table.Column<int>(type: "int", nullable: false),
                    StocktakeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StocktakeAuthorizedAccounts", x => new { x.AccountId, x.StocktakeId });
                    table.ForeignKey(
                        name: "FK_StocktakeAuthorizedAccounts_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StocktakeAuthorizedAccounts_Stocktakes_StocktakeId",
                        column: x => x.StocktakeId,
                        principalTable: "Stocktakes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Stocktakes_EndDate",
                table: "Stocktakes",
                column: "EndDate");

            migrationBuilder.CreateIndex(
                name: "IX_Stocktakes_StartDate",
                table: "Stocktakes",
                column: "StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Stocktakes_Status",
                table: "Stocktakes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_StocktakeAuthorizedAccounts_StocktakeId",
                table: "StocktakeAuthorizedAccounts",
                column: "StocktakeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StocktakeAuthorizedAccounts");

            migrationBuilder.DropIndex(
                name: "IX_Stocktakes_EndDate",
                table: "Stocktakes");

            migrationBuilder.DropIndex(
                name: "IX_Stocktakes_StartDate",
                table: "Stocktakes");

            migrationBuilder.DropIndex(
                name: "IX_Stocktakes_Status",
                table: "Stocktakes");

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
    }
}
