/*
==============================================================

File : ItemSpecificationConfiguration.cs

Purpose :
Configures Item Specification database mapping and
relationships.

==============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    /// <summary>
    /// Contains Entity Framework configuration for
    /// ItemSpecification.
    /// </summary>
    public class ItemSpecificationConfiguration :
        IEntityTypeConfiguration<ItemSpecification>
    {
        public void Configure(
            EntityTypeBuilder<ItemSpecification> builder)
        {
            #region Table and Primary Key

            builder.ToTable("ItemSpecifications");

            builder.HasKey(x =>
                x.ItemSpecificationId);

            #endregion

            #region Specification Value

            builder.Property(x =>
                    x.SpecificationValue)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.SortOrder)
                .IsRequired();

            #endregion

            #region Item Relationship

            builder.HasOne(x => x.Item)
                .WithMany(x => x.ItemSpecifications)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region Specification Relationship

            builder.HasOne(x => x.Specification)
                .WithMany()
                .HasForeignKey(x => x.SpecificationId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region UOM Relationship

            /*
             * UOM is optional.
             *
             * Example:
             * Diameter = 25 MM
             * Grade    = EN8
             *
             * Grade does not require a UOM.
             */
            builder.HasOne(x => x.Uom)
                .WithMany()
                .HasForeignKey(x => x.UomId)
                .OnDelete(DeleteBehavior.Restrict);

            #endregion

            #region Duplicate Protection

            /*
             * The same active Specification cannot be assigned
             * twice to the same Item.
             *
             * Example:
             * Diameter should not appear twice for one Item.
             */
            builder.HasIndex(x => new
            {
                x.ItemId,
                x.SpecificationId
            })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

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