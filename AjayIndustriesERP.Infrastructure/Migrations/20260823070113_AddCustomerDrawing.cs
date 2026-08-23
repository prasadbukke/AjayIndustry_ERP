using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerDrawing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerDrawings",
                columns: table => new
                {
                    CustomerDrawingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    DrawingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DrawingName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DrawingType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RevisionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDrawings", x => x.CustomerDrawingId);
                    table.ForeignKey(
                        name: "FK_CustomerDrawings_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerDrawings_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDrawings_CustomerId_ItemId",
                table: "CustomerDrawings",
                columns: new[] { "CustomerId", "ItemId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDrawings_ItemId",
                table: "CustomerDrawings",
                column: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerDrawings");
        }
    }
}
