using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddItemProcessRoutingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ItemProcessRoutings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    RevisionNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_ItemProcessRoutings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemProcessRoutings_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "ItemId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ItemProcessRoutingSteps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemProcessRoutingId = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    ProductionOperationId = table.Column<int>(type: "int", nullable: false),
                    DefaultMachineId = table.Column<int>(type: "int", nullable: true),
                    SetupTimeMinutes = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    CycleTimeMinutes = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    OperationInstruction = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_ItemProcessRoutingSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemProcessRoutingSteps_ItemProcessRoutings_ItemProcessRoutingId",
                        column: x => x.ItemProcessRoutingId,
                        principalTable: "ItemProcessRoutings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemProcessRoutingSteps_Machines_DefaultMachineId",
                        column: x => x.DefaultMachineId,
                        principalTable: "Machines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ItemProcessRoutingSteps_ProductionOperations_ProductionOperationId",
                        column: x => x.ProductionOperationId,
                        principalTable: "ProductionOperations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemProcessRoutings_Code",
                table: "ItemProcessRoutings",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemProcessRoutings_EffectiveFrom",
                table: "ItemProcessRoutings",
                column: "EffectiveFrom");

            migrationBuilder.CreateIndex(
                name: "IX_ItemProcessRoutings_IsActive",
                table: "ItemProcessRoutings",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ItemProcessRoutings_IsDeleted",
                table: "ItemProcessRoutings",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ItemProcessRoutings_ItemId",
                table: "ItemProcessRoutings",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemProcessRoutings_ItemId_RevisionNumber",
                table: "ItemProcessRoutings",
                columns: new[] { "ItemId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemProcessRoutings_Status",
                table: "ItemProcessRoutings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ItemProcessRoutingSteps_DefaultMachineId",
                table: "ItemProcessRoutingSteps",
                column: "DefaultMachineId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemProcessRoutingSteps_IsActive",
                table: "ItemProcessRoutingSteps",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ItemProcessRoutingSteps_IsDeleted",
                table: "ItemProcessRoutingSteps",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ItemProcessRoutingSteps_ItemProcessRoutingId_SequenceNumber",
                table: "ItemProcessRoutingSteps",
                columns: new[] { "ItemProcessRoutingId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemProcessRoutingSteps_ProductionOperationId",
                table: "ItemProcessRoutingSteps",
                column: "ProductionOperationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemProcessRoutingSteps");

            migrationBuilder.DropTable(
                name: "ItemProcessRoutings");
        }
    }
}
