// =============================================================
// File: SupplierPaymentTransactionConfiguration.cs
// Module: Supplier Payment
// Layer: Infrastructure - Entity Framework Configuration
//
// Purpose:
// Configures actual Supplier Payment transaction records.
//
// Responsibilities:
// - Configure SupplierPayment foreign key
// - Configure Payment Date
// - Configure Transaction Amount precision
// - Configure Payment Mode
// - Configure Bank Name
// - Configure Reference Number
// - Configure Remarks
// - Configure useful transaction indexes
//
// Important Business Rules:
// - One SupplierPayment can contain multiple transactions.
// - Each transaction represents one actual payment event.
// - Transaction Amount must be positive.
// - Transaction Amount validation against Outstanding
//   is performed by the Application Service.
// - Deleted transactions are ignored while calculating
//   Total Paid and Outstanding.
// - Paid Amount and Outstanding are NOT stored separately.
// =============================================================

using AjayIndustriesERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AjayIndustriesERP.Infrastructure.Configurations
{
    public class SupplierPaymentTransactionConfiguration
        : IEntityTypeConfiguration<SupplierPaymentTransaction>
    {
        public void Configure(
            EntityTypeBuilder<SupplierPaymentTransaction> builder)
        {
            // =================================================
            // TABLE
            // =================================================

            #region Table

            builder.ToTable(
                "SupplierPaymentTransactions");


            builder.HasKey(x =>
                x.Id);

            #endregion


            // =================================================
            // SUPPLIER PAYMENT RELATIONSHIP
            // =================================================

            #region Supplier Payment

            builder.Property(x =>
                    x.SupplierPaymentId)
                .IsRequired();


            builder.HasOne(x =>
                    x.SupplierPayment)
                .WithMany(x =>
                    x.Transactions)
                .HasForeignKey(x =>
                    x.SupplierPaymentId)
                .OnDelete(
                    DeleteBehavior.Cascade);

            #endregion


            // =================================================
            // PAYMENT DATE
            // =================================================

            #region Payment Date

            builder.Property(x =>
                    x.PaymentDate)
                .IsRequired();

            #endregion


            // =================================================
            // TRANSACTION AMOUNT
            // =================================================

            #region Transaction Amount

            builder.Property(x =>
                    x.Amount)
                .IsRequired()
                .HasPrecision(
                    18,
                    2);

            #endregion


            // =================================================
            // PAYMENT MODE
            // =================================================

            #region Payment Mode

            builder.Property(x =>
                    x.PaymentMode)
                .IsRequired()
                .HasMaxLength(
                    50);

            #endregion


            // =================================================
            // BANK INFORMATION
            // =================================================

            #region Bank Information

            builder.Property(x =>
                    x.BankName)
                .HasMaxLength(
                    150);


            builder.Property(x =>
                    x.ReferenceNumber)
                .HasMaxLength(
                    150);

            #endregion


            // =================================================
            // REMARKS
            // =================================================

            #region Remarks

            builder.Property(x =>
                    x.Remarks)
                .HasMaxLength(
                    1000);

            #endregion


            // =================================================
            // INDEXES
            // =================================================

            #region Indexes

            builder.HasIndex(x =>
                x.SupplierPaymentId);


            builder.HasIndex(x =>
                x.PaymentDate);


            builder.HasIndex(x => new
            {
                x.SupplierPaymentId,
                x.PaymentDate
            });


            builder.HasIndex(x =>
                x.ReferenceNumber);

            #endregion
        }
    }
}