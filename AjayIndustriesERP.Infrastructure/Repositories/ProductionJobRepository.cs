/*
============================================================
File: ProductionJobRepository.cs

Purpose:
Provides Entity Framework Core data access for Production Jobs.

Responsibilities:
- Retrieve Production Job Header and Steps.
- Search and paginate Production Jobs.
- Retrieve confirmed Customer PO Items.
- Retrieve current Released Routing with Routing Steps.
- Calculate allocated Production Quantity.
- Retrieve last Production Job Code.
- Persist Production Job changes.

Important:
- Main Production Job queries exclude soft-deleted Jobs.
- Cancelled Jobs do not consume Customer PO Quantity.
- Routing retrieval only returns the current Released Routing.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class ProductionJobRepository
        : IProductionJobRepository
    {
        #region Fields

        private readonly ApplicationDbContext _context;

        #endregion


        #region Constructor

        public ProductionJobRepository(
            ApplicationDbContext context)
        {
            _context = context;
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
                .Include(x =>
    x.Item)
    .ThenInclude(x =>
        x.Drawings
            .Where(drawing =>
                !drawing.IsDeleted &&
                drawing.IsActive))
                .Include(x =>
                    x.ItemProcessRouting)
                .Include(x =>
                    x.CustomerPurchaseOrderItem)
                    .ThenInclude(x =>
                        x.CustomerPurchaseOrder)
                .Include(x =>
                    x.Steps
                        .Where(step =>
                            !step.IsDeleted))
                    .ThenInclude(x =>
                        x.ProductionOperation)
                .Include(x =>
                    x.Steps
                        .Where(step =>
                            !step.IsDeleted))
                    .ThenInclude(x =>
                        x.DefaultMachine)
                .Include(x =>
                    x.Steps
                        .Where(step =>
                            !step.IsDeleted))
                    .ThenInclude(x =>
                        x.AssignedMachine)
                .Include(x =>
                    x.Steps
                        .Where(step =>
                            !step.IsDeleted))
                    .ThenInclude(x =>
                        x.History)
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
                    x.Steps)
                    .ThenInclude(x =>
                        x.History)
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
                await query.CountAsync();


            var jobs =
                await query
                    .Include(x =>
    x.Item)
    .ThenInclude(x =>
        x.Drawings
            .Where(drawing =>
                !drawing.IsDeleted &&
                drawing.IsActive))
                    .Include(x =>
                        x.CustomerPurchaseOrderItem)
                        .ThenInclude(x =>
                            x.CustomerPurchaseOrder)
                    .OrderByDescending(x =>
                        x.CreatedOn)
                    .ThenByDescending(x =>
                        x.Id)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(pageSize)
                    .ToListAsync();


            return new PagedResult<ProductionJob>
            {
                Items = jobs,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
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
                            // Production Job Code
                            x.Code
                                .ToLower()
                                .Contains(search)

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

                            // Routing Code
                            x.RoutingCode
                                .ToLower()
                                .Contains(search)

                            ||

                            // Customer PO Number
                            x.CustomerPurchaseOrderItem
                                .CustomerPurchaseOrder
                                .CustomerPurchaseOrderNumber
                                .ToLower()
                                .Contains(search)

                            ||

                            // ERP Customer PO Code
                            x.CustomerPurchaseOrderItem
                                .CustomerPurchaseOrder
                                .Code
                                .ToLower()
                                .Contains(search)

                            ||

                            // Customer Name
                            x.CustomerPurchaseOrderItem
                                .CustomerPurchaseOrder
                                .CustomerName
                                .ToLower()
                                .Contains(search)
                        ));

            #endregion


            #region Record Count

            var totalRecords =
                await query.CountAsync();

            #endregion


            #region Pagination

            var jobs =
                await query
                    .Include(x =>
    x.Item)
    .ThenInclude(x =>
        x.Drawings
            .Where(drawing =>
                !drawing.IsDeleted &&
                drawing.IsActive))
                    .Include(x =>
                        x.CustomerPurchaseOrderItem)
                        .ThenInclude(x =>
                            x.CustomerPurchaseOrder)
                    .OrderByDescending(x =>
                        x.CreatedOn)
                    .ThenByDescending(x =>
                        x.Id)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(pageSize)
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

        public async Task<List<CustomerPurchaseOrderItem>>
            GetCustomerPurchaseOrderItemsForProductionAsync()
        {
            return await _context
                .CustomerPurchaseOrderItems
                .AsNoTracking()
                .Include(x =>
                    x.CustomerPurchaseOrder)
                .Include(x =>
                    x.Item)
                .Where(x =>
                    !x.CustomerPurchaseOrder.IsDeleted &&
                    x.CustomerPurchaseOrder.Status ==
                        CustomerPurchaseOrderStatus.Confirmed)
                .OrderByDescending(x =>
                    x.CustomerPurchaseOrder.ReceivedDate)
                .ThenBy(x =>
                    x.ItemName)
                .ToListAsync();
        }


        public async Task<CustomerPurchaseOrderItem?>
            GetCustomerPurchaseOrderItemForProductionAsync(
                int customerPurchaseOrderItemId)
        {
            return await _context
                .CustomerPurchaseOrderItems
                .AsNoTracking()
                .Include(x =>
                    x.CustomerPurchaseOrder)
                .Include(x =>
                    x.Item)
                .FirstOrDefaultAsync(x =>
                    x.Id ==
                        customerPurchaseOrderItemId &&
                    !x.CustomerPurchaseOrder.IsDeleted &&
                    x.CustomerPurchaseOrder.Status ==
                        CustomerPurchaseOrderStatus.Confirmed);
        }


        public async Task<decimal>
    GetAllocatedJobQuantityAsync(
        int customerPurchaseOrderItemId,
        int? excludeProductionJobId = null)
        {
            return await _context
                .ProductionJobs
                .AsNoTracking()
                .Where(x =>
                    x.CustomerPurchaseOrderItemId ==
                        customerPurchaseOrderItemId &&
                    !x.IsDeleted &&
                    x.Status !=
                        ProductionJobStatus.Cancelled &&
                    (
                        !excludeProductionJobId.HasValue ||
                        x.Id != excludeProductionJobId.Value
                    ))
                .Select(x =>
                    (decimal?)x.JobQuantity)
                .SumAsync()
                ?? 0m;
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
                    .ThenInclude(x =>
                        x.ProductionOperation)
                .Include(x =>
                    x.Steps
                        .Where(step =>
                            !step.IsDeleted &&
                            step.IsActive))
                    .ThenInclude(x =>
                        x.DefaultMachine)
                .OrderByDescending(x =>
                    x.RevisionNumber)
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

                // Deleted Production Job Codes are intentionally
                // included. Codes must never be reused.

                .Where(x =>
                    x.Code.StartsWith(prefix))
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
                    x.CustomerPurchaseOrderItem)
                    .ThenInclude(x =>
                        x.CustomerPurchaseOrder)
                .OrderByDescending(x =>
                    x.ModifiedOn ?? x.CreatedOn)
                .ThenByDescending(x =>
                    x.Id)
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
                    x.Steps)
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