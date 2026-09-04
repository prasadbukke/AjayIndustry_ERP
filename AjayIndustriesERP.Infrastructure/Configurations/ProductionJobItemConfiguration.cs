/*
============================================================
File: ProductionJobItemConfiguration.cs

Purpose:
Configures ProductionJobItem for Entity Framework Core.

Production Structure:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Item
        ↓
Production Job Step

Responsibilities:
- Configure parent Production Job relationship.
- Configure exact Customer PO Item relationship.
- Configure Item relationship and snapshots.
- Configure Ordered / Production / Completed Quantity.
- Configure Released Routing relationship and snapshots.
- Configure Item-wise Production Steps.
- Prevent the same Customer PO Item from appearing in
  multiple Production Jobs.

Important:
- One Customer PO Item belongs to one Production Job Item.
- Ordered Quantity comes from Customer PO.
- Production Quantity is planned by Admin.
- Completed Quantity is cumulative actual Production output.
- Routing changes later must not modify copied Production
  Job Steps.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class ProductionJobItemConfiguration
        : IEntityTypeConfiguration<ProductionJobItem>
    {
        public void Configure(
            EntityTypeBuilder<ProductionJobItem> builder)
        {
            #region Table

            builder.ToTable(
                "ProductionJobItems");

            #endregion


            #region Primary Key

            builder.HasKey(x =>
                x.Id);

            #endregion


            #region Production Job Relationship

            builder.Property(x =>
                    x.ProductionJobId)
                .IsRequired();


            builder.HasOne(x =>
                    x.ProductionJob)
                .WithMany(x =>
                    x.Items)
                .HasForeignKey(x =>
                    x.ProductionJobId)
                .OnDelete(
                    DeleteBehavior.Cascade);


            builder.HasIndex(x =>
                x.ProductionJobId);

            #endregion


            #region Customer PO Item Relationship

            builder.Property(x =>
                    x.CustomerPurchaseOrderItemId)
                .IsRequired();


            builder.HasOne(x =>
                    x.CustomerPurchaseOrderItem)
                .WithMany()
                .HasForeignKey(x =>
                    x.CustomerPurchaseOrderItemId)
                .OnDelete(
                    DeleteBehavior.Restrict);


            /*
             * One Customer PO Item can belong to only
             * one Production Job Item.
             *
             * Therefore the old design:
             *
             * PO Item
             *   → PJOB-001
             *   → PJOB-002
             *
             * is no longer possible.
             */
            builder.HasIndex(x =>
                    x.CustomerPurchaseOrderItemId)
                .IsUnique();

            #endregion


            #region Item Relationship

            builder.Property(x =>
                    x.ItemId)
                .IsRequired();


            builder.HasOne(x =>
                    x.Item)
                .WithMany()
                .HasForeignKey(x =>
                    x.ItemId)
                .OnDelete(
                    DeleteBehavior.Restrict);


            builder.HasIndex(x =>
                x.ItemId);

            #endregion


            #region Item Snapshot

            builder.Property(x =>
                    x.ItemCode)
                .IsRequired()
                .HasMaxLength(50);


            builder.Property(x =>
                    x.ItemName)
                .IsRequired()
                .HasMaxLength(200);


            builder.Property(x =>
                    x.UnitName)
                .HasMaxLength(100);

            #endregion


            #region Production Quantity

            builder.Property(x =>
                    x.OrderedQuantity)
                .IsRequired()
                .HasPrecision(
                    18,
                    3);


            builder.Property(x =>
                    x.ProductionQuantity)
                .IsRequired()
                .HasPrecision(
                    18,
                    3);


            builder.Property(x =>
                    x.CompletedQuantity)
                .IsRequired()
                .HasPrecision(
                    18,
                    3);

            #endregion


            #region Routing Relationship

            builder.Property(x =>
                    x.ItemProcessRoutingId)
                .IsRequired();


            builder.HasOne(x =>
                    x.ItemProcessRouting)
                .WithMany()
                .HasForeignKey(x =>
                    x.ItemProcessRoutingId)
                .OnDelete(
                    DeleteBehavior.Restrict);


            builder.HasIndex(x =>
                x.ItemProcessRoutingId);


            builder.Property(x =>
                    x.RoutingCode)
                .IsRequired()
                .HasMaxLength(50);


            builder.Property(x =>
                    x.RoutingRevisionNumber)
                .IsRequired();

            #endregion


            #region Pipeline Modification Reason

            builder.Property(x =>
                    x.PipelineModificationReason)
                .HasMaxLength(1000);

            #endregion


            #region Production Steps

            builder.HasMany(x =>
                    x.Steps)
                .WithOne(x =>
                    x.ProductionJobItem)
                .HasForeignKey(x =>
                    x.ProductionJobItemId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            #region Query Indexes

            builder.HasIndex(x =>
                x.IsDeleted);

            #endregion
        }
    }
}