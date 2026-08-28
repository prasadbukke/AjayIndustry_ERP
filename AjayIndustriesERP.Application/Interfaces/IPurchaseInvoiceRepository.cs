/*
============================================================
File: IPurchaseInvoiceRepository.cs

Module:
Purchase Invoice / Supplier Bill

Purpose:
Defines all database operations required by the
Purchase Invoice module.

Responsibilities:
- Read Purchase Invoice details.
- Search and paginate Purchase Invoices.
- Load Draft Purchase Invoice for update.
- Load deleted Draft Purchase Invoices.
- Load Purchase Orders having received material.
- Load exact GRN receipt items.
- Calculate already billed GRN quantity.
- Load Supplier and Company masters.
- Validate duplicate Supplier Invoice Number.
- Generate financial-year Purchase Invoice number.
- Add / Update Purchase Invoice.

Important:
- Business rules do NOT belong in Repository.
- PurchaseInvoiceService will perform all validation,
  quantity protection and financial calculations.
- Draft + Finalized active Purchase Invoice Items reserve
  GRN received quantity.
- Deleted Purchase Invoices do NOT reserve quantity.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IPurchaseInvoiceRepository
    {
        #region Purchase Invoice Read

        Task<PurchaseInvoice?>
            GetByIdAsync(
                int id);


        Task<PurchaseInvoice?>
            GetForUpdateAsync(
                int id);

        #endregion


        #region Pagination / Search

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


        #region Deleted Purchase Invoices

        Task<List<PurchaseInvoice>>
            GetDeletedAsync();


        Task<PurchaseInvoice?>
            GetDeletedForUpdateAsync(
                int id);

        #endregion


        #region Purchase Order Sources

        /// <summary>
        /// Returns Purchase Orders having active GRN
        /// received quantities available for Purchase Invoice.
        ///
        /// Final billable quantity validation will still
        /// be performed by PurchaseInvoiceService.
        /// </summary>
        Task<List<PurchaseOrder>>
            GetPurchaseOrdersForInvoiceAsync();


        /// <summary>
        /// Loads one Purchase Order with Supplier,
        /// Company and Purchase Order Items required
        /// for trusted Purchase Invoice preparation.
        /// </summary>
        Task<PurchaseOrder?>
            GetPurchaseOrderForInvoiceAsync(
                int purchaseOrderId);

        #endregion


        #region GRN Sources

        /// <summary>
        /// Returns all active GRN Item rows having
        /// ReceivedQuantity > 0 against the selected
        /// Purchase Order.
        ///
        /// Includes exact GoodsReceiptNote,
        /// PurchaseOrderItem and Item references.
        /// </summary>
        Task<List<GoodsReceiptNoteItem>>
            GetReceivedGoodsReceiptItemsForInvoiceAsync(
                int purchaseOrderId);


        /// <summary>
        /// Loads one exact GRN Item source.
        ///
        /// Used by Create / Edit / Finalize validation
        /// so browser-posted snapshots are never trusted.
        /// </summary>
        Task<GoodsReceiptNoteItem?>
            GetGoodsReceiptNoteItemForInvoiceAsync(
                int goodsReceiptNoteItemId);

        #endregion


        #region Quantity Allocation

        /// <summary>
        /// Returns Purchase Invoice quantity already allocated
        /// against one exact GoodsReceiptNoteItem.
        ///
        /// Includes:
        /// - Active Draft Purchase Invoices
        /// - Active Finalized Purchase Invoices
        ///
        /// Excludes:
        /// - Deleted Purchase Invoices
        /// - Deleted Purchase Invoice Items
        ///
        /// excludePurchaseInvoiceId is used during Edit,
        /// Finalize and Restore so current Purchase Invoice
        /// does not reserve its own quantity twice.
        /// </summary>
        Task<decimal>
            GetAllocatedPurchaseInvoiceQuantityAsync(
                int goodsReceiptNoteItemId,
                int? excludePurchaseInvoiceId = null);

        #endregion


        #region Supplier

        Task<Supplier?>
            GetSupplierForPurchaseInvoiceAsync(
                int supplierId);


        /// <summary>
        /// Supplier's actual Invoice Number must be unique
        /// for that Supplier.
        ///
        /// Same invoice number from two different Suppliers
        /// is allowed.
        /// </summary>
        Task<bool>
            SupplierInvoiceNumberExistsAsync(
                int supplierId,
                string supplierInvoiceNumber,
                int? excludePurchaseInvoiceId = null);

        #endregion


        #region Company

        Task<Company?>
            GetCompanyForPurchaseInvoiceAsync();

        #endregion


        #region Code Generation

        Task<string?>
            GetLastCodeAsync(
                string codePrefix);

        #endregion


        #region Save

        Task AddAsync(
            PurchaseInvoice purchaseInvoice);


        Task UpdateAsync(
            PurchaseInvoice purchaseInvoice);

        #endregion
    }
}