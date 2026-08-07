/*
==============================================================

File : SpecificationRepository.cs

Purpose :
Handles Specification Master database operations.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    /// <summary>
    /// Provides database operations for Specification Master.
    /// </summary>
    public class SpecificationRepository :
        ISpecificationRepository
    {
        private readonly ApplicationDbContext _context;

        public SpecificationRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        #region Read Operations

        public async Task<List<Specification>> GetAllAsync()
        {
            return await _context.Specifications
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.SpecificationName)
                .ToListAsync();
        }

        public async Task<Specification?> GetByIdAsync(
            int specificationId)
        {
            return await _context.Specifications
                .FirstOrDefaultAsync(x =>
                    x.SpecificationId == specificationId &&
                    !x.IsDeleted);
        }

        public async Task<List<Specification>> SearchAsync(
            string searchText)
        {
            var normalizedSearch =
                searchText.Trim().ToLower();

            return await _context.Specifications
                .Where(x =>
                    !x.IsDeleted &&
                    (
                        x.SpecificationCode
                            .ToLower()
                            .Contains(normalizedSearch) ||

                        x.SpecificationName
                            .ToLower()
                            .Contains(normalizedSearch) ||

                        (
                            x.Description != null &&
                            x.Description
                                .ToLower()
                                .Contains(normalizedSearch)
                        )
                    ))
                .OrderBy(x => x.SpecificationName)
                .ToListAsync();
        }

        public async Task<PagedResult<Specification>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            var query =
                _context.Specifications
                    .Where(x => !x.IsDeleted);

            var totalRecords =
                await query.CountAsync();

            var records = await query
                .OrderBy(x => x.SpecificationName)
                .Skip(
                    (pageNumber - 1) *
                    pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Specification>
            {
                Items = records,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        #endregion

        #region Write Operations

        public async Task AddAsync(
            Specification specification)
        {
            await _context.Specifications
                .AddAsync(specification);
        }

        public Task UpdateAsync(
            Specification specification)
        {
            _context.Specifications.Update(
                specification);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(
            Specification specification)
        {
            specification.IsDeleted = true;

            _context.Specifications.Update(
                specification);

            return Task.CompletedTask;
        }

        #endregion

        #region Duplicate Validation

        public async Task<bool> ExistsByCodeAsync(
            string specificationCode)
        {
            var normalizedCode =
                specificationCode
                    .Trim()
                    .ToLower();

            /*
             * Deleted records are included because
             * Specification Codes are never reused.
             */
            return await _context.Specifications
                .AnyAsync(x =>
                    x.SpecificationCode
                        .ToLower() ==
                    normalizedCode);
        }

        public async Task<bool> ExistsByCodeAsync(
            string specificationCode,
            int specificationId)
        {
            var normalizedCode =
                specificationCode
                    .Trim()
                    .ToLower();

            return await _context.Specifications
                .AnyAsync(x =>
                    x.SpecificationCode
                        .ToLower() ==
                    normalizedCode &&
                    x.SpecificationId !=
                    specificationId);
        }

        public async Task<bool> ExistsByNameAsync(
            string specificationName)
        {
            var normalizedName =
                specificationName
                    .Trim()
                    .ToLower();

            return await _context.Specifications
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.SpecificationName
                        .ToLower() ==
                    normalizedName);
        }

        public async Task<bool> ExistsByNameAsync(
            string specificationName,
            int specificationId)
        {
            var normalizedName =
                specificationName
                    .Trim()
                    .ToLower();

            return await _context.Specifications
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.SpecificationName
                        .ToLower() ==
                    normalizedName &&
                    x.SpecificationId !=
                    specificationId);
        }

        #endregion

        #region Code Generation

        public async Task<string?>
            GetLastSpecificationCodeAsync()
        {
            /*
             * Deleted records are intentionally included
             * so old codes are never reused.
             */
            return await _context.Specifications
                .OrderByDescending(
                    x => x.SpecificationId)
                .Select(
                    x => x.SpecificationCode)
                .FirstOrDefaultAsync();
        }

        #endregion

        #region Save Changes

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}