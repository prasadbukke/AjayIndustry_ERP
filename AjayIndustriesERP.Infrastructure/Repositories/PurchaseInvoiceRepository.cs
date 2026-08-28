/*
============================================================
File: PurchaseInvoiceRepository.cs

Module:
Purchase Invoice / Supplier Bill

Purpose:
Handles Entity Framework Core database operations required
by Purchase Invoice module.

Responsibilities:
- Read Purchase Invoice details.
- Search and paginate Purchase Invoices.
- Load Draft Purchase Invoice for Edit.
- Load deleted Purchase Invoices.
- Load Purchase Orders having GRN receipts.
- Load exact GRN receipt lines.
- Calculate quantity already allocated to Purchase Invoices.
- Load Supplier / Company.
- Validate Supplier Invoice Number duplicate.
- Generate internal Purchase Invoice number.
- Add / Update Purchase Invoice.

Important:
- Business rules remain in PurchaseInvoiceService.
- Purchase Invoice quantity source is exact
  GoodsReceiptNoteItem.ReceivedQuantity.
- Draft + Finalized active Purchase Invoices reserve
  GRN received quantity.
- Deleted Purchase Invoices do not reserve quantity.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class PurchaseInvoiceRepository
        : IPurchaseInvoiceRepository
    {
        #region Fields

        private readonly ApplicationDbContext
            _context;

        #endregion


        #region Constructor

        public PurchaseInvoiceRepository(
            ApplicationDbContext context)
        {
            _context =
                context;
        }

        #endregion


        #region Get By Id

        public async Task<PurchaseInvoice?>
            GetByIdAsync(
                int id)
        {
            return await _context
                .PurchaseInvoices
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)
                .Include(x =>
                    x.PurchaseOrder)
                .Include(x =>
                    x.Supplier)
                .Include(x =>
                    x.Company)
                .Include(x =>
                    x.Items)
                    .ThenInclude(x =>
                        x.PurchaseOrderItem)
                .Include(x =>
                    x.Items)
                    .ThenInclude(x =>
                        x.GoodsReceiptNote)
                .Include(x =>
                    x.Items)
                    .ThenInclude(x =>
                        x.GoodsReceiptNoteItem)
                .Include(x =>
                    x.Items)
                    .ThenInclude(x =>
                        x.Item)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Get For Update

        public async Task<PurchaseInvoice?>
            GetForUpdateAsync(
                int id)
        {
            return await _context
                .PurchaseInvoices
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)
                .Include(x =>
                    x.Items)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Pagination

        public async Task<PagedResult<PurchaseInvoice>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            var query =
                _context
                    .PurchaseInvoices
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted)
                    .OrderByDescending(x =>
                        x.PurchaseInvoiceDate)
                    .ThenByDescending(x =>
                        x.Id);


            var totalRecords =
                await query
                    .CountAsync();


            var items =
                await query
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(
                        pageSize)
                    .ToListAsync();


            return new PagedResult<PurchaseInvoice>
            {
                Items =
                    items,

                PageNumber =
                    pageNumber,

                PageSize =
                    pageSize,

                TotalRecords =
                    totalRecords
            };
        }

        #endregion


        #region Search Pagination

        public async Task<PagedResult<PurchaseInvoice>>
            SearchPagedAsync(
                string searchText,
                int pageNumber,
                int pageSize)
        {
            var search =
                searchText
                    .Trim();


            var query =
                _context
                    .PurchaseInvoices
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        (
                            x.Code.Contains(
                                search)

                            ||

                            x.SupplierInvoiceNumber.Contains(
                                search)

                            ||

                            x.SupplierName.Contains(
                                search)

                            ||

                            x.PurchaseOrderCode.Contains(
                                search)
                        ));


            var totalRecords =
                await query
                    .CountAsync();


            var items =
                await query
                    .OrderByDescending(x =>
                        x.PurchaseInvoiceDate)
                    .ThenByDescending(x =>
                        x.Id)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(
                        pageSize)
                    .ToListAsync();


            return new PagedResult<PurchaseInvoice>
            {
                Items =
                    items,

                PageNumber =
                    pageNumber,

                PageSize =
                    pageSize,

                TotalRecords =
                    totalRecords
            };
        }

        #endregion


        #region Deleted Purchase Invoices

        public async Task<List<PurchaseInvoice>>
            GetDeletedAsync()
        {
            return await _context
                .PurchaseInvoices
                .AsNoTracking()
                .Where(x =>
                    x.IsDeleted)
                .OrderByDescending(x =>
                    x.ModifiedOn)
                .ThenByDescending(x =>
                    x.Id)
                .ToListAsync();
        }


        public async Task<PurchaseInvoice?>
            GetDeletedForUpdateAsync(
                int id)
        {
            return await _context
                .PurchaseInvoices
                .Where(x =>
                    x.Id == id &&
                    x.IsDeleted)
                .Include(x =>
                    x.Items)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Purchase Orders For Invoice

        public async Task<List<PurchaseOrder>>
            GetPurchaseOrdersForInvoiceAsync()
        {
            /*
             * Do NOT depend on PO workflow status here.
             *
             * Current GRN Phase does not yet update
             * PO Status to PartiallyReceived / Received.
             *
             * Actual eligibility source is:
             * active GRN Item with ReceivedQuantity > 0.
             */
            return await _context
                .PurchaseOrders
                .AsNoTracking()
                .Where(po =>
                    !po.IsDeleted &&
                    po.IsActive &&
                    _context
                        .GoodsReceiptNoteItems
                        .Any(grnItem =>
                            !grnItem.IsDeleted &&
                            grnItem.IsActive &&
                            grnItem.ReceivedQuantity > 0 &&
                            !grnItem.GoodsReceiptNote.IsDeleted &&
                            grnItem.GoodsReceiptNote.IsActive &&
                            grnItem.GoodsReceiptNote
                                .PurchaseOrderId ==
                                po.Id))
                .Include(x =>
                    x.Supplier)
                .OrderByDescending(x =>
                    x.PODate)
                .ThenByDescending(x =>
                    x.Id)
                .ToListAsync();
        }

        #endregion


        #region Purchase Order For Invoice

        public async Task<PurchaseOrder?>
            GetPurchaseOrderForInvoiceAsync(
                int purchaseOrderId)
        {
            return await _context
                .PurchaseOrders
                .AsNoTracking()
                .Where(x =>
                    x.Id ==
                        purchaseOrderId &&
                    !x.IsDeleted &&
                    x.IsActive)
                .Include(x =>
                    x.Supplier)
                .Include(x =>
                    x.Company)
                .Include(x =>
                    x.Items)
                    .ThenInclude(x =>
                        x.Item)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Received GRN Items

        public async Task<List<GoodsReceiptNoteItem>>
            GetReceivedGoodsReceiptItemsForInvoiceAsync(
                int purchaseOrderId)
        {
            return await _context
                .GoodsReceiptNoteItems
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive &&
                    x.ReceivedQuantity > 0 &&

                    !x.GoodsReceiptNote.IsDeleted &&
                    x.GoodsReceiptNote.IsActive &&

                    x.GoodsReceiptNote
                        .PurchaseOrderId ==
                        purchaseOrderId)
                .Include(x =>
                    x.GoodsReceiptNote)
                .Include(x =>
                    x.PurchaseOrderItem)
                .Include(x =>
                    x.Item)
                .OrderBy(x =>
                    x.GoodsReceiptNote.GRNDate)
                .ThenBy(x =>
                    x.GoodsReceiptNoteId)
                .ThenBy(x =>
                    x.Id)
                .ToListAsync();
        }

        #endregion


        #region Exact GRN Item

        public async Task<GoodsReceiptNoteItem?>
            GetGoodsReceiptNoteItemForInvoiceAsync(
                int goodsReceiptNoteItemId)
        {
            return await _context
                .GoodsReceiptNoteItems
                .AsNoTracking()
                .Where(x =>
                    x.Id ==
                        goodsReceiptNoteItemId &&

                    !x.IsDeleted &&
                    x.IsActive &&

                    x.ReceivedQuantity > 0 &&

                    !x.GoodsReceiptNote.IsDeleted &&
                    x.GoodsReceiptNote.IsActive)
                .Include(x =>
                    x.GoodsReceiptNote)
                    .ThenInclude(x =>
                        x.PurchaseOrder)
                .Include(x =>
                    x.PurchaseOrderItem)
                .Include(x =>
                    x.Item)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Allocated Purchase Invoice Quantity

        public async Task<decimal>
            GetAllocatedPurchaseInvoiceQuantityAsync(
                int goodsReceiptNoteItemId,
                int? excludePurchaseInvoiceId = null)
        {
            var query =
                _context
                    .PurchaseInvoiceItems
                    .AsNoTracking()
                    .Where(x =>
                        x.GoodsReceiptNoteItemId ==
                            goodsReceiptNoteItemId &&

                        !x.IsDeleted &&
                        x.IsActive &&

                        !x.PurchaseInvoice.IsDeleted &&
                        x.PurchaseInvoice.IsActive &&

                        (
                            x.PurchaseInvoice.Status ==
                                PurchaseInvoiceStatus.Draft

                            ||

                            x.PurchaseInvoice.Status ==
                                PurchaseInvoiceStatus.Finalized
                        ));


            if (excludePurchaseInvoiceId.HasValue)
            {
                query =
                    query.Where(x =>
                        x.PurchaseInvoiceId !=
                            excludePurchaseInvoiceId.Value);
            }


            return await query
                .SumAsync(x =>
                    (decimal?)
                        x.PurchaseInvoiceQuantity)

                ?? 0m;
        }

        #endregion


        #region Supplier

        public async Task<Supplier?>
            GetSupplierForPurchaseInvoiceAsync(
                int supplierId)
        {
            return await _context
                .Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.SupplierId ==
                        supplierId &&
                    !x.IsDeleted);
        }

        #endregion


        #region Supplier Invoice Duplicate

        public async Task<bool>
            SupplierInvoiceNumberExistsAsync(
                int supplierId,
                string supplierInvoiceNumber,
                int? excludePurchaseInvoiceId = null)
        {
            var normalizedInvoiceNumber =
                supplierInvoiceNumber
                    .Trim();


            var query =
                _context
                    .PurchaseInvoices
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive &&

                        x.SupplierId ==
                            supplierId &&

                        x.SupplierInvoiceNumber ==
                            normalizedInvoiceNumber);


            if (excludePurchaseInvoiceId.HasValue)
            {
                query =
                    query.Where(x =>
                        x.Id !=
                            excludePurchaseInvoiceId.Value);
            }


            return await query
                .AnyAsync();
        }

        #endregion


        #region Company

        public async Task<Company?>
            GetCompanyForPurchaseInvoiceAsync()
        {
            return await _context
                .Companies
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive)
                .OrderBy(x =>
                    x.CompanyId)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Last Code

        public async Task<string?>
            GetLastCodeAsync(
                string codePrefix)
        {
            /*
             * Deleted Purchase Invoice numbers are
             * intentionally included.
             *
             * Internal accounting numbers must never
             * be reused.
             */
            return await _context
                .PurchaseInvoices
                .AsNoTracking()
                .Where(x =>
                    x.Code.StartsWith(
                        codePrefix))
                .OrderByDescending(x =>
                    x.Id)
                .Select(x =>
                    x.Code)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Add

        public async Task AddAsync(
            PurchaseInvoice purchaseInvoice)
        {
            await _context
                .PurchaseInvoices
                .AddAsync(
                    purchaseInvoice);


            await _context
                .SaveChangesAsync();
        }

        #endregion


        #region Update

        public async Task UpdateAsync(
            PurchaseInvoice purchaseInvoice)
        {
            _context
                .PurchaseInvoices
                .Update(
                    purchaseInvoice);


            await _context
                .SaveChangesAsync();
        }

        #endregion
    }
}