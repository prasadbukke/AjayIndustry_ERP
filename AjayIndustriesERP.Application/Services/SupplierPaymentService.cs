// =============================================================
// File: SupplierPaymentService.cs
// Module: Supplier Payment
// Layer: Application - Service
//
// Purpose:
// Implements complete Supplier Payment business workflow.
//
// Final Structure:
//
// Finalized Purchase Invoice
//          ↓
// SupplierPayment Header
//          ↓
// SupplierPaymentTransaction (1 : Many)
//
// Supported Operations:
// - Create Payment + First Transaction
// - Add another Transaction under same Payment No.
// - Edit existing Transaction
// - Calculate Paid Amount
// - Calculate Outstanding
// - Calculate Payment Status
// - Soft Delete Payment
// - Restore Payment
//
// Important Business Rules:
// - One Purchase Invoice = One Supplier Payment No.
// - Multiple part payments use SAME Payment No.
// - Supplier and Company derive from Purchase Invoice.
// - Payment No. / Invoice / Supplier / Company cannot be
//   changed while editing a transaction.
// - Transaction amount cannot cause overpayment.
// - Completed invoice cannot accept another transaction.
// - Paid / Outstanding / Status are calculated live.
// =============================================================

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Application.Services
{
    public class SupplierPaymentService
        : ISupplierPaymentService
    {
        private readonly ISupplierPaymentRepository
            _repository;


        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        #region Constructor

        public SupplierPaymentService(
            ISupplierPaymentRepository repository)
        {
            _repository = repository;
        }

        #endregion


        // =====================================================
        // BASIC READ
        // =====================================================

        #region Basic Read

        public async Task<SupplierPayment?>
            GetByIdAsync(
                int id)
        {
            return await _repository
                .GetByIdAsync(id);
        }


        public async Task<SupplierPayment?>
            GetByPurchaseInvoiceIdAsync(
                int purchaseInvoiceId)
        {
            return await _repository
                .GetByPurchaseInvoiceIdAsync(
                    purchaseInvoiceId);
        }


        public async Task<List<SupplierPayment>>
            GetDeletedAsync()
        {
            return await _repository
                .GetDeletedAsync();
        }

        #endregion


        // =====================================================
        // INDEX / SEARCH
        // =====================================================

        #region Index / Search

        public async Task<PagedResult<SupplierPayment>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            NormalizePaging(
                ref pageNumber,
                ref pageSize);


            return await _repository
                .GetPagedAsync(
                    pageNumber,
                    pageSize);
        }


        public async Task<PagedResult<SupplierPayment>>
            SearchPagedAsync(
                string? searchText,
                int pageNumber,
                int pageSize)
        {
            NormalizePaging(
                ref pageNumber,
                ref pageSize);


            return await _repository
                .SearchPagedAsync(
                    searchText,
                    pageNumber,
                    pageSize);
        }

        #endregion


        // =====================================================
        // PURCHASE INVOICE SOURCE
        // =====================================================

        #region Purchase Invoice Source

        public async Task<List<PurchaseInvoice>>
            GetAvailablePurchaseInvoicesAsync()
        {
            return await _repository
                .GetAvailablePurchaseInvoicesAsync();
        }

        #endregion


        // =====================================================
        // PAYMENT CALCULATIONS
        // =====================================================

        #region Payment Calculations

        public async Task<decimal>
            GetPaidAmountAsync(
                int supplierPaymentId)
        {
            var payment =
                await _repository
                    .GetByIdAsync(
                        supplierPaymentId);


            if (payment == null)
            {
                throw new InvalidOperationException(
                    "Supplier Payment not found.");
            }


            var paidAmount =
                await _repository
                    .GetPaidAmountAsync(
                        supplierPaymentId);


            return RoundMoney(
                paidAmount);
        }


        public async Task<decimal>
            GetOutstandingAmountAsync(
                int supplierPaymentId)
        {
            var payment =
                await _repository
                    .GetByIdAsync(
                        supplierPaymentId);


            if (payment == null)
            {
                throw new InvalidOperationException(
                    "Supplier Payment not found.");
            }


            if (payment.PurchaseInvoice == null)
            {
                throw new InvalidOperationException(
                    "Purchase Invoice information is not available.");
            }


            var invoiceTotal =
                RoundMoney(
                    payment.PurchaseInvoice
                        .GrandTotal);


            var paidAmount =
                RoundMoney(
                    await _repository
                        .GetPaidAmountAsync(
                            supplierPaymentId));


            var outstanding =
                RoundMoney(
                    invoiceTotal -
                    paidAmount);


            return outstanding < 0m
                ? 0m
                : outstanding;
        }


        public async Task<string>
            GetPaymentStatusAsync(
                int supplierPaymentId)
        {
            var payment =
                await _repository
                    .GetByIdAsync(
                        supplierPaymentId);


            if (payment == null)
            {
                throw new InvalidOperationException(
                    "Supplier Payment not found.");
            }


            if (payment.PurchaseInvoice == null)
            {
                throw new InvalidOperationException(
                    "Purchase Invoice information is not available.");
            }


            var invoiceTotal =
                RoundMoney(
                    payment.PurchaseInvoice
                        .GrandTotal);


            var paidAmount =
                RoundMoney(
                    await _repository
                        .GetPaidAmountAsync(
                            supplierPaymentId));


            return CalculatePaymentStatus(
                invoiceTotal,
                paidAmount);
        }

        #endregion


        // =====================================================
        // CREATE PAYMENT + FIRST TRANSACTION
        // =====================================================

        #region Create Payment

        public async Task<SupplierPayment>
            CreateAsync(
                int purchaseInvoiceId,
                SupplierPaymentTransaction firstTransaction)
        {
            if (purchaseInvoiceId <= 0)
            {
                throw new InvalidOperationException(
                    "Please select a valid Purchase Invoice.");
            }


            if (firstTransaction == null)
            {
                throw new InvalidOperationException(
                    "Payment transaction information is required.");
            }


            // =================================================
            // LOAD PURCHASE INVOICE
            // =================================================

            var purchaseInvoice =
                await _repository
                    .GetPurchaseInvoiceForPaymentAsync(
                        purchaseInvoiceId);


            if (purchaseInvoice == null)
            {
                throw new InvalidOperationException(
                    "Selected Purchase Invoice was not found.");
            }


            ValidatePurchaseInvoice(
                purchaseInvoice);


            // =================================================
            // ONE PURCHASE INVOICE = ONE PAYMENT HEADER
            // =================================================

            var alreadyExists =
                await _repository
                    .ExistsForPurchaseInvoiceAsync(
                        purchaseInvoiceId);


            if (alreadyExists)
            {
                throw new InvalidOperationException(
                    "A Supplier Payment already exists for this Purchase Invoice. " +
                    "Open the existing Payment No. to add another payment. " +
                    "If it was deleted, restore it instead.");
            }


            // =================================================
            // FIRST TRANSACTION VALIDATION
            // =================================================

            NormalizeTransaction(
                firstTransaction);


            ValidateTransactionFields(
                firstTransaction);


            var invoiceTotal =
                RoundMoney(
                    purchaseInvoice
                        .GrandTotal);


            if (invoiceTotal <= 0m)
            {
                throw new InvalidOperationException(
                    "Purchase Invoice total must be greater than zero.");
            }


            if (firstTransaction.Amount >
                invoiceTotal)
            {
                throw new InvalidOperationException(
                    $"Payment Amount cannot exceed Invoice Total of ₹{invoiceTotal:N2}.");
            }


            // =================================================
            // PAYMENT NUMBER
            // =================================================

            var paymentCode =
                await GeneratePaymentCodeAsync(
                    firstTransaction.PaymentDate);


            var now =
                DateTime.Now;


            // =================================================
            // HEADER
            //
            // Supplier + Company derive from Purchase Invoice.
            // =================================================

            var supplierPayment =
                new SupplierPayment
                {
                    Code =
                        paymentCode,

                    PurchaseInvoiceId =
                        purchaseInvoice.Id,

                    SupplierId =
                        purchaseInvoice.SupplierId,

                    CompanyId =
                        purchaseInvoice.CompanyId,

                    IsActive =
                        true,

                    IsDeleted =
                        false,

                    CreatedOn =
                        now
                };


            // =================================================
            // FIRST TRANSACTION
            // =================================================

            PrepareNewTransaction(
                firstTransaction,
                now);


            supplierPayment.Transactions
                .Add(
                    firstTransaction);


            // =================================================
            // SAVE HEADER + TRANSACTION
            // =================================================

            await _repository
                .AddAsync(
                    supplierPayment);


            return supplierPayment;
        }

        #endregion


        // =====================================================
        // ADD PAYMENT TRANSACTION
        // =====================================================

        #region Add Payment Transaction

        public async Task<SupplierPaymentTransaction>
            AddTransactionAsync(
                int supplierPaymentId,
                SupplierPaymentTransaction transaction)
        {
            if (supplierPaymentId <= 0)
            {
                throw new InvalidOperationException(
                    "Invalid Supplier Payment.");
            }


            if (transaction == null)
            {
                throw new InvalidOperationException(
                    "Payment transaction information is required.");
            }


            // =================================================
            // LOAD TRACKED PAYMENT
            // =================================================

            var payment =
                await _repository
                    .GetForUpdateAsync(
                        supplierPaymentId);


            if (payment == null)
            {
                throw new InvalidOperationException(
                    "Supplier Payment not found.");
            }


            if (payment.PurchaseInvoice == null)
            {
                throw new InvalidOperationException(
                    "Purchase Invoice information is not available.");
            }


            ValidatePurchaseInvoice(
                payment.PurchaseInvoice);


            NormalizeTransaction(
                transaction);


            ValidateTransactionFields(
                transaction);


            // =================================================
            // CURRENT POSITION
            // =================================================

            var invoiceTotal =
                RoundMoney(
                    payment.PurchaseInvoice
                        .GrandTotal);


            var currentPaid =
                RoundMoney(
                    payment.Transactions

                        .Where(x =>
                            x.IsActive &&
                            !x.IsDeleted)

                        .Sum(x =>
                            x.Amount));


            var outstanding =
                RoundMoney(
                    invoiceTotal -
                    currentPaid);


            // =================================================
            // COMPLETED PROTECTION
            // =================================================

            if (outstanding <= 0m)
            {
                throw new InvalidOperationException(
                    "This Purchase Invoice is already fully paid. " +
                    "No additional payment can be added.");
            }


            // =================================================
            // OVERPAYMENT PROTECTION
            // =================================================

            if (transaction.Amount >
                outstanding)
            {
                throw new InvalidOperationException(
                    $"Payment Amount cannot exceed current Outstanding of ₹{outstanding:N2}.");
            }


            // =================================================
            // NEW TRANSACTION UNDER SAME PAYMENT NO.
            // =================================================

            var now =
                DateTime.Now;


            transaction.SupplierPaymentId =
                payment.Id;


            PrepareNewTransaction(
                transaction,
                now);


            await _repository
                .AddTransactionAsync(
                    transaction);


            return transaction;
        }

        #endregion


        // =====================================================
        // EDIT PAYMENT TRANSACTION
        // =====================================================

        #region Edit Payment Transaction

        public async Task<SupplierPaymentTransaction>
            UpdateTransactionAsync(
                int supplierPaymentId,
                int transactionId,
                SupplierPaymentTransaction transaction)
        {
            if (supplierPaymentId <= 0)
            {
                throw new InvalidOperationException(
                    "Invalid Supplier Payment.");
            }


            if (transactionId <= 0)
            {
                throw new InvalidOperationException(
                    "Invalid payment transaction.");
            }


            if (transaction == null)
            {
                throw new InvalidOperationException(
                    "Payment transaction information is required.");
            }


            // =================================================
            // LOAD TRACKED PAYMENT + ALL TRANSACTIONS
            // =================================================

            var payment =
                await _repository
                    .GetForUpdateAsync(
                        supplierPaymentId);


            if (payment == null)
            {
                throw new InvalidOperationException(
                    "Supplier Payment not found.");
            }


            if (payment.PurchaseInvoice == null)
            {
                throw new InvalidOperationException(
                    "Purchase Invoice information is not available.");
            }


            ValidatePurchaseInvoice(
                payment.PurchaseInvoice);


            // =================================================
            // FIND EXISTING TRANSACTION
            // =================================================

            var existingTransaction =
                payment.Transactions
                    .FirstOrDefault(x =>
                        x.Id == transactionId &&
                        x.SupplierPaymentId ==
                            supplierPaymentId &&
                        x.IsActive &&
                        !x.IsDeleted);


            if (existingTransaction == null)
            {
                throw new InvalidOperationException(
                    "Payment transaction not found.");
            }


            // =================================================
            // NORMALIZE POSTED VALUES
            // =================================================

            NormalizeTransaction(
                transaction);


            ValidateTransactionFields(
                transaction);


            // =================================================
            // EDIT AMOUNT VALIDATION
            //
            // Invoice Total = ₹30,000
            //
            // T1 = ₹10,000
            // T2 = ₹10,000 <- editing
            // T3 = ₹5,000
            //
            // Other Transactions = ₹15,000
            //
            // Maximum T2:
            // ₹30,000 - ₹15,000
            // = ₹15,000
            // =================================================

            var invoiceTotal =
                RoundMoney(
                    payment.PurchaseInvoice
                        .GrandTotal);


            var otherTransactionsTotal =
                RoundMoney(
                    payment.Transactions

                        .Where(x =>
                            x.Id !=
                                existingTransaction.Id &&
                            x.IsActive &&
                            !x.IsDeleted)

                        .Sum(x =>
                            x.Amount));


            var maximumEditableAmount =
                RoundMoney(
                    invoiceTotal -
                    otherTransactionsTotal);


            if (maximumEditableAmount <= 0m)
            {
                throw new InvalidOperationException(
                    "This transaction cannot be increased because the Purchase Invoice is already fully allocated by other payments.");
            }


            if (transaction.Amount >
                maximumEditableAmount)
            {
                throw new InvalidOperationException(
                    $"Payment Amount cannot exceed maximum allowed amount of ₹{maximumEditableAmount:N2}.");
            }


            // =================================================
            // UPDATE ONLY TRANSACTION FIELDS
            //
            // DO NOT CHANGE:
            // - SupplierPaymentId
            // - Payment No.
            // - PurchaseInvoiceId
            // - SupplierId
            // - CompanyId
            // =================================================

            existingTransaction.PaymentDate =
                transaction.PaymentDate;


            existingTransaction.Amount =
                transaction.Amount;


            existingTransaction.PaymentMode =
                transaction.PaymentMode;


            existingTransaction.BankName =
                transaction.BankName;


            existingTransaction.ReferenceNumber =
                transaction.ReferenceNumber;


            existingTransaction.Remarks =
                transaction.Remarks;


            existingTransaction.ModifiedOn =
                DateTime.Now;


            // =================================================
            // SAVE TRACKED GRAPH
            // =================================================

            await _repository
                .UpdateAsync(
                    payment);


            return existingTransaction;
        }

        #endregion


        // =====================================================
        // DELETE PAYMENT
        // =====================================================

        #region Delete

        public async Task DeleteAsync(
            int id)
        {
            var payment =
                await _repository
                    .GetForUpdateAsync(
                        id);


            if (payment == null)
            {
                throw new InvalidOperationException(
                    "Supplier Payment not found.");
            }


            /*
             * Soft-delete parent only.
             *
             * Transaction records remain preserved.
             */

            payment.IsDeleted =
                true;


            payment.IsActive =
                false;


            payment.ModifiedOn =
                DateTime.Now;


            await _repository
                .UpdateAsync(
                    payment);
        }

        #endregion


        // =====================================================
        // RESTORE PAYMENT
        // =====================================================

        #region Restore

        public async Task RestoreAsync(
            int id)
        {
            var payment =
                await _repository
                    .GetDeletedForUpdateAsync(
                        id);


            if (payment == null)
            {
                throw new InvalidOperationException(
                    "Deleted Supplier Payment not found.");
            }


            if (payment.PurchaseInvoice == null)
            {
                throw new InvalidOperationException(
                    "Purchase Invoice information is not available.");
            }


            // =================================================
            // PURCHASE INVOICE MUST STILL BE VALID
            // =================================================

            ValidatePurchaseInvoice(
                payment.PurchaseInvoice);


            // =================================================
            // TRANSACTION TOTAL REVALIDATION
            // =================================================

            var transactionTotal =
                RoundMoney(
                    payment.Transactions

                        .Where(x =>
                            x.IsActive &&
                            !x.IsDeleted)

                        .Sum(x =>
                            x.Amount));


            var invoiceTotal =
                RoundMoney(
                    payment.PurchaseInvoice
                        .GrandTotal);


            if (transactionTotal >
                invoiceTotal)
            {
                throw new InvalidOperationException(
                    $"Supplier Payment cannot be restored because transaction total ₹{transactionTotal:N2} exceeds Purchase Invoice total ₹{invoiceTotal:N2}.");
            }


            // =================================================
            // RESTORE SAME PAYMENT HEADER
            // =================================================

            payment.IsDeleted =
                false;


            payment.IsActive =
                true;


            payment.ModifiedOn =
                DateTime.Now;


            await _repository
                .UpdateAsync(
                    payment);
        }

        #endregion


        // =====================================================
        // PURCHASE INVOICE VALIDATION
        // =====================================================

        #region Purchase Invoice Validation

        private static void ValidatePurchaseInvoice(
            PurchaseInvoice purchaseInvoice)
        {
            if (purchaseInvoice.IsDeleted ||
                !purchaseInvoice.IsActive)
            {
                throw new InvalidOperationException(
                    "Purchase Invoice is not active.");
            }


            if (purchaseInvoice.Status !=
                PurchaseInvoiceStatus.Finalized)
            {
                throw new InvalidOperationException(
                    "Supplier Payment can be made only against a Finalized Purchase Invoice.");
            }


            if (purchaseInvoice.SupplierId <= 0)
            {
                throw new InvalidOperationException(
                    "Purchase Invoice does not have a valid Supplier.");
            }


            if (purchaseInvoice.CompanyId <= 0)
            {
                throw new InvalidOperationException(
                    "Purchase Invoice does not have a valid Company.");
            }


            if (purchaseInvoice.GrandTotal <= 0m)
            {
                throw new InvalidOperationException(
                    "Purchase Invoice total must be greater than zero.");
            }
        }

        #endregion


        // =====================================================
        // TRANSACTION VALIDATION
        // =====================================================

        #region Transaction Validation

        private static void ValidateTransactionFields(
            SupplierPaymentTransaction transaction)
        {
            if (transaction.PaymentDate ==
                default)
            {
                throw new InvalidOperationException(
                    "Payment Date is required.");
            }


            if (transaction.Amount <= 0m)
            {
                throw new InvalidOperationException(
                    "Payment Amount must be greater than zero.");
            }


            if (string.IsNullOrWhiteSpace(
                transaction.PaymentMode))
            {
                throw new InvalidOperationException(
                    "Payment Mode is required.");
            }


            if (transaction.PaymentMode.Length >
                50)
            {
                throw new InvalidOperationException(
                    "Payment Mode cannot exceed 50 characters.");
            }


            if (transaction.BankName != null &&
                transaction.BankName.Length >
                    150)
            {
                throw new InvalidOperationException(
                    "Bank Name cannot exceed 150 characters.");
            }


            if (transaction.ReferenceNumber != null &&
                transaction.ReferenceNumber.Length >
                    150)
            {
                throw new InvalidOperationException(
                    "Reference Number cannot exceed 150 characters.");
            }


            if (transaction.Remarks != null &&
                transaction.Remarks.Length >
                    1000)
            {
                throw new InvalidOperationException(
                    "Remarks cannot exceed 1000 characters.");
            }
        }


        private static void NormalizeTransaction(
            SupplierPaymentTransaction transaction)
        {
            transaction.Amount =
                RoundMoney(
                    transaction.Amount);


            transaction.PaymentMode =
                transaction.PaymentMode?
                    .Trim()
                ?? string.Empty;


            transaction.BankName =
                NormalizeOptionalText(
                    transaction.BankName);


            transaction.ReferenceNumber =
                NormalizeOptionalText(
                    transaction.ReferenceNumber);


            transaction.Remarks =
                NormalizeOptionalText(
                    transaction.Remarks);
        }


        private static void PrepareNewTransaction(
            SupplierPaymentTransaction transaction,
            DateTime now)
        {
            transaction.Id =
                0;


            transaction.IsActive =
                true;


            transaction.IsDeleted =
                false;


            transaction.CreatedOn =
                now;


            transaction.ModifiedOn =
                null;
        }

        #endregion


        // =====================================================
        // PAYMENT STATUS
        // =====================================================

        #region Payment Status

        private static string CalculatePaymentStatus(
            decimal invoiceTotal,
            decimal paidAmount)
        {
            invoiceTotal =
                RoundMoney(
                    invoiceTotal);


            paidAmount =
                RoundMoney(
                    paidAmount);


            if (paidAmount <= 0m)
            {
                return "Pending";
            }


            if (paidAmount >=
                invoiceTotal)
            {
                return "Completed";
            }


            return "Partially Paid";
        }

        #endregion


        // =====================================================
        // PAYMENT CODE GENERATION
        // =====================================================

        #region Payment Code Generation

        private async Task<string>
            GeneratePaymentCodeAsync(
                DateTime paymentDate)
        {
            /*
             * Financial Year = April to March
             *
             * Example:
             * 01-04-2026 to 31-03-2027
             *
             * AI/SPAY/26-27/00001
             */

            var financialYearStart =
                paymentDate.Month >= 4
                    ? paymentDate.Year
                    : paymentDate.Year - 1;


            var financialYearEnd =
                financialYearStart + 1;


            var fyText =
                $"{financialYearStart % 100:00}-" +
                $"{financialYearEnd % 100:00}";


            var prefix =
                $"AI/SPAY/{fyText}/";


            var lastCode =
                await _repository
                    .GetLastCodeAsync(
                        prefix);


            var nextNumber =
                1;


            if (!string.IsNullOrWhiteSpace(
                lastCode))
            {
                var lastPart =
                    lastCode
                        .Split('/')
                        .LastOrDefault();


                if (int.TryParse(
                    lastPart,
                    out var lastNumber))
                {
                    nextNumber =
                        lastNumber + 1;
                }
            }


            return
                $"{prefix}{nextNumber:00000}";
        }

        #endregion


        // =====================================================
        // COMMON HELPERS
        // =====================================================

        #region Common Helpers

        private static decimal RoundMoney(
            decimal value)
        {
            return Math.Round(
                value,
                2,
                MidpointRounding.AwayFromZero);
        }


        private static string?
            NormalizeOptionalText(
                string? value)
        {
            if (string.IsNullOrWhiteSpace(
                value))
            {
                return null;
            }


            return value.Trim();
        }


        private static void NormalizePaging(
            ref int pageNumber,
            ref int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }


            if (pageSize < 1)
            {
                pageSize = 10;
            }


            if (pageSize > 100)
            {
                pageSize = 100;
            }
        }

        #endregion
    }
}