/*
============================================================
File: ICustomerReceiptRepository.cs

Module:
Customer Receipt

Purpose:
Defines persistence operations required by
Customer Receipt module.

Responsibilities:
- Read Customer Receipts.
- Support active / deleted Receipt listing.
- Load Customers eligible for Receipt creation.
- Load Finalized Customer Invoices.
- Calculate finalized allocations against Invoice.
- Support outstanding Invoice calculation.
- Generate next Receipt code.
- Add / update Customer Receipt.
- Load Company information for historical snapshot.

Important:
- Repository performs data access only.
- Business validation belongs to CustomerReceiptService.
- Invoice outstanding is derived from Finalized
  Customer Receipt Allocations.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface ICustomerReceiptRepository
    {
        #region Receipt Read

        Task<CustomerReceipt?>
            GetByIdAsync(
                int id);


        Task<CustomerReceipt?>
            GetForUpdateAsync(
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


        Task<Customer?>
            GetCustomerForReceiptAsync(
                int customerId);

        #endregion


        #region Invoice Source

        /*
         * Returns Finalized Invoices belonging to
         * the selected Customer.
         *
         * Service layer determines whether the Invoice
         * still has outstanding balance.
         */
        Task<List<Invoice>>
            GetFinalizedInvoicesForReceiptAsync(
                int customerId);


        /*
         * Returns one trusted Finalized Invoice
         * for Create / Update / Finalize validation.
         */
        Task<Invoice?>
            GetFinalizedInvoiceForReceiptAsync(
                int invoiceId);

        #endregion


        #region Invoice Allocation

        /*
         * Returns total amount already allocated against
         * an Invoice through Finalized Customer Receipts.
         *
         * excludeCustomerReceiptId is used while editing
         * or finalizing the current Receipt so its own
         * allocation is not counted twice.
         */
        Task<decimal>
            GetFinalizedAllocatedAmountAsync(
                int invoiceId,
                int? excludeCustomerReceiptId = null);

        #endregion


        #region Company

        Task<Company?>
            GetCompanyForReceiptAsync();

        #endregion


        #region Receipt Code

        Task<string?> GetLastCodeAsync(
    string codePrefix);

        #endregion


        #region Write

        Task AddAsync(
            CustomerReceipt customerReceipt);


        Task UpdateAsync(
            CustomerReceipt customerReceipt);

        #endregion
    }
}