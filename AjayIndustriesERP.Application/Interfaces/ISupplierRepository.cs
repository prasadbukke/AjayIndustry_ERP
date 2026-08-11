/*
==============================================================

File : ISupplierRepository.cs

Purpose :
Defines persistence operations for Supplier Master.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    /// <summary>
    /// Defines database operations for Supplier Master.
    /// </summary>
    public interface ISupplierRepository
    {
        #region Read Operations

        Task<List<Supplier>> GetAllAsync();

        Task<Supplier?> GetByIdAsync(
            int supplierId);

        Task<List<Supplier>> SearchAsync(
            string searchText);

        Task<PagedResult<Supplier>> GetPagedAsync(
            int pageNumber,
            int pageSize);

        #endregion

        #region Write Operations

        Task AddAsync(
            Supplier supplier);

        Task UpdateAsync(
            Supplier supplier);

        Task DeleteAsync(
            Supplier supplier);

        #endregion

        #region Duplicate Validation

        Task<bool> ExistsByCodeAsync(
            string supplierCode);

        Task<bool> ExistsByCodeAsync(
            string supplierCode,
            int supplierId);

        Task<bool> ExistsByNameAsync(
            string supplierName);

        Task<bool> ExistsByNameAsync(
            string supplierName,
            int supplierId);

        Task<bool> ExistsByGstinAsync(
            string gstin);

        Task<bool> ExistsByGstinAsync(
            string gstin,
            int supplierId);

        #endregion

        #region Code Generation

        Task<string?> GetLastSupplierCodeAsync();

        #endregion

        #region Save Changes

        Task SaveChangesAsync();

        #endregion
    }
}