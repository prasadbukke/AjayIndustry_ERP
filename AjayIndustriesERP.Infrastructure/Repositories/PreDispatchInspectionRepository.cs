/*
============================================================
File: PreDispatchInspectionRepository.cs

Purpose:
Provides Entity Framework Core data access for
Pre-Dispatch / Final Inspection Reports.

Responsibilities:
- Retrieve PDI Header, Lines and Observations.
- Search and paginate PDI Reports.
- Load Completed Production Jobs for Inspection.
- Load complete Production Job source information.
- Calculate allocated Inspection Quantity.
- Retrieve last generated PDI Code.
- Persist PDI Report changes.
- Retrieve deleted PDI Reports for restore.

Important:
- Business rules belong in PreDispatchInspectionService.
- Normal queries exclude soft-deleted PDI Reports.
- PDI Code lookup includes deleted Reports because
  document numbers must never be reused.
- Customer Drawing is NOT queried here.
  It belongs to the Customer Drawing module and will be
  resolved through ICustomerDrawingService.
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
                    x.Lines
                        .Where(line =>
                            !line.IsDeleted))
                    .ThenInclude(x =>
                        x.Observations
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
                    x.Lines)
                    .ThenInclude(x =>
                        x.Observations)
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
                                x.CustomerItemCode != null &&
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
                                x.PartNumber != null &&
                                x.PartNumber
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            // Workshop Drawing
                            (
                                x.WorkshopDrawingNumber != null &&
                                x.WorkshopDrawingNumber
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            // Customer Drawing
                            (
                                x.CustomerDrawingNumber != null &&
                                x.CustomerDrawingNumber
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            // Invoice Number
                            (
                                x.InvoiceNumber != null &&
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

        public async Task<List<ProductionJob>>
            GetProductionJobsForInspectionAsync()
        {
            return await _context
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
                .Include(x =>
                    x.Item)
                    .ThenInclude(x =>
                        x.Uom)
                .OrderByDescending(x =>
                    x.CompletedOn)
                .ThenByDescending(x =>
                    x.Id)
                .ToListAsync();
        }


        public async Task<ProductionJob?>
            GetProductionJobForInspectionAsync(
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

                // =========================================
                // CUSTOMER PO SOURCE
                // =========================================

                .Include(x =>
                    x.CustomerPurchaseOrderItem)
                    .ThenInclude(x =>
                        x.CustomerPurchaseOrder)

                // =========================================
                // ITEM + MAIN UOM
                // =========================================

                .Include(x =>
                    x.Item)
                    .ThenInclude(x =>
                        x.Uom)

                // =========================================
                // ITEM SPECIFICATIONS
                // =========================================

                .Include(x =>
                    x.Item)
                    .ThenInclude(x =>
                        x.ItemSpecifications
                            .Where(specification =>
                                !specification.IsDeleted &&
                                specification.IsActive))
                    .ThenInclude(x =>
                        x.Specification)

                .Include(x =>
                    x.Item)
                    .ThenInclude(x =>
                        x.ItemSpecifications
                            .Where(specification =>
                                !specification.IsDeleted &&
                                specification.IsActive))
                    .ThenInclude(x =>
                        x.Uom)

                // =========================================
                // CURRENT WORKSHOP DRAWING
                // =========================================

                .Include(x =>
                    x.Item)
                    .ThenInclude(x =>
                        x.Drawings
                            .Where(drawing =>
                                !drawing.IsDeleted &&
                                drawing.IsActive))

                .FirstOrDefaultAsync();
        }


        public async Task<decimal>
            GetAllocatedInspectionQuantityAsync(
                int productionJobId,
                int? excludePreDispatchInspectionId = null)
        {
            return await _context
                .PreDispatchInspections
                .AsNoTracking()
                .Where(x =>
                    x.ProductionJobId ==
                        productionJobId
                    &&
                    !x.IsDeleted
                    &&
                    (
                        !excludePreDispatchInspectionId.HasValue
                        ||
                        x.Id !=
                            excludePreDispatchInspectionId.Value
                    ))
                .Select(x =>
                    (decimal?)
                        x.InspectionQuantity)
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

                /*
                 * IsDeleted intentionally NOT filtered.
                 *
                 * PDI / Inspection Report numbers are
                 * permanent document numbers and must
                 * never be reused after deletion.
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
             * Entity is already tracked by the scoped
             * ApplicationDbContext.
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
                    x.Lines)
                    .ThenInclude(x =>
                        x.Observations)
                .FirstOrDefaultAsync();
        }

        #endregion
    }
}