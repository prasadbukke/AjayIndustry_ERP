// =============================================================
// File: SupplierPaymentConfiguration.cs
// Module: Supplier Payment
// Layer: Infrastructure - Entity Framework Configuration
//
// Purpose:
// Configures the SupplierPayment header table and its
// Entity Framework relationships.
//
// Structure:
//
// PurchaseInvoice
//      │
//      │ 1 : 1
//      ▼
// SupplierPayment
//      │
//      │ 1 : Many
//      ▼
// SupplierPaymentTransaction
//
// Important Business Rules:
// - One Purchase Invoice can have only ONE SupplierPayment.
// - One SupplierPayment can contain multiple transactions.
// - Supplier and Company are derived from Purchase Invoice.
// - Payment Date / Amount / Mode / Bank / Reference belong
//   to SupplierPaymentTransaction.
// - Paid Amount and Outstanding Amount are calculated live.
// - Payment completion status is calculated and NOT stored.
// - Supplier Payment is soft deleted in normal ERP workflow.
// =============================================================

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class SupplierPaymentConfiguration
        : IEntityTypeConfiguration<SupplierPayment>
    {
        public void Configure(
            EntityTypeBuilder<SupplierPayment> builder)
        {
            // =================================================
            // TABLE
            // =================================================

            #region Table

            builder.ToTable(
                "SupplierPayments");


            builder.HasKey(x =>
                x.Id);

            #endregion


            // =================================================
            // PAYMENT IDENTIFICATION
            // =================================================

            #region Payment Identification

            builder.Property(x =>
                    x.Code)
                .IsRequired()
                .HasMaxLength(50);


            builder.HasIndex(x =>
                    x.Code)
                .IsUnique();

            #endregion


            // =================================================
            // PURCHASE INVOICE
            // =================================================

            #region Purchase Invoice

            builder.Property(x =>
                    x.PurchaseInvoiceId)
                .IsRequired();


            /*
             * ONE Purchase Invoice
             * can have only ONE Supplier Payment header.
             *
             * Example:
             *
             * PI-001
             *   ↓
             * SPAY-001
             *   ├── Transaction 1
             *   ├── Transaction 2
             *   └── Transaction 3
             *
             * A second SPAY header for PI-001 is not allowed.
             */
            builder.HasOne(x =>
                    x.PurchaseInvoice)
                .WithOne()
                .HasForeignKey<SupplierPayment>(x =>
                    x.PurchaseInvoiceId)
                .OnDelete(
                    DeleteBehavior.Restrict);


            builder.HasIndex(x =>
                    x.PurchaseInvoiceId)
                .IsUnique();

            #endregion


            // =================================================
            // SUPPLIER
            // =================================================

            #region Supplier

            builder.Property(x =>
                    x.SupplierId)
                .IsRequired();


            builder.HasOne(x =>
                    x.Supplier)
                .WithMany()
                .HasForeignKey(x =>
                    x.SupplierId)
                .OnDelete(
                    DeleteBehavior.Restrict);


            builder.HasIndex(x =>
                x.SupplierId);

            #endregion


            // =================================================
            // COMPANY
            // =================================================

            #region Company

            builder.Property(x =>
                    x.CompanyId)
                .IsRequired();


            builder.HasOne(x =>
                    x.Company)
                .WithMany()
                .HasForeignKey(x =>
                    x.CompanyId)
                .OnDelete(
                    DeleteBehavior.Restrict);


            builder.HasIndex(x =>
                x.CompanyId);

            #endregion


            // =================================================
            // PAYMENT TRANSACTIONS
            // =================================================

            #region Payment Transactions

            /*
             * One Supplier Payment header can contain
             * multiple actual payment transactions.
             *
             * Example:
             *
             * SupplierPayment = ₹30,000 Invoice
             *
             * Transaction 1 = ₹10,000
             * Transaction 2 = ₹10,000
             * Transaction 3 = ₹10,000
             */
            builder.HasMany(x =>
                    x.Transactions)
                .WithOne(x =>
                    x.SupplierPayment)
                .HasForeignKey(x =>
                    x.SupplierPaymentId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            // =================================================
            // QUERY / REPORT INDEXES
            // =================================================

            #region Query Indexes

            builder.HasIndex(x => new
            {
                x.SupplierId,
                x.CompanyId
            });

            #endregion
        }
    }
}