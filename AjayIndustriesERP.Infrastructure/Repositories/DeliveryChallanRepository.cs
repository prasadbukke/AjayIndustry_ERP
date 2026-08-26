/*
============================================================
File: DeliveryChallanRepository.cs

Purpose:
Provides Entity Framework Core data access for
Delivery Challan / Dispatch documents.

Responsibilities:
- Retrieve Delivery Challan Header and Items.
- Search and paginate Delivery Challans.
- Load Finalized PDI Reports for dispatch.
- Calculate quantity already allocated to Challans.
- Retrieve last Delivery Challan Code.
- Persist Delivery Challan changes.
- Retrieve deleted Challans for restore.

Important:
- Business rules belong in DeliveryChallanService.
- Main Challan queries exclude soft-deleted records.
- Draft and Finalized active Challans both reserve
  Dispatch Quantity.
- Deleted Challan quantities do not reserve dispatch.
- Deleted Challan Codes are intentionally considered
  during Code generation so numbers are never reused.
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
    public class DeliveryChallanRepository
        : IDeliveryChallanRepository
    {
        #region Fields

        private readonly ApplicationDbContext
            _context;

        #endregion


        #region Constructor

        public DeliveryChallanRepository(
            ApplicationDbContext context)
        {
            _context =
                context;
        }

        #endregion


        #region Read Operations

        public async Task<DeliveryChallan?>
            GetByIdAsync(
                int id)
        {
            return await _context
                .DeliveryChallans
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)
                .Include(x =>
                    x.Items
                        .Where(item =>
                            !item.IsDeleted))
                    .ThenInclude(x =>
                        x.PreDispatchInspection)
                .FirstOrDefaultAsync();
        }


        public async Task<DeliveryChallan?>
            GetForUpdateAsync(
                int id)
        {
            return await _context
                .DeliveryChallans
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)
                .Include(x =>
                    x.Items)
                    .ThenInclude(x =>
                        x.PreDispatchInspection)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Search And Pagination

        public async Task<PagedResult<DeliveryChallan>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            #region Query

            var query =
                _context
                    .DeliveryChallans
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

            var challans =
                await query
                    .Include(x =>
                        x.Items
                            .Where(item =>
                                !item.IsDeleted))
                    .OrderByDescending(x =>
                        x.ChallanDate)
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

            return new PagedResult<DeliveryChallan>
            {
                Items =
                    challans,

                PageNumber =
                    pageNumber,

                PageSize =
                    pageSize,

                TotalRecords =
                    totalRecords
            };

            #endregion
        }


        public async Task<PagedResult<DeliveryChallan>>
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
                    .DeliveryChallans
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted
                        &&
                        (
                            // Challan Code
                            x.Code
                                .ToLower()
                                .Contains(search)

                            ||

                            // Customer
                            x.CustomerName
                                .ToLower()
                                .Contains(search)

                            ||

                            // Transporter
                            (
                                x.TransporterName != null &&
                                x.TransporterName
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            // Vehicle
                            (
                                x.VehicleNumber != null &&
                                x.VehicleNumber
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            // LR / Transport Reference
                            (
                                x.TransportReference != null &&
                                x.TransportReference
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            // Dispatch From
                            (
                                x.DispatchFrom != null &&
                                x.DispatchFrom
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            // Destination
                            (
                                x.Destination != null &&
                                x.Destination
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            // Challan Item Source Information
                            x.Items.Any(item =>
                                !item.IsDeleted
                                &&
                                (
                                    // PDI Report
                                    item.PreDispatchInspectionCode
                                        .ToLower()
                                        .Contains(search)

                                    ||

                                    // Production Job
                                    item.ProductionJobCode
                                        .ToLower()
                                        .Contains(search)

                                    ||

                                    // Customer PO
                                    item.CustomerPurchaseOrderCode
                                        .ToLower()
                                        .Contains(search)

                                    ||

                                    item.CustomerPurchaseOrderNumber
                                        .ToLower()
                                        .Contains(search)

                                    ||

                                    // Customer Item Code
                                    (
                                        item.CustomerItemCode != null &&
                                        item.CustomerItemCode
                                            .ToLower()
                                            .Contains(search)
                                    )

                                    ||

                                    // Item Code
                                    item.ItemCode
                                        .ToLower()
                                        .Contains(search)

                                        ||

// Product ID
(
    item.ProductReference != null &&
    item.ProductReference
        .ToLower()
        .Contains(search)
)

                                    ||

                                    // Item Name
                                    item.ItemName
                                        .ToLower()
                                        .Contains(search)

                                    ||

                                    // Part Number
                                    (
                                        item.PartNumber != null &&
                                        item.PartNumber
                                            .ToLower()
                                            .Contains(search)
                                    )

                                    ||

                                    // Customer Drawing
                                    (
                                        item.CustomerDrawingNumber != null &&
                                        item.CustomerDrawingNumber
                                            .ToLower()
                                            .Contains(search)
                                    )
                                ))
                        ));

            #endregion


            #region Record Count

            var totalRecords =
                await query
                    .CountAsync();

            #endregion


            #region Pagination

            var challans =
                await query
                    .Include(x =>
                        x.Items
                            .Where(item =>
                                !item.IsDeleted))
                    .OrderByDescending(x =>
                        x.ChallanDate)
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

            return new PagedResult<DeliveryChallan>
            {
                Items =
                    challans,

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


        #region Finalized PDI Source

        public async Task<List<PreDispatchInspection>>
            GetFinalizedPdisForDispatchAsync()
        {
            return await _context
                .PreDispatchInspections
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive &&
                    x.Status ==
                        PreDispatchInspectionStatus.Finalized &&
                    x.AcceptedQuantity > 0)
                .OrderByDescending(x =>
                    x.InspectionDate)
                .ThenByDescending(x =>
                    x.Id)
                .ToListAsync();
        }


        public async Task<PreDispatchInspection?>
            GetFinalizedPdiForDispatchAsync(
                int preDispatchInspectionId)
        {
            return await _context
                .PreDispatchInspections
                .AsNoTracking()
                .Where(x =>
                    x.Id ==
                        preDispatchInspectionId &&
                    !x.IsDeleted &&
                    x.IsActive &&
                    x.Status ==
                        PreDispatchInspectionStatus.Finalized &&
                    x.AcceptedQuantity > 0)
                .FirstOrDefaultAsync();
        }


        public async Task<decimal>
            GetAllocatedDispatchQuantityAsync(
                int preDispatchInspectionId,
                int? excludeDeliveryChallanId = null)
        {
            #region Query

            var query =
                _context
                    .DeliveryChallanItems
                    .AsNoTracking()
                    .Where(x =>
                        x.PreDispatchInspectionId ==
                            preDispatchInspectionId
                        &&
                        !x.IsDeleted
                        &&
                        !x.DeliveryChallan.IsDeleted
                        &&
                        x.DeliveryChallan.IsActive
                        &&
                        (
                            !excludeDeliveryChallanId.HasValue
                            ||
                            x.DeliveryChallanId !=
                                excludeDeliveryChallanId.Value
                        ));

            #endregion


            #region Sum Dispatch Quantity

            return await query
                .Select(x =>
                    (decimal?)
                        x.DispatchQuantity)
                .SumAsync()
                ?? 0m;

            #endregion
        }

        #endregion


        #region Challan Code

        public async Task<string?>
            GetLastCodeAsync(
                string prefix)
        {
            return await _context
                .DeliveryChallans

                /*
                 * IsDeleted intentionally NOT filtered.
                 *
                 * Delivery Challan document numbers are
                 * permanent and must never be reused.
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
            DeliveryChallan deliveryChallan)
        {
            await _context
                .DeliveryChallans
                .AddAsync(
                    deliveryChallan);


            await _context
                .SaveChangesAsync();
        }


        public async Task UpdateAsync(
            DeliveryChallan deliveryChallan)
        {
            /*
             * DeliveryChallan is normally loaded by a
             * tracking query through GetForUpdateAsync()
             * or GetDeletedForUpdateAsync().
             */

            await _context
                .SaveChangesAsync();
        }

        #endregion


        #region Deleted Challans

        public async Task<List<DeliveryChallan>>
            GetDeletedAsync()
        {
            return await _context
                .DeliveryChallans
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


        public async Task<DeliveryChallan?>
            GetDeletedForUpdateAsync(
                int id)
        {
            return await _context
                .DeliveryChallans
                .Where(x =>
                    x.Id == id &&
                    x.IsDeleted)
                .Include(x =>
                    x.Items)
                    .ThenInclude(x =>
                        x.PreDispatchInspection)
                .FirstOrDefaultAsync();
        }

        #endregion

        #region Master Snapshot Sources

        public async Task<Customer?>
            GetCustomerForDispatchAsync(
                int customerId)
        {
            if (customerId <= 0)
            {
                return null;
            }

            return await _context
                .Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == customerId &&
                        x.IsActive &&
                        !x.IsDeleted);
        }


        public async Task<Company?>
            GetCompanyForDispatchAsync()
        {
            /*
             * Current ERP rule:
             *
             * Ajay Industries currently operates using one
             * active Company / Workshop master record.
             *
             * If multiple workshops are introduced later,
             * only this lookup strategy needs to change.
             * Delivery Challan snapshot structure remains same.
             */

            return await _context
                .Companies
                .AsNoTracking()
                .Where(
                    x =>
                        x.IsActive &&
                        !x.IsDeleted)
                .OrderBy(
                    x => x.CompanyId)
                .FirstOrDefaultAsync();
        }

        #endregion
    }
}