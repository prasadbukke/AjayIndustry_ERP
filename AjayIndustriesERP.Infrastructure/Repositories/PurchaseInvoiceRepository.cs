using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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


        // =====================================================
        // PURCHASE INVOICE - READ
        // =====================================================

        #region Purchase Invoice Read

        public async Task<PurchaseInvoice?>
            GetByIdAsync(
                int id)
        {
            return await _context
                .PurchaseInvoices
                .AsNoTracking()
                .Include(x =>
                    x.PurchaseOrder)
                .Include(x =>
                    x.Supplier)
                .Include(x =>
                    x.Company)
                .Include(x =>
                    x.Items)
                    .ThenInclude(x =>
                        x.GoodsReceiptNote)
                .Include(x =>
                    x.Items)
                    .ThenInclude(x =>
                        x.GoodsReceiptNoteItem)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted &&
                    x.IsActive);
        }


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
                        !x.IsDeleted &&
                        x.IsActive);


            return await ToPagedResultAsync(
                query,
                pageNumber,
                pageSize);
        }

        #endregion


        // =====================================================
        // PURCHASE INVOICE - SEARCH / FILTER
        // =====================================================

        #region Purchase Invoice Search

        public async Task<PagedResult<PurchaseInvoice>>
            SearchPagedAsync(
                string? searchText,
                DateTime? purchaseInvoiceDate,
                DateTime? supplierInvoiceDate,
                int pageNumber,
                int pageSize)
        {
            var query =
                _context
                    .PurchaseInvoices
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive);


            // =================================================
            // SEARCH BOX
            //
            // Search supports:
            //
            // - ERP Purchase Invoice Code
            // - Supplier Invoice Number
            // - Supplier Name
            // - Purchase Order Code
            // - Purchase Invoice Date
            // - Supplier Invoice Date
            //
            // If entered text is recognized as a date,
            // both invoice date fields are searched.
            // =================================================

            if (!string.IsNullOrWhiteSpace(
                searchText))
            {
                var search =
                    searchText.Trim();


                // ---------------------------------------------
                // Try to recognize entered value as Date.
                // ---------------------------------------------

                if (TryParseSearchDate(
                    search,
                    out var searchDate))
                {
                    var fromDate =
                        searchDate.Date;


                    var toDate =
                        fromDate.AddDays(1);


                    /*
                     * Match EITHER:
                     *
                     * Purchase Invoice Date
                     * OR
                     * Supplier Invoice Date
                     */
                    query =
                        query.Where(x =>
                            (
                                x.PurchaseInvoiceDate >= fromDate &&
                                x.PurchaseInvoiceDate < toDate
                            )
                            ||
                            (
                                x.SupplierInvoiceDate >= fromDate &&
                                x.SupplierInvoiceDate < toDate
                            ));
                }
                else
                {
                    // -----------------------------------------
                    // Normal text search.
                    // -----------------------------------------

                    query =
                        query.Where(x =>
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
                                search));
                }
            }


            // =================================================
            // OPTIONAL OLD DATE FILTER SUPPORT
            //
            // These are kept for backward compatibility.
            //
            // Index UI will no longer show separate date
            // filters, but an old bookmarked URL containing
            // these parameters will still work.
            // =================================================

            if (purchaseInvoiceDate.HasValue)
            {
                var fromDate =
                    purchaseInvoiceDate.Value.Date;


                var toDate =
                    fromDate.AddDays(1);


                query =
                    query.Where(x =>
                        x.PurchaseInvoiceDate >= fromDate &&
                        x.PurchaseInvoiceDate < toDate);
            }


            if (supplierInvoiceDate.HasValue)
            {
                var fromDate =
                    supplierInvoiceDate.Value.Date;


                var toDate =
                    fromDate.AddDays(1);


                query =
                    query.Where(x =>
                        x.SupplierInvoiceDate >= fromDate &&
                        x.SupplierInvoiceDate < toDate);
            }


            return await ToPagedResultAsync(
                query,
                pageNumber,
                pageSize);
        }

        #endregion


        // =====================================================
        // PURCHASE INVOICE - CREATE
        // =====================================================

        #region Purchase Invoice Create

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


        // =====================================================
        // PURCHASE INVOICE - UPDATE
        // =====================================================

        #region Purchase Invoice Update

        public async Task<PurchaseInvoice?>
            GetForUpdateAsync(
                int id)
        {
            return await _context
                .PurchaseInvoices
                .Include(x =>
                    x.Items)
                    .ThenInclude(x =>
                        x.GoodsReceiptNote)
                .Include(x =>
                    x.Items)
                    .ThenInclude(x =>
                        x.GoodsReceiptNoteItem)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted &&
                    x.IsActive);
        }


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


        // =====================================================
        // DELETED / RESTORE
        // =====================================================

        #region Deleted Purchase Invoices

        public async Task<List<PurchaseInvoice>>
            GetDeletedAsync()
        {
            return await _context
                .PurchaseInvoices
                .IgnoreQueryFilters()
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
                .IgnoreQueryFilters()
                .Include(x =>
                    x.Items)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsDeleted);
        }

        #endregion


        // =====================================================
        // PURCHASE ORDER SOURCE
        // =====================================================

        #region Purchase Order Source

        public async Task<List<PurchaseOrder>>
            GetPurchaseOrdersForInvoiceAsync()
        {
            return await _context
                .PurchaseOrders
                .AsNoTracking()
                .Include(x =>
                    x.Supplier)
                .Include(x =>
                    x.Company)
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive)
                .OrderByDescending(x =>
                    x.CreatedOn)
                .ThenByDescending(x =>
                    x.Id)
                .ToListAsync();
        }


        public async Task<PurchaseOrder?>
            GetPurchaseOrderForInvoiceAsync(
                int purchaseOrderId)
        {
            return await _context
                .PurchaseOrders
                .AsNoTracking()
                .Include(x =>
                    x.Supplier)
                .Include(x =>
                    x.Company)
                .Include(x =>
                    x.Items)
                .FirstOrDefaultAsync(x =>
                    x.Id == purchaseOrderId &&
                    !x.IsDeleted &&
                    x.IsActive);
        }

        #endregion


        // =====================================================
        // GRN SOURCE
        // =====================================================

        #region GRN Source

        public async Task<List<GoodsReceiptNoteItem>>
            GetReceivedGoodsReceiptItemsForInvoiceAsync(
                int purchaseOrderId)
        {
            return await _context
                .GoodsReceiptNoteItems
                .AsNoTracking()
                .Include(x =>
                    x.GoodsReceiptNote)
                .Include(x =>
                    x.PurchaseOrderItem)
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive &&

                    x.ReceivedQuantity > 0m &&

                    x.GoodsReceiptNote != null &&

                    !x.GoodsReceiptNote.IsDeleted &&
                    x.GoodsReceiptNote.IsActive &&

                    x.GoodsReceiptNote.PurchaseOrderId ==
                    purchaseOrderId)
                .OrderBy(x =>
                    x.GoodsReceiptNote.GRNDate)
                .ThenBy(x =>
                    x.GoodsReceiptNoteId)
                .ThenBy(x =>
                    x.Id)
                .ToListAsync();
        }


        public async Task<GoodsReceiptNoteItem?>
            GetGoodsReceiptNoteItemForInvoiceAsync(
                int goodsReceiptNoteItemId)
        {
            return await _context
                .GoodsReceiptNoteItems
                .AsNoTracking()
                .Include(x =>
                    x.GoodsReceiptNote)
                .Include(x =>
                    x.PurchaseOrderItem)
                .FirstOrDefaultAsync(x =>
                    x.Id ==
                    goodsReceiptNoteItemId &&

                    !x.IsDeleted &&
                    x.IsActive &&

                    x.GoodsReceiptNote != null &&

                    !x.GoodsReceiptNote.IsDeleted &&
                    x.GoodsReceiptNote.IsActive);
        }

        #endregion


        // =====================================================
        // QUANTITY RESERVATION
        // =====================================================

        #region Quantity Reservation

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


        // =====================================================
        // SUPPLIER INVOICE NUMBER VALIDATION
        // =====================================================

        #region Supplier Invoice Number Validation

        public async Task<bool>
            SupplierInvoiceNumberExistsAsync(
                int supplierId,
                string supplierInvoiceNumber,
                int? excludePurchaseInvoiceId = null)
        {
            var normalizedNumber =
                supplierInvoiceNumber.Trim();


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
                        normalizedNumber);


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


        // =====================================================
        // PURCHASE INVOICE CODE
        // =====================================================

        #region Purchase Invoice Code

        public async Task<string?>
            GetLastCodeAsync(
                string prefix)
        {
            return await _context
                .PurchaseInvoices
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x =>
                    x.Code.StartsWith(
                        prefix))
                .OrderByDescending(x =>
                    x.Code)
                .Select(x =>
                    x.Code)
                .FirstOrDefaultAsync();
        }

        #endregion


        // =====================================================
        // SEARCH DATE PARSER
        // =====================================================

        #region Search Date Parser

        private static bool TryParseSearchDate(
            string searchText,
            out DateTime date)
        {
            /*
             * Explicit formats prevent values such as
             * Supplier Invoice No. "123456" from being
             * accidentally interpreted as a date.
             */
            var formats =
                new[]
                {
                    "dd-MM-yyyy",
                    "dd/MM/yyyy",
                    "yyyy-MM-dd",
                    "MM/dd/yyyy",
                    "dd-MM-yy",
                    "dd/MM/yy"
                };


            return DateTime.TryParseExact(
                searchText,
                formats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out date);
        }

        #endregion


        // =====================================================
        // PAGINATION
        // =====================================================

        #region Pagination Helper

        private static async Task<PagedResult<PurchaseInvoice>>
            ToPagedResultAsync(
                IQueryable<PurchaseInvoice> query,
                int pageNumber,
                int pageSize)
        {
            if (pageNumber <= 0)
            {
                pageNumber =
                    1;
            }


            if (pageSize <= 0)
            {
                pageSize =
                    10;
            }


            if (pageSize > 100)
            {
                pageSize =
                    100;
            }


            var totalRecords =
                await query.CountAsync();


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
    }
}