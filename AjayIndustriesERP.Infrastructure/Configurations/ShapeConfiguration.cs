/*
==============================================================

File : ShapeConfiguration.cs

Purpose :
Configures Shape Master database mapping.

==============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    /// <summary>
    /// Contains Entity Framework configuration for Shape.
    /// </summary>
    public class ShapeConfiguration :
        IEntityTypeConfiguration<Shape>
    {
        public void Configure(
            EntityTypeBuilder<Shape> builder)
        {
            #region Table and Primary Key

            builder.ToTable("Shapes");

            builder.HasKey(x => x.ShapeId);

            #endregion

            #region Shape Code

            builder.Property(x => x.ShapeCode)
                .HasMaxLength(20)
                .IsRequired();

            /*
             * Shape Code must remain unique even after
             * soft deletion, because codes are never reused.
             */
            builder.HasIndex(x => x.ShapeCode)
                .IsUnique();

            #endregion

            #region Shape Name

            builder.Property(x => x.ShapeName)
                .HasMaxLength(100)
                .IsRequired();

            /*
             * Active Shape Names must be unique.
             * A deleted Shape Name may be created again.
             */
            builder.HasIndex(x => x.ShapeName)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            #endregion

            #region Description

            builder.Property(x => x.Description)
                .HasMaxLength(500);

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