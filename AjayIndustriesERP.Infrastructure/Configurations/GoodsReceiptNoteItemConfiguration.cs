using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ============================================================
// File: GoodsReceiptNoteItemConfiguration.cs
// Purpose:
// Contains Entity Framework Core database configuration for each
// GoodsReceiptNoteItem record.
//
// Configures:
// - GoodsReceiptNoteItems table
// - GRN relationship
// - PurchaseOrderItem relationship
// - Item relationship
// - Quantity precision
// - Item snapshot fields
// - Receipt / Material status
// - Useful database indexes
//
// This table also becomes the source for calculating previously
// received quantity for a PO item in future GRNs.
// ============================================================

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class GoodsReceiptNoteItemConfiguration
        : IEntityTypeConfiguration<GoodsReceiptNoteItem>
    {
        public void Configure(
            EntityTypeBuilder<GoodsReceiptNoteItem> builder)
        {
            builder.ToTable("GoodsReceiptNoteItems");


            // =============================================
            // PRIMARY KEY
            // =============================================

            builder.HasKey(x => x.Id);


            // =============================================
            // CODE
            // =============================================

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);


            // =============================================
            // GRN
            // =============================================

            builder.HasOne(x => x.GoodsReceiptNote)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.GoodsReceiptNoteId)
                .OnDelete(DeleteBehavior.Cascade);


            // =============================================
            // PURCHASE ORDER ITEM
            // =============================================

            builder.HasOne(x => x.PurchaseOrderItem)
                .WithMany()
                .HasForeignKey(x => x.PurchaseOrderItemId)
                .OnDelete(DeleteBehavior.Restrict);


            // =============================================
            // ITEM
            // =============================================

            builder.HasOne(x => x.Item)
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict);


            // =============================================
            // ITEM SNAPSHOT
            // =============================================

            builder.Property(x => x.ItemCode)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.ItemName)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(x => x.Specification)
                .HasMaxLength(1000);

            builder.Property(x => x.UnitName)
                .IsRequired()
                .HasMaxLength(100);


            // =============================================
            // QUANTITIES
            // =============================================

            builder.Property(x => x.OrderedQuantity)
                .HasPrecision(18, 3);

            builder.Property(x => x.PreviouslyReceivedQuantity)
                .HasPrecision(18, 3);

            builder.Property(x => x.BalanceQuantity)
                .HasPrecision(18, 3);

            builder.Property(x => x.ReceivedQuantity)
                .HasPrecision(18, 3);

            builder.Property(x => x.PendingQuantity)
                .HasPrecision(18, 3);


            // =============================================
            // ENUMS
            // =============================================

            builder.Property(x => x.ReceiptStatus)
                .IsRequired();

            builder.Property(x => x.MaterialStatus);


            // =============================================
            // REMARKS
            // =============================================

            builder.Property(x => x.Remarks)
                .HasMaxLength(1000);


            // =============================================
            // INDEXES
            // =============================================

            builder.HasIndex(x => x.GoodsReceiptNoteId);

            builder.HasIndex(x => x.PurchaseOrderItemId);

            builder.HasIndex(x => x.ItemId);
        }
    }
}