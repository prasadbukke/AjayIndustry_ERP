/*
============================================================
File: IPurchaseInvoiceService.cs

Module:
Purchase Invoice / Supplier Bill

Purpose:
Defines Purchase Invoice business operations.

Source Flow:
Purchase Order
    → Goods Receipt Note
    → Purchase Invoice
    → Supplier Payment
    → Supplier Outstanding

Responsibilities:
- Read / search Purchase Invoices.
- Load Purchase Orders having received material.
- Load available received GRN quantities.
- Calculate remaining billable quantity.
- Prepare new Purchase Invoice Draft.
- Create trusted Purchase Invoice.
- Update Draft Purchase Invoice.
- Finalize Purchase Invoice.
- Soft-delete Draft Purchase Invoice.
- Restore deleted Draft Purchase Invoice.

Important:
- Purchase Invoice is based on actual GRN
  ReceivedQuantity.
- Draft + Finalized active Purchase Invoices reserve
  GRN received quantity.
- Deleted Purchase Invoices do not reserve quantity.
- Finalized Purchase Invoice cannot be edited/deleted.
- Supplier Invoice Number must be unique per Supplier.
- MaterialStatus is currently informational only and
  does not block Purchase Invoice creation in Phase 1.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IPurchaseInvoiceService
    {
        #region Read Operations

        Task<PurchaseInvoice?>
            GetByIdAsync(
                int id);

        #endregion


        #region Search And Pagination

        Task<PagedResult<PurchaseInvoice>>
            GetPagedAsync(
                int pageNumber,
                int pageSize);


        Task<PagedResult<PurchaseInvoice>>
            SearchPagedAsync(
                string searchText,
                int pageNumber,
                int pageSize);

        #endregion


        #region Purchase Order Sources

        /// <summary>
        /// Returns Purchase Orders having received
        /// GRN quantity available for Purchase Invoice.
        /// </summary>
        Task<List<PurchaseOrder>>
            GetPurchaseOrdersForInvoiceAsync();


        /// <summary>
        /// Loads one Purchase Order used as
        /// Purchase Invoice source.
        /// </summary>
        Task<PurchaseOrder?>
            GetPurchaseOrderForInvoiceAsync(
                int purchaseOrderId);

        #endregion


        #region GRN Sources

        /// <summary>
        /// Returns received GRN lines against the
        /// selected Purchase Order that still have
        /// quantity available for Purchase Invoice.
        ///
        /// When editing, excludePurchaseInvoiceId allows
        /// the current Draft Purchase Invoice quantity
        /// to remain available to itself.
        /// </summary>
        Task<List<GoodsReceiptNoteItem>>
            GetAvailableGoodsReceiptItemsAsync(
                int purchaseOrderId,
                int? excludePurchaseInvoiceId = null);


        /// <summary>
        /// Calculates remaining billable quantity
        /// against one exact GRN Item.
        ///
        /// Formula:
        ///
        /// GRN ReceivedQuantity
        /// -
        /// Active Draft / Finalized
        /// PurchaseInvoiceQuantity
        /// =
        /// Remaining Billable Quantity
        /// </summary>
        Task<decimal>
            GetRemainingPurchaseInvoiceQuantityAsync(
                int goodsReceiptNoteItemId,
                int? excludePurchaseInvoiceId = null);

        #endregion


        #region Prepare Draft

        /// <summary>
        /// Prepares an unsaved Purchase Invoice from
        /// one selected Purchase Order.
        ///
        /// Loads:
        /// - Supplier
        /// - Company
        /// - Payment Terms
        /// - Due Date
        /// - Available GRN received quantities
        /// - PO Rate
        /// - HSN
        /// - GST
        /// - Item snapshots
        /// </summary>
        Task<PurchaseInvoice>
            PrepareDraftAsync(
                int purchaseOrderId);

        #endregion


        #region Create

        Task<PurchaseInvoice>
            CreateAsync(
                PurchaseInvoice purchaseInvoice);

        #endregion


        #region Update

        /// <summary>
        /// Only Draft Purchase Invoice can be updated.
        /// </summary>
        Task<PurchaseInvoice>
            UpdateAsync(
                PurchaseInvoice purchaseInvoice);

        #endregion


        #region Finalize

        /// <summary>
        /// Revalidates all GRN quantities and
        /// financial values before Finalization.
        ///
        /// Finalized Purchase Invoice becomes the
        /// accounting source for Supplier Payment.
        /// </summary>
        Task<PurchaseInvoice>
            FinalizeAsync(
                int id);

        #endregion


        #region Delete

        /// <summary>
        /// Only Draft Purchase Invoice can be
        /// soft-deleted.
        /// </summary>
        Task DeleteAsync(
            int id);

        #endregion


        #region Deleted Purchase Invoices

        Task<List<PurchaseInvoice>>
            GetDeletedAsync();


        /// <summary>
        /// Restores deleted Draft Purchase Invoice
        /// only after rechecking available GRN quantity.
        /// </summary>
        Task RestoreAsync(
            int id);

        #endregion
    }
}