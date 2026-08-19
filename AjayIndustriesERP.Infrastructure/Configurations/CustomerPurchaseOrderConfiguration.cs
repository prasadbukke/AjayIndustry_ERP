/*
============================================================
File: CustomerPurchaseOrderConfiguration.cs

Purpose:
Configures CustomerPurchaseOrder for Entity Framework Core.

Responsibilities:
- Map Customer Purchase Orders to database table.
- Configure Customer relationship.
- Configure required fields and field lengths.
- Configure Customer PO Priority and Status.
- Enforce unique internal Customer PO Code.
- Prevent duplicate Customer PO Number for the same Customer.

Important:
- Same Customer + Same Customer PO Number is not allowed.
- Different Customers may use the same Customer PO Number.
- Customer relationship uses Restrict delete behavior.
- CustomerName is stored as a historical snapshot.
- Production Pipeline information is intentionally not mapped here.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class CustomerPurchaseOrderConfiguration
        : IEntityTypeConfiguration<CustomerPurchaseOrder>
    {
        public void Configure(
            EntityTypeBuilder<CustomerPurchaseOrder> builder)
        {
            #region Table

            builder.ToTable(
                "CustomerPurchaseOrders");

            #endregion


            #region Primary Key

            builder.HasKey(x => x.Id);

            #endregion


            #region Internal Customer PO Code

            builder.Property(x => x.Code)
                .IsRequired()
                .HasMaxLength(50);


            builder.HasIndex(x => x.Code)
                .IsUnique();

            #endregion


            #region Customer Relationship

            builder.Property(x => x.CustomerId)
                .IsRequired();


            builder.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => x.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion


            #region Customer Snapshot

            builder.Property(x => x.CustomerName)
                .IsRequired()
                .HasMaxLength(200);

            #endregion


            #region Customer Purchase Order Information

            builder.Property(
                    x => x.CustomerPurchaseOrderNumber)
                .IsRequired()
                .HasMaxLength(100);


            builder.Property(
                    x => x.CustomerPurchaseOrderDate)
                .IsRequired();


            builder.Property(
                    x => x.ReceivedDate)
                .IsRequired();


            builder.Property(
                    x => x.RequiredDeliveryDate)
                .IsRequired();

            #endregion


            #region Priority

            builder.Property(x => x.Priority)
                .IsRequired();

            #endregion


            #region Status

            builder.Property(x => x.Status)
                .IsRequired();

            #endregion


            #region Customer Reference

            builder.Property(x => x.CustomerReference)
                .HasMaxLength(200);

            #endregion


            #region Remarks

            builder.Property(x => x.Remarks)
                .HasMaxLength(1000);

            #endregion


            #region Duplicate Customer PO Protection

            /*
             Same Customer + Same Customer PO Number
             must never be duplicated.

             Example:

             ABC Ltd + PO-100  -> Allowed first time
             ABC Ltd + PO-100  -> Duplicate / Blocked

             XYZ Ltd + PO-100  -> Allowed
            */

            builder.HasIndex(x => new
            {
                x.CustomerId,
                x.CustomerPurchaseOrderNumber
            })
                .IsUnique();

            #endregion


            #region Query Indexes

            builder.HasIndex(x =>
                x.CustomerId);


            builder.HasIndex(x =>
                x.CustomerPurchaseOrderDate);


            builder.HasIndex(x =>
                x.RequiredDeliveryDate);


            builder.HasIndex(x =>
                x.Status);


            builder.HasIndex(x =>
                x.Priority);


            builder.HasIndex(x =>
                x.IsDeleted);

            #endregion
        }
    }
}