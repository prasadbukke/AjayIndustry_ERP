using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCustomerDrawingRevisionWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomerDrawings_CustomerId_ItemId",
                table: "CustomerDrawings");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDrawings_CustomerId_DrawingNumber",
                table: "CustomerDrawings",
                columns: new[] { "CustomerId", "DrawingNumber" },
                unique: true,
                filter: "[IsActive] = 1 AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDrawings_CustomerId_DrawingNumber_RevisionNumber",
                table: "CustomerDrawings",
                columns: new[] { "CustomerId", "DrawingNumber", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDrawings_CustomerId_ItemId",
                table: "CustomerDrawings",
                columns: new[] { "CustomerId", "ItemId" },
                unique: true,
                filter: "[IsActive] = 1 AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CustomerDrawings_CustomerId_DrawingNumber",
                table: "CustomerDrawings");

            migrationBuilder.DropIndex(
                name: "IX_CustomerDrawings_CustomerId_DrawingNumber_RevisionNumber",
                table: "CustomerDrawings");

            migrationBuilder.DropIndex(
                name: "IX_CustomerDrawings_CustomerId_ItemId",
                table: "CustomerDrawings");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDrawings_CustomerId_ItemId",
                table: "CustomerDrawings",
                columns: new[] { "CustomerId", "ItemId" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }
    }
}
