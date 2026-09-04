/*
============================================================
File: IInvoiceService.cs

Module:
Invoice

Purpose:
Defines Invoice business operations.

Current Invoice Source Flow:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Item
        ↓
Invoice Item

Invoice Item Source Identity:

ProductionJobId
        +
CustomerPurchaseOrderItemId

Important:
- One Production Job may contain multiple Production Items.
- Invoice availability is calculated Item-wise.
- PDI is NOT mandatory.
- Delivery Challan is NOT mandatory.
- Missing PDI / Challan requires warning confirmation only.
- Draft + Finalized active Invoices reserve quantity.
- Deleted Invoices do not reserve quantity.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IInvoiceService
    {
        #region Read Operations

        Task<Invoice?>
            GetByIdAsync(
                int id);

        #endregion


        #region Search And Pagination

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


        #region Customer Purchase Order Sources

        /*
         * Returns Customer POs having at least one
         * completed Production Item with remaining
         * invoiceable quantity.
         */

        Task<List<CustomerPurchaseOrder>>
            GetCustomerPurchaseOrdersForInvoiceAsync();


        Task<CustomerPurchaseOrder?>
            GetCustomerPurchaseOrderForInvoiceAsync(
                int customerPurchaseOrderId);

        #endregion


        #region Production Sources

        /*
         * Method name is retained for compatibility.
         *
         * A returned Production Job may contain multiple
         * ProductionJobItems.
         *
         * Only completed Production Items having
         * remaining invoiceable quantity are relevant
         * to Invoice preparation.
         */

        Task<List<ProductionJob>>
            GetCompletedProductionJobsForInvoiceAsync(
                int customerPurchaseOrderId);


        Task<ProductionJob?>
            GetCompletedProductionJobForInvoiceAsync(
                int productionJobId);

        #endregion


        #region Invoice Quantity Availability

        /*
         * CRITICAL:
         *
         * Availability is calculated using BOTH:
         *
         * ProductionJobId
         * +
         * CustomerPurchaseOrderItemId
         *
         * Example:
         *
         * PJ-001
         *   Item A = PO Item 10
         *   Item B = PO Item 11
         *
         * Invoice of Item A must not consume
         * availability of Item B.
         */

        Task<decimal>
            GetRemainingInvoiceQuantityAsync(
                int productionJobId,
                int customerPurchaseOrderItemId,
                int? excludeInvoiceId = null);

        #endregion


        #region PDI And Delivery Challan Warning

        /*
         * Retained as Job-level UI helper.
         *
         * Internally the Service checks each completed
         * Production Item belonging to the supplied Jobs.
         *
         * A Job is returned when at least one relevant
         * Production Item does not have:
         *
         * - Finalized PDI
         * OR
         * - Delivery Challan
         *
         * This warning does NOT block Invoice.
         */

        Task<List<int>>
            GetProductionJobIdsRequiringWarningAsync(
                IEnumerable<int> productionJobIds);

        #endregion


        #region Prepare Draft

        /*
         * Prepares unsaved Invoice from Customer PO.
         *
         * All completed Production Items having
         * available quantity are loaded as Invoice lines.
         */

        Task<Invoice?>
            PrepareDraftAsync(
                int customerPurchaseOrderId);

        #endregion


        #region Create

        /*
         * confirmSourceWarning:
         *
         * false:
         * Missing PDI / Challan causes confirmation
         * message.
         *
         * true:
         * User explicitly confirmed and Invoice may
         * continue.
         */

        Task<Invoice>
            CreateAsync(
                Invoice invoice,
                bool confirmSourceWarning);

        #endregion


        #region Update Draft

        Task<Invoice>
            UpdateAsync(
                Invoice invoice,
                bool confirmSourceWarning);

        #endregion


        #region Finalize

        Task<Invoice>
            FinalizeAsync(
                int id,
                bool confirmSourceWarning);

        #endregion


        #region Delete

        Task DeleteAsync(
            int id);

        #endregion


        #region Deleted Invoices

        Task<List<Invoice>>
            GetDeletedAsync();


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