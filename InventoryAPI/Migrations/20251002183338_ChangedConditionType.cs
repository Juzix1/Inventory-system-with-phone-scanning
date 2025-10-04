using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class ChangedConditionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "itemCondition",
                table: "InventoryItems");

            migrationBuilder.AddColumn<int>(
                name: "ItemConditionId",
                table: "InventoryItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "ItemConditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConditionName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemConditions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryItems_ItemConditionId",
                table: "InventoryItems",
                column: "ItemConditionId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryItems_ItemConditions_ItemConditionId",
                table: "InventoryItems",
                column: "ItemConditionId",
                principalTable: "ItemConditions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryItems_ItemConditions_ItemConditionId",
                table: "InventoryItems");

            migrationBuilder.DropTable(
                name: "ItemConditions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryItems_ItemConditionId",
                table: "InventoryItems");

            migrationBuilder.DropColumn(
                name: "ItemConditionId",
                table: "InventoryItems");

            migrationBuilder.AddColumn<string>(
                name: "itemCondition",
                table: "InventoryItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
