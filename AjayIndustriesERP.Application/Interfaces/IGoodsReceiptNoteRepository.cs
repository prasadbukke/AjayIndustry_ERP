// ============================================================
// File: IGoodsReceiptNoteRepository.cs
// Purpose:
// Defines all database operations required by the
// Goods Receipt Note (GRN) module.
//
// Responsibilities:
// - Read GRN list/details
// - Load GRN for Edit
// - Load Purchase Orders and PO items
// - Calculate previous received quantity
// - Validate Supplier Challan duplicate
// - Detect later GRNs before allowing Edit
// - Generate next GRN number
// - Add / Update GRN
//
// Important:
// Business rules do NOT belong in this interface/repository.
// They remain inside GoodsReceiptNoteService.
// ============================================================

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IGoodsReceiptNoteRepository
    {
        Task<List<GoodsReceiptNote>> GetAllAsync();

        Task<GoodsReceiptNote?> GetByIdAsync(
            int id);

        Task<GoodsReceiptNote?> GetForUpdateAsync(
            int id);

        Task<List<PurchaseOrder>>
            GetPurchaseOrdersForReceiptAsync();

        Task<PurchaseOrder?>
            GetPurchaseOrderForReceiptAsync(
                int purchaseOrderId);

        Task<decimal>
            GetPreviouslyReceivedQuantityAsync(
                int purchaseOrderItemId,
                int? excludeGoodsReceiptNoteId = null);

        Task<bool>
            SupplierChallanNumberExistsAsync(
                int supplierId,
                string supplierChallanNumber,
                int? excludeGoodsReceiptNoteId = null);

        Task<bool>
            HasLaterGoodsReceiptNoteAsync(
                int purchaseOrderId,
                int goodsReceiptNoteId);

        Task<string?>
            GetLastGoodsReceiptNoteCodeAsync(
                string codePrefix);

        Task<List<GoodsReceiptNote>> SearchAsync(
    string searchText);

        Task<PagedResult<GoodsReceiptNote>> GetPagedAsync(
            int pageNumber,
            int pageSize);

        Task<List<GoodsReceiptNoteItem>> GetReceiptHistoryAsync(
        int purchaseOrderId,
        int upToGoodsReceiptNoteId);

        Task AddAsync(
            GoodsReceiptNote goodsReceiptNote);

        Task UpdateAsync(
            GoodsReceiptNote goodsReceiptNote);
    }
}