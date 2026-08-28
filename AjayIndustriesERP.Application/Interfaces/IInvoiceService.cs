/*
============================================================
File: IInvoiceService.cs

Module:
Invoice

Purpose:
Defines business operations available for Invoice module.

Responsibilities:
- Read and search Invoices.
- Load eligible Customer Purchase Orders.
- Load Completed Production Jobs for selected Customer PO.
- Calculate remaining invoiceable Production quantity.
- Check PDI / Delivery Challan warning status.
- Prepare new Invoice Draft from Customer PO.
- Create and update Draft Invoice.
- Calculate trusted financial values.
- Finalize Invoice.
- Soft-delete and restore Draft Invoice.
- Generate finalized Invoice PDF.

Important:
- New Invoice source flow:
  Customer PO → Completed Production Job → Invoice.
- Delivery Challan is NOT mandatory for Invoice.
- PDI is NOT mandatory for Invoice.
- Missing PDI / Delivery Challan requires warning confirmation only.
- Draft + Finalized active Invoices reserve Production quantity.
- Deleted Invoices do not reserve Production quantity.
- Invoice source quantities are always rebuilt / validated
  from trusted Production Job records.
- Finalized Invoice is immutable.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IInvoiceService
    {
        #region Read Operations

        Task<Invoice?> GetByIdAsync(
            int id);

        #endregion


        #region Search And Pagination

        Task<PagedResult<Invoice>> GetPagedAsync(
            int pageNumber,
            int pageSize);


        Task<PagedResult<Invoice>> SearchPagedAsync(
            string searchText,
            int pageNumber,
            int pageSize);

        #endregion


        #region Customer Purchase Order Sources

        /// <summary>
        /// Returns Customer Purchase Orders having at least
        /// one Completed Production Job with remaining
        /// invoiceable quantity.
        /// </summary>
        Task<List<CustomerPurchaseOrder>>
            GetCustomerPurchaseOrdersForInvoiceAsync();


        /// <summary>
        /// Returns one trusted Customer Purchase Order
        /// including its Items for Invoice validation.
        /// </summary>
        Task<CustomerPurchaseOrder?>
            GetCustomerPurchaseOrderForInvoiceAsync(
                int customerPurchaseOrderId);

        #endregion


        #region Completed Production Job Sources

        /// <summary>
        /// Returns Completed Production Jobs belonging
        /// to the selected Customer Purchase Order.
        ///
        /// PDI and Delivery Challan are NOT mandatory
        /// for this source list.
        /// </summary>
        Task<List<ProductionJob>>
            GetCompletedProductionJobsForInvoiceAsync(
                int customerPurchaseOrderId);


        /// <summary>
        /// Returns one trusted Completed Production Job
        /// for Invoice source validation.
        /// </summary>
        Task<ProductionJob?>
            GetCompletedProductionJobForInvoiceAsync(
                int productionJobId);

        #endregion


        #region Invoice Quantity Availability

        /// <summary>
        /// Calculates remaining quantity available for
        /// invoicing against one Completed Production Job.
        ///
        /// Draft + Finalized active Invoices reserve quantity.
        ///
        /// During Edit / Finalize, excludeInvoiceId prevents
        /// the current Invoice from allocating against itself.
        /// </summary>
        Task<decimal> GetRemainingInvoiceQuantityAsync(
            int productionJobId,
            int? excludeInvoiceId = null);

        #endregion


        #region PDI And Delivery Challan Warning

        /// <summary>
        /// Returns Production Job IDs where either:
        ///
        /// - Finalized PDI is missing, or
        /// - Delivery Challan is missing.
        ///
        /// These conditions do NOT make the Production Job
        /// ineligible for Invoice.
        ///
        /// They are used only to display warning and require
        /// explicit confirmation before Invoice submission.
        /// </summary>
        Task<List<int>>
            GetProductionJobIdsRequiringWarningAsync(
                IEnumerable<int> productionJobIds);

        #endregion


        #region Prepare Draft

        /// <summary>
        /// Prepares an unsaved Invoice Draft from the
        /// selected Customer Purchase Order.
        ///
        /// Responsibilities:
        /// - Load trusted Customer PO.
        /// - Load eligible Completed Production Jobs.
        /// - Load Customer Master.
        /// - Load Company Master.
        /// - Prepare Customer snapshot.
        /// - Prepare Company snapshot.
        /// - Auto-load Billing Address.
        /// - Auto-load Payment Terms / Credit Days.
        /// - Auto-load Company Invoice Terms.
        /// - Prepare Production Job lines having remaining
        ///   invoiceable quantity.
        ///
        /// PDI / Delivery Challan are not mandatory.
        /// </summary>
        Task<Invoice?> PrepareDraftAsync(
            int customerPurchaseOrderId);

        #endregion


        #region Create

        /// <summary>
        /// Creates a new Draft Invoice.
        ///
        /// Browser-posted source snapshots and calculated
        /// amounts are not trusted.
        ///
        /// InvoiceService must rebuild source values from
        /// Customer PO / Completed Production Job records
        /// and calculate all financial amounts.
        ///
        /// When selected Production Jobs are missing PDI
        /// or Delivery Challan, confirmSourceWarning must
        /// be true before submission is accepted.
        /// </summary>
        Task<Invoice> CreateAsync(
            Invoice invoice,
            bool confirmSourceWarning);

        #endregion


        #region Update

        /// <summary>
        /// Updates an existing Draft Invoice.
        ///
        /// Finalized Invoice cannot be updated.
        ///
        /// Customer / Company historical snapshots must not
        /// be refreshed during normal Draft Edit.
        ///
        /// Production source quantities are revalidated.
        ///
        /// Missing PDI / Delivery Challan requires explicit
        /// warning confirmation.
        /// </summary>
        Task<Invoice> UpdateAsync(
            Invoice invoice,
            bool confirmSourceWarning);

        #endregion


        #region Finalize

        /// <summary>
        /// Finalizes Invoice after re-validating:
        ///
        /// - Customer Purchase Order.
        /// - Completed Production Jobs.
        /// - Production Job belongs to selected Customer PO.
        /// - Remaining invoiceable quantities.
        /// - Rates / Discounts / GST.
        /// - Financial totals.
        /// - PDI / Delivery Challan warning confirmation.
        ///
        /// PDI and Delivery Challan are NOT mandatory.
        ///
        /// Finalized Invoice becomes immutable.
        /// </summary>
        Task<Invoice> FinalizeAsync(
            int id,
            bool confirmSourceWarning);

        #endregion


        #region Delete

        /// <summary>
        /// Soft-deletes Draft Invoice.
        ///
        /// Finalized Invoice cannot be deleted.
        /// </summary>
        Task DeleteAsync(
            int id);

        #endregion


        #region Deleted Records

        Task<List<Invoice>> GetDeletedAsync();


        /// <summary>
        /// Restores deleted Draft Invoice only when all
        /// required Production Job quantities are still
        /// available.
        ///
        /// PDI / Delivery Challan status does not block
        /// restore.
        /// </summary>
        Task RestoreAsync(
            int id);

        #endregion


        #region PDF

        /// <summary>
        /// Generates customer-facing PDF for Finalized
        /// Invoice only.
        /// </summary>
        Task<byte[]> GeneratePdfAsync(
            int id);

        #endregion
    }
}