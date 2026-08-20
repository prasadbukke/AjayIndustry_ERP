/*
============================================================
File: ProductionOperationRepository.cs

Purpose:
Provides Entity Framework Core data access for
Production Operation Master.

Responsibilities:
- Retrieve active Production Operations.
- Retrieve records for Details/Edit.
- Search and paginate Operation records.
- Retrieve soft-deleted Operations separately.
- Check duplicate active Operation Names.
- Retrieve last generated Operation Code.
- Persist Production Operation changes.

Important:
- Main Operation Index displays only non-deleted records.
- Deleted Operations are displayed on a separate page.
- Deleted Operation Codes are included while generating
  the next Operation Code so codes are never reused.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;

using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class ProductionOperationRepository
        : IProductionOperationRepository
    {
        #region Fields

        private readonly ApplicationDbContext _context;

        #endregion


        #region Constructor

        public ProductionOperationRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        #endregion


        #region Read Operations

        public async Task<List<ProductionOperation>>
            GetAllAsync()
        {
            return await _context
                .ProductionOperations
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted)
                .OrderBy(x =>
                    x.OperationName)
                .ThenBy(x =>
                    x.Code)
                .ToListAsync();
        }


        public async Task<ProductionOperation?>
            GetByIdAsync(
                int id)
        {
            return await _context
                .ProductionOperations
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }


        public async Task<ProductionOperation?>
            GetForUpdateAsync(
                int id)
        {
            return await _context
                .ProductionOperations
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        #endregion


        #region Search And Pagination

        public async Task<PagedResult<ProductionOperation>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            var query =
                _context
                    .ProductionOperations
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted);


            var totalRecords =
                await query.CountAsync();


            var operations =
                await query
                    .OrderBy(x =>
                        x.OperationName)
                    .ThenBy(x =>
                        x.Code)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(pageSize)
                    .ToListAsync();


            return new PagedResult<ProductionOperation>
            {
                Items = operations,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }


        public async Task<PagedResult<ProductionOperation>>
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
                    .ProductionOperations
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        (
                            x.Code
                                .ToLower()
                                .Contains(search)

                            ||

                            x.OperationName
                                .ToLower()
                                .Contains(search)

                            ||

                            (
                                x.Description != null &&
                                x.Description
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            (
                                x.Remarks != null &&
                                x.Remarks
                                    .ToLower()
                                    .Contains(search)
                            )
                        ));


            var totalRecords =
                await query.CountAsync();


            var operations =
                await query
                    .OrderBy(x =>
                        x.OperationName)
                    .ThenBy(x =>
                        x.Code)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(pageSize)
                    .ToListAsync();


            return new PagedResult<ProductionOperation>
            {
                Items = operations,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        #endregion


        #region Deleted Operations

        public async Task<List<ProductionOperation>>
            GetDeletedAsync()
        {
            return await _context
                .ProductionOperations
                .AsNoTracking()
                .Where(x =>
                    x.IsDeleted)
                .OrderByDescending(x =>
                    x.ModifiedOn ?? x.CreatedOn)
                .ThenByDescending(x =>
                    x.Id)
                .ToListAsync();
        }


        public async Task<ProductionOperation?>
            GetDeletedForUpdateAsync(
                int id)
        {
            return await _context
                .ProductionOperations
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsDeleted);
        }

        #endregion


        #region Validation

        public async Task<bool>
            OperationNameExistsAsync(
                string operationName,
                int? excludeOperationId = null)
        {
            var normalizedName =
                operationName
                    .Trim()
                    .ToUpper();


            return await _context
                .ProductionOperations
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDeleted &&

                    x.OperationName
                        .ToUpper() ==
                        normalizedName &&

                    (
                        !excludeOperationId.HasValue ||
                        x.Id !=
                            excludeOperationId.Value
                    ));
        }

        #endregion


        #region Operation Code

        public async Task<string?>
            GetLastOperationCodeAsync()
        {
            const string prefix =
                "AI/OPR/";


            return await _context
                .ProductionOperations

                // Deleted records intentionally included.
                // Operation Codes must never be reused.

                .Where(x =>
                    x.Code.StartsWith(prefix))
                .OrderByDescending(x =>
                    x.Id)
                .Select(x =>
                    x.Code)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Write Operations

        public async Task AddAsync(
            ProductionOperation operation)
        {
            await _context
                .ProductionOperations
                .AddAsync(operation);


            await _context
                .SaveChangesAsync();
        }


        public async Task UpdateAsync(
            ProductionOperation operation)
        {
            await _context
                .SaveChangesAsync();
        }

        #endregion
    }
}