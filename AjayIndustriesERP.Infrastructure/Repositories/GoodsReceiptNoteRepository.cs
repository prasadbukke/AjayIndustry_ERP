// ============================================================
// File: GoodsReceiptNoteRepository.cs
// Purpose:
// Handles all Entity Framework Core database operations
// required by the Goods Receipt Note (GRN) module.
//
// Responsibilities:
// - Retrieve GRN list/details
// - Retrieve tracked GRN for Edit
// - Retrieve eligible Purchase Orders
// - Retrieve PO with all item lines
// - Calculate previous receipt quantity
// - Check Supplier Challan duplicate
// - Detect later GRN for transaction-safe Edit
// - Retrieve last GRN code
// - Add / Update GRN
//
// Important:
// Business logic is handled by GoodsReceiptNoteService.
// ============================================================

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;

using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class GoodsReceiptNoteRepository
        : IGoodsReceiptNoteRepository
    {
        private readonly ApplicationDbContext _context;

        public GoodsReceiptNoteRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // GET ALL
        // =====================================================

        public async Task<List<GoodsReceiptNote>> GetAllAsync()
        {
            return await _context.GoodsReceiptNotes
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive)
                .Include(x => x.PurchaseOrder)
                .Include(x => x.Supplier)
                .OrderByDescending(x => x.GRNDate)
                .ThenByDescending(x => x.Id)
                .ToListAsync();
        }

        // ============================================================
        // SEARCH GRN PURCHASE ORDER GROUPS
        // ============================================================
        //
        // Purpose:
        // Searches GRNs by meaningful transaction fields.
        //
        // Search Fields:
        // - GRN Number
        // - Purchase Order Number
        // - Supplier Name
        // - Supplier Challan Number
        //
        // Important:
        // If one GRN matches the search, complete GRN history of that
        // Purchase Order is returned so the Index can continue showing
        // one PO parent row with complete receipt history.
        // ============================================================

        public async Task<List<GoodsReceiptNote>>
            SearchAsync(
                string searchText)
        {
            if (string.IsNullOrWhiteSpace(
                searchText))
            {
                return await GetAllAsync();
            }


            var search =
                searchText
                    .Trim()
                    .ToLower();


            // First find all Purchase Orders having at least
            // one matching GRN / Supplier / Challan.

            var purchaseOrderIds =
                await _context.GoodsReceiptNotes
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive &&
                        (
                            x.Code
                                .ToLower()
                                .Contains(search)

                            ||

                            x.PurchaseOrder.Code
                                .ToLower()
                                .Contains(search)

                            ||

                            x.SupplierName
                                .ToLower()
                                .Contains(search)

                            ||

                            (
                                x.SupplierChallanNumber != null &&
                                x.SupplierChallanNumber
                                    .ToLower()
                                    .Contains(search)
                            )
                        ))
                    .Select(x =>
                        x.PurchaseOrderId)
                    .Distinct()
                    .ToListAsync();


            if (purchaseOrderIds.Count == 0)
            {
                return new List<GoodsReceiptNote>();
            }


            // Return COMPLETE history for matching PO groups.

            return await _context.GoodsReceiptNotes
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive &&
                    purchaseOrderIds.Contains(
                        x.PurchaseOrderId))
                .Include(x =>
                    x.PurchaseOrder)
                .Include(x =>
                    x.Supplier)
                .Include(x =>
                      x.Items)
                .OrderByDescending(x =>
                    x.Id)
                .ToListAsync();
        }


        // ============================================================
        // PAGINATION - ONE PURCHASE ORDER = ONE PAGE RECORD
        // ============================================================
        //
        // Purpose:
        // Performs GRN Index pagination by Purchase Order group.
        //
        // Example:
        //
        // PO-00001
        //   GRN-001
        //   GRN-002
        //
        // PO-00002
        //   GRN-003
        //
        // PageSize = 10 means:
        // 10 Purchase Order groups,
        // NOT 10 individual GRN rows.
        //
        // All GRNs belonging to those selected PO groups are then
        // returned so their complete history can be displayed.
        // ============================================================

        public async Task<PagedResult<GoodsReceiptNote>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            var baseQuery =
                _context.GoodsReceiptNotes
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive);


            // One record per Purchase Order.
            // Latest GRN Id is used to order newest receipt groups first.

            var groupedQuery =
                baseQuery
                    .GroupBy(x =>
                        x.PurchaseOrderId)
                    .Select(group =>
                        new
                        {
                            PurchaseOrderId =
                                group.Key,

                            LatestGrnId =
                                group.Max(x =>
                                    x.Id)
                        });


            var totalRecords =
                await groupedQuery.CountAsync();


            var pageGroups =
                await groupedQuery
                    .OrderByDescending(x =>
                        x.LatestGrnId)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(
                        pageSize)
                    .ToListAsync();


            var purchaseOrderIds =
                pageGroups
                    .Select(x =>
                        x.PurchaseOrderId)
                    .ToList();


            if (purchaseOrderIds.Count == 0)
            {
                return new PagedResult<GoodsReceiptNote>
                {
                    Items =
                        new List<GoodsReceiptNote>(),

                    PageNumber =
                        pageNumber,

                    PageSize =
                        pageSize,

                    TotalRecords =
                        totalRecords
                };
            }


            var goodsReceiptNotes =
                await _context.GoodsReceiptNotes
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive &&
                        purchaseOrderIds.Contains(
                            x.PurchaseOrderId))
                    .Include(x =>
                        x.PurchaseOrder)
                    .Include(x =>
                        x.Supplier)
                    .Include(x =>
                          x.Items)
                    .ToListAsync();


            // Preserve PO group order from pagination query.

            var purchaseOrderSortOrder =
                pageGroups
                    .Select(
                        (group, index) =>
                            new
                            {
                                group.PurchaseOrderId,
                                Index = index
                            })
                    .ToDictionary(
                        x => x.PurchaseOrderId,
                        x => x.Index);


            goodsReceiptNotes =
                goodsReceiptNotes
                    .OrderBy(x =>
                        purchaseOrderSortOrder[
                            x.PurchaseOrderId])
                    .ThenByDescending(x =>
                        x.Id)
                    .ToList();


            return new PagedResult<GoodsReceiptNote>
            {
                Items =
                    goodsReceiptNotes,

                PageNumber =
                    pageNumber,

                PageSize =
                    pageSize,

                TotalRecords =
                    totalRecords
            };
        }

        // ============================================================
        // GET ITEM RECEIPT HISTORY
        // Purpose:
        // Loads all actual receipt transactions for Purchase Order items
        // up to the selected GRN.
        //
        // Used For:
        // GRN Details page to show when and how much quantity of each
        // PO item was received.
        //
        // Important:
        // Only lines with ReceivedQuantity > 0 are returned.
        // Future/later GRNs are not shown when viewing an older GRN.
        // ============================================================

        public async Task<List<GoodsReceiptNoteItem>>
            GetReceiptHistoryAsync(
                int purchaseOrderId,
                int upToGoodsReceiptNoteId)
        {
            return await _context.GoodsReceiptNoteItems
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive &&
                    x.ReceivedQuantity > 0 &&
                    x.GoodsReceiptNote.PurchaseOrderId ==
                        purchaseOrderId &&
                    x.GoodsReceiptNoteId <=
                        upToGoodsReceiptNoteId &&
                    !x.GoodsReceiptNote.IsDeleted &&
                    x.GoodsReceiptNote.IsActive)
                .Include(x =>
                    x.GoodsReceiptNote)
                .OrderBy(x =>
                    x.GoodsReceiptNote.GRNDate)
                .ThenBy(x =>
                    x.GoodsReceiptNoteId)
                .ToListAsync();
        }

        // =====================================================
        // GET DETAILS
        // =====================================================

        public async Task<GoodsReceiptNote?> GetByIdAsync(
            int id)
        {
            return await _context.GoodsReceiptNotes
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)
                .Include(x => x.PurchaseOrder)
                .Include(x => x.Supplier)
                .Include(x => x.Items)
                    .ThenInclude(x => x.PurchaseOrderItem)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Item)
                .FirstOrDefaultAsync();
        }


        // =====================================================
        // GET TRACKED GRN FOR EDIT
        // =====================================================

        public async Task<GoodsReceiptNote?> GetForUpdateAsync(
            int id)
        {
            return await _context.GoodsReceiptNotes
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)
                .Include(x => x.Items)
                .FirstOrDefaultAsync();
        }


        // =====================================================
        // PURCHASE ORDERS AVAILABLE FOR GRN
        // =====================================================

        public async Task<List<PurchaseOrder>>
            GetPurchaseOrdersForReceiptAsync()
        {
            return await _context.PurchaseOrders
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive &&
                    (
                        x.Status ==
                            PurchaseOrderStatus.Sent ||

                        x.Status ==
                            PurchaseOrderStatus.PartiallyReceived
                    ))
                .OrderByDescending(x => x.PODate)
                .ThenByDescending(x => x.Id)
                .ToListAsync();
        }


        // =====================================================
        // GET PURCHASE ORDER WITH ITEMS
        // =====================================================

        public async Task<PurchaseOrder?>
            GetPurchaseOrderForReceiptAsync(
                int purchaseOrderId)
        {
            return await _context.PurchaseOrders
                .AsNoTracking()
                .Where(x =>
                    x.Id == purchaseOrderId &&
                    !x.IsDeleted &&
                    x.IsActive)
                .Include(x => x.Supplier)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Item)
                .FirstOrDefaultAsync();
        }


        // =====================================================
        // PREVIOUSLY RECEIVED QUANTITY
        // =====================================================
        //
        // excludeGoodsReceiptNoteId is used during Edit.
        //
        // Example:
        //
        // GRN-001 = 40
        // Editing GRN-001
        //
        // Previous must be 0, not 40.
        //
        // GRN-001 = 40
        // Creating GRN-002
        //
        // Previous = 40.
        // =====================================================

        public async Task<decimal>
            GetPreviouslyReceivedQuantityAsync(
                int purchaseOrderItemId,
                int? excludeGoodsReceiptNoteId = null)
        {
            var query =
                _context.GoodsReceiptNoteItems
                    .AsNoTracking()
                    .Where(x =>
                        x.PurchaseOrderItemId ==
                            purchaseOrderItemId &&
                        !x.IsDeleted &&
                        x.IsActive &&
                        !x.GoodsReceiptNote.IsDeleted &&
                        x.GoodsReceiptNote.IsActive);


            if (excludeGoodsReceiptNoteId.HasValue)
            {
                query =
                    query.Where(x =>
                        x.GoodsReceiptNoteId !=
                        excludeGoodsReceiptNoteId.Value);
            }


            return await query
                .SumAsync(x =>
                    (decimal?)x.ReceivedQuantity)
                ?? 0;
        }


        // =====================================================
        // SUPPLIER CHALLAN DUPLICATE
        // =====================================================

        public async Task<bool>
            SupplierChallanNumberExistsAsync(
                int supplierId,
                string supplierChallanNumber,
                int? excludeGoodsReceiptNoteId = null)
        {
            var normalizedChallanNumber =
                supplierChallanNumber.Trim();


            var query =
                _context.GoodsReceiptNotes
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive &&
                        x.SupplierId == supplierId &&
                        x.SupplierChallanNumber != null &&
                        x.SupplierChallanNumber ==
                            normalizedChallanNumber);


            if (excludeGoodsReceiptNoteId.HasValue)
            {
                query =
                    query.Where(x =>
                        x.Id !=
                        excludeGoodsReceiptNoteId.Value);
            }


            return await query.AnyAsync();
        }


        // =====================================================
        // CHECK LATER GRN
        // =====================================================
        //
        // Only the latest GRN against a PO can be edited.
        //
        // Editing an older GRN would invalidate receipt history
        // of the later GRN.
        // =====================================================

        public async Task<bool>
            HasLaterGoodsReceiptNoteAsync(
                int purchaseOrderId,
                int goodsReceiptNoteId)
        {
            return await _context.GoodsReceiptNotes
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.IsActive &&
                    x.PurchaseOrderId ==
                        purchaseOrderId &&
                    x.Id >
                        goodsReceiptNoteId);
        }


        // =====================================================
        // LAST GRN CODE
        // =====================================================

        public async Task<string?>
            GetLastGoodsReceiptNoteCodeAsync(
                string codePrefix)
        {
            return await _context.GoodsReceiptNotes
                .AsNoTracking()
                .Where(x =>
                    x.Code.StartsWith(
                        codePrefix))
                .OrderByDescending(x => x.Id)
                .Select(x => x.Code)
                .FirstOrDefaultAsync();
        }


        // =====================================================
        // ADD
        // =====================================================

        public async Task AddAsync(
            GoodsReceiptNote goodsReceiptNote)
        {
            await _context.GoodsReceiptNotes
                .AddAsync(goodsReceiptNote);

            await _context.SaveChangesAsync();
        }


        // =====================================================
        // UPDATE
        // =====================================================

        public async Task UpdateAsync(
            GoodsReceiptNote goodsReceiptNote)
        {
            _context.GoodsReceiptNotes
                .Update(goodsReceiptNote);

            await _context.SaveChangesAsync();
        }
    }
}