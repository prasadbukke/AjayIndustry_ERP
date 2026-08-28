/*
============================================================
File: CustomerReceiptAllocationConfiguration.cs

Module:
Customer Receipt

Purpose:
Entity Framework Core configuration for
CustomerReceiptAllocation.

Important:
- Common BaseEntity fields are not configured here.
- This configuration contains only allocation-specific
  properties, indexes and relationships.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class CustomerReceiptAllocationConfiguration
        : IEntityTypeConfiguration<CustomerReceiptAllocation>
    {
        public void Configure(
            EntityTypeBuilder<CustomerReceiptAllocation> builder)
        {
            #region Table

            builder.ToTable(
                "CustomerReceiptAllocations");

            #endregion


            #region Primary Key

            builder.HasKey(x =>
                x.Id);

            #endregion


            #region Identification

            builder.Property(x =>
                    x.SequenceNumber)
                .IsRequired();

            #endregion


            #region Invoice Snapshot

            builder.Property(x =>
                    x.InvoiceCode)
                .IsRequired()
                .HasMaxLength(
                    50);


            builder.Property(x =>
                    x.InvoiceDate)
                .IsRequired();


            builder.Property(x =>
                    x.InvoiceGrandTotal)
                .HasPrecision(
                    18,
                    2);

            #endregion


            #region Allocation Amounts

            builder.Property(x =>
                    x.AlreadyReceivedAmount)
                .HasPrecision(
                    18,
                    2);


            builder.Property(x =>
                    x.AllocatedAmount)
                .HasPrecision(
                    18,
                    2);


            builder.Property(x =>
                    x.BalanceAfterReceipt)
                .HasPrecision(
                    18,
                    2);

            #endregion


            #region Customer Receipt Relationship

            builder.HasOne(x =>
                    x.CustomerReceipt)
                .WithMany(x =>
                    x.Allocations)
                .HasForeignKey(x =>
                    x.CustomerReceiptId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            #region Invoice Relationship

            builder.HasOne(x =>
                    x.Invoice)
                .WithMany()
                .HasForeignKey(x =>
                    x.InvoiceId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            #endregion


            #region Indexes

            builder.HasIndex(x =>
                x.CustomerReceiptId);


            builder.HasIndex(x =>
                x.InvoiceId);


            builder.HasIndex(x =>
                new
                {
                    x.CustomerReceiptId,
                    x.InvoiceId
                });

            #endregion
        }
    }
}