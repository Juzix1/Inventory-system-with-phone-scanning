using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangedPersonInCharge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "personInCharge",
                table: "InventoryItems");

            migrationBuilder.AddColumn<int>(
                name: "personInChargeId",
                table: "InventoryItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_personInChargeId",
                table: "InventoryItems",
                column: "personInChargeId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Accounts_personInChargeId",
                table: "InventoryItems",
                column: "personInChargeId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Accounts_personInChargeId",
                table: "InventoryItems");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_personInChargeId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "personInChargeId",
                table: "InventoryItems");

            migrationBuilder.AddColumn<string>(
                name: "personInCharge",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
