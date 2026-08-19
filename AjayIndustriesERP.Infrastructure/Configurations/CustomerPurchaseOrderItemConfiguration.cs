/*
============================================================
File: CustomerPurchaseOrderItemConfiguration.cs

Purpose:
Configures CustomerPurchaseOrderItem for Entity Framework Core.

Responsibilities:
- Map Customer PO Items to database table.
- Configure parent Customer PO relationship.
- Configure existing Item Master relationship.
- Configure Item snapshot fields.
- Configure Customer-specific Item / Drawing references.
- Configure Ordered Quantity precision.
- Configure optional line Delivery Date and Priority overrides.

Important:
- Customer PO deletion cascades to its line items.
- Item Master deletion is restricted when referenced.
- The same Item may legitimately appear on multiple PO lines,
  therefore CustomerPurchaseOrderId + ItemId is NOT unique.
- Production Job relationships will be introduced later.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class CustomerPurchaseOrderItemConfiguration
        : IEntityTypeConfiguration<CustomerPurchaseOrderItem>
    {
        public void Configure(
            EntityTypeBuilder<CustomerPurchaseOrderItem> builder)
        {
            #region Table

            builder.ToTable(
                "CustomerPurchaseOrderItems");

            #endregion


            #region Primary Key

            builder.HasKey(x => x.Id);

            #endregion


            #region Internal Line Code

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);


            builder.HasIndex(x => x.Code)
                .IsUnique();

            #endregion


            #region Customer Purchase Order Relationship

            builder.Property(
                    x => x.CustomerPurchaseOrderId)
                .IsRequired();


            builder.HasOne(
                    x => x.CustomerPurchaseOrder)
                .WithMany(
                    x => x.Items)
                .HasForeignKey(
                    x => x.CustomerPurchaseOrderId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            #region Item Master Relationship

            builder.Property(x => x.ItemId)
                .IsRequired();


            builder.HasOne(x => x.Item)
                .WithMany()
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion


            #region Item Snapshot

            builder.Property(x => x.ItemCode)
                .IsRequired()
                .HasMaxLength(50);


            builder.Property(x => x.ItemName)
                .IsRequired()
                .HasMaxLength(250);


            builder.Property(x => x.Specification)
                .HasMaxLength(1000);


            builder.Property(x => x.UnitName)
                .IsRequired()
                .HasMaxLength(100);

            #endregion


            #region Customer Item Reference

            builder.Property(x => x.CustomerItemCode)
                .HasMaxLength(100);


            builder.Property(
                    x => x.CustomerDrawingNumber)
                .HasMaxLength(150);


            builder.Property(x => x.Revision)
                .HasMaxLength(50);

            #endregion


            #region Ordered Quantity

            builder.Property(x => x.OrderedQuantity)
                .HasPrecision(18, 3)
                .IsRequired();

            #endregion


            #region Delivery Date Override

            builder.Property(
                x => x.RequiredDeliveryDate);

            #endregion


            #region Priority Override

            builder.Property(
                x => x.Priority);

            #endregion


            #region Remarks

            builder.Property(x => x.Remarks)
                .HasMaxLength(1000);

            #endregion


            #region Query Indexes

            builder.HasIndex(
                x => x.CustomerPurchaseOrderId);


            builder.HasIndex(
                x => x.ItemId);


            builder.HasIndex(
                x => x.RequiredDeliveryDate);


            builder.HasIndex(
                x => x.Priority);


            builder.HasIndex(
                x => x.IsDeleted);

            #endregion
        }
    }
}