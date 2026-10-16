/*
============================================================

File: StockTransactionLineConfiguration.cs

Purpose:
Configures StockTransactionLine for Entity Framework Core.

Responsibilities:
- Map StockTransactionLine to database.
- Configure StockTransaction relationship.
- Configure Item relationship.
- Configure Warehouse relationship.
- Configure stock quantity precision.
- Configure optional Rate and Amount precision.
- Configure Remarks.
- Configure useful query indexes.

Important:
- QuantityIn and QuantityOut are stored separately.
- Current stock is calculated from stock movement lines.
- Item and Warehouse relationships use Restrict delete behavior.
- Parent StockTransaction relationship uses Cascade delete.

============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class StockTransactionLineConfiguration
        : IEntityTypeConfiguration<StockTransactionLine>
    {
        public void Configure(
            EntityTypeBuilder<StockTransactionLine> builder)
        {
            #region Table

            builder.ToTable(
                "StockTransactionLines");

            #endregion


            #region Primary Key

            builder.HasKey(x =>
                x.Id);

            #endregion


            #region Stock Transaction Relationship

            builder.Property(x =>
                    x.StockTransactionId)
                .IsRequired();


            builder.HasOne(x =>
                    x.StockTransaction)
                .WithMany(x =>
                    x.Items)
                .HasForeignKey(x =>
                    x.StockTransactionId)
                .OnDelete(
                    DeleteBehavior.Cascade);

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

            #endregion


            #region Warehouse Relationship

            builder.Property(x =>
                    x.WarehouseId)
                .IsRequired();


            builder.HasOne(x =>
                    x.Warehouse)
                .WithMany()
                .HasForeignKey(x =>
                    x.WarehouseId)
                .OnDelete(
                    DeleteBehavior.Restrict);

            #endregion


            #region Quantities

            builder.Property(x =>
                    x.QuantityIn)
                .HasPrecision(18, 3)
                .IsRequired();


            builder.Property(x =>
                    x.QuantityOut)
                .HasPrecision(18, 3)
                .IsRequired();

            #endregion


            #region Stock Value

            builder.Property(x =>
                    x.Rate)
                .HasPrecision(18, 4);


            builder.Property(x =>
                    x.Amount)
                .HasPrecision(18, 2);

            #endregion


            #region Remarks

            builder.Property(x =>
                    x.Remarks)
                .HasMaxLength(1000);

            #endregion


            #region Query Indexes

            builder.HasIndex(x =>
                x.StockTransactionId);


            builder.HasIndex(x =>
                x.ItemId);


            builder.HasIndex(x =>
                x.WarehouseId);


            builder.HasIndex(x =>
                new
                {
                    x.ItemId,
                    x.WarehouseId
                });

            #endregion
        }
    }
}