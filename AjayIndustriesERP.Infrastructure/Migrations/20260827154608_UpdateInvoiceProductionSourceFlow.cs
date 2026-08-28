using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInvoiceProductionSourceFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceItems_InvoiceId_DeliveryChallanItemId",
                table: "InvoiceItems");

            migrationBuilder.AlterColumn<decimal>(
                name: "DeliveryChallanQuantity",
                table: "InvoiceItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3);

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryChallanItemId",
                table: "InvoiceItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryChallanId",
                table: "InvoiceItems",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "DeliveryChallanCode",
                table: "InvoiceItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_CustomerPurchaseOrderItemId",
                table: "InvoiceItems",
                column: "CustomerPurchaseOrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId_ProductionJobId",
                table: "InvoiceItems",
                columns: new[] { "InvoiceId", "ProductionJobId" },
                unique: true,
                filter: "[ProductionJobId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_ProductionJobId",
                table: "InvoiceItems",
                column: "ProductionJobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceItems_CustomerPurchaseOrderItemId",
                table: "InvoiceItems");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceItems_InvoiceId_ProductionJobId",
                table: "InvoiceItems");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceItems_ProductionJobId",
                table: "InvoiceItems");

            migrationBuilder.AlterColumn<decimal>(
                name: "DeliveryChallanQuantity",
                table: "InvoiceItems",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryChallanItemId",
                table: "InvoiceItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DeliveryChallanId",
                table: "InvoiceItems",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DeliveryChallanCode",
                table: "InvoiceItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceItems_InvoiceId_DeliveryChallanItemId",
                table: "InvoiceItems",
                columns: new[] { "InvoiceId", "DeliveryChallanItemId" },
                unique: true);
        }
    }
}
