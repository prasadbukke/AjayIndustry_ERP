/*
============================================================
File: PurchaseInvoiceItemConfiguration.cs

Module:
Purchase Invoice / Supplier Bill

Purpose:
Configures PurchaseInvoiceItem Entity Framework mapping.

Important:
- One Purchase Invoice Item points to one exact
  GoodsReceiptNoteItem source.
- PurchaseOrderItem / GRN / Item deletions are restricted.
- Draft + Finalized Purchase Invoice quantity reservation
  is handled in Repository / Service.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class PurchaseInvoiceItemConfiguration
        : IEntityTypeConfiguration<PurchaseInvoiceItem>
    {
        public void Configure(
            EntityTypeBuilder<PurchaseInvoiceItem> builder)
        {
            #region Table / Key

            builder.ToTable(
                "PurchaseInvoiceItems");


            builder.HasKey(x =>
                x.Id);

            #endregion


            #region Sequence

            builder.Property(x =>
                    x.SequenceNumber)
                .IsRequired();

            #endregion


            #region Purchase Order Source

            builder.Property(x =>
                    x.PurchaseOrderCode)
                .IsRequired()
                .HasMaxLength(
                    50);


            builder.Property(x =>
                    x.PurchaseOrderQuantity)
                .HasColumnType(
                    "decimal(18,3)");


            builder.HasIndex(x =>
                x.PurchaseOrderItemId);


            builder.HasOne(x =>
                    x.PurchaseOrderItem)
                .WithMany()
                .HasForeignKey(x =>
                    x.PurchaseOrderItemId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            #endregion


            #region GRN Source

            builder.Property(x =>
                    x.GoodsReceiptNoteCode)
                .IsRequired()
                .HasMaxLength(
                    50);


            builder.Property(x =>
                    x.GoodsReceiptQuantity)
                .HasColumnType(
                    "decimal(18,3)");


            builder.Property(x =>
                    x.SupplierChallanNumber)
                .HasMaxLength(
                    100);


            builder.HasIndex(x =>
                x.GoodsReceiptNoteId);


            builder.HasIndex(x =>
                x.GoodsReceiptNoteItemId);


            builder.HasOne(x =>
                    x.GoodsReceiptNote)
                .WithMany()
                .HasForeignKey(x =>
                    x.GoodsReceiptNoteId)
                .OnDelete(
                    DeleteBehavior.Restrict);


            builder.HasOne(x =>
                    x.GoodsReceiptNoteItem)
                .WithMany()
                .HasForeignKey(x =>
                    x.GoodsReceiptNoteItemId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            #endregion


            #region Item Snapshot

            builder.Property(x =>
                    x.ItemCode)
                .IsRequired()
                .HasMaxLength(
                    100);


            builder.Property(x =>
                    x.ItemName)
                .IsRequired()
                .HasMaxLength(
                    300);


            builder.Property(x =>
                    x.Description)
                .HasMaxLength(
                    1000);


            builder.Property(x =>
                    x.Specification)
                .HasMaxLength(
                    2000);


            builder.Property(x =>
                    x.UnitName)
                .HasMaxLength(
                    100);


            builder.Property(x =>
                    x.HsnCode)
                .HasMaxLength(
                    50);


            builder.HasIndex(x =>
                x.ItemId);


            builder.HasOne(x =>
                    x.Item)
                .WithMany()
                .HasForeignKey(x =>
                    x.ItemId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            #endregion


            #region Drawing Snapshot

            builder.Property(x =>
                    x.DrawingNumber)
                .HasMaxLength(
                    100);


            builder.Property(x =>
                    x.DrawingRevision)
                .HasMaxLength(
                    100);

            #endregion


            #region Quantity

            builder.Property(x =>
                    x.PurchaseInvoiceQuantity)
                .HasColumnType(
                    "decimal(18,3)")
                .IsRequired();

            #endregion


            #region Commercial Values

            builder.Property(x =>
                    x.Rate)
                .HasColumnType(
                    "decimal(18,2)");


            builder.Property(x =>
                    x.GrossAmount)
                .HasColumnType(
                    "decimal(18,2)");


            builder.Property(x =>
                    x.DiscountPercent)
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

            #endregion


            #region GST

            builder.Property(x =>
                    x.GstRate)
                .HasColumnType(
                    "decimal(18,2)");


            builder.Property(x =>
                    x.CgstRate)
                .HasColumnType(
                    "decimal(18,2)");


            builder.Property(x =>
                    x.SgstRate)
                .HasColumnType(
                    "decimal(18,2)");


            builder.Property(x =>
                    x.IgstRate)
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
                    x.TotalTaxAmount)
                .HasColumnType(
                    "decimal(18,2)");

            #endregion


            #region Line Total

            builder.Property(x =>
                    x.LineTotal)
                .HasColumnType(
                    "decimal(18,2)");

            #endregion


            #region Purchase Invoice Relationship

            builder.HasIndex(x =>
                x.PurchaseInvoiceId);


            builder.HasOne(x =>
                    x.PurchaseInvoice)
                .WithMany(x =>
                    x.Items)
                .HasForeignKey(x =>
                    x.PurchaseInvoiceId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            #region Composite Indexes

            builder.HasIndex(x =>
                new
                {
                    x.PurchaseInvoiceId,
                    x.SequenceNumber
                });


            builder.HasIndex(x =>
                new
                {
                    x.PurchaseInvoiceId,
                    x.GoodsReceiptNoteItemId
                });


            /*
             * Intentionally NOT unique.
             *
             * Historical deleted rows may exist.
             * Duplicate active source validation is handled
             * by PurchaseInvoiceService.
             */
            builder.HasIndex(x =>
                new
                {
                    x.GoodsReceiptNoteItemId,
                    x.IsDeleted,
                    x.IsActive
                });

            #endregion
        }
    }
}