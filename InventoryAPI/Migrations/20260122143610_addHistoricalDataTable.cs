using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryAPI.Migrations
{
    /// <inheritdoc />
    public partial class addHistoricalDataTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HistoricalItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginalItemId = table.Column<int>(type: "int", nullable: false),
                    itemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    itemDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ItemTypeId = table.Column<int>(type: "int", nullable: false),
                    itemWeight = table.Column<double>(type: "float", nullable: false),
                    itemPrice = table.Column<double>(type: "float", nullable: false),
                    addedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    archivedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoricalItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistoricalItems_ItemTypes_ItemTypeId",
                        column: x => x.ItemTypeId,
                        principalTable: "ItemTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoricalItems_ItemTypeId",
                table: "HistoricalItems",
                column: "ItemTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoricalItems");
        }
    }
}
