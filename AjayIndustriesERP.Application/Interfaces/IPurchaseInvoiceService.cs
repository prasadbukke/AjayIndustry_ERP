using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IPurchaseInvoiceService
    {
        // =====================================================
        // READ
        // =====================================================

        #region Read

        Task<PurchaseInvoice?> GetByIdAsync(
            int id);


        Task<PagedResult<PurchaseInvoice>> GetPagedAsync(
            int pageNumber,
            int pageSize);


        /*
         * Search / Filter Purchase Invoices.
         *
         * searchText supports:
         * - ERP Purchase Invoice Code
         * - Supplier Invoice Number
         * - Supplier Name
         * - Purchase Order Code
         *
         * purchaseInvoiceDate:
         * - ERP Purchase Invoice Date
         *
         * supplierInvoiceDate:
         * - Supplier's actual Invoice Date
         *
         * All filters are optional and may be combined.
         */
        Task<PagedResult<PurchaseInvoice>> SearchPagedAsync(
            string? searchText,
            DateTime? purchaseInvoiceDate,
            DateTime? supplierInvoiceDate,
            int pageNumber,
            int pageSize);

        #endregion


        // =====================================================
        // PURCHASE ORDER SOURCE
        // =====================================================

        #region Purchase Order Source

        /*
         * Returns Purchase Orders which have GRN received
         * quantity still available for Purchase Invoice.
         */
        Task<List<PurchaseOrder>>
            GetPurchaseOrdersForInvoiceAsync();


        /*
         * Loads trusted Purchase Order with Supplier,
         * Company and Items required by Purchase Invoice.
         */
        Task<PurchaseOrder?>
            GetPurchaseOrderForInvoiceAsync(
                int purchaseOrderId);

        #endregion


        // =====================================================
        // GRN AVAILABILITY
        // =====================================================

        #region GRN Availability

        /*
         * Returns received GRN items having quantity
         * available for Purchase Invoice.
         *
         * excludePurchaseInvoiceId:
         * Used during Edit / Finalize / Restore so current
         * invoice quantity does not block itself.
         */
        Task<List<GoodsReceiptNoteItem>>
            GetAvailableGoodsReceiptItemsAsync(
                int purchaseOrderId,
                int? excludePurchaseInvoiceId = null);


        /*
         * Remaining Billable Quantity:
         *
         * GRN Received Quantity
         *      -
         * Active Purchase Invoice allocated quantity
         */
        Task<decimal>
            GetRemainingPurchaseInvoiceQuantityAsync(
                int goodsReceiptNoteItemId,
                int? excludePurchaseInvoiceId = null);

        #endregion


        // =====================================================
        // PREPARE CREATE
        // =====================================================

        #region Prepare Draft

        /*
         * Prepares Purchase Invoice form from selected PO.
         *
         * Important:
         * - GRN received quantity is loaded.
         * - GST / HSN / Drawing / Item data comes from
         *   trusted source.
         * - Supplier Invoice Rate starts at zero because
         *   user enters actual Supplier Bill Rate manually.
         */
        Task<PurchaseInvoice>
            PrepareDraftAsync(
                int purchaseOrderId);

        #endregion


        // =====================================================
        // CREATE
        // =====================================================

        #region Create

        /*
         * Creates Purchase Invoice in Draft status.
         *
         * Supplier Invoice PDF metadata, when supplied,
         * is stored with Purchase Invoice.
         */
        Task<PurchaseInvoice>
            CreateAsync(
                PurchaseInvoice purchaseInvoice);

        #endregion


        // =====================================================
        // UPDATE
        // =====================================================

        #region Update

        /*
         * Only Draft Purchase Invoice financial data
         * can be edited.
         *
         * Finalized Purchase Invoice remains locked.
         */
        Task<PurchaseInvoice>
            UpdateAsync(
                PurchaseInvoice purchaseInvoice);

        #endregion


        // =====================================================
        // APPROVE / FINALIZE
        // =====================================================

        #region Finalize

        /*
         * UI label:
         *      Approve
         *
         * Internal workflow/status:
         *      Draft -> Finalized
         *
         * No separate Approved status is required.
         */
        Task<PurchaseInvoice>
            FinalizeAsync(
                int id);

        #endregion


        // =====================================================
        // DELETE
        // =====================================================

        #region Delete

        /*
         * Soft Delete is allowed for:
         *
         * - Draft Purchase Invoice
         * - Finalized Purchase Invoice
         *
         * Delete does NOT physically delete Supplier PDF.
         *
         * Deleted Purchase Invoice stops reserving GRN
         * quantity.
         *
         * Future:
         * When Supplier Payment module is available,
         * dependency validation can prevent deletion of an
         * Invoice having linked payments.
         */
        Task DeleteAsync(
            int id);

        #endregion


        // =====================================================
        // DELETED LIST
        // =====================================================

        #region Deleted

        /*
         * Returns all soft-deleted Purchase Invoices,
         * including Draft and Finalized invoices.
         */
        Task<List<PurchaseInvoice>>
            GetDeletedAsync();

        #endregion


        // =====================================================
        // RESTORE
        // =====================================================

        #region Restore

        /*
         * Restore is allowed for deleted:
         *
         * - Draft
         * - Finalized
         *
         * Before Restore:
         * - Supplier Invoice Number is revalidated.
         * - GRN available quantity is revalidated.
         *
         * Original workflow status is preserved.
         */
        Task RestoreAsync(
            int id);

        #endregion
    }
}