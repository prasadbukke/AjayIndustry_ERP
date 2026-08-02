using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class ItemCategoryConfiguration : IEntityTypeConfiguration<ItemCategory>
    {
        public void Configure(EntityTypeBuilder<ItemCategory> builder)
        {
            builder.ToTable("ItemCategories");

            builder.HasKey(x => x.ItemCategoryId);

            builder.Property(x => x.CategoryCode)
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(x => x.CategoryCode)
                .IsUnique();

            builder.Property(x => x.CategoryName)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(x => x.CategoryName)
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