/*
==============================================================

File : SupplierConfiguration.cs

Purpose :
Configures Supplier Master database mapping.

Important Rules :
- Supplier Code is permanently unique.
- Deleted Supplier Codes are never reused.
- Active Supplier Name must be unique.
- GSTIN is optional but must be unique when provided.
- PAN is NOT unique because one legal entity may have
  multiple GST registrations / supplier locations.

==============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    /// <summary>
    /// Contains Entity Framework configuration for Supplier.
    /// </summary>
    public class SupplierConfiguration :
        IEntityTypeConfiguration<Supplier>
    {
        public void Configure(
            EntityTypeBuilder<Supplier> builder)
        {
            #region Table and Primary Key

            builder.ToTable("Suppliers");

            builder.HasKey(x => x.SupplierId);

            #endregion

            #region Supplier Code

            builder.Property(x => x.SupplierCode)
                .HasMaxLength(20)
                .IsRequired();

            /*
             * Supplier Code remains unique even after
             * Soft Delete so old codes are never reused.
             */
            builder.HasIndex(x => x.SupplierCode)
                .IsUnique();

            #endregion

            #region Supplier Name

            builder.Property(x => x.SupplierName)
                .HasMaxLength(150)
                .IsRequired();

            /*
             * Exact duplicate active Supplier Names
             * are not allowed.
             */
            builder.HasIndex(x => x.SupplierName)
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");

            #endregion

            #region Contact Person

            builder.Property(x => x.ContactPerson)
                .HasMaxLength(100);

            #endregion

            #region Contact Information

            builder.Property(x => x.MobileNumber)
                .HasMaxLength(20);

            builder.Property(x => x.AlternateMobileNumber)
                .HasMaxLength(20);

            builder.Property(x => x.Email)
                .HasMaxLength(150);

            #endregion

            #region Tax Information

            builder.Property(x => x.Gstin)
                .HasMaxLength(15);

            /*
             * GSTIN is optional.
             * When supplied, an active GSTIN cannot be
             * registered against multiple Supplier records.
             */
            builder.HasIndex(x => x.Gstin)
                .IsUnique()
                .HasFilter(
                     "[Gstin] IS NOT NULL AND [IsDeleted] = 0");

            builder.Property(x => x.Pan)
                .HasMaxLength(10);

            #endregion

            #region Address

            builder.Property(x => x.AddressLine1)
                .HasMaxLength(200);

            builder.Property(x => x.AddressLine2)
                .HasMaxLength(200);

            builder.Property(x => x.City)
                .HasMaxLength(100);

            builder.Property(x => x.State)
                .HasMaxLength(100);

            builder.Property(x => x.Pincode)
                .HasMaxLength(10);

            #endregion

            #region Payment Terms

            builder.Property(x => x.PaymentTermsDays);

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