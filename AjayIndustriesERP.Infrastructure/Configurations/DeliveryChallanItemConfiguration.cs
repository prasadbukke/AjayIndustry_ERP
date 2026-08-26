/*
============================================================
File: DeliveryChallanItemConfiguration.cs

Purpose:
Configures Entity Framework mapping for
DeliveryChallanItem.

Responsibilities:
- Configure DeliveryChallanItems table.
- Configure parent Delivery Challan relationship.
- Configure Finalized PDI relationship.
- Configure Production Job / Customer PO / Item snapshots.
- Configure Customer Drawing snapshot.
- Configure Dispatch Quantity precision.
- Configure useful indexes and line sequencing.

Important:
- DeliveryChallanItem is a dispatch snapshot line.
- PDI relationship uses Restrict delete behaviour.
- Dispatch Quantity business validation belongs
  in DeliveryChallanService.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class DeliveryChallanItemConfiguration
        : IEntityTypeConfiguration<DeliveryChallanItem>
    {
        #region Configure

        public void Configure(
            EntityTypeBuilder<DeliveryChallanItem> builder)
        {
            #region Table

            builder.ToTable(
                "DeliveryChallanItems");

            #endregion


            #region Primary Key

            builder.HasKey(
                x => x.Id);

            #endregion


            #region Delivery Challan Relationship

            builder.HasOne(
                    x => x.DeliveryChallan)
                .WithMany(
                    x => x.Items)
                .HasForeignKey(
                    x => x.DeliveryChallanId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            #region Sequence Number

            builder.Property(
                    x => x.SequenceNumber)
                .IsRequired();


            builder.HasIndex(
                    x => new
                    {
                        x.DeliveryChallanId,
                        x.SequenceNumber
                    })
                .IsUnique();

            #endregion


            #region PDI Source

            builder.HasOne(
                    x => x.PreDispatchInspection)
                .WithMany()
                .HasForeignKey(
                    x => x.PreDispatchInspectionId)
                .OnDelete(
                    DeleteBehavior.Restrict);


            builder.Property(
                    x => x.PreDispatchInspectionCode)
                .IsRequired()
                .HasMaxLength(
                    50);


            builder.Property(
                    x => x.PdiAcceptedQuantity)
                .HasPrecision(
                    18,
                    3)
                .IsRequired();


            builder.HasIndex(
                x => x.PreDispatchInspectionId);

            #endregion


            #region Production Job Snapshot

            builder.Property(
                    x => x.ProductionJobCode)
                .IsRequired()
                .HasMaxLength(
                    50);


            builder.HasIndex(
                x => x.ProductionJobId);

            #endregion


            #region Customer PO Snapshot

            builder.Property(
                    x => x.CustomerPurchaseOrderCode)
                .IsRequired()
                .HasMaxLength(
                    50);


            builder.Property(
                    x => x.CustomerPurchaseOrderNumber)
                .IsRequired()
                .HasMaxLength(
                    100);


            builder.Property(
                    x => x.CustomerItemCode)
                .HasMaxLength(
                    100);


            builder.HasIndex(
                x => x.CustomerPurchaseOrderItemId);

            #endregion


            #region Item Snapshot

            builder.Property(
                    x => x.ItemCode)
                .IsRequired()
                .HasMaxLength(
                    100);


            builder.Property(
                    x => x.ItemName)
                .IsRequired()
                .HasMaxLength(
                    250);


            builder.Property(
                    x => x.PartNumber)
                .HasMaxLength(
                    100);


            builder.Property(
                    x => x.UnitName)
                .HasMaxLength(
                    50);


            builder.HasIndex(
                x => x.ItemId);

            #endregion

            #region Product Reference

            builder.Property(
                    x => x.ProductReference)
                .HasMaxLength(
                    100);

            #region HSN Information

            builder.Property(
                    x => x.HsnNumber)
                .HasMaxLength(
                    50);

            #endregion

            #endregion

            #region Customer Drawing Snapshot

            builder.Property(
                    x => x.CustomerDrawingNumber)
                .HasMaxLength(
                    100);


            builder.Property(
                    x => x.CustomerDrawingRevision)
                .HasMaxLength(
                    50);


            builder.HasIndex(
                x => x.CustomerDrawingId);

            #endregion


            #region Dispatch Quantity

            builder.Property(
                    x => x.DispatchQuantity)
                .HasPrecision(
                    18,
                    3)
                .IsRequired();

            #endregion


            #region Additional Indexes

            builder.HasIndex(
                x => new
                {
                    x.PreDispatchInspectionId,
                    x.DeliveryChallanId
                });


            builder.HasIndex(
                x => new
                {
                    x.CustomerPurchaseOrderItemId,
                    x.ItemId
                });

            #endregion
        }

        #endregion
    }
}