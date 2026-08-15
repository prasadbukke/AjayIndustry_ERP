/*
==============================================================

File : IPurchaseOrderRepository.cs

Purpose :
Defines persistence operations for Purchase Order transactions.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    /// <summary>
    /// Defines database operations for Purchase Orders.
    /// </summary>
    public interface IPurchaseOrderRepository
    {
        #region Read Operations

        Task<List<PurchaseOrder>> GetAllAsync();

        Task<PurchaseOrder?> GetByIdAsync(
            int purchaseOrderId);

        Task<List<PurchaseOrder>> SearchAsync(
            string searchText);

        Task<PagedResult<PurchaseOrder>> GetPagedAsync(
            int pageNumber,
            int pageSize);

        #endregion


        #region Write Operations

        Task AddAsync(
            PurchaseOrder purchaseOrder);

        Task UpdateAsync(
            PurchaseOrder purchaseOrder);

        Task DeleteAsync(
            PurchaseOrder purchaseOrder);

        #endregion


        #region Duplicate Validation

        Task<bool> ExistsByCodeAsync(
            string purchaseOrderCode);

        Task<bool> ExistsByCodeAsync(
            string purchaseOrderCode,
            int purchaseOrderId);

        #endregion


        #region Code Generation

        Task<string?> GetLastPurchaseOrderCodeAsync(string codePrefix);

        #endregion


        #region Save Changes

        Task SaveChangesAsync();

        #endregion
    }
}