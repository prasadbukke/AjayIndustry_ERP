/*
============================================================
File: ProductionJobRepository.cs

Purpose:
Provides Entity Framework Core data access for Production Jobs.

Production Structure:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Item
        ↓
Production Job Step

Responsibilities:
- Retrieve Production Job Header with Item-wise Production data.
- Search and paginate Production Jobs.
- Retrieve confirmed Customer Purchase Orders.
- Retrieve one Customer PO with all active Items.
- Check whether a Customer PO already has a Production Job.
- Retrieve current Released Routing with Routing Steps.
- Retrieve active Production Operations.
- Retrieve Machines used for Production execution.
- Retrieve last Production Job Code.
- Persist Production Job changes.

Important:
- One Customer PO has one Production Job.
- Main Production Job queries exclude soft-deleted Jobs.
- Deleted Production Jobs keep their original Job Code.
- Old multiple Job Quantity allocation logic is removed.
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
    public class ProductionJobRepository
        : IProductionJobRepository
    {
        #region Fields

        private readonly ApplicationDbContext
            _context;

        #endregion


        #region Constructor

        public ProductionJobRepository(
            ApplicationDbContext context)
        {
            _context =
                context;
        }

        #endregion


        #region Read Operations

        public async Task<ProductionJob?>
            GetByIdAsync(
                int id)
        {
            return await _context
                .ProductionJobs
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)

                // =========================================
                // CUSTOMER PO
                // =========================================

                .Include(x =>
                    x.CustomerPurchaseOrder)

                // =========================================
                // PRODUCTION ITEMS -> ITEM / DRAWINGS
                // =========================================

                .Include(x =>
                    x.Items
                        .Where(item =>
                            !item.IsDeleted))
                    .ThenInclude(item =>
                        item.Item)
                    .ThenInclude(item =>
                        item.Drawings
                            .Where(drawing =>
                                !drawing.IsDeleted &&
                                drawing.IsActive))

                // =========================================
                // PRODUCTION ITEMS -> CUSTOMER PO ITEM
                // =========================================

                .Include(x =>
                    x.Items
                        .Where(item =>
                            !item.IsDeleted))
                    .ThenInclude(item =>
                        item.CustomerPurchaseOrderItem)

                // =========================================
                // PRODUCTION ITEMS -> ROUTING
                // =========================================

                .Include(x =>
                    x.Items
                        .Where(item =>
                            !item.IsDeleted))
                    .ThenInclude(item =>
                        item.ItemProcessRouting)

                // =========================================
                // PRODUCTION ITEMS -> STEPS -> OPERATION
                // =========================================

                .Include(x =>
                    x.Items
                        .Where(item =>
                            !item.IsDeleted))
                    .ThenInclude(item =>
                        item.Steps
                            .Where(step =>
                                !step.IsDeleted))
                    .ThenInclude(step =>
                        step.ProductionOperation)

                // =========================================
                // PRODUCTION ITEMS -> STEPS -> DEFAULT MACHINE
                // =========================================

                .Include(x =>
                    x.Items
                        .Where(item =>
                            !item.IsDeleted))
                    .ThenInclude(item =>
                        item.Steps
                            .Where(step =>
                                !step.IsDeleted))
                    .ThenInclude(step =>
                        step.DefaultMachine)

                // =========================================
                // PRODUCTION ITEMS -> STEPS -> ASSIGNED MACHINE
                // =========================================

                .Include(x =>
                    x.Items
                        .Where(item =>
                            !item.IsDeleted))
                    .ThenInclude(item =>
                        item.Steps
                            .Where(step =>
                                !step.IsDeleted))
                    .ThenInclude(step =>
                        step.AssignedMachine)

                // =========================================
                // PRODUCTION ITEMS -> STEPS -> HISTORY
                // =========================================

                .Include(x =>
                    x.Items
                        .Where(item =>
                            !item.IsDeleted))
                    .ThenInclude(item =>
                        item.Steps
                            .Where(step =>
                                !step.IsDeleted))
                    .ThenInclude(step =>
                        step.History)

                .AsSplitQuery()
                .FirstOrDefaultAsync();
        }


        public async Task<ProductionJob?>
            GetForUpdateAsync(
                int id)
        {
            return await _context
                .ProductionJobs
                .Where(x =>
                    x.Id == id &&
                    !x.IsDeleted)

                .Include(x =>
                    x.CustomerPurchaseOrder)

                .Include(x =>
                    x.Items)
                    .ThenInclude(item =>
                        item.CustomerPurchaseOrderItem)

                .Include(x =>
                    x.Items)
                    .ThenInclude(item =>
                        item.Steps)
                    .ThenInclude(step =>
                        step.History)

                .AsSplitQuery()
                .FirstOrDefaultAsync();
        }


        public async Task<ProductionJob?>
            GetByCustomerPurchaseOrderIdAsync(
                int customerPurchaseOrderId)
        {
            if (customerPurchaseOrderId <= 0)
            {
                return null;
            }


            return await _context
                .ProductionJobs
                .AsNoTracking()
                .Where(x =>
                    x.CustomerPurchaseOrderId ==
                        customerPurchaseOrderId &&
                    !x.IsDeleted)

                .Include(x =>
                    x.CustomerPurchaseOrder)

                .Include(x =>
                    x.Items
                        .Where(item =>
                            !item.IsDeleted))

                .AsSplitQuery()
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Search And Pagination

        public async Task<PagedResult<ProductionJob>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            var query =
                _context
                    .ProductionJobs
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted);


            var totalRecords =
                await query
                    .CountAsync();


            var jobs =
                await query
                    .Include(x =>
                        x.CustomerPurchaseOrder)

                    .Include(x =>
                        x.Items
                            .Where(item =>
                                !item.IsDeleted))

                    .OrderByDescending(x =>
                        x.CreatedOn)

                    .ThenByDescending(x =>
                        x.Id)

                    .Skip(
                        (pageNumber - 1) *
                        pageSize)

                    .Take(
                        pageSize)

                    .AsSplitQuery()

                    .ToListAsync();


            return new PagedResult<ProductionJob>
            {
                Items =
                    jobs,

                PageNumber =
                    pageNumber,

                PageSize =
                    pageSize,

                TotalRecords =
                    totalRecords
            };
        }


        public async Task<PagedResult<ProductionJob>>
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
                    .ProductionJobs
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted
                        &&
                        (
                            // =================================
                            // PRODUCTION JOB CODE
                            // =================================

                            x.Code
                                .ToLower()
                                .Contains(search)

                            ||

                            // =================================
                            // CUSTOMER PO CODE
                            // =================================

                            x.CustomerPurchaseOrder
                                .Code
                                .ToLower()
                                .Contains(search)

                            ||

                            // =================================
                            // CUSTOMER PO NUMBER
                            // =================================

                            x.CustomerPurchaseOrder
                                .CustomerPurchaseOrderNumber
                                .ToLower()
                                .Contains(search)

                            ||

                            // =================================
                            // CUSTOMER NAME
                            // =================================

                            x.CustomerPurchaseOrder
                                .CustomerName
                                .ToLower()
                                .Contains(search)

                            ||

                            // =================================
                            // PRODUCTION ITEM
                            // =================================

                            x.Items.Any(item =>
                                !item.IsDeleted
                                &&
                                (
                                    item.ItemCode
                                        .ToLower()
                                        .Contains(search)

                                    ||

                                    item.ItemName
                                        .ToLower()
                                        .Contains(search)

                                    ||

                                    item.RoutingCode
                                        .ToLower()
                                        .Contains(search)
                                ))
                        ));

            #endregion


            #region Record Count

            var totalRecords =
                await query
                    .CountAsync();

            #endregion


            #region Pagination

            var jobs =
                await query

                    .Include(x =>
                        x.CustomerPurchaseOrder)

                    .Include(x =>
                        x.Items
                            .Where(item =>
                                !item.IsDeleted))

                    .OrderByDescending(x =>
                        x.CreatedOn)

                    .ThenByDescending(x =>
                        x.Id)

                    .Skip(
                        (pageNumber - 1) *
                        pageSize)

                    .Take(
                        pageSize)

                    .AsSplitQuery()

                    .ToListAsync();

            #endregion


            #region Result

            return new PagedResult<ProductionJob>
            {
                Items =
                    jobs,

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


        #region Customer PO Source

        public async Task<List<CustomerPurchaseOrder>>
            GetCustomerPurchaseOrdersForProductionAsync()
        {
            return await _context
                .CustomerPurchaseOrders
                .AsNoTracking()

                .Include(x =>
                    x.Items
                        .Where(item =>
                            !item.IsDeleted &&
                            item.IsActive))
                    .ThenInclude(item =>
                        item.Item)

                .Where(x =>
                    !x.IsDeleted
                    &&
                    x.Status ==
                        CustomerPurchaseOrderStatus.Confirmed

                    /*
                     * One Customer PO = One Production Job.
                     *
                     * Even a Cancelled or soft-deleted
                     * Production Job keeps the original PO
                     * relationship and Job identity.
                     *
                     * Deleted Jobs must be restored instead of
                     * creating another Production Job ID.
                     */
                    &&
                    !_context
                        .ProductionJobs
                        .Any(job =>
                            job.CustomerPurchaseOrderId ==
                                x.Id))

                .OrderByDescending(x =>
                    x.ReceivedDate)

                .ThenByDescending(x =>
                    x.Id)

                .AsSplitQuery()

                .ToListAsync();
        }


        public async Task<CustomerPurchaseOrder?>
            GetCustomerPurchaseOrderForProductionAsync(
                int customerPurchaseOrderId)
        {
            if (customerPurchaseOrderId <= 0)
            {
                return null;
            }


            return await _context
                .CustomerPurchaseOrders
                .AsNoTracking()

                .Include(x =>
                    x.Items
                        .Where(item =>
                            !item.IsDeleted &&
                            item.IsActive))
                    .ThenInclude(item =>
                        item.Item)

                .FirstOrDefaultAsync(x =>
                    x.Id ==
                        customerPurchaseOrderId
                    &&
                    !x.IsDeleted
                    &&
                    x.Status ==
                        CustomerPurchaseOrderStatus.Confirmed);
        }

        #endregion


        #region Routing

        public async Task<ItemProcessRouting?>
            GetReleasedRoutingForItemAsync(
                int itemId)
        {
            return await _context
                .ItemProcessRoutings
                .AsNoTracking()

                .Where(x =>
                    x.ItemId == itemId &&
                    !x.IsDeleted &&
                    x.IsActive &&
                    x.Status ==
                        ItemProcessRoutingStatus.Released)

                .Include(x =>
                    x.Steps
                        .Where(step =>
                            !step.IsDeleted &&
                            step.IsActive))
                    .ThenInclude(step =>
                        step.ProductionOperation)

                .Include(x =>
                    x.Steps
                        .Where(step =>
                            !step.IsDeleted &&
                            step.IsActive))
                    .ThenInclude(step =>
                        step.DefaultMachine)

                .OrderByDescending(x =>
                    x.RevisionNumber)

                .AsSplitQuery()

                .FirstOrDefaultAsync();
        }

        #endregion


        #region Draft Pipeline Lookups

        public async Task<List<ProductionOperation>>
            GetProductionOperationsForPipelineAsync()
        {
            return await _context
                .ProductionOperations
                .AsNoTracking()

                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive)

                .OrderBy(x =>
                    x.OperationName)

                .ThenBy(x =>
                    x.Code)

                .ToListAsync();
        }

        #endregion


        #region Production Execution Lookups

        public async Task<List<Machine>>
            GetMachinesForExecutionAsync()
        {
            return await _context
                .Machines
                .AsNoTracking()

                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive)

                .OrderBy(x =>
                    x.MachineName)

                .ThenBy(x =>
                    x.Code)

                .ToListAsync();
        }


        public async Task<Machine?>
            GetMachineForExecutionAsync(
                int machineId)
        {
            return await _context
                .Machines
                .AsNoTracking()

                .FirstOrDefaultAsync(x =>
                    x.Id == machineId &&
                    !x.IsDeleted &&
                    x.IsActive);
        }

        #endregion


        #region Job Code

        public async Task<string?>
            GetLastJobCodeAsync(
                string prefix)
        {
            return await _context
                .ProductionJobs

                /*
                 * Deleted Production Job Codes are
                 * intentionally included.
                 *
                 * Production Job Codes must never be reused.
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


        #region Deleted Jobs

        public async Task<List<ProductionJob>>
            GetDeletedAsync()
        {
            return await _context
                .ProductionJobs
                .AsNoTracking()

                .Where(x =>
                    x.IsDeleted)

                .Include(x =>
                    x.CustomerPurchaseOrder)

                .Include(x =>
                    x.Items)

                .OrderByDescending(x =>
                    x.ModifiedOn
                    ??
                    x.CreatedOn)

                .ThenByDescending(x =>
                    x.Id)

                .AsSplitQuery()

                .ToListAsync();
        }


        public async Task<ProductionJob?>
            GetDeletedForUpdateAsync(
                int id)
        {
            return await _context
                .ProductionJobs

                .Where(x =>
                    x.Id == id &&
                    x.IsDeleted)

                .Include(x =>
                    x.CustomerPurchaseOrder)

                .Include(x =>
                    x.Items)

                    .ThenInclude(item =>
                        item.Steps)

                    .ThenInclude(step =>
                        step.History)

                .AsSplitQuery()

                .FirstOrDefaultAsync();
        }

        #endregion


        #region Write Operations

        public async Task AddAsync(
            ProductionJob productionJob)
        {
            await _context
                .ProductionJobs
                .AddAsync(
                    productionJob);


            await _context
                .SaveChangesAsync();
        }


        public async Task UpdateAsync(
            ProductionJob productionJob)
        {
            await _context
                .SaveChangesAsync();
        }

        #endregion
    }
}