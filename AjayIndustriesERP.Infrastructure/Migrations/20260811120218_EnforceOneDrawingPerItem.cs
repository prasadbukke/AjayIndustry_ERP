using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnforceOneDrawingPerItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Drawings_ItemId",
                table: "Drawings");

            migrationBuilder.CreateIndex(
                name: "IX_Drawings_ItemId",
                table: "Drawings",
                column: "ItemId",
                unique: true,
                filter: "[IsActive] = 1 AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Drawings_ItemId",
                table: "Drawings");

            migrationBuilder.CreateIndex(
                name: "IX_Drawings_ItemId",
                table: "Drawings",
                column: "ItemId",
                unique: true,
                filter: "[IsPrimary] = 1 AND [IsActive] = 1 AND [IsDeleted] = 0");
        }
    }
}
