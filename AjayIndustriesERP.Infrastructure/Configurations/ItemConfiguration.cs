/*
==============================================================

File : ItemConfiguration.cs

Purpose :
Configures Item entity database mapping and relationships.

==============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    /// <summary>
    /// Contains Entity Framework configuration for Item.
    /// </summary>
    public class ItemConfiguration :
        IEntityTypeConfiguration<Item>
    {
        public void Configure(
            EntityTypeBuilder<Item> builder)
        {
            #region Table and Primary Key

            builder.ToTable("Items");

            builder.HasKey(x => x.ItemId);

            #endregion

            #region Item Code

            builder.Property(x => x.ItemCode)
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(x => x.ItemCode)
                .IsUnique();

            #endregion

            #region Item Name

            builder.Property(x => x.ItemName)
                .HasMaxLength(150)
                .IsRequired();

            /*
 * Item Name alone is NOT unique.
 *
 * Example:
 *
 * MS Round Bar + Round + Diameter 25 MM
 * MS Round Bar + Round + Diameter 30 MM
 *
 * These are valid separate Items.
 *
 * Final duplicate validation is handled using:
 * ItemName + Shape + Specifications.
 */
            builder.HasIndex(x => x.ItemName)
                .HasFilter("[IsDeleted] = 0");

            #endregion

            #region Description

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            #endregion

            #region Relationships

            builder.HasOne(x => x.ItemCategory)
                .WithMany()
                .HasForeignKey(x => x.ItemCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Brand)
                .WithMany()
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Uom)
                .WithMany()
                .HasForeignKey(x => x.UomId)
                .OnDelete(DeleteBehavior.Restrict);

            /*
             * Shape is optional because every Item does not
             * necessarily require a physical material shape.
             */
            builder.HasOne(x => x.Shape)
                .WithMany()
                .HasForeignKey(x => x.ShapeId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region Audit Fields

            builder.Property(x => x.CreatedBy)
                .HasMaxLength(100);

            builder.Property(x => x.ModifiedBy)
                .HasMaxLength(100);

            #endregion
        }
    }
}