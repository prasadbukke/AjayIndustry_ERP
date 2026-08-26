using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeliveryChallanModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryChallans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChallanDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TransporterName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    VehicleNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransportReference = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DispatchFrom = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Destination = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FinalizedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinalizedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryChallans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryChallanItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeliveryChallanId = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    PreDispatchInspectionId = table.Column<int>(type: "int", nullable: false),
                    PreDispatchInspectionCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PdiAcceptedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ProductionJobId = table.Column<int>(type: "int", nullable: false),
                    ProductionJobCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerPurchaseOrderItemId = table.Column<int>(type: "int", nullable: false),
                    CustomerPurchaseOrderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerPurchaseOrderNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PartNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnitName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProductReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerDrawingId = table.Column<int>(type: "int", nullable: true),
                    CustomerDrawingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerDrawingRevision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DispatchQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryChallanItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryChallanItems_DeliveryChallans_DeliveryChallanId",
                        column: x => x.DeliveryChallanId,
                        principalTable: "DeliveryChallans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryChallanItems_PreDispatchInspections_PreDispatchInspectionId",
                        column: x => x.PreDispatchInspectionId,
                        principalTable: "PreDispatchInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanItems_CustomerDrawingId",
                table: "DeliveryChallanItems",
                column: "CustomerDrawingId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanItems_CustomerPurchaseOrderItemId",
                table: "DeliveryChallanItems",
                column: "CustomerPurchaseOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanItems_CustomerPurchaseOrderItemId_ItemId",
                table: "DeliveryChallanItems",
                columns: new[] { "CustomerPurchaseOrderItemId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanItems_DeliveryChallanId_SequenceNumber",
                table: "DeliveryChallanItems",
                columns: new[] { "DeliveryChallanId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanItems_ItemId",
                table: "DeliveryChallanItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanItems_PreDispatchInspectionId",
                table: "DeliveryChallanItems",
                column: "PreDispatchInspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanItems_PreDispatchInspectionId_DeliveryChallanId",
                table: "DeliveryChallanItems",
                columns: new[] { "PreDispatchInspectionId", "DeliveryChallanId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallanItems_ProductionJobId",
                table: "DeliveryChallanItems",
                column: "ProductionJobId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallans_ChallanDate",
                table: "DeliveryChallans",
                column: "ChallanDate");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallans_Code",
                table: "DeliveryChallans",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallans_CustomerId",
                table: "DeliveryChallans",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallans_CustomerId_ChallanDate",
                table: "DeliveryChallans",
                columns: new[] { "CustomerId", "ChallanDate" });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryChallans_Status",
                table: "DeliveryChallans",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryChallanItems");

            migrationBuilder.DropTable(
                name: "DeliveryChallans");
        }
    }
}
