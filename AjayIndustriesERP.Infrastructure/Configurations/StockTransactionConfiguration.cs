/*
============================================================

File: StockTransactionConfiguration.cs

Purpose:
Configures StockTransaction for Entity Framework Core.

Responsibilities:
- Map StockTransaction to database.
- Configure primary key.
- Configure unique transaction Code.
- Configure Transaction Date.
- Configure Transaction Type.
- Configure Reference Type.
- Configure optional source Reference Id and Number.
- Configure Remarks.
- Configure Stock Transaction Lines relationship.
- Configure useful query indexes.

Important:
- Actual item-wise stock IN / OUT is stored in
  StockTransactionLine.
- ReferenceId is intentionally generic because the source
  can be GRN, Production Job, Delivery Challan, Invoice,
  Manual Adjustment, etc.
- Source document relationships are not configured as
  direct foreign keys here.

============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class StockTransactionConfiguration
        : IEntityTypeConfiguration<StockTransaction>
    {
        public void Configure(
            EntityTypeBuilder<StockTransaction> builder)
        {
            #region Table

            builder.ToTable(
                "StockTransactions");

            #endregion


            #region Primary Key

            builder.HasKey(x =>
                x.Id);

            #endregion


            #region Transaction Code

            builder.Property(x =>
                    x.Code)
                .IsRequired()
                .HasMaxLength(50);


            builder.HasIndex(x =>
                    x.Code)
                .IsUnique();

            #endregion


            #region Transaction Information

            builder.Property(x =>
                    x.TransactionDate)
                .IsRequired();


            builder.Property(x =>
                    x.TransactionType)
                .IsRequired();

            #endregion


            #region Source Reference

            builder.Property(x =>
                    x.ReferenceType)
                .IsRequired();


            builder.Property(x =>
                    x.ReferenceId);


            builder.Property(x =>
                    x.ReferenceNumber)
                .HasMaxLength(100);

            #endregion


            #region Remarks

            builder.Property(x =>
                    x.Remarks)
                .HasMaxLength(1000);

            #endregion


            #region Transaction Lines

            builder.HasMany(x =>
                    x.Items)
                .WithOne(x =>
                    x.StockTransaction)
                .HasForeignKey(x =>
                    x.StockTransactionId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            #region Query Indexes

            builder.HasIndex(x =>
                x.TransactionDate);


            builder.HasIndex(x =>
                x.TransactionType);


            builder.HasIndex(x =>
                x.ReferenceType);


            builder.HasIndex(x =>
                x.ReferenceId);


            builder.HasIndex(x =>
                new
                {
                    x.ReferenceType,
                    x.ReferenceId
                });

            #endregion
        }
    }
}