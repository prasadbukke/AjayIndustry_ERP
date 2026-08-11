/*
==============================================================

File : ISupplierService.cs

Purpose :
Defines business operations for Supplier Master.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    /// <summary>
    /// Defines Supplier Master business operations.
    /// </summary>
    public interface ISupplierService
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

        Task CreateAsync(
            Supplier supplier);

        Task UpdateAsync(
            Supplier supplier);

        Task DeleteAsync(
            int supplierId);

        #endregion
    }
}