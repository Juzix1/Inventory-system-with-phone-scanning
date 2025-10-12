using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangedPersonInCharge2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Accounts_personInChargeId",
                table: "InventoryItems");

            migrationBuilder.RenameColumn(
                name: "personInChargeId",
                table: "InventoryItems",
                newName: "PersonInChargeId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryItems_personInChargeId",
                table: "InventoryItems",
                newName: "IX_InventoryItems_PersonInChargeId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Accounts_PersonInChargeId",
                table: "InventoryItems",
                column: "PersonInChargeId",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_Accounts_PersonInChargeId",
                table: "InventoryItems");

            migrationBuilder.RenameColumn(
                name: "PersonInChargeId",
                table: "InventoryItems",
                newName: "personInChargeId");

            migrationBuilder.RenameIndex(
                name: "IX_InventoryItems_PersonInChargeId",
                table: "InventoryItems",
                newName: "IX_InventoryItems_personInChargeId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_Accounts_personInChargeId",
                table: "InventoryItems",
                column: "personInChargeId",
                principalTable: "Accounts",
                principalColumn: "Id");
        }
    }
}
