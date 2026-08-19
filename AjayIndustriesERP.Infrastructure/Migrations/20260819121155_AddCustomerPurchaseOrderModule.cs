using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerPurchaseOrderModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerPurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerPurchaseOrderNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerPurchaseOrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequiredDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CustomerReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerPurchaseOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerPurchaseOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerPurchaseOrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerPurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Specification = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UnitName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerDrawingNumber = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Revision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OrderedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RequiredDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerPurchaseOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerPurchaseOrderItems_CustomerPurchaseOrders_CustomerPurchaseOrderId",
                        column: x => x.CustomerPurchaseOrderId,
                        principalTable: "CustomerPurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerPurchaseOrderItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPurchaseOrderItems_Code",
                table: "CustomerPurchaseOrderItems",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPurchaseOrderItems_CustomerPurchaseOrderId",
                table: "CustomerPurchaseOrderItems",
                column: "CustomerPurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPurchaseOrderItems_IsDeleted",
                table: "CustomerPurchaseOrderItems",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPurchaseOrderItems_ItemId",
                table: "CustomerPurchaseOrderItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPurchaseOrderItems_Priority",
                table: "CustomerPurchaseOrderItems",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPurchaseOrderItems_RequiredDeliveryDate",
                table: "CustomerPurchaseOrderItems",
                column: "RequiredDeliveryDate");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPurchaseOrders_Code",
                table: "CustomerPurchaseOrders",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPurchaseOrders_CustomerId",
                table: "CustomerPurchaseOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPurchaseOrders_CustomerId_CustomerPurchaseOrderNumber",
                table: "CustomerPurchaseOrders",
                columns: new[] { "CustomerId", "CustomerPurchaseOrderNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPurchaseOrders_CustomerPurchaseOrderDate",
                table: "CustomerPurchaseOrders",
                column: "CustomerPurchaseOrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPurchaseOrders_IsDeleted",
                table: "CustomerPurchaseOrders",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPurchaseOrders_Priority",
                table: "CustomerPurchaseOrders",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPurchaseOrders_RequiredDeliveryDate",
                table: "CustomerPurchaseOrders",
                column: "RequiredDeliveryDate");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPurchaseOrders_Status",
                table: "CustomerPurchaseOrders",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerPurchaseOrderItems");

            migrationBuilder.DropTable(
                name: "CustomerPurchaseOrders");
        }
    }
}
