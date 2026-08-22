using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductionJobModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ItemProcessRoutingSteps_ItemProcessRoutingId_SequenceNumber",
                table: "ItemProcessRoutingSteps");

            migrationBuilder.CreateTable(
                name: "ProductionJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CustomerPurchaseOrderItemId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UnitName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    JobQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ItemProcessRoutingId = table.Column<int>(type: "int", nullable: false),
                    RoutingCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoutingRevisionNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PlannedStartOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedCompletionOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_ProductionJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionJobs_CustomerPurchaseOrderItems_CustomerPurchaseOrderItemId",
                        column: x => x.CustomerPurchaseOrderItemId,
                        principalTable: "CustomerPurchaseOrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionJobs_ItemProcessRoutings_ItemProcessRoutingId",
                        column: x => x.ItemProcessRoutingId,
                        principalTable: "ItemProcessRoutings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionJobs_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionJobSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionJobId = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    ProductionOperationId = table.Column<int>(type: "int", nullable: false),
                    OperationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OperationName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    OperationType = table.Column<int>(type: "int", nullable: false),
                    DefaultMachineId = table.Column<int>(type: "int", nullable: true),
                    AssignedMachineId = table.Column<int>(type: "int", nullable: true),
                    SetupTimeMinutes = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    CycleTimeMinutes = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    OperationInstruction = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RoutingRemarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GoodQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    RejectedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    ExecutionRemarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionJobSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionJobSteps_Machines_AssignedMachineId",
                        column: x => x.AssignedMachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionJobSteps_Machines_DefaultMachineId",
                        column: x => x.DefaultMachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProductionJobSteps_ProductionJobs_ProductionJobId",
                        column: x => x.ProductionJobId,
                        principalTable: "ProductionJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductionJobSteps_ProductionOperations_ProductionOperationId",
                        column: x => x.ProductionOperationId,
                        principalTable: "ProductionOperations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductionJobStepHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductionJobStepId = table.Column<int>(type: "int", nullable: false),
                    PreviousStatus = table.Column<int>(type: "int", nullable: true),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    MachineId = table.Column<int>(type: "int", nullable: true),
                    MachineCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MachineName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GoodQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    RejectedQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ChangedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionJobStepHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductionJobStepHistories_ProductionJobSteps_ProductionJobStepId",
                        column: x => x.ProductionJobStepId,
                        principalTable: "ProductionJobSteps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemProcessRoutingSteps_ItemProcessRoutingId_SequenceNumber",
                table: "ItemProcessRoutingSteps",
                columns: new[] { "ItemProcessRoutingId", "SequenceNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobs_Code",
                table: "ProductionJobs",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobs_CustomerPurchaseOrderItemId",
                table: "ProductionJobs",
                column: "CustomerPurchaseOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobs_IsDeleted",
                table: "ProductionJobs",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobs_ItemId",
                table: "ProductionJobs",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobs_ItemProcessRoutingId",
                table: "ProductionJobs",
                column: "ItemProcessRoutingId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobs_PlannedStartOn",
                table: "ProductionJobs",
                column: "PlannedStartOn");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobs_Status",
                table: "ProductionJobs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobStepHistories_MachineId",
                table: "ProductionJobStepHistories",
                column: "MachineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobStepHistories_ProductionJobStepId_ChangedOn",
                table: "ProductionJobStepHistories",
                columns: new[] { "ProductionJobStepId", "ChangedOn" });

            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobSteps_AssignedMachineId",
                table: "ProductionJobSteps",
                column: "AssignedMachineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobSteps_DefaultMachineId",
                table: "ProductionJobSteps",
                column: "DefaultMachineId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobSteps_IsDeleted",
                table: "ProductionJobSteps",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobSteps_ProductionJobId_SequenceNumber",
                table: "ProductionJobSteps",
                columns: new[] { "ProductionJobId", "SequenceNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobSteps_ProductionOperationId",
                table: "ProductionJobSteps",
                column: "ProductionOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductionJobSteps_Status",
                table: "ProductionJobSteps",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductionJobStepHistories");

            migrationBuilder.DropTable(
                name: "ProductionJobSteps");

            migrationBuilder.DropTable(
                name: "ProductionJobs");

            migrationBuilder.DropIndex(
                name: "IX_ItemProcessRoutingSteps_ItemProcessRoutingId_SequenceNumber",
                table: "ItemProcessRoutingSteps");

            migrationBuilder.CreateIndex(
                name: "IX_ItemProcessRoutingSteps_ItemProcessRoutingId_SequenceNumber",
                table: "ItemProcessRoutingSteps",
                columns: new[] { "ItemProcessRoutingId", "SequenceNumber" },
                unique: true);
        }
    }
}
