/*
============================================================
File: PurchaseInvoiceConfiguration.cs

Module:
Purchase Invoice / Supplier Bill

Purpose:
Configures PurchaseInvoice Entity Framework mapping.

Important:
- Common BaseEntity fields are configured by the
  existing project conventions / BaseEntity mapping.
- Purchase Invoice Code is globally unique.
- Supplier Invoice Number duplicate validation is handled
  by PurchaseInvoiceService per Supplier.
- Purchase Order / Supplier / Company deletion is restricted.
- Purchase Invoice Items are deleted through cascade when
  the Purchase Invoice itself is physically removed.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class PurchaseInvoiceConfiguration
        : IEntityTypeConfiguration<PurchaseInvoice>
    {
        public void Configure(
            EntityTypeBuilder<PurchaseInvoice> builder)
        {
            #region Table / Key

            builder.ToTable(
                "PurchaseInvoices");


            builder.HasKey(x =>
                x.Id);

            #endregion


            #region Identification

            builder.Property(x =>
                    x.Code)
                .IsRequired()
                .HasMaxLength(
                    50);


            builder.HasIndex(x =>
                    x.Code)
                .IsUnique();


            builder.Property(x =>
                    x.PurchaseInvoiceDate)
                .IsRequired();


            builder.Property(x =>
                    x.Status)
                .IsRequired();

            #endregion


            #region Supplier Invoice

            builder.Property(x =>
                    x.SupplierInvoiceNumber)
                .IsRequired()
                .HasMaxLength(
                    100);


            builder.Property(x =>
                    x.SupplierInvoiceDate)
                .IsRequired();


            /*
             * Not marked UNIQUE at database level.
             *
             * Business rule:
             * Supplier Invoice Number must be unique
             * for the same active Supplier.
             *
             * Soft-deleted Draft Purchase Invoices should
             * not permanently block the same Supplier
             * Invoice Number.
             */
            builder.HasIndex(x =>
                new
                {
                    x.SupplierId,
                    x.SupplierInvoiceNumber
                });

            #endregion


            #region Purchase Order

            builder.Property(x =>
                    x.PurchaseOrderCode)
                .IsRequired()
                .HasMaxLength(
                    50);


            builder.HasIndex(x =>
                x.PurchaseOrderId);


            builder.HasOne(x =>
                    x.PurchaseOrder)
                .WithMany()
                .HasForeignKey(x =>
                    x.PurchaseOrderId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            #endregion


            #region Supplier

            builder.Property(x =>
                    x.SupplierName)
                .IsRequired()
                .HasMaxLength(
                    200);


            builder.Property(x =>
                    x.SupplierSnapshotJson)
                .HasColumnType(
                    "nvarchar(max)");


            builder.HasIndex(x =>
                x.SupplierId);


            builder.HasOne(x =>
                    x.Supplier)
                .WithMany()
                .HasForeignKey(x =>
                    x.SupplierId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            #endregion


            #region Company

            builder.Property(x =>
                    x.CompanyName)
                .IsRequired()
                .HasMaxLength(
                    200);


            builder.Property(x =>
                    x.CompanySnapshotJson)
                .HasColumnType(
                    "nvarchar(max)");


            builder.HasIndex(x =>
                x.CompanyId);


            builder.HasOne(x =>
                    x.Company)
                .WithMany()
                .HasForeignKey(x =>
                    x.CompanyId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            #endregion


            #region Payment

            builder.Property(x =>
                    x.PaymentTerms)
                .HasMaxLength(
                    500);


            builder.Property(x =>
                    x.PlaceOfSupply)
                .HasMaxLength(
                    150);

            #endregion


            #region Financial Totals

            builder.Property(x =>
                    x.GrossAmount)
                .HasColumnType(
                    "decimal(18,2)");


            builder.Property(x =>
                    x.DiscountAmount)
                .HasColumnType(
                    "decimal(18,2)");


            builder.Property(x =>
                    x.TaxableAmount)
                .HasColumnType(
                    "decimal(18,2)");


            builder.Property(x =>
                    x.CgstAmount)
                .HasColumnType(
                    "decimal(18,2)");


            builder.Property(x =>
                    x.SgstAmount)
                .HasColumnType(
                    "decimal(18,2)");


            builder.Property(x =>
                    x.IgstAmount)
                .HasColumnType(
                    "decimal(18,2)");


            builder.Property(x =>
                    x.TransportCharges)
                .HasColumnType(
                    "decimal(18,2)");


            builder.Property(x =>
                    x.OtherCharges)
                .HasColumnType(
                    "decimal(18,2)");


            builder.Property(x =>
                    x.RoundOffAmount)
                .HasColumnType(
                    "decimal(18,2)");


            builder.Property(x =>
                    x.GrandTotal)
                .HasColumnType(
                    "decimal(18,2)");

            #endregion


            #region Remarks / Finalization

            builder.Property(x =>
                    x.Remarks)
                .HasMaxLength(
                    2000);


            builder.Property(x =>
                    x.FinalizedBy)
                .HasMaxLength(
                    200);

            #endregion


            #region Items

            builder.HasMany(x =>
                    x.Items)
                .WithOne(x =>
                    x.PurchaseInvoice)
                .HasForeignKey(x =>
                    x.PurchaseInvoiceId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            #region Indexes

            builder.HasIndex(x =>
                x.PurchaseInvoiceDate);


            builder.HasIndex(x =>
                x.SupplierInvoiceDate);


            builder.HasIndex(x =>
                x.Status);


            builder.HasIndex(x =>
                new
                {
                    x.SupplierId,
                    x.PurchaseInvoiceDate
                });


            builder.HasIndex(x =>
                new
                {
                    x.PurchaseOrderId,
                    x.Status
                });

            #endregion
        }
    }
}