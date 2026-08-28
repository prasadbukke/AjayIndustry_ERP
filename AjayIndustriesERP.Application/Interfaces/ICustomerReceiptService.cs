/*
============================================================
File: ICustomerReceiptService.cs

Module:
Customer Receipt

Purpose:
Defines business operations for Customer Receipt module.

Responsibilities:
- Read and search Customer Receipts.
- Load Customers for Receipt creation.
- Load Customer Finalized Invoices having
  outstanding balance.
- Calculate trusted Invoice outstanding amount.
- Create and update Draft Receipts.
- Finalize Customer Receipt.
- Soft-delete / restore Draft Receipts.
- Generate Finalized Receipt PDF.

Important:
- Service layer is authoritative for all financial
  validations.
- Browser-posted Invoice totals, already received
  amounts and outstanding amounts are not trusted.
- Invoice outstanding is calculated from:
      Invoice GrandTotal
      -
      Finalized Customer Receipt Allocations.
- A Finalized Receipt cannot be edited or deleted.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface ICustomerReceiptService
    {
        #region Receipt Read

        Task<CustomerReceipt?>
            GetByIdAsync(
                int id);

        #endregion


        #region Receipt Listing

        Task<PagedResult<CustomerReceipt>>
            GetPagedAsync(
                int pageNumber,
                int pageSize);


        Task<PagedResult<CustomerReceipt>>
            SearchPagedAsync(
                string? searchTerm,
                int pageNumber,
                int pageSize);


        Task<PagedResult<CustomerReceipt>>
            GetDeletedPagedAsync(
                int pageNumber,
                int pageSize);


        Task<PagedResult<CustomerReceipt>>
            SearchDeletedPagedAsync(
                string? searchTerm,
                int pageNumber,
                int pageSize);

        #endregion


        #region Customer

        Task<List<Customer>>
            GetCustomersForReceiptAsync();

        #endregion


        #region Invoice Source

        /*
         * Returns only Finalized Invoices belonging
         * to the selected Customer that still have
         * an outstanding balance.
         *
         * excludeCustomerReceiptId is useful while
         * editing an existing Draft Receipt.
         */
        Task<List<Invoice>>
            GetOutstandingInvoicesForCustomerAsync(
                int customerId,
                int? excludeCustomerReceiptId = null);


        /*
         * Returns trusted current outstanding amount
         * for one Finalized Invoice.
         */
        Task<decimal>
            GetInvoiceOutstandingAsync(
                int invoiceId,
                int? excludeCustomerReceiptId = null);

        #endregion


        #region Create

        Task<CustomerReceipt>
            CreateAsync(
                CustomerReceipt customerReceipt);

        #endregion


        #region Update

        Task<CustomerReceipt>
            UpdateAsync(
                CustomerReceipt customerReceipt);

        #endregion


        #region Finalize

        Task<CustomerReceipt>
            FinalizeAsync(
                int id);

        #endregion


        #region Delete / Restore

        Task DeleteAsync(
            int id);


        Task RestoreAsync(
            int id);

        #endregion


        #region PDF

        Task<byte[]>
            GeneratePdfAsync(
                int id);

        #endregion
    }
}