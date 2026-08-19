// ============================================================
// File: IGoodsReceiptNoteService.cs
// Purpose:
// Defines GRN business operations used by the Web Controller.
//
// Responsibilities:
// - List GRNs
// - Get GRN details
// - Load Purchase Orders
// - Prepare PO for Create
// - Prepare existing GRN for Edit
// - Create GRN
// - Update GRN
//
// Important:
// Uses Domain entities only.
// Web ViewModels remain in the Web layer.
// ============================================================

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IGoodsReceiptNoteService
    {
        Task<List<GoodsReceiptNote>> GetAllAsync();

        Task<GoodsReceiptNote?> GetByIdAsync(
            int id);

        Task<List<PurchaseOrder>>
            GetPurchaseOrdersForReceiptAsync();

        Task<GoodsReceiptNote>
            PrepareForPurchaseOrderAsync(
                int purchaseOrderId);

        Task<GoodsReceiptNote>
            PrepareForEditAsync(
                int id);

        Task<List<GoodsReceiptNote>> SearchAsync(
    string searchText);

        Task<PagedResult<GoodsReceiptNote>> GetPagedAsync(
            int pageNumber,
            int pageSize);

        Task<List<GoodsReceiptNoteItem>>
    GetReceiptHistoryAsync(
        int purchaseOrderId,
        int upToGoodsReceiptNoteId);

        Task<GoodsReceiptNote> CreateAsync(
            GoodsReceiptNote goodsReceiptNote);

        Task<GoodsReceiptNote> UpdateAsync(
            GoodsReceiptNote goodsReceiptNote);
    }
}