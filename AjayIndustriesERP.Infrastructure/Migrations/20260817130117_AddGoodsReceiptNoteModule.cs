using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

// ============================================================
// File: GoodsReceiptNote.cs
// Purpose:
// Represents the header/master record of a Goods Receipt Note.
//
// A GRN records physical material received from a supplier
// against an existing Purchase Order.
//
// Contains:
// - GRN number and date
// - Purchase Order reference
// - Supplier reference and snapshot
// - Supplier challan information
// - Remarks
// - Collection of received GRN items
//
// Phase 1:
// This entity records receipt information only.
// Inventory stock updates and Purchase Order status updates
// are intentionally not handled yet.
// ============================================================

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGoodsReceiptNoteModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GoodsReceiptNotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GRNDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PurchaseOrderId = table.Column<int>(type: "int", nullable: false),
                    SupplierId = table.Column<int>(type: "int", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    SupplierChallanNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SupplierChallanDate = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_GoodsReceiptNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptNotes_PurchaseOrders_PurchaseOrderId",
                        column: x => x.PurchaseOrderId,
                        principalTable: "PurchaseOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptNotes_Suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Suppliers",
                        principalColumn: "SupplierId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoodsReceiptNoteItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    GoodsReceiptNoteId = table.Column<int>(type: "int", nullable: false),
                    PurchaseOrderItemId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Specification = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UnitName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OrderedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PreviouslyReceivedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    BalanceQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReceiptStatus = table.Column<int>(type: "int", nullable: false),
                    ReceivedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PendingQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    MaterialStatus = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_GoodsReceiptNoteItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptNoteItems_GoodsReceiptNotes_GoodsReceiptNoteId",
                        column: x => x.GoodsReceiptNoteId,
                        principalTable: "GoodsReceiptNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptNoteItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodsReceiptNoteItems_PurchaseOrderItems_PurchaseOrderItemId",
                        column: x => x.PurchaseOrderItemId,
                        principalTable: "PurchaseOrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNoteItems_GoodsReceiptNoteId",
                table: "GoodsReceiptNoteItems",
                column: "GoodsReceiptNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNoteItems_ItemId",
                table: "GoodsReceiptNoteItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNoteItems_PurchaseOrderItemId",
                table: "GoodsReceiptNoteItems",
                column: "PurchaseOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNotes_Code",
                table: "GoodsReceiptNotes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNotes_PurchaseOrderId",
                table: "GoodsReceiptNotes",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodsReceiptNotes_SupplierId",
                table: "GoodsReceiptNotes",
                column: "SupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoodsReceiptNoteItems");

            migrationBuilder.DropTable(
                name: "GoodsReceiptNotes");
        }
    }
}
