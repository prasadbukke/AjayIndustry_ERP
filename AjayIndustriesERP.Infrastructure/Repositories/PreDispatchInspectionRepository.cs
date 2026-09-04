/*
============================================================
File: PreDispatchInspectionRepository.cs

Purpose:
Provides Entity Framework Core data access for
Pre-Dispatch / Final Inspection Reports.

Production Structure:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Item
        ↓
PDI Report

Responsibilities:
- Retrieve PDI Header, Lines and Observations.
- Search and paginate PDI Reports.
- Load Production Jobs containing completed Production Items.
- Load selected Production Job Item source information.
- Calculate allocated Inspection Quantity Item-wise.
- Retrieve last generated PDI Code.
- Persist PDI Report changes.
- Retrieve deleted PDI Reports for restore.

Important:
- ProductionJob is the parent transaction.
- ProductionJobItem is the actual PDI source.
- One Production Job can contain multiple Items.
- One ProductionJobItem may have multiple PDI Reports.
- Inspection allocation is calculated using
  ProductionJobItemId.
- A Production Job Item becomes eligible for PDI when
  its current ProductionQuantity has been completed.
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
    public class PreDispatchInspectionRepository
        : IPreDispatchInspectionRepository
    {
        #region Fields

        private readonly ApplicationDbContext
            _context;

        #endregion


        #region Constructor

        public PreDispatchInspectionRepository(
            ApplicationDbContext context)
        {
            _context =
                context;
        }

        #endregion


        #region Read Operations

        public async Task<PreDispatchInspection?>
            GetByIdAsync(
                int id)
        {
            return await _context
                .PreDispatchInspections
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)

                .Include(x =>
                    x.ProductionJob)

                .Include(x =>
                    x.ProductionJobItem)

                .Include(x =>
                    x.Lines
                        .Where(line =>
                            !line.IsDeleted))
                    .ThenInclude(line =>
                        line.Observations
                            .Where(observation =>
                                !observation.IsDeleted))

                .FirstOrDefaultAsync();
        }


        public async Task<PreDispatchInspection?>
            GetForUpdateAsync(
                int id)
        {
            return await _context
                .PreDispatchInspections
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)

                .Include(x =>
                    x.ProductionJob)

                .Include(x =>
                    x.ProductionJobItem)

                .Include(x =>
                    x.Lines)
                    .ThenInclude(line =>
                        line.Observations)

                .FirstOrDefaultAsync();
        }

        #endregion


        #region Search And Pagination

        public async Task<PagedResult<PreDispatchInspection>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            #region Query

            var query =
                _context
                    .PreDispatchInspections
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted);

            #endregion


            #region Record Count

            var totalRecords =
                await query
                    .CountAsync();

            #endregion


            #region Pagination

            var reports =
                await query
                    .OrderByDescending(x =>
                        x.InspectionDate)
                    .ThenByDescending(x =>
                        x.Id)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(
                        pageSize)
                    .ToListAsync();

            #endregion


            #region Result

            return new PagedResult<PreDispatchInspection>
            {
                Items =
                    reports,

                PageNumber =
                    pageNumber,

                PageSize =
                    pageSize,

                TotalRecords =
                    totalRecords
            };

            #endregion
        }


        public async Task<PagedResult<PreDispatchInspection>>
            SearchPagedAsync(
                string searchText,
                int pageNumber,
                int pageSize)
        {
            #region Normalize Search

            var search =
                searchText
                    .Trim()
                    .ToLower();

            #endregion


            #region Search Query

            var query =
                _context
                    .PreDispatchInspections
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted
                        &&
                        (
                            // PDI Code
                            x.Code
                                .ToLower()
                                .Contains(search)

                            ||

                            // Production Job
                            x.ProductionJobCode
                                .ToLower()
                                .Contains(search)

                            ||

                            // Customer
                            x.CustomerName
                                .ToLower()
                                .Contains(search)

                            ||

                            // Customer PO Code
                            x.CustomerPurchaseOrderCode
                                .ToLower()
                                .Contains(search)

                            ||

                            // Customer PO Number
                            x.CustomerPurchaseOrderNumber
                                .ToLower()
                                .Contains(search)

                            ||

                            // Customer Item Code
                            (
                                x.CustomerItemCode != null
                                &&
                                x.CustomerItemCode
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            // Item Code
                            x.ItemCode
                                .ToLower()
                                .Contains(search)

                            ||

                            // Item Name
                            x.ItemName
                                .ToLower()
                                .Contains(search)

                            ||

                            // Part Number
                            (
                                x.PartNumber != null
                                &&
                                x.PartNumber
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            // Workshop Drawing
                            (
                                x.WorkshopDrawingNumber != null
                                &&
                                x.WorkshopDrawingNumber
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            // Customer Drawing
                            (
                                x.CustomerDrawingNumber != null
                                &&
                                x.CustomerDrawingNumber
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            // Invoice Number
                            (
                                x.InvoiceNumber != null
                                &&
                                x.InvoiceNumber
                                    .ToLower()
                                    .Contains(search)
                            )
                        ));

            #endregion


            #region Record Count

            var totalRecords =
                await query
                    .CountAsync();

            #endregion


            #region Pagination

            var reports =
                await query
                    .OrderByDescending(x =>
                        x.InspectionDate)
                    .ThenByDescending(x =>
                        x.Id)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(
                        pageSize)
                    .ToListAsync();

            #endregion


            #region Result

            return new PagedResult<PreDispatchInspection>
            {
                Items =
                    reports,

                PageNumber =
                    pageNumber,

                PageSize =
                    pageSize,

                TotalRecords =
                    totalRecords
            };

            #endregion
        }

        #endregion


        #region Production Job Source

        /*
         * Returns Production Jobs which contain at least
         * one active ProductionJobItem whose CURRENT
         * Production Quantity has been completed.
         *
         * Important:
         *
         * Parent Production Job does not have to be
         * ProductionJobStatus.Completed.
         *
         * Example:
         *
         * PO has:
         *
         * Item A - current production completed
         * Item B - still in production
         *
         * Item A must still be available for PDI.
         */

        public async Task<List<ProductionJob>>
            GetProductionJobsForInspectionAsync()
        {
            return await _context
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
                            item.ProductionQuantity))

                // =========================================
                // CUSTOMER PO HEADER
                // =========================================

                .Include(job =>
                    job.CustomerPurchaseOrder)

                // =========================================
                // PRODUCTION JOB ITEMS
                // =========================================

                .Include(job =>
                    job.Items)
                    .ThenInclude(item =>
                        item.CustomerPurchaseOrderItem)

                // =========================================
                // ITEM MASTER
                // =========================================

                .Include(job =>
                    job.Items)
                    .ThenInclude(item =>
                        item.Item)

                // =========================================
                // ORDER
                // =========================================

                .OrderByDescending(job =>
                    job.ModifiedOn ??
                    job.CreatedOn)

                .ThenByDescending(job =>
                    job.Id)

                .ToListAsync();
        }


        /*
         * IMPORTANT:
         *
         * Existing interface method name is retained.
         *
         * The supplied ID represents ProductionJobItemId,
         * not ProductionJobId.
         *
         * Return type remains ProductionJob so existing
         * interface structure does not have to change.
         *
         * The selected ProductionJobItem can be obtained
         * from:
         *
         * productionJob.Items
         *     .First(x => x.Id == productionJobItemId)
         */

        public async Task<ProductionJob?>
            GetProductionJobForInspectionAsync(
                int productionJobItemId)
        {
            return await _context
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
                        item.Id ==
                            productionJobItemId
                        &&
                        !item.IsDeleted
                        &&
                        item.IsActive
                        &&
                        item.ProductionQuantity > 0m
                        &&
                        item.CompletedQuantity >=
                            item.ProductionQuantity))

                // =========================================
                // CUSTOMER PO HEADER
                // =========================================

                .Include(job =>
                    job.CustomerPurchaseOrder)

                // =========================================
                // CUSTOMER PO ITEM
                // =========================================

                .Include(job =>
                    job.Items)
                    .ThenInclude(item =>
                        item.CustomerPurchaseOrderItem)

                // =========================================
                // ITEM + MAIN UOM
                // =========================================

                .Include(job =>
                    job.Items)
                    .ThenInclude(item =>
                        item.Item)
                    .ThenInclude(item =>
                        item.Uom)

                // =========================================
                // ITEM SPECIFICATIONS
                // + SPECIFICATION MASTER
                // =========================================

                .Include(job =>
                    job.Items)
                    .ThenInclude(item =>
                        item.Item)
                    .ThenInclude(item =>
                        item.ItemSpecifications)
                    .ThenInclude(specification =>
                        specification.Specification)

                // =========================================
                // ITEM SPECIFICATIONS
                // + SPECIFICATION UOM
                // =========================================

                .Include(job =>
                    job.Items)
                    .ThenInclude(item =>
                        item.Item)
                    .ThenInclude(item =>
                        item.ItemSpecifications)
                    .ThenInclude(specification =>
                        specification.Uom)

                // =========================================
                // WORKSHOP DRAWINGS
                // =========================================

                .Include(job =>
                    job.Items)
                    .ThenInclude(item =>
                        item.Item)
                    .ThenInclude(item =>
                        item.Drawings)

                .FirstOrDefaultAsync();
        }


        /*
         * Allocation is ProductionJobItem-wise.
         *
         * Example:
         *
         * Completed Quantity = 100
         *
         * PDI 1 = 60
         * PDI 2 = 25
         *
         * Remaining PDI Quantity = 15
         */

        public async Task<decimal>
            GetAllocatedInspectionQuantityAsync(
                int productionJobItemId,
                int? excludePreDispatchInspectionId = null)
        {
            return await _context
                .PreDispatchInspections
                .AsNoTracking()
                .Where(pdi =>
                    pdi.ProductionJobItemId ==
                        productionJobItemId
                    &&
                    !pdi.IsDeleted
                    &&
                    (
                        !excludePreDispatchInspectionId
                            .HasValue
                        ||
                        pdi.Id !=
                            excludePreDispatchInspectionId
                                .Value
                    ))
                .Select(pdi =>
                    (decimal?)
                        pdi.InspectionQuantity)
                .SumAsync()
                ?? 0m;
        }

        #endregion


        #region PDI Code

        public async Task<string?>
            GetLastCodeAsync(
                string prefix)
        {
            return await _context
                .PreDispatchInspections
                .AsNoTracking()

                /*
                 * Deleted PDI Reports are intentionally
                 * included.
                 *
                 * Generated Inspection document numbers
                 * must never be reused.
                 */

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

        public async Task AddAsync(
            PreDispatchInspection
                preDispatchInspection)
        {
            await _context
                .PreDispatchInspections
                .AddAsync(
                    preDispatchInspection);


            await _context
                .SaveChangesAsync();
        }


        public async Task UpdateAsync(
            PreDispatchInspection
                preDispatchInspection)
        {
            /*
             * Entity is already tracked because
             * Update / Finalize / Delete operations load
             * it using GetForUpdateAsync or
             * GetDeletedForUpdateAsync.
             */

            await _context
                .SaveChangesAsync();
        }

        #endregion


        #region Deleted Reports

        public async Task<List<PreDispatchInspection>>
            GetDeletedAsync()
        {
            return await _context
                .PreDispatchInspections
                .AsNoTracking()
                .Where(x =>
                    x.IsDeleted)
                .OrderByDescending(x =>
                    x.ModifiedOn ??
                    x.CreatedOn)
                .ThenByDescending(x =>
                    x.Id)
                .ToListAsync();
        }


        public async Task<PreDispatchInspection?>
            GetDeletedForUpdateAsync(
                int id)
        {
            return await _context
                .PreDispatchInspections
                .Where(x =>
                    x.Id == id &&
                    x.IsDeleted)

                .Include(x =>
                    x.ProductionJob)

                .Include(x =>
                    x.ProductionJobItem)

                .Include(x =>
                    x.Lines)
                    .ThenInclude(line =>
                        line.Observations)

                .FirstOrDefaultAsync();
        }

        #endregion
    }
}