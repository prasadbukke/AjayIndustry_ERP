/*
==============================================================

File : WarehouseConfiguration.cs

Purpose :
Warehouse Entity Configuration.

==============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
    {
        public void Configure(EntityTypeBuilder<Warehouse> builder)
        {
            builder.ToTable("Warehouses");

            builder.HasKey(x => x.WarehouseId);

            builder.Property(x => x.WarehouseCode)
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(x => x.WarehouseCode)
                .IsUnique();

            builder.Property(x => x.WarehouseName)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(x => x.WarehouseName)
                .IsUnique();

            builder.Property(x => x.Description)
                .HasMaxLength(250);

            builder.Property(x => x.WarehouseType)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.CreatedBy)
                .HasMaxLength(100);

            builder.Property(x => x.ModifiedBy)
                .HasMaxLength(100);
        }
    }
}