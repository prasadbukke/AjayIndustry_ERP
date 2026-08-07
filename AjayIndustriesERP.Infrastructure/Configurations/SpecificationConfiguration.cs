/*
==============================================================

File : SpecificationConfiguration.cs

Purpose :
Configures Specification Master database mapping.

==============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class SpecificationConfiguration :
        IEntityTypeConfiguration<Specification>
    {
        public void Configure(
            EntityTypeBuilder<Specification> builder)
        {
            #region Table and Primary Key

            builder.ToTable("Specifications");

            builder.HasKey(x => x.SpecificationId);

            #endregion

            #region Code

            builder.Property(x => x.SpecificationCode)
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(x => x.SpecificationCode)
                .IsUnique();

            #endregion

            #region Name

            builder.Property(x => x.SpecificationName)
                .HasMaxLength(100)
                .IsRequired();

            builder.HasIndex(x => x.SpecificationName)
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