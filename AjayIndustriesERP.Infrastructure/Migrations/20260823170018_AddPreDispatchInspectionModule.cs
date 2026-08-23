using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPreDispatchInspectionModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PreDispatchInspections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InspectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<int>(type: "int", nullable: false),
                    ProductionJobId = table.Column<int>(type: "int", nullable: false),
                    ProductionJobCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    CustomerPurchaseOrderItemId = table.Column<int>(type: "int", nullable: false),
                    CustomerPurchaseOrderCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerPurchaseOrderNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PartNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UnitName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    WorkshopDrawingId = table.Column<int>(type: "int", nullable: true),
                    WorkshopDrawingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WorkshopDrawingRevision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CustomerDrawingId = table.Column<int>(type: "int", nullable: true),
                    CustomerDrawingNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerDrawingRevision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvoiceQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    InspectionQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    AcceptedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ReworkQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    RejectedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SupplierRemarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    InspectionRemarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    InspectedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ReviewedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FinalizedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinalizedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PdfFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PdfFilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreDispatchInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreDispatchInspections_ProductionJobs_ProductionJobId",
                        column: x => x.ProductionJobId,
                        principalTable: "ProductionJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PreDispatchInspectionLines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreDispatchInspectionId = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    Parameter = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Specification = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    InspectionMethod = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Result = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_PreDispatchInspectionLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreDispatchInspectionLines_PreDispatchInspections_PreDispatchInspectionId",
                        column: x => x.PreDispatchInspectionId,
                        principalTable: "PreDispatchInspections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PreDispatchInspectionObservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreDispatchInspectionLineId = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    IsIntervalReading = table.Column<bool>(type: "bit", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PreDispatchInspectionObservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PreDispatchInspectionObservations_PreDispatchInspectionLines_PreDispatchInspectionLineId",
                        column: x => x.PreDispatchInspectionLineId,
                        principalTable: "PreDispatchInspectionLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PreDispatchInspectionLines_PreDispatchInspectionId_SequenceNumber",
                table: "PreDispatchInspectionLines",
                columns: new[] { "PreDispatchInspectionId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreDispatchInspectionObservations_PreDispatchInspectionLineId_IsIntervalReading_SequenceNumber",
                table: "PreDispatchInspectionObservations",
                columns: new[] { "PreDispatchInspectionLineId", "IsIntervalReading", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreDispatchInspections_Code",
                table: "PreDispatchInspections",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PreDispatchInspections_CustomerId",
                table: "PreDispatchInspections",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_PreDispatchInspections_CustomerPurchaseOrderItemId",
                table: "PreDispatchInspections",
                column: "CustomerPurchaseOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PreDispatchInspections_ItemId",
                table: "PreDispatchInspections",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PreDispatchInspections_ProductionJobId",
                table: "PreDispatchInspections",
                column: "ProductionJobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PreDispatchInspectionObservations");

            migrationBuilder.DropTable(
                name: "PreDispatchInspectionLines");

            migrationBuilder.DropTable(
                name: "PreDispatchInspections");
        }
    }
}
