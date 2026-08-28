/*
============================================================
File: CustomerReceiptConfiguration.cs

Module:
Customer Receipt

Purpose:
Entity Framework Core configuration for CustomerReceipt.

Important:
- Common BaseEntity fields are not configured here.
- This configuration contains only Customer Receipt
  specific properties, indexes and relationships.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class CustomerReceiptConfiguration
        : IEntityTypeConfiguration<CustomerReceipt>
    {
        public void Configure(
            EntityTypeBuilder<CustomerReceipt> builder)
        {
            #region Table

            builder.ToTable(
                "CustomerReceipts");

            #endregion


            #region Primary Key

            builder.HasKey(x =>
                x.Id);

            #endregion


            #region Identification

            builder.Property(x =>
                    x.Code)
                .IsRequired()
                .HasMaxLength(
                    50);


            builder.HasIndex(x =>
                    x.Code)
                .IsUnique();


            builder.Property(x =>
                    x.ReceiptDate)
                .IsRequired();

            #endregion


            #region Customer Snapshot

            builder.Property(x =>
                    x.CustomerCode)
                .HasMaxLength(
                    50);


            builder.Property(x =>
                    x.CustomerName)
                .IsRequired()
                .HasMaxLength(
                    200);


            builder.Property(x =>
                    x.CustomerSnapshotJson)
                .HasColumnType(
                    "nvarchar(max)");

            #endregion


            #region Company Snapshot

            builder.Property(x =>
                    x.CompanyName)
                .HasMaxLength(
                    200);


            builder.Property(x =>
                    x.CompanySnapshotJson)
                .HasColumnType(
                    "nvarchar(max)");

            #endregion


            #region Payment Information

            builder.Property(x =>
                    x.PaymentMode)
                .IsRequired();


            builder.Property(x =>
                    x.ReferenceNumber)
                .HasMaxLength(
                    100);


            builder.Property(x =>
                    x.ChequeNumber)
                .HasMaxLength(
                    50);


            builder.Property(x =>
                    x.BankName)
                .HasMaxLength(
                    200);

            #endregion


            #region Amount

            builder.Property(x =>
                    x.TotalReceivedAmount)
                .HasPrecision(
                    18,
                    2);

            #endregion


            #region Remarks

            builder.Property(x =>
                    x.Remarks)
                .HasMaxLength(
                    1000);

            #endregion


            #region Workflow

            builder.Property(x =>
                    x.Status)
                .IsRequired();


            builder.Property(x =>
                    x.FinalizedBy)
                .HasMaxLength(
                    200);

            #endregion


            #region Customer Relationship

            builder.HasOne(x =>
                    x.Customer)
                .WithMany()
                .HasForeignKey(x =>
                    x.CustomerId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            #endregion


            #region Allocations Relationship

            builder.HasMany(x =>
                    x.Allocations)
                .WithOne(x =>
                    x.CustomerReceipt)
                .HasForeignKey(x =>
                    x.CustomerReceiptId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            #region Indexes

            builder.HasIndex(x =>
                x.CustomerId);


            builder.HasIndex(x =>
                x.ReceiptDate);


            builder.HasIndex(x =>
                x.Status);

            #endregion
        }
    }
}