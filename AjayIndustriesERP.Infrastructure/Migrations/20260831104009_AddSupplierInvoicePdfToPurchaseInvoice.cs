using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AjayIndustriesERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierInvoicePdfToPurchaseInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SupplierInvoicePdfOriginalName",
                table: "PurchaseInvoices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierInvoicePdfPath",
                table: "PurchaseInvoices",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SupplierInvoicePdfUploadedOn",
                table: "PurchaseInvoices",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupplierInvoicePdfOriginalName",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "SupplierInvoicePdfPath",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "SupplierInvoicePdfUploadedOn",
                table: "PurchaseInvoices");
        }
    }
}
