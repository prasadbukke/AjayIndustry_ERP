using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignProductionPdiInvoiceFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            #region Remove Old Production Job Foreign Keys

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionJobs_CustomerPurchaseOrderItems_CustomerPurchaseOrderItemId",
                table: "ProductionJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionJobs_ItemProcessRoutings_ItemProcessRoutingId",
                table: "ProductionJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionJobs_Items_ItemId",
                table: "ProductionJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionJobSteps_ProductionJobs_ProductionJobId",
                table: "ProductionJobSteps");

            #endregion


            #region Remove Old Production Job Indexes

            migrationBuilder.DropIndex(
                name: "IX_ProductionJobs_CustomerPurchaseOrderItemId",
                table: "ProductionJobs");

            migrationBuilder.DropIndex(
                name: "IX_ProductionJobs_ItemId",
                table: "ProductionJobs");

            migrationBuilder.DropIndex(
                name: "IX_ProductionJobs_ItemProcessRoutingId",
                table: "ProductionJobs");

            #endregion


            #region Remove Old Production Job Item-Level Columns

            migrationBuilder.DropColumn(
                name: "CustomerPurchaseOrderItemId",
                table: "ProductionJobs");

            migrationBuilder.DropColumn(
                name: "ItemCode",
                table: "ProductionJobs");

            migrationBuilder.DropColumn(
                name: "ItemId",
                table: "ProductionJobs");

            migrationBuilder.DropColumn(
                name: "ItemName",
                table: "ProductionJobs");

            migrationBuilder.DropColumn(
                name: "ItemProcessRoutingId",
                table: "ProductionJobs");

            migrationBuilder.DropColumn(
                name: "JobQuantity",
                table: "ProductionJobs");

            migrationBuilder.DropColumn(
                name: "PipelineModificationReason",
                table: "ProductionJobs");

            migrationBuilder.DropColumn(
                name: "RoutingCode",
                table: "ProductionJobs");

            migrationBuilder.DropColumn(
                name: "RoutingRevisionNumber",
                table: "ProductionJobs");

            migrationBuilder.DropColumn(
                name: "UnitName",
                table: "ProductionJobs");

            #endregion


            #region Convert Production Job Step Source

            /*
             * Old:
             *
             * ProductionJobStep
             *      → ProductionJob
             *
             * New:
             *
             * ProductionJobStep
             *      → ProductionJobItem
             *
             * Database is clean, therefore existing
             * ProductionJobId values do not require
             * data conversion.
             */

            migrationBuilder.RenameColumn(
                name: "ProductionJobId",
                table: "ProductionJobSteps",
                newName: "ProductionJobItemId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionJobSteps_ProductionJobId_SequenceNumber",
                table: "ProductionJobSteps",
                newName: "IX_ProductionJobSteps_ProductionJobItemId_SequenceNumber");

            #endregion


            #region Add Customer PO Source To Production Job

            /*
             * IMPORTANT:
             *
             * CustomerPurchaseOrderId is a NEW column.
             *
             * It must NOT be created by renaming
             * RoutingRevisionNumber.
             */

            migrationBuilder.AddColumn<int>(
                name: "CustomerPurchaseOrderId",
                table: "ProductionJobs",
                type: "int",
                nullable: false);

            #endregion


            #region Add Production Job Item Source To PDI

            /*
             * Database is clean, therefore no default 0
             * value is required.
             *
             * This prevents creation of an invalid FK
             * reference to ProductionJobItem Id = 0.
             */

            migrationBuilder.AddColumn<int>(
                name: "ProductionJobItemId",
                table: "PreDispatchInspections",
                type: "int",
                nullable: false);

            #endregion


            #region Create Production Job Items

            migrationBuilder.CreateTable(
                name: "ProductionJobItems",
                columns: table => new
                {
                    Id = table.Column<int>(
                        type: "int",
                        nullable: false)
                        .Annotation(
                            "SqlServer:Identity",
                            "1, 1"),

                    ProductionJobId =
                        table.Column<int>(
                            type: "int",
                            nullable: false),

                    CustomerPurchaseOrderItemId =
                        table.Column<int>(
                            type: "int",
                            nullable: false),

                    ItemId =
                        table.Column<int>(
                            type: "int",
                            nullable: false),

                    ItemCode =
                        table.Column<string>(
                            type: "nvarchar(50)",
                            maxLength: 50,
                            nullable: false),

                    ItemName =
                        table.Column<string>(
                            type: "nvarchar(200)",
                            maxLength: 200,
                            nullable: false),

                    UnitName =
                        table.Column<string>(
                            type: "nvarchar(100)",
                            maxLength: 100,
                            nullable: true),

                    OrderedQuantity =
                        table.Column<decimal>(
                            type: "decimal(18,3)",
                            precision: 18,
                            scale: 3,
                            nullable: false),

                    ProductionQuantity =
                        table.Column<decimal>(
                            type: "decimal(18,3)",
                            precision: 18,
                            scale: 3,
                            nullable: false),

                    CompletedQuantity =
                        table.Column<decimal>(
                            type: "decimal(18,3)",
                            precision: 18,
                            scale: 3,
                            nullable: false),

                    ItemProcessRoutingId =
                        table.Column<int>(
                            type: "int",
                            nullable: false),

                    RoutingCode =
                        table.Column<string>(
                            type: "nvarchar(50)",
                            maxLength: 50,
                            nullable: false),

                    RoutingRevisionNumber =
                        table.Column<int>(
                            type: "int",
                            nullable: false),

                    PipelineModificationReason =
                        table.Column<string>(
                            type: "nvarchar(1000)",
                            maxLength: 1000,
                            nullable: true),

                    IsActive =
                        table.Column<bool>(
                            type: "bit",
                            nullable: false),

                    IsDeleted =
                        table.Column<bool>(
                            type: "bit",
                            nullable: false),

                    CreatedOn =
                        table.Column<DateTime>(
                            type: "datetime2",
                            nullable: false),

                    CreatedBy =
                        table.Column<string>(
                            type: "nvarchar(max)",
                            nullable: false),

                    ModifiedOn =
                        table.Column<DateTime>(
                            type: "datetime2",
                            nullable: true),

                    ModifiedBy =
                        table.Column<string>(
                            type: "nvarchar(max)",
                            nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_ProductionJobItems",
                        x => x.Id);


                    table.ForeignKey(
                        name: "FK_ProductionJobItems_CustomerPurchaseOrderItems_CustomerPurchaseOrderItemId",
                        column: x =>
                            x.CustomerPurchaseOrderItemId,
                        principalTable:
                            "CustomerPurchaseOrderItems",
                        principalColumn:
                            "Id",
                        onDelete:
                            ReferentialAction.Restrict);


                    table.ForeignKey(
                        name: "FK_ProductionJobItems_ItemProcessRoutings_ItemProcessRoutingId",
                        column: x =>
                            x.ItemProcessRoutingId,
                        principalTable:
                            "ItemProcessRoutings",
                        principalColumn:
                            "Id",
                        onDelete:
                            ReferentialAction.Restrict);


                    table.ForeignKey(
                        name: "FK_ProductionJobItems_Items_ItemId",
                        column: x =>
                            x.ItemId,
                        principalTable:
                            "Items",
                        principalColumn:
                            "ItemId",
                        onDelete:
                            ReferentialAction.Restrict);


                    table.ForeignKey(
                        name: "FK_ProductionJobItems_ProductionJobs_ProductionJobId",
                        column: x =>
                            x.ProductionJobId,
                        principalTable:
                            "ProductionJobs",
                        principalColumn:
                            "Id",
                        onDelete:
                            ReferentialAction.Cascade);
                });

            #endregion


            #region Create Indexes

            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobs_CustomerPurchaseOrderId",
                table: "ProductionJobs",
                column: "CustomerPurchaseOrderId",
                unique: true);


            migrationBuilder.CreateIndex(
                name: "IX_PreDispatchInspections_ProductionJobItemId",
                table: "PreDispatchInspections",
                column: "ProductionJobItemId");


            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobItems_CustomerPurchaseOrderItemId",
                table: "ProductionJobItems",
                column: "CustomerPurchaseOrderItemId",
                unique: true);


            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobItems_IsDeleted",
                table: "ProductionJobItems",
                column: "IsDeleted");


            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobItems_ItemId",
                table: "ProductionJobItems",
                column: "ItemId");


            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobItems_ItemProcessRoutingId",
                table: "ProductionJobItems",
                column: "ItemProcessRoutingId");


            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobItems_ProductionJobId",
                table: "ProductionJobItems",
                column: "ProductionJobId");

            #endregion


            #region Add New Foreign Keys

            migrationBuilder.AddForeignKey(
                name: "FK_PreDispatchInspections_ProductionJobItems_ProductionJobItemId",
                table: "PreDispatchInspections",
                column: "ProductionJobItemId",
                principalTable: "ProductionJobItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);


            migrationBuilder.AddForeignKey(
                name: "FK_ProductionJobs_CustomerPurchaseOrders_CustomerPurchaseOrderId",
                table: "ProductionJobs",
                column: "CustomerPurchaseOrderId",
                principalTable: "CustomerPurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);


            migrationBuilder.AddForeignKey(
                name: "FK_ProductionJobSteps_ProductionJobItems_ProductionJobItemId",
                table: "ProductionJobSteps",
                column: "ProductionJobItemId",
                principalTable: "ProductionJobItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            #endregion
        }


        /// <inheritdoc />
        protected override void Down(
            MigrationBuilder migrationBuilder)
        {
            #region Remove New Foreign Keys

            migrationBuilder.DropForeignKey(
                name: "FK_PreDispatchInspections_ProductionJobItems_ProductionJobItemId",
                table: "PreDispatchInspections");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionJobs_CustomerPurchaseOrders_CustomerPurchaseOrderId",
                table: "ProductionJobs");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductionJobSteps_ProductionJobItems_ProductionJobItemId",
                table: "ProductionJobSteps");

            #endregion


            #region Remove Production Job Items

            migrationBuilder.DropTable(
                name: "ProductionJobItems");

            #endregion


            #region Remove New Indexes

            migrationBuilder.DropIndex(
                name: "IX_ProductionJobs_CustomerPurchaseOrderId",
                table: "ProductionJobs");

            migrationBuilder.DropIndex(
                name: "IX_PreDispatchInspections_ProductionJobItemId",
                table: "PreDispatchInspections");

            #endregion


            #region Remove New Columns

            migrationBuilder.DropColumn(
                name: "ProductionJobItemId",
                table: "PreDispatchInspections");

            migrationBuilder.DropColumn(
                name: "CustomerPurchaseOrderId",
                table: "ProductionJobs");

            #endregion


            #region Restore Production Job Step Source

            migrationBuilder.RenameColumn(
                name: "ProductionJobItemId",
                table: "ProductionJobSteps",
                newName: "ProductionJobId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductionJobSteps_ProductionJobItemId_SequenceNumber",
                table: "ProductionJobSteps",
                newName: "IX_ProductionJobSteps_ProductionJobId_SequenceNumber");

            #endregion


            #region Restore Old Production Job Columns

            migrationBuilder.AddColumn<int>(
                name: "CustomerPurchaseOrderItemId",
                table: "ProductionJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);


            migrationBuilder.AddColumn<string>(
                name: "ItemCode",
                table: "ProductionJobs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");


            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "ProductionJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);


            migrationBuilder.AddColumn<string>(
                name: "ItemName",
                table: "ProductionJobs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");


            migrationBuilder.AddColumn<int>(
                name: "ItemProcessRoutingId",
                table: "ProductionJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);


            migrationBuilder.AddColumn<decimal>(
                name: "JobQuantity",
                table: "ProductionJobs",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m);


            migrationBuilder.AddColumn<string>(
                name: "PipelineModificationReason",
                table: "ProductionJobs",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);


            migrationBuilder.AddColumn<string>(
                name: "RoutingCode",
                table: "ProductionJobs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");


            migrationBuilder.AddColumn<int>(
                name: "RoutingRevisionNumber",
                table: "ProductionJobs",
                type: "int",
                nullable: false,
                defaultValue: 0);


            migrationBuilder.AddColumn<string>(
                name: "UnitName",
                table: "ProductionJobs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            #endregion


            #region Restore Old Indexes

            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobs_CustomerPurchaseOrderItemId",
                table: "ProductionJobs",
                column: "CustomerPurchaseOrderItemId");


            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobs_ItemId",
                table: "ProductionJobs",
                column: "ItemId");


            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobs_ItemProcessRoutingId",
                table: "ProductionJobs",
                column: "ItemProcessRoutingId");

            #endregion


            #region Restore Old Foreign Keys

            migrationBuilder.AddForeignKey(
                name: "FK_ProductionJobs_CustomerPurchaseOrderItems_CustomerPurchaseOrderItemId",
                table: "ProductionJobs",
                column: "CustomerPurchaseOrderItemId",
                principalTable: "CustomerPurchaseOrderItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);


            migrationBuilder.AddForeignKey(
                name: "FK_ProductionJobs_ItemProcessRoutings_ItemProcessRoutingId",
                table: "ProductionJobs",
                column: "ItemProcessRoutingId",
                principalTable: "ItemProcessRoutings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);


            migrationBuilder.AddForeignKey(
                name: "FK_ProductionJobs_Items_ItemId",
                table: "ProductionJobs",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Restrict);


            migrationBuilder.AddForeignKey(
                name: "FK_ProductionJobSteps_ProductionJobs_ProductionJobId",
                table: "ProductionJobSteps",
                column: "ProductionJobId",
                principalTable: "ProductionJobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            #endregion
        }
    }
}