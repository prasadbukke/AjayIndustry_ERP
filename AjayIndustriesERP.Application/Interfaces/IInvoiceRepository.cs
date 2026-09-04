/*
============================================================
File: IInvoiceRepository.cs

Module:
Invoice

Purpose:
Defines database operations required by Invoice module.

Current Production Structure:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Items

Invoice Item Source Identity:

ProductionJobId
        +
CustomerPurchaseOrderItemId

Important:
- One Production Job may contain multiple Production Items.
- Invoice quantity allocation must therefore be Item-wise.
- ProductionJobId alone is NOT sufficient for allocation.
- No new Invoice database column is required.
- PDI and Delivery Challan remain warning-only.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IInvoiceRepository
    {
        #region Invoice Read

        Task<Invoice?>
            GetByIdAsync(
                int id);


        Task<Invoice?>
            GetForUpdateAsync(
                int id);

        #endregion


        #region Pagination And Search

        Task<PagedResult<Invoice>>
            GetPagedAsync(
                int pageNumber,
                int pageSize);


        Task<PagedResult<Invoice>>
            SearchPagedAsync(
                string searchText,
                int pageNumber,
                int pageSize);

        #endregion


        #region Customer Purchase Order Source

        Task<List<CustomerPurchaseOrder>>
            GetCustomerPurchaseOrdersForInvoiceAsync();


        Task<CustomerPurchaseOrder?>
            GetCustomerPurchaseOrderForInvoiceAsync(
                int customerPurchaseOrderId);

        #endregion


        #region Production Source

        /*
         * Returns Production Jobs belonging to the selected
         * Customer PO and containing at least one completed
         * ProductionJobItem.
         */

        Task<List<ProductionJob>>
            GetCompletedProductionJobsForInvoiceAsync(
                int customerPurchaseOrderId);


        /*
         * Returns one Production Job together with
         * its ProductionJobItems and trusted source data.
         */

        Task<ProductionJob?>
            GetCompletedProductionJobForInvoiceAsync(
                int productionJobId);

        #endregion


        #region Invoice Quantity Allocation

        /*
         * Allocation is Item-wise.
         *
         * Example:
         *
         * Production Job = PJ-001
         *
         * Item A = Customer PO Item 10
         * Item B = Customer PO Item 11
         *
         * Invoicing Item A must NOT reduce
         * available quantity of Item B.
         */

        Task<decimal>
            GetAllocatedInvoiceQuantityAsync(
                int productionJobId,
                int customerPurchaseOrderItemId,
                int? excludeInvoiceId = null);

        #endregion


        #region PDI Warning Status

        /*
         * PDI is warning-only for Invoice.
         *
         * Check PDI for the specific Production Item,
         * identified using:
         *
         * ProductionJobId +
         * CustomerPurchaseOrderItemId.
         */

        Task<bool>
            HasFinalizedPdiAsync(
                int productionJobId,
                int customerPurchaseOrderItemId);

        #endregion


        #region Delivery Challan Warning Status

        /*
         * Delivery Challan is also warning-only.
         *
         * Check Challan for the specific Production Item,
         * not merely the Production Job header.
         */

        Task<bool>
            HasDeliveryChallanAsync(
                int productionJobId,
                int customerPurchaseOrderItemId);

        #endregion


        #region Invoice Code

        Task<string?>
            GetLastCodeAsync(
                string prefix);

        #endregion


        #region Persistence

        Task<Invoice>
            AddAsync(
                Invoice invoice);


        Task<Invoice>
            UpdateAsync(
                Invoice invoice);

        #endregion


        #region Deleted Invoice

        Task<List<Invoice>>
            GetDeletedAsync();


        Task<Invoice?>
            GetDeletedForUpdateAsync(
                int id);

        #endregion


        #region Snapshot Sources

        Task<Customer?>
            GetCustomerForInvoiceAsync(
                int customerId);


        Task<Company?>
            GetCompanyForInvoiceAsync();

        #endregion
    }
}