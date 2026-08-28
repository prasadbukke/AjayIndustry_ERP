/*
============================================================
File: InvoiceRepository.cs

Module:
Invoice

Purpose:
Implements database operations required by Invoice module.

Responsibilities:
- Read Invoice records.
- Search and paginate Invoices.
- Load eligible Customer Purchase Orders.
- Load Completed Production Jobs for Invoice.
- Calculate already invoiced Production quantity.
- Check PDI / Delivery Challan status.
- Generate next Invoice code source.
- Handle Draft delete / restore support.
- Load Customer and Company snapshot sources.

Important:
- New Invoice source flow:
  Customer PO → Completed Production Job → Invoice.
- Delivery Challan is NOT mandatory for Invoice.
- PDI is NOT mandatory for Invoice.
- PDI / Challan status is checked only for warning workflow.
- Draft + Finalized active Invoices reserve Production quantity.
- Deleted Invoices do not reserve Production quantity.
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
    public class InvoiceRepository
        : IInvoiceRepository
    {
        #region Fields

        private readonly ApplicationDbContext
            _context;

        #endregion


        #region Constructor

        public InvoiceRepository(
            ApplicationDbContext context)
        {
            _context =
                context;
        }

        #endregion


        #region Invoice Read

        public async Task<Invoice?>
            GetByIdAsync(
                int id)
        {
            return await _context
                .Invoices
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)
                .Include(x =>
                    x.Items)
                .FirstOrDefaultAsync();
        }


        public async Task<Invoice?>
            GetForUpdateAsync(
                int id)
        {
            return await _context
                .Invoices
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)
                .Include(x =>
                    x.Items)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Pagination And Search

        public async Task<PagedResult<Invoice>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            var query =
                _context
                    .Invoices
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted);


            var totalRecords =
                await query
                    .CountAsync();


            var invoices =
                await query
                    .OrderByDescending(x =>
                        x.InvoiceDate)
                    .ThenByDescending(x =>
                        x.Id)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(
                        pageSize)
                    .ToListAsync();


            return new PagedResult<Invoice>
            {
                Items =
                    invoices,

                PageNumber =
                    pageNumber,

                PageSize =
                    pageSize,

                TotalRecords =
                    totalRecords
            };
        }


        public async Task<PagedResult<Invoice>>
            SearchPagedAsync(
                string searchText,
                int pageNumber,
                int pageSize)
        {
            var search =
                searchText
                    .Trim()
                    .ToLower();


            var query =
                _context
                    .Invoices
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        (
                            x.Code
                                .ToLower()
                                .Contains(
                                    search)

                            ||

                            x.CustomerName
                                .ToLower()
                                .Contains(
                                    search)
                        ));


            var totalRecords =
                await query
                    .CountAsync();


            var invoices =
                await query
                    .OrderByDescending(x =>
                        x.InvoiceDate)
                    .ThenByDescending(x =>
                        x.Id)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(
                        pageSize)
                    .ToListAsync();


            return new PagedResult<Invoice>
            {
                Items =
                    invoices,

                PageNumber =
                    pageNumber,

                PageSize =
                    pageSize,

                TotalRecords =
                    totalRecords
            };
        }

        #endregion


        #region Customer Purchase Order Source

        public async Task<List<CustomerPurchaseOrder>>
            GetCustomerPurchaseOrdersForInvoiceAsync()
        {
            /*
             * ProductionJob does not directly store
             * CustomerPurchaseOrderId.
             *
             * Actual relationship is:
             *
             * ProductionJob
             *   → CustomerPurchaseOrderItem
             *   → CustomerPurchaseOrder
             */

            var completedJobs =
                await _context
                    .ProductionJobs
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive &&
                        x.Status ==
                            ProductionJobStatus.Completed)
                    .Include(x =>
                        x.CustomerPurchaseOrderItem)
                        .ThenInclude(x =>
                            x.CustomerPurchaseOrder)
                    .ToListAsync();


            var eligibleCustomerPoIds =
                new HashSet<int>();


            foreach (var productionJob
                in completedJobs)
            {
                var customerPo =
                    productionJob
                        .CustomerPurchaseOrderItem
                        ?.CustomerPurchaseOrder;


                if (customerPo == null)
                {
                    continue;
                }


                if (customerPo.IsDeleted ||
                    !customerPo.IsActive)
                {
                    continue;
                }


                if (productionJob.JobQuantity <= 0)
                {
                    continue;
                }


                var allocatedQuantity =
                    await GetAllocatedInvoiceQuantityAsync(
                        productionJob.Id);


                var remainingQuantity =
                    productionJob.JobQuantity -
                    allocatedQuantity;


                if (remainingQuantity <= 0)
                {
                    continue;
                }


                eligibleCustomerPoIds.Add(
                    customerPo.Id);
            }


            if (eligibleCustomerPoIds.Count == 0)
            {
                return new List<CustomerPurchaseOrder>();
            }


            return await _context
                .CustomerPurchaseOrders
                .AsNoTracking()
                .Where(x =>
                    eligibleCustomerPoIds.Contains(
                        x.Id) &&
                    !x.IsDeleted &&
                    x.IsActive)
                .Include(x =>
                    x.Customer)
                .Include(x =>
                    x.Items)
                .OrderByDescending(x =>
                    x.ReceivedDate)
                .ThenByDescending(x =>
                    x.Id)
                .ToListAsync();
        }


        public async Task<CustomerPurchaseOrder?>
            GetCustomerPurchaseOrderForInvoiceAsync(
                int customerPurchaseOrderId)
        {
            return await _context
                .CustomerPurchaseOrders
                .AsNoTracking()
                .Where(x =>
                    x.Id ==
                        customerPurchaseOrderId &&
                    !x.IsDeleted &&
                    x.IsActive)
                .Include(x =>
                    x.Customer)
                .Include(x =>
                    x.Items)
                    .ThenInclude(x =>
                        x.Item)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Completed Production Job Source

        public async Task<List<ProductionJob>>
            GetCompletedProductionJobsForInvoiceAsync(
                int customerPurchaseOrderId)
        {
            /*
             * PDI / Delivery Challan are intentionally
             * NOT part of this eligibility query.
             *
             * Completed Production Job is the trusted
             * Invoice quantity source.
             */

            return await _context
                .ProductionJobs
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive &&
                    x.Status ==
                        ProductionJobStatus.Completed &&
                    x.CustomerPurchaseOrderItem
                        .CustomerPurchaseOrder
                        .Id ==
                        customerPurchaseOrderId)
                .Include(x =>
                    x.Item)
                .Include(x =>
                    x.CustomerPurchaseOrderItem)
                    .ThenInclude(x =>
                        x.CustomerPurchaseOrder)
                .OrderBy(x =>
                    x.Id)
                .ToListAsync();
        }


        public async Task<ProductionJob?>
            GetCompletedProductionJobForInvoiceAsync(
                int productionJobId)
        {
            return await _context
                .ProductionJobs
                .AsNoTracking()
                .Where(x =>
                    x.Id ==
                        productionJobId &&
                    !x.IsDeleted &&
                    x.IsActive &&
                    x.Status ==
                        ProductionJobStatus.Completed)
                .Include(x =>
                    x.Item)
                .Include(x =>
                    x.CustomerPurchaseOrderItem)
                    .ThenInclude(x =>
                        x.CustomerPurchaseOrder)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Production Quantity Allocation

        public async Task<decimal>
            GetAllocatedInvoiceQuantityAsync(
                int productionJobId,
                int? excludeInvoiceId = null)
        {
            /*
             * Active Draft + Finalized Invoices reserve
             * Production Job quantity.
             *
             * Deleted Invoice headers do not reserve.
             * Deleted / inactive Invoice Items do not reserve.
             */

            var query =
                from invoiceItem
                    in _context
                        .InvoiceItems
                        .AsNoTracking()

                join invoice
                    in _context
                        .Invoices
                        .AsNoTracking()

                    on invoiceItem.InvoiceId
                    equals invoice.Id

                where

                    invoiceItem
                        .ProductionJobId
                        .HasValue

                    &&

                    invoiceItem
                        .ProductionJobId
                        .Value ==
                        productionJobId

                    &&

                    !invoiceItem.IsDeleted

                    &&

                    invoiceItem.IsActive

                    &&

                    !invoice.IsDeleted

                    &&

                    invoice.IsActive

                    &&

                    (
                        invoice.Status ==
                            InvoiceStatus.Draft

                        ||

                        invoice.Status ==
                            InvoiceStatus.Finalized
                    )

                select new
                {
                    InvoiceId =
                        invoice.Id,

                    InvoiceQuantity =
                        invoiceItem
                            .InvoiceQuantity
                };


            if (excludeInvoiceId.HasValue)
            {
                query =
                    query.Where(x =>
                        x.InvoiceId !=
                            excludeInvoiceId.Value);
            }


            return await query
                .Select(x =>
                    (decimal?)
                        x.InvoiceQuantity)
                .SumAsync()
                ?? 0m;
        }

        #endregion


        #region PDI Status

        public Task<bool>
            HasFinalizedPdiAsync(
                int productionJobId)
        {
            /*
             * IMPORTANT:
             *
             * PDI is warning-only for Invoice.
             *
             * The actual PDI Entity / DbSet names are not
             * available in the current Invoice source files.
             *
             * Previous guessed names:
             *
             *     _context.Pdis
             *     _context.PdiItems
             *
             * do NOT exist in ApplicationDbContext.
             *
             * Therefore we intentionally do NOT invent
             * another Entity / DbSet name here.
             *
             * Until the actual PDI repository/entity is wired,
             * return false conservatively.
             *
             * Effect:
             * - Invoice remains allowed.
             * - User receives the PDI/DC warning.
             * - confirmSourceWarning = true allows submission.
             *
             * Once the real PDI model is available,
             * only this method needs to be replaced.
             */

            return Task.FromResult(
                false);
        }

        #endregion


        #region Delivery Challan Status

        public async Task<bool>
            HasDeliveryChallanAsync(
                int productionJobId)
        {
            /*
             * Delivery Challan is warning-only.
             *
             * Any active DC Item linked with this
             * Production Job means Challan exists.
             */

            return await _context
                .DeliveryChallanItems
                .AsNoTracking()
                .AnyAsync(x =>
                    x.ProductionJobId ==
                        productionJobId &&
                    !x.IsDeleted &&
                    x.IsActive);
        }

        #endregion


        #region Invoice Code

        public async Task<string?>
            GetLastCodeAsync(
                string prefix)
        {
            /*
             * IsDeleted intentionally NOT filtered.
             *
             * Deleted Invoice Codes must also be
             * considered so numbers are never reused.
             */

            return await _context
                .Invoices
                .AsNoTracking()
                .Where(x =>
                    x.Code.StartsWith(
                        prefix))
                .OrderByDescending(x =>
                    x.Id)
                .Select(x =>
                    x.Code)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Persistence

        public async Task<Invoice>
            AddAsync(
                Invoice invoice)
        {
            await _context
                .Invoices
                .AddAsync(
                    invoice);


            await _context
                .SaveChangesAsync();


            return invoice;
        }


        public async Task<Invoice>
            UpdateAsync(
                Invoice invoice)
        {
            /*
             * Invoice was loaded using tracked
             * GetForUpdateAsync / GetDeletedForUpdateAsync.
             *
             * Do not call DbSet.Update() unnecessarily.
             */

            await _context
                .SaveChangesAsync();


            return invoice;
        }

        #endregion


        #region Deleted Invoice

        public async Task<List<Invoice>>
            GetDeletedAsync()
        {
            return await _context
                .Invoices
                .AsNoTracking()
                .Where(x =>
                    x.IsDeleted)
                .Include(x =>
                    x.Items)
                .OrderByDescending(x =>
                    x.ModifiedOn ??
                    x.CreatedOn)
                .ThenByDescending(x =>
                    x.Id)
                .ToListAsync();
        }


        public async Task<Invoice?>
            GetDeletedForUpdateAsync(
                int id)
        {
            return await _context
                .Invoices
                .Where(x =>
                    x.Id == id &&
                    x.IsDeleted)
                .Include(x =>
                    x.Items)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Snapshot Sources

        public async Task<Customer?>
            GetCustomerForInvoiceAsync(
                int customerId)
        {
            return await _context
                .Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id ==
                        customerId &&
                    !x.IsDeleted);
        }


        public async Task<Company?>
            GetCompanyForInvoiceAsync()
        {
            return await _context
                .Companies
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted)
                .OrderBy(x =>
                    x.CompanyId)
                .FirstOrDefaultAsync();
        }

        #endregion
    }
}