/*
============================================================
File: InvoiceConfiguration.cs

Module:
Invoice

Purpose:
Configures Invoice entity for Entity Framework Core.

Responsibilities:
- Configure Invoice table and primary key.
- Configure required fields and maximum lengths.
- Configure Customer / Company snapshot fields.
- Configure Billing Address fields.
- Configure financial decimal precision.
- Configure Invoice workflow indexes.
- Configure Invoice → InvoiceItem relationship.

Important:
- CustomerId / CompanyId are snapshot references.
- CustomerSnapshotJson and CompanySnapshotJson use
  nvarchar(max) for future scalar Master fields.
- InvoiceTermsAndConditions is stored on Invoice so Draft
  Invoice terms can differ from Company Master defaults.
- Financial values are persisted as calculated snapshots.
============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class InvoiceConfiguration
        : IEntityTypeConfiguration<Invoice>
    {
        #region Configure

        public void Configure(
            EntityTypeBuilder<Invoice> builder)
        {
            #region Table And Primary Key

            builder.ToTable(
                "Invoices");

            builder.HasKey(
                x => x.Id);

            #endregion


            #region Identification

            builder.Property(
                    x => x.Code)
                .IsRequired()
                .HasMaxLength(
                    50);


            builder.HasIndex(
                    x => x.Code)
                .IsUnique();


            builder.Property(
                    x => x.InvoiceDate)
                .IsRequired();


            builder.Property(
                    x => x.Status)
                .IsRequired();

            #endregion


            #region Customer Reference

            builder.Property(
                    x => x.CustomerName)
                .IsRequired()
                .HasMaxLength(
                    250);


            builder.Property(
                    x => x.CustomerSnapshotJson)
                .HasColumnType(
                    "nvarchar(max)");


            builder.HasIndex(
                x => x.CustomerId);

            #endregion


            #region Billing Address Snapshot

            builder.Property(
                    x => x.BillingAddressLine1)
                .HasMaxLength(
                    500);


            builder.Property(
                    x => x.BillingAddressLine2)
                .HasMaxLength(
                    500);


            builder.Property(
                    x => x.BillingCity)
                .HasMaxLength(
                    150);


            builder.Property(
                    x => x.BillingDistrict)
                .HasMaxLength(
                    150);


            builder.Property(
                    x => x.BillingState)
                .HasMaxLength(
                    150);


            builder.Property(
                    x => x.BillingPincode)
                .HasMaxLength(
                    20);


            builder.Property(
                    x => x.BillingCountry)
                .HasMaxLength(
                    100);

            #endregion


            #region Company / Workshop Snapshot

            builder.Property(
                    x => x.CompanyName)
                .HasMaxLength(
                    250);


            builder.Property(
                    x => x.CompanySnapshotJson)
                .HasColumnType(
                    "nvarchar(max)");


            builder.HasIndex(
                x => x.CompanyId);

            #endregion


            #region Payment Information

            builder.Property(
                    x => x.PaymentTerms)
                .HasMaxLength(
                    500);

            #endregion


            #region GST Information

            builder.Property(
                    x => x.PlaceOfSupply)
                .HasMaxLength(
                    150);


            builder.Property(
                    x => x.IsInterState)
                .IsRequired();

            #endregion


            #region Financial Totals

            builder.Property(
                    x => x.GrossAmount)
                .HasPrecision(
                    18,
                    2);


            builder.Property(
                    x => x.DiscountAmount)
                .HasPrecision(
                    18,
                    2);


            builder.Property(
                    x => x.TaxableAmount)
                .HasPrecision(
                    18,
                    2);


            builder.Property(
                    x => x.CgstAmount)
                .HasPrecision(
                    18,
                    2);


            builder.Property(
                    x => x.SgstAmount)
                .HasPrecision(
                    18,
                    2);


            builder.Property(
                    x => x.IgstAmount)
                .HasPrecision(
                    18,
                    2);


            builder.Property(
                    x => x.OtherCharges)
                .HasPrecision(
                    18,
                    2);


            builder.Property(
                    x => x.RoundOffAmount)
                .HasPrecision(
                    18,
                    2);


            builder.Property(
                    x => x.GrandTotal)
                .HasPrecision(
                    18,
                    2);

            #endregion


            #region Terms And Conditions

            builder.Property(
                    x => x.InvoiceTermsAndConditions)
                .HasMaxLength(
                    4000);

            #endregion


            #region Remarks

            builder.Property(
                    x => x.Remarks)
                .HasMaxLength(
                    2000);

            #endregion


            #region Finalization

            builder.Property(
                    x => x.FinalizedBy)
                .HasMaxLength(
                    150);

            #endregion


            #region Relationships

            builder.HasMany(
                    x => x.Items)
                .WithOne(
                    x => x.Invoice)
                .HasForeignKey(
                    x => x.InvoiceId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            #region Indexes

            builder.HasIndex(
                x => x.InvoiceDate);


            builder.HasIndex(
                x => x.DueDate);


            builder.HasIndex(
                x => x.Status);


            builder.HasIndex(
                x => new
                {
                    x.CustomerId,
                    x.InvoiceDate
                });

            #endregion
        }

        #endregion
    }
}