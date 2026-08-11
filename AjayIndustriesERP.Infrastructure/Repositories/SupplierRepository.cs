/*
==============================================================

File : SupplierRepository.cs

Purpose :
Handles Supplier Master database operations.

Important :
- Supplier Code duplicate checks include deleted records.
- Supplier Name duplicate checks use active records only.
- GSTIN duplicate checks use active records only.

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
    /// Provides persistence operations for Supplier Master.
    /// </summary>
    public class SupplierRepository :
        ISupplierRepository
    {
        private readonly ApplicationDbContext _context;

        public SupplierRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        #region Read Operations

        public async Task<List<Supplier>>
            GetAllAsync()
        {
            return await _context.Suppliers
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.SupplierName)
                .ToListAsync();
        }

        public async Task<Supplier?> GetByIdAsync(
            int supplierId)
        {
            return await _context.Suppliers
                .FirstOrDefaultAsync(x =>
                    x.SupplierId == supplierId &&
                    !x.IsDeleted);
        }

        public async Task<List<Supplier>> SearchAsync(
            string searchText)
        {
            if (string.IsNullOrWhiteSpace(
                searchText))
            {
                return await GetAllAsync();
            }

            var search =
                searchText
                    .Trim()
                    .ToLower();

            return await _context.Suppliers
                .Where(x =>
                    !x.IsDeleted
                    &&
                    (
                        x.SupplierCode
                            .ToLower()
                            .Contains(search)

                        ||

                        x.SupplierName
                            .ToLower()
                            .Contains(search)

                        ||

                        (
                            x.ContactPerson != null &&
                            x.ContactPerson
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.MobileNumber != null &&
                            x.MobileNumber
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.AlternateMobileNumber != null &&
                            x.AlternateMobileNumber
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.Email != null &&
                            x.Email
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.Gstin != null &&
                            x.Gstin
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.Pan != null &&
                            x.Pan
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.City != null &&
                            x.City
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.State != null &&
                            x.State
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.Pincode != null &&
                            x.Pincode
                                .ToLower()
                                .Contains(search)
                        )
                    ))
                .OrderBy(x => x.SupplierName)
                .ToListAsync();
        }

        public async Task<PagedResult<Supplier>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            var query =
                _context.Suppliers
                    .Where(x => !x.IsDeleted);

            var totalRecords =
                await query.CountAsync();

            var records =
                await query
                    .OrderBy(x =>
                        x.SupplierName)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(pageSize)
                    .ToListAsync();

            return new PagedResult<Supplier>
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
            Supplier supplier)
        {
            await _context.Suppliers
                .AddAsync(supplier);
        }

        public Task UpdateAsync(
            Supplier supplier)
        {
            _context.Suppliers.Update(
                supplier);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(
            Supplier supplier)
        {
            supplier.IsDeleted = true;

            _context.Suppliers.Update(
                supplier);

            return Task.CompletedTask;
        }

        #endregion

        #region Supplier Code Validation

        public async Task<bool> ExistsByCodeAsync(
            string supplierCode)
        {
            var normalizedCode =
                supplierCode
                    .Trim()
                    .ToUpperInvariant();

            /*
             * Deleted Suppliers are deliberately included.
             * Supplier Codes must never be reused.
             */
            return await _context.Suppliers
                .AnyAsync(x =>
                    x.SupplierCode
                        .ToUpper() ==
                    normalizedCode);
        }

        public async Task<bool> ExistsByCodeAsync(
            string supplierCode,
            int supplierId)
        {
            var normalizedCode =
                supplierCode
                    .Trim()
                    .ToUpperInvariant();

            return await _context.Suppliers
                .AnyAsync(x =>
                    x.SupplierCode
                        .ToUpper() ==
                    normalizedCode &&
                    x.SupplierId != supplierId);
        }

        #endregion

        #region Supplier Name Validation

        public async Task<bool> ExistsByNameAsync(
            string supplierName)
        {
            var normalizedName =
                supplierName
                    .Trim()
                    .ToLower();

            return await _context.Suppliers
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.SupplierName
                        .ToLower() ==
                    normalizedName);
        }

        public async Task<bool> ExistsByNameAsync(
            string supplierName,
            int supplierId)
        {
            var normalizedName =
                supplierName
                    .Trim()
                    .ToLower();

            return await _context.Suppliers
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.SupplierId != supplierId &&
                    x.SupplierName
                        .ToLower() ==
                    normalizedName);
        }

        #endregion

        #region GSTIN Validation

        public async Task<bool> ExistsByGstinAsync(
            string gstin)
        {
            var normalizedGstin =
                gstin
                    .Trim()
                    .ToUpperInvariant();

            return await _context.Suppliers
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.Gstin != null &&
                    x.Gstin
                        .ToUpper() ==
                    normalizedGstin);
        }

        public async Task<bool> ExistsByGstinAsync(
            string gstin,
            int supplierId)
        {
            var normalizedGstin =
                gstin
                    .Trim()
                    .ToUpperInvariant();

            return await _context.Suppliers
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.SupplierId != supplierId &&
                    x.Gstin != null &&
                    x.Gstin
                        .ToUpper() ==
                    normalizedGstin);
        }

        #endregion

        #region Code Generation

        public async Task<string?>
            GetLastSupplierCodeAsync()
        {
            /*
             * Deleted Suppliers are included so the next
             * generated code is never reused.
             */
            return await _context.Suppliers
                .OrderByDescending(x =>
                    x.SupplierId)
                .Select(x =>
                    x.SupplierCode)
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