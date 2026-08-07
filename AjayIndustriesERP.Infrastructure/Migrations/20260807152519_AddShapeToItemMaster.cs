using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShapeToItemMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Items_ItemCode",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_ItemName",
                table: "Items");

            migrationBuilder.AddColumn<int>(
                name: "ShapeId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemCode",
                table: "Items",
                column: "ItemCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemName",
                table: "Items",
                column: "ItemName",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ShapeId",
                table: "Items",
                column: "ShapeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Shapes_ShapeId",
                table: "Items",
                column: "ShapeId",
                principalTable: "Shapes",
                principalColumn: "ShapeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Shapes_ShapeId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_ItemCode",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_ItemName",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_ShapeId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ShapeId",
                table: "Items");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemCode",
                table: "Items",
                column: "ItemCode",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ItemName",
                table: "Items",
                column: "ItemName",
                unique: true);
        }
    }
}
