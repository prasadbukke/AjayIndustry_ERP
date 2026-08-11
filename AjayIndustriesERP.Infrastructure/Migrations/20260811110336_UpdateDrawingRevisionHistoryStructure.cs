using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDrawingRevisionHistoryStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Drawings_DrawingNumber",
                table: "Drawings");

            migrationBuilder.DropIndex(
                name: "IX_Drawings_ItemId",
                table: "Drawings");

            migrationBuilder.AlterColumn<string>(
                name: "RevisionNumber",
                table: "Drawings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drawings_DrawingNumber",
                table: "Drawings",
                column: "DrawingNumber",
                unique: true,
                filter: "[IsActive] = 1 AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Drawings_DrawingNumber_RevisionNumber",
                table: "Drawings",
                columns: new[] { "DrawingNumber", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drawings_ItemId",
                table: "Drawings",
                column: "ItemId",
                unique: true,
                filter: "[IsPrimary] = 1 AND [IsActive] = 1 AND [IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Drawings_DrawingNumber",
                table: "Drawings");

            migrationBuilder.DropIndex(
                name: "IX_Drawings_DrawingNumber_RevisionNumber",
                table: "Drawings");

            migrationBuilder.DropIndex(
                name: "IX_Drawings_ItemId",
                table: "Drawings");

            migrationBuilder.AlterColumn<string>(
                name: "RevisionNumber",
                table: "Drawings",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_Drawings_DrawingNumber",
                table: "Drawings",
                column: "DrawingNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drawings_ItemId",
                table: "Drawings",
                column: "ItemId",
                unique: true,
                filter: "[IsPrimary] = 1 AND [IsDeleted] = 0");
        }
    }
}
