using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

// ============================================================
// File: GoodsReceiptNoteConfiguration.cs
// Purpose:
// Contains Entity Framework Core database configuration for the
// GoodsReceiptNote header entity.
//
// Configures:
// - GoodsReceiptNotes table
// - Primary key
// - Unique GRN code
// - Purchase Order relationship
// - Supplier relationship
// - Field lengths
// - Delete behavior
//
// Keeping EF configuration in this separate file follows the
// project's Clean Architecture and database configuration pattern.
// ============================================================

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class GoodsReceiptNoteConfiguration
        : IEntityTypeConfiguration<GoodsReceiptNote>
    {
        public void Configure(
            EntityTypeBuilder<GoodsReceiptNote> builder)
        {
            builder.ToTable("GoodsReceiptNotes");


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

            builder.HasIndex(x => x.Code)
                .IsUnique();


            // =============================================
            // DATE
            // =============================================

            builder.Property(x => x.GRNDate)
                .IsRequired();


            // =============================================
            // PURCHASE ORDER
            // =============================================

            builder.HasOne(x => x.PurchaseOrder)
                .WithMany()
                .HasForeignKey(x => x.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Restrict);


            // =============================================
            // SUPPLIER
            // =============================================

            builder.HasOne(x => x.Supplier)
                .WithMany()
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.SupplierName)
                .IsRequired()
                .HasMaxLength(250);


            // =============================================
            // CHALLAN
            // =============================================

            builder.Property(x => x.SupplierChallanNumber)
                .HasMaxLength(100);

            // =====================================================
            // UNIQUE SUPPLIER CHALLAN
            // =====================================================
            //
            // Prevents duplicate challan numbers for the same supplier
            // at database level.
            //
            // SupplierChallanNumber is optional, therefore null values
            // are excluded from the unique index.
            // =====================================================

            builder.HasIndex(x => new
            {
                x.SupplierId,
                x.SupplierChallanNumber
            })
                .IsUnique()
                .HasFilter(
                    "[SupplierChallanNumber] IS NOT NULL");


            // =============================================
            // REMARKS
            // =============================================

            builder.Property(x => x.Remarks)
                .HasMaxLength(1000);
        }
    }
}