using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable("Companies");

            builder.HasKey(x => x.CompanyId);

            builder.Property(x => x.CompanyCode)
                .HasMaxLength(20)
                .IsRequired();

            builder.HasIndex(x => x.CompanyCode)
                .IsUnique();

            builder.Property(x => x.CompanyName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(x => x.GstNumber)
                .HasMaxLength(15)
                .IsRequired();

            builder.HasIndex(x => x.GstNumber)
                .IsUnique();

            builder.Property(x => x.PanNumber)
                .HasMaxLength(10);

            builder.Property(x => x.PhoneNumber)
                .HasMaxLength(20);

            builder.Property(x => x.Email)
                .HasMaxLength(100);

            builder.Property(x => x.Website)
                .HasMaxLength(200);

            builder.Property(x => x.ContactPerson)
                .HasMaxLength(100);

            builder.Property(x => x.Address)
                .HasMaxLength(500);

            builder.Property(x => x.City)
                .HasMaxLength(100);

            builder.Property(x => x.State)
                .HasMaxLength(100);

            builder.Property(x => x.Country)
                .HasMaxLength(100)
                .HasDefaultValue("India");

            builder.Property(x => x.PostalCode)
                .HasMaxLength(10);

            #region ISO Certification

            builder.Property(x => x.IsoCertificationNumber)
                .HasMaxLength(100);

            #endregion


            #region Bank Details

            builder.Property(x => x.BankName)
                .HasMaxLength(200);

            builder.Property(x => x.BankAccountHolderName)
                .HasMaxLength(200);

            builder.Property(x => x.BankAccountNumber)
                .HasMaxLength(100);

            builder.Property(x => x.BankIfscCode)
                .HasMaxLength(20);

            builder.Property(x => x.BankBranchName)
                .HasMaxLength(200);

            builder.Property(x => x.BankAccountType)
                .HasMaxLength(50);

            #endregion


            #region Terms And Conditions

            builder.Property(x => x.PurchaseOrderTermsAndConditions)
                .HasMaxLength(4000);

            builder.Property(x => x.InvoiceTermsAndConditions)
                .HasMaxLength(4000);

            #endregion

            builder.Property(x => x.CreatedBy)
                .HasMaxLength(100);

            builder.Property(x => x.ModifiedBy)
                .HasMaxLength(100);
        }
    }
}