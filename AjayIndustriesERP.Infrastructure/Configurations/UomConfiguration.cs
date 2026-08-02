/*
==============================================================

File : UomConfiguration.cs

Purpose :
Represents UOM Entity Configuration.

==============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class UomConfiguration : IEntityTypeConfiguration<Uom>
    {
        public void Configure(EntityTypeBuilder<Uom> builder)
        {
            builder.ToTable("Uoms");

            builder.HasKey(x => x.UomId);

            builder.Property(x => x.UomCode)
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(x => x.UomCode)
                .IsUnique();

            builder.Property(x => x.UomName)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(x => x.UomName)
                .IsUnique();

            builder.Property(x => x.Description)
                .HasMaxLength(250);

            builder.Property(x => x.CreatedBy)
                .HasMaxLength(100);

            builder.Property(x => x.ModifiedBy)
                .HasMaxLength(100);
        }
    }
}   