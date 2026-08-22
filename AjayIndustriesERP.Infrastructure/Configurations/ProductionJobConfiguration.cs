/*
============================================================
File: ProductionJobConfiguration.cs

Purpose:
Configures ProductionJob for Entity Framework Core.

Responsibilities:
- Map Production Job Header to database.
- Configure Customer PO Item relationship.
- Configure Item relationship.
- Configure Item Process Routing relationship.
- Configure Job Quantity precision.
- Configure Routing and Item snapshot fields.
- Configure Production Job Status and planning dates.
- Configure indexes used by Production queries.

Important:
- One Customer PO Item may have multiple Production Jobs.
- Routing reference identifies the Routing Revision used when
  the Job was created.
- Production Job Code is globally unique.
- Customer PO, Item and Routing records cannot be physically
  deleted while referenced by a Production Job.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class ProductionJobConfiguration
        : IEntityTypeConfiguration<ProductionJob>
    {
        public void Configure(
            EntityTypeBuilder<ProductionJob> builder)
        {
            #region Table

            builder.ToTable(
                "ProductionJobs");

            #endregion


            #region Primary Key

            builder.HasKey(x =>
                x.Id);

            #endregion


            #region Production Job Code

            builder.Property(x =>
                    x.Code)
                .IsRequired()
                .HasMaxLength(50);


            builder.HasIndex(x =>
                    x.Code)
                .IsUnique();

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


            #region Job Quantity

            builder.Property(x =>
                    x.JobQuantity)
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


            builder.Property(x =>
                    x.RoutingCode)
                .IsRequired()
                .HasMaxLength(50);


            builder.Property(x =>
                    x.RoutingRevisionNumber)
                .IsRequired();

            #endregion


            #region Job Status

            builder.Property(x =>
                    x.Status)
                .IsRequired();

            #endregion


            #region Remarks And Cancellation

            builder.Property(x =>
                    x.Remarks)
                .HasMaxLength(1000);


            builder.Property(x =>
                    x.PipelineModificationReason)
                .HasMaxLength(1000);


            builder.Property(x =>
                    x.CancellationReason)
                .HasMaxLength(1000);


            builder.HasIndex(x =>
                x.CancelledOn);

            #endregion


            #region Production Steps

            builder.HasMany(x =>
                    x.Steps)
                .WithOne(x =>
                    x.ProductionJob)
                .HasForeignKey(x =>
                    x.ProductionJobId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            #region Query Indexes

            builder.HasIndex(x =>
                x.CustomerPurchaseOrderItemId);


            builder.HasIndex(x =>
                x.ItemId);


            builder.HasIndex(x =>
                x.ItemProcessRoutingId);


            builder.HasIndex(x =>
                x.Status);


            builder.HasIndex(x =>
                x.PlannedStartOn);


            builder.HasIndex(x =>
                x.IsDeleted);

            #endregion
        }
    }
}