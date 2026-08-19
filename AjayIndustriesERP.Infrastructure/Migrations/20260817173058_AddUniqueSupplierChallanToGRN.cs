using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueSupplierChallanToGRN : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GoodsReceiptNotes_SupplierId",
                table: "GoodsReceiptNotes");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNotes_SupplierId_SupplierChallanNumber",
                table: "GoodsReceiptNotes",
                columns: new[] { "SupplierId", "SupplierChallanNumber" },
                unique: true,
                filter: "[SupplierChallanNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GoodsReceiptNotes_SupplierId_SupplierChallanNumber",
                table: "GoodsReceiptNotes");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNotes_SupplierId",
                table: "GoodsReceiptNotes",
                column: "SupplierId");
        }
    }
}
