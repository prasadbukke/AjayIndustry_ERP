using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IPurchaseInvoiceRepository
    {
        #region Purchase Invoice - Read

        Task<PurchaseInvoice?> GetByIdAsync(
            int id);


        Task<PagedResult<PurchaseInvoice>> GetPagedAsync(
            int pageNumber,
            int pageSize);


        /*
         * Purchase Invoice Search / Filter
         *
         * searchText:
         * - ERP Purchase Invoice Code
         * - Supplier Invoice Number
         * - Supplier Name
         * - Purchase Order Code
         *
         * purchaseInvoiceDate:
         * - ERP Purchase Invoice Date
         *
         * supplierInvoiceDate:
         * - Supplier's original Invoice Date
         */
        Task<PagedResult<PurchaseInvoice>> SearchPagedAsync(
            string? searchText,
            DateTime? purchaseInvoiceDate,
            DateTime? supplierInvoiceDate,
            int pageNumber,
            int pageSize);

        #endregion


        #region Purchase Invoice - Create / Update

        Task AddAsync(
            PurchaseInvoice purchaseInvoice);


        Task UpdateAsync(
            PurchaseInvoice purchaseInvoice);


        Task<PurchaseInvoice?> GetForUpdateAsync(
            int id);

        #endregion


        #region Purchase Invoice - Delete / Restore

        Task<List<PurchaseInvoice>> GetDeletedAsync();


        Task<PurchaseInvoice?> GetDeletedForUpdateAsync(
            int id);

        #endregion


        #region Purchase Order Source

        Task<List<PurchaseOrder>>
            GetPurchaseOrdersForInvoiceAsync();


        Task<PurchaseOrder?>
            GetPurchaseOrderForInvoiceAsync(
                int purchaseOrderId);

        #endregion


        #region GRN Source

        Task<List<GoodsReceiptNoteItem>>
            GetReceivedGoodsReceiptItemsForInvoiceAsync(
                int purchaseOrderId);


        Task<GoodsReceiptNoteItem?>
            GetGoodsReceiptNoteItemForInvoiceAsync(
                int goodsReceiptNoteItemId);

        #endregion


        #region Quantity Reservation

        /*
         * Draft + Finalized active Purchase Invoices
         * reserve GRN received quantity.
         *
         * excludePurchaseInvoiceId is used during
         * Edit / Finalize / Restore revalidation.
         */
        Task<decimal>
            GetAllocatedPurchaseInvoiceQuantityAsync(
                int goodsReceiptNoteItemId,
                int? excludePurchaseInvoiceId = null);

        #endregion


        #region Supplier Invoice Validation

        /*
         * Supplier Invoice Number must be unique
         * for the same active Supplier.
         */
        Task<bool>
            SupplierInvoiceNumberExistsAsync(
                int supplierId,
                string supplierInvoiceNumber,
                int? excludePurchaseInvoiceId = null);

        #endregion


        #region Purchase Invoice Code

        Task<string?> GetLastCodeAsync(
            string prefix);

        #endregion
    }
}