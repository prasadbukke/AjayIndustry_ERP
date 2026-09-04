/*
============================================================
File: InvoiceRepository.cs

Module:
Invoice

Purpose:
Implements database operations required by Invoice module.

Current Production Structure:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Items

Invoice Item Source Identity:

ProductionJobId
        +
CustomerPurchaseOrderItemId

Responsibilities:
- Read Invoice records.
- Search and paginate Invoices.
- Load eligible Customer Purchase Orders.
- Load Production Jobs containing completed Production Items.
- Calculate already invoiced quantity Item-wise.
- Check PDI status Item-wise.
- Check Delivery Challan status Item-wise.
- Generate next Invoice code source.
- Handle Draft delete / restore support.
- Load Customer and Company snapshot sources.

Important:
- One Production Job may contain multiple Production Items.
- Quantity allocation MUST NOT be calculated only by
  ProductionJobId.
- PDI is NOT mandatory for Invoice.
- Delivery Challan is NOT mandatory for Invoice.
- PDI / Challan status is warning-only.
- Draft + Finalized active Invoices reserve quantity.
- Deleted Invoices do not reserve quantity.
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
                        (pageNumber - 1)
                        *
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
                        !x.IsDeleted
                        &&
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
                        (pageNumber - 1)
                        *
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
             * A Customer PO is eligible when:
             *
             * - It has an active Production Job.
             * - At least one ProductionJobItem has completed
             *   its current Production plan.
             * - Some completed quantity remains uninvoiced.
             *
             * PDI / Delivery Challan are NOT used here.
             */

            var productionJobs =
                await _context
                    .ProductionJobs
                    .AsNoTracking()
                    .Where(job =>
                        !job.IsDeleted
                        &&
                        job.IsActive
                        &&
                        job.Status !=
                            ProductionJobStatus.Cancelled
                        &&
                        job.Items.Any(item =>
                            !item.IsDeleted
                            &&
                            item.IsActive
                            &&
                            item.ProductionQuantity > 0m
                            &&
                            item.CompletedQuantity >=
                                item.ProductionQuantity
                            &&
                            item.CompletedQuantity > 0m))
                    .Include(job =>
                        job.CustomerPurchaseOrder)
                    .Include(job =>
                        job.Items)
                    .ToListAsync();


            var eligibleCustomerPoIds =
                new HashSet<int>();


            foreach (var productionJob
                     in productionJobs)
            {
                var customerPurchaseOrder =
                    productionJob
                        .CustomerPurchaseOrder;


                if (customerPurchaseOrder == null)
                {
                    continue;
                }


                if (
                    customerPurchaseOrder.IsDeleted
                    ||
                    !customerPurchaseOrder.IsActive
                )
                {
                    continue;
                }


                var completedItems =
                    productionJob
                        .Items
                        .Where(item =>
                            !item.IsDeleted
                            &&
                            item.IsActive
                            &&
                            item.ProductionQuantity > 0m
                            &&
                            item.CompletedQuantity >=
                                item.ProductionQuantity
                            &&
                            item.CompletedQuantity > 0m)
                        .ToList();


                foreach (var productionJobItem
                         in completedItems)
                {
                    var allocatedQuantity =
                        await GetAllocatedInvoiceQuantityAsync(
                            productionJob.Id,
                            productionJobItem
                                .CustomerPurchaseOrderItemId);


                    var availableQuantity =
                        productionJobItem
                            .CompletedQuantity
                        -
                        allocatedQuantity;


                    if (availableQuantity <= 0m)
                    {
                        continue;
                    }


                    eligibleCustomerPoIds.Add(
                        productionJob
                            .CustomerPurchaseOrderId);


                    break;
                }
            }


            if (eligibleCustomerPoIds.Count == 0)
            {
                return
                    new List<CustomerPurchaseOrder>();
            }


            return await _context
                .CustomerPurchaseOrders
                .AsNoTracking()
                .Where(po =>
                    eligibleCustomerPoIds.Contains(
                        po.Id)
                    &&
                    !po.IsDeleted
                    &&
                    po.IsActive)
                .Include(po =>
                    po.Customer)
                .Include(po =>
                    po.Items)
                    .ThenInclude(item =>
                        item.Item)
                .OrderByDescending(po =>
                    po.ReceivedDate)
                .ThenByDescending(po =>
                    po.Id)
                .ToListAsync();
        }


        public async Task<CustomerPurchaseOrder?>
            GetCustomerPurchaseOrderForInvoiceAsync(
                int customerPurchaseOrderId)
        {
            return await _context
                .CustomerPurchaseOrders
                .AsNoTracking()
                .Where(po =>
                    po.Id ==
                        customerPurchaseOrderId
                    &&
                    !po.IsDeleted
                    &&
                    po.IsActive)
                .Include(po =>
                    po.Customer)
                .Include(po =>
                    po.Items)
                    .ThenInclude(item =>
                        item.Item)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Production Source

        public async Task<List<ProductionJob>>
            GetCompletedProductionJobsForInvoiceAsync(
                int customerPurchaseOrderId)
        {
            /*
             * Method name is retained for compatibility.
             *
             * Parent Production Job itself does NOT have
             * to be fully Completed.
             *
             * A ProductionJobItem is eligible when its
             * current ProductionQuantity has been completed.
             */

            return await _context
                .ProductionJobs
                .AsNoTracking()
                .Where(job =>
                    job.CustomerPurchaseOrderId ==
                        customerPurchaseOrderId
                    &&
                    !job.IsDeleted
                    &&
                    job.IsActive
                    &&
                    job.Status !=
                        ProductionJobStatus.Cancelled
                    &&
                    job.Items.Any(item =>
                        !item.IsDeleted
                        &&
                        item.IsActive
                        &&
                        item.ProductionQuantity > 0m
                        &&
                        item.CompletedQuantity >=
                            item.ProductionQuantity
                        &&
                        item.CompletedQuantity > 0m))

                // =========================================
                // CUSTOMER PO
                // =========================================

                .Include(job =>
                    job.CustomerPurchaseOrder)

                // =========================================
                // PRODUCTION ITEMS + CUSTOMER PO ITEM
                // =========================================

                .Include(job =>
                    job.Items)
                    .ThenInclude(item =>
                        item.CustomerPurchaseOrderItem)

                // =========================================
                // PRODUCTION ITEMS + ITEM MASTER
                // =========================================

                .Include(job =>
                    job.Items)
                    .ThenInclude(item =>
                        item.Item)

                .OrderBy(job =>
                    job.Id)
                .ToListAsync();
        }


        public async Task<ProductionJob?>
            GetCompletedProductionJobForInvoiceAsync(
                int productionJobId)
        {
            return await _context
                .ProductionJobs
                .AsNoTracking()
                .Where(job =>
                    job.Id ==
                        productionJobId
                    &&
                    !job.IsDeleted
                    &&
                    job.IsActive
                    &&
                    job.Status !=
                        ProductionJobStatus.Cancelled
                    &&
                    job.Items.Any(item =>
                        !item.IsDeleted
                        &&
                        item.IsActive
                        &&
                        item.ProductionQuantity > 0m
                        &&
                        item.CompletedQuantity >=
                            item.ProductionQuantity
                        &&
                        item.CompletedQuantity > 0m))

                // =========================================
                // CUSTOMER PO
                // =========================================

                .Include(job =>
                    job.CustomerPurchaseOrder)

                // =========================================
                // PRODUCTION ITEMS + CUSTOMER PO ITEM
                // =========================================

                .Include(job =>
                    job.Items)
                    .ThenInclude(item =>
                        item.CustomerPurchaseOrderItem)

                // =========================================
                // PRODUCTION ITEMS + ITEM MASTER
                // =========================================

                .Include(job =>
                    job.Items)
                    .ThenInclude(item =>
                        item.Item)

                .FirstOrDefaultAsync();
        }

        #endregion


        #region Invoice Quantity Allocation

        public async Task<decimal>
            GetAllocatedInvoiceQuantityAsync(
                int productionJobId,
                int customerPurchaseOrderItemId,
                int? excludeInvoiceId = null)
        {
            /*
             * CRITICAL:
             *
             * Allocation is calculated using BOTH:
             *
             * ProductionJobId
             * +
             * CustomerPurchaseOrderItemId
             *
             * This prevents quantities of multiple Items
             * under the same Production Job from mixing.
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

                    invoiceItem
                        .CustomerPurchaseOrderItemId ==
                        customerPurchaseOrderItemId

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


        #region PDI Warning Status

        public async Task<bool>
            HasFinalizedPdiAsync(
                int productionJobId,
                int customerPurchaseOrderItemId)
        {
            /*
             * PDI is warning-only.
             *
             * Check the exact Production Item source.
             *
             * PreDispatchInspection stores:
             *
             * ProductionJobId
             * CustomerPurchaseOrderItemId
             * ProductionJobItemId
             *
             * The first two already uniquely identify
             * the Invoice Item source.
             */

            return await _context
                .PreDispatchInspections
                .AsNoTracking()
                .AnyAsync(pdi =>
                    pdi.ProductionJobId ==
                        productionJobId
                    &&
                    pdi.CustomerPurchaseOrderItemId ==
                        customerPurchaseOrderItemId
                    &&
                    !pdi.IsDeleted
                    &&
                    pdi.IsActive
                    &&
                    pdi.Status ==
                        PreDispatchInspectionStatus.Finalized);
        }

        #endregion


        #region Delivery Challan Warning Status

        public async Task<bool>
            HasDeliveryChallanAsync(
                int productionJobId,
                int customerPurchaseOrderItemId)
        {
            /*
             * Delivery Challan is warning-only.
             *
             * Check the exact Production Item using:
             *
             * ProductionJobId
             * +
             * CustomerPurchaseOrderItemId
             */

            return await _context
                .DeliveryChallanItems
                .AsNoTracking()
                .AnyAsync(item =>
                    item.ProductionJobId ==
                        productionJobId
                    &&
                    item.CustomerPurchaseOrderItemId ==
                        customerPurchaseOrderItemId
                    &&
                    !item.IsDeleted
                    &&
                    item.IsActive);
        }

        #endregion


        #region Invoice Code

        public async Task<string?>
            GetLastCodeAsync(
                string prefix)
        {
            /*
             * Deleted Invoice Codes are intentionally
             * included.
             *
             * Invoice document numbers must never
             * be reused.
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
             * Invoice is already tracked when loaded using
             * GetForUpdateAsync / GetDeletedForUpdateAsync.
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
                    x.ModifiedOn
                    ??
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
                    x.Id == id
                    &&
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
                .FirstOrDefaultAsync(customer =>
                    customer.Id ==
                        customerId
                    &&
                    !customer.IsDeleted);
        }


        public async Task<Company?>
            GetCompanyForInvoiceAsync()
        {
            return await _context
                .Companies
                .AsNoTracking()
                .Where(company =>
                    !company.IsDeleted)
                .OrderBy(company =>
                    company.CompanyId)
                .FirstOrDefaultAsync();
        }

        #endregion
    }
}