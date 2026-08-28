/*
============================================================
File: IInvoiceRepository.cs

Module:
Invoice

Purpose:
Defines database operations required by Invoice module.

Responsibilities:
- Read Invoice records.
- Search and paginate Invoices.
- Load eligible Customer Purchase Orders.
- Load Completed Production Jobs for Invoice.
- Calculate already invoiced Production quantity.
- Check PDI / Delivery Challan status.
- Generate next Invoice code.
- Handle Draft delete / restore support.
- Load Customer and Company snapshot sources.

Important:
- New Invoice source flow:
  Customer PO → Completed Production Job → Invoice.
- Delivery Challan is NOT mandatory for Invoice.
- PDI is NOT mandatory for Invoice.
- PDI / Challan status is checked only for warning workflow.
- Draft + Finalized active Invoices reserve Production quantity.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IInvoiceRepository
    {
        #region Invoice Read

        Task<Invoice?> GetByIdAsync(
            int id);


        Task<Invoice?> GetForUpdateAsync(
            int id);

        #endregion


        #region Pagination And Search

        Task<PagedResult<Invoice>> GetPagedAsync(
            int pageNumber,
            int pageSize);


        Task<PagedResult<Invoice>> SearchPagedAsync(
            string searchText,
            int pageNumber,
            int pageSize);

        #endregion


        #region Customer Purchase Order Source

        /*
         * Returns Customer POs having at least one
         * Completed Production Job that still has
         * invoiceable quantity.
         */
        Task<List<CustomerPurchaseOrder>>
            GetCustomerPurchaseOrdersForInvoiceAsync();


        /*
         * Loads one Customer PO with its Items
         * for Invoice source validation.
         */
        Task<CustomerPurchaseOrder?>
            GetCustomerPurchaseOrderForInvoiceAsync(
                int customerPurchaseOrderId);

        #endregion


        #region Completed Production Job Source

        /*
         * Returns Completed Production Jobs belonging
         * to the selected Customer PO.
         *
         * InvoiceService will calculate remaining
         * invoiceable quantity for every Job.
         */
        Task<List<ProductionJob>>
            GetCompletedProductionJobsForInvoiceAsync(
                int customerPurchaseOrderId);


        /*
         * Loads one trusted Completed Production Job.
         */
        Task<ProductionJob?>
            GetCompletedProductionJobForInvoiceAsync(
                int productionJobId);

        #endregion


        #region Production Quantity Allocation

        /*
         * Returns quantity already reserved / invoiced
         * against one Production Job.
         *
         * Active Draft + Finalized Invoices reserve quantity.
         * Deleted Invoices do not reserve quantity.
         *
         * excludeInvoiceId is used during Edit / Finalize
         * so the current Invoice does not reserve against itself.
         */
        Task<decimal>
            GetAllocatedInvoiceQuantityAsync(
                int productionJobId,
                int? excludeInvoiceId = null);

        #endregion


        #region PDI / Delivery Challan Status

        /*
         * Used only for Invoice warning workflow.
         *
         * Missing PDI does NOT block Invoice automatically.
         */
        Task<bool> HasFinalizedPdiAsync(
            int productionJobId);


        /*
         * Used only for Invoice warning workflow.
         *
         * Missing Delivery Challan does NOT block
         * Invoice automatically.
         */
        Task<bool> HasDeliveryChallanAsync(
            int productionJobId);

        #endregion


        #region Invoice Code

        /*
         * Includes deleted Invoices so Invoice numbers
         * are never reused.
         */
        Task<string?> GetLastCodeAsync(
            string prefix);

        #endregion


        #region Persistence

        Task<Invoice> AddAsync(
            Invoice invoice);


        Task<Invoice> UpdateAsync(
            Invoice invoice);

        #endregion


        #region Deleted Invoice

        Task<List<Invoice>> GetDeletedAsync();


        Task<Invoice?> GetDeletedForUpdateAsync(
            int id);

        #endregion


        #region Snapshot Sources

        Task<Customer?> GetCustomerForInvoiceAsync(
            int customerId);


        Task<Company?> GetCompanyForInvoiceAsync();

        #endregion
    }
}