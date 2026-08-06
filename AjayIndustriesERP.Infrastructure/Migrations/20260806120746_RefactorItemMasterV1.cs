using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorItemMasterV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Warehouses_WarehouseId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_ItemCode",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_WarehouseId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "MaximumStock",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "MinimumStock",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "OpeningStock",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ReorderLevel",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "Items");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemCode",
                table: "Items",
                column: "ItemCode",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_ItemCode",
                table: "Items");

            migrationBuilder.AddColumn<decimal>(
                name: "MaximumStock",
                table: "Items",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinimumStock",
                table: "Items",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "OpeningStock",
                table: "Items",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ReorderLevel",
                table: "Items",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "Items",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemCode",
                table: "Items",
                column: "ItemCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_WarehouseId",
                table: "Items",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Warehouses_WarehouseId",
                table: "Items",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "WarehouseId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
