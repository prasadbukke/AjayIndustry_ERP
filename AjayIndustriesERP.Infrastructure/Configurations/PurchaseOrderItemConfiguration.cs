using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class PurchaseOrderItemConfiguration
        : IEntityTypeConfiguration<PurchaseOrderItem>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
        {
            builder.ToTable("PurchaseOrderItems");

            builder.HasKey(x => x.Id);


            // Standard Code
            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);


            // Item Snapshot
            builder.Property(x => x.ItemCode)
                .HasMaxLength(100);

            builder.Property(x => x.ItemName)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.Specification)
                .HasMaxLength(1000);

            builder.Property(x => x.UnitName)
                .HasMaxLength(50);

            builder.Property(x => x.HSNCode)
                .HasMaxLength(50);


            // Drawing Snapshot
            builder.Property(x => x.DrawingNumber)
                .HasMaxLength(100);

            builder.Property(x => x.DrawingRevision)
                .HasMaxLength(50);


            // Quantity / Rate
            builder.Property(x => x.Quantity)
                .HasPrecision(18, 3);

            builder.Property(x => x.UnitPrice)
                .HasPrecision(18, 2);


            // Discount
            builder.Property(x => x.DiscountPercent)
                .HasPrecision(9, 4);

            builder.Property(x => x.DiscountAmount)
                .HasPrecision(18, 2);


            // Tax
            builder.Property(x => x.TaxableAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.GSTPercent)
                .HasPrecision(9, 4);

            builder.Property(x => x.CGSTAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.SGSTAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.IGSTAmount)
                .HasPrecision(18, 2);

            builder.Property(x => x.LineTotal)
                .HasPrecision(18, 2);


            // Additional
            builder.Property(x => x.Remarks)
                .HasMaxLength(500);


            // BaseEntity
            builder.Property(x => x.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.ModifiedBy)
                .HasMaxLength(100);


            // Item Relation
            builder.HasOne(x => x.Item)
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict);


            // Drawing Relation - Optional
            builder.HasOne(x => x.Drawing)
                .WithMany()
                .HasForeignKey(x => x.DrawingId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}