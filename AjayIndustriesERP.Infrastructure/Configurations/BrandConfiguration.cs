using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("Brands");

            builder.HasKey(x => x.BrandId);

            builder.Property(x => x.BrandCode)
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(x => x.BrandCode)
                .IsUnique();

            builder.Property(x => x.BrandName)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(x => x.BrandName)
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