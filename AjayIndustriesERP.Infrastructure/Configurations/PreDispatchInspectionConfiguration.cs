/*
============================================================
File: PreDispatchInspectionConfiguration.cs

Purpose:
Configures PreDispatchInspection entity for EF Core.

Responsibilities:
- Configure primary key.
- Configure PDI Code uniqueness.
- Configure snapshot field lengths.
- Configure quantity precision.
- Configure Production Job relationship.
- Configure child Inspection Lines.
- Configure useful database indexes.

Important:
- One Production Job may have multiple PDI Reports.
- PDI Code is permanently unique.
- Snapshot data must remain independent of later
  Customer / Item / Drawing changes.
- Production Job deletion must not cascade-delete PDI.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class PreDispatchInspectionConfiguration
        : IEntityTypeConfiguration<PreDispatchInspection>
    {
        public void Configure(
            EntityTypeBuilder<PreDispatchInspection> builder)
        {
            #region Table

            builder.ToTable(
                "PreDispatchInspections");

            #endregion


            #region Primary Key

            builder.HasKey(
                x => x.Id);

            #endregion


            #region Identification

            builder.Property(
                    x => x.Code)
                .IsRequired()
                .HasMaxLength(50);


            builder.HasIndex(
                    x => x.Code)
                .IsUnique();


            builder.Property(
                    x => x.InspectionDate)
                .IsRequired();


            builder.Property(
                    x => x.Status)
                .IsRequired();


            builder.Property(
                    x => x.Result)
                .IsRequired();

            #endregion


            #region Production Job

            builder.Property(
                    x => x.ProductionJobCode)
                .IsRequired()
                .HasMaxLength(50);


            builder.HasOne(
                    x => x.ProductionJob)
                .WithMany()
                .HasForeignKey(
                    x => x.ProductionJobId)
                .OnDelete(
                    DeleteBehavior.Restrict);


            /*
             * Multiple PDI Reports are allowed
             * against one Production Job.
             */
            builder.HasIndex(
                x => x.ProductionJobId);

            #endregion


            #region Customer Snapshot

            builder.Property(
                    x => x.CustomerName)
                .IsRequired()
                .HasMaxLength(250);


            builder.HasIndex(
                x => x.CustomerId);

            #endregion


            #region Customer PO Snapshot

            builder.Property(
                    x => x.CustomerPurchaseOrderCode)
                .IsRequired()
                .HasMaxLength(50);


            builder.Property(
                    x => x.CustomerPurchaseOrderNumber)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(
                    x => x.CustomerItemCode)
                .HasMaxLength(100);


            builder.HasIndex(
                x => x.CustomerPurchaseOrderItemId);

            #endregion


            #region Item Snapshot

            builder.Property(
                    x => x.ItemCode)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(
                    x => x.ItemName)
                .IsRequired()
                .HasMaxLength(250);


            builder.Property(
                    x => x.PartNumber)
                .HasMaxLength(100);


            builder.Property(
                    x => x.UnitName)
                .HasMaxLength(50);


            builder.HasIndex(
                x => x.ItemId);

            #endregion


            #region Workshop Drawing Snapshot

            builder.Property(
                    x => x.WorkshopDrawingNumber)
                .HasMaxLength(100);


            builder.Property(
                    x => x.WorkshopDrawingRevision)
                .HasMaxLength(50);

            #endregion


            #region Customer Drawing Snapshot

            builder.Property(
                    x => x.CustomerDrawingNumber)
                .HasMaxLength(100);


            builder.Property(
                    x => x.CustomerDrawingRevision)
                .HasMaxLength(50);

            #endregion


            #region Invoice Information

            builder.Property(
                    x => x.InvoiceNumber)
                .HasMaxLength(100);


            builder.Property(
                    x => x.InvoiceQuantity)
                .HasPrecision(
                    18,
                    3);

            #endregion


            #region Inspection Quantity

            builder.Property(
                    x => x.InspectionQuantity)
                .HasPrecision(
                    18,
                    3);


            builder.Property(
                    x => x.AcceptedQuantity)
                .HasPrecision(
                    18,
                    3);


            builder.Property(
                    x => x.ReworkQuantity)
                .HasPrecision(
                    18,
                    3);


            builder.Property(
                    x => x.RejectedQuantity)
                .HasPrecision(
                    18,
                    3);

            #endregion


            #region Remarks

            builder.Property(
                    x => x.SupplierRemarks)
                .HasMaxLength(1000);


            builder.Property(
                    x => x.InspectionRemarks)
                .HasMaxLength(2000);

            #endregion


            #region Inspection And Approval

            builder.Property(
                    x => x.InspectedBy)
                .HasMaxLength(150);


            builder.Property(
                    x => x.ReviewedBy)
                .HasMaxLength(150);

            #endregion


            #region Finalization

            builder.Property(
                    x => x.FinalizedBy)
                .HasMaxLength(150);

            #endregion


            #region PDF

            builder.Property(
                    x => x.PdfFileName)
                .HasMaxLength(255);


            builder.Property(
                    x => x.PdfFilePath)
                .HasMaxLength(500);

            #endregion


            #region Inspection Lines

            builder.HasMany(
                    x => x.Lines)
                .WithOne(
                    x => x.PreDispatchInspection)
                .HasForeignKey(
                    x => x.PreDispatchInspectionId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion
        }
    }
}