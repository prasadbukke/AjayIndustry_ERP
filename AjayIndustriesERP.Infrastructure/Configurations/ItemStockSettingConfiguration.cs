/*
============================================================

File: ItemStockSettingConfiguration.cs

Purpose:
Configures ItemStockSetting for Entity Framework Core.

Responsibilities:
- Map ItemStockSetting to database.
- Configure Item relationship.
- Configure Warehouse relationship.
- Configure stock quantity precision.
- Prevent duplicate settings for the same
  Item + Warehouse combination.
- Configure useful query indexes.

Important:
- Actual stock quantity is not stored in this table.
- One Item can have one Stock Setting per Warehouse.
- Item and Warehouse records cannot be deleted through
  ItemStockSetting relationships.

============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class ItemStockSettingConfiguration
        : IEntityTypeConfiguration<ItemStockSetting>
    {
        public void Configure(
            EntityTypeBuilder<ItemStockSetting> builder)
        {
            #region Table

            builder.ToTable(
                "ItemStockSettings");

            #endregion


            #region Primary Key

            builder.HasKey(x =>
                x.Id);

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


            #region Stock Levels

            builder.Property(x =>
                    x.MinimumStock)
                .HasPrecision(18, 3)
                .IsRequired();


            builder.Property(x =>
                    x.MaximumStock)
                .HasPrecision(18, 3)
                .IsRequired();


            builder.Property(x =>
                    x.ReorderLevel)
                .HasPrecision(18, 3)
                .IsRequired();


            builder.Property(x =>
                    x.ReorderQuantity)
                .HasPrecision(18, 3)
                .IsRequired();

            #endregion


            #region Unique Item Warehouse Setting

            /*
             * Only one Stock Setting is allowed
             * for the same Item + Warehouse.
             */

            builder.HasIndex(x =>
                    new
                    {
                        x.ItemId,
                        x.WarehouseId
                    })
                .IsUnique();

            #endregion


            #region Query Indexes

            builder.HasIndex(x =>
                x.ItemId);


            builder.HasIndex(x =>
                x.WarehouseId);

            #endregion
        }
    }
}