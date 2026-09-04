/*
============================================================
File: ProductionJobConfiguration.cs

Purpose:
Configures ProductionJob for Entity Framework Core.

Production Structure:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Item
        ↓
Production Job Step

Responsibilities:
- Map Production Job Header.
- Configure Customer Purchase Order relationship.
- Enforce one Production Job per Customer PO.
- Configure Production Job lifecycle fields.
- Configure planning / actual dates.
- Configure Production Job Items relationship.
- Configure Production Job query indexes.

Important:
- One Customer Purchase Order has one Production Job.
- Item / Quantity / Routing belong to ProductionJobItem.
- Production Steps belong to ProductionJobItem.
- Production Job Code is globally unique.
- Customer PO cannot be physically deleted while referenced.
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


            #region Customer Purchase Order Relationship

            builder.Property(x =>
                    x.CustomerPurchaseOrderId)
                .IsRequired();


            builder.HasOne(x =>
                    x.CustomerPurchaseOrder)
                .WithMany()
                .HasForeignKey(x =>
                    x.CustomerPurchaseOrderId)
                .OnDelete(
                    DeleteBehavior.Restrict);


            /*
             * One Customer PO = One Production Job.
             *
             * This is enforced at database level also,
             * not only through Application Service.
             */
            builder.HasIndex(x =>
                    x.CustomerPurchaseOrderId)
                .IsUnique();

            #endregion


            #region Job Status

            builder.Property(x =>
                    x.Status)
                .IsRequired();


            builder.HasIndex(x =>
                x.Status);

            #endregion


            #region Planning

            builder.HasIndex(x =>
                x.PlannedStartOn);

            #endregion


            #region Remarks And Cancellation

            builder.Property(x =>
                    x.Remarks)
                .HasMaxLength(1000);


            builder.Property(x =>
                    x.CancellationReason)
                .HasMaxLength(1000);


            builder.HasIndex(x =>
                x.CancelledOn);

            #endregion


            #region Production Job Items

            builder.HasMany(x =>
                    x.Items)
                .WithOne(x =>
                    x.ProductionJob)
                .HasForeignKey(x =>
                    x.ProductionJobId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            #region Soft Delete

            builder.HasIndex(x =>
                x.IsDeleted);

            #endregion
        }
    }
}