/*
==============================================================

File : IPurchaseOrderService.cs

Purpose :
Defines business operations for Purchase Orders.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    /// <summary>
    /// Defines Purchase Order business operations.
    /// </summary>
    public interface IPurchaseOrderService
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

        Task<bool> IsIntraStateAsync(
    int companyId,
    int supplierId);

        #endregion


        #region Write Operations

        Task CreateAsync(
            PurchaseOrder purchaseOrder);

        Task UpdateAsync(
            PurchaseOrder purchaseOrder);

        Task ConfirmAsync(
            int purchaseOrderId);

        Task MarkAsSentAsync(
            int purchaseOrderId);

        Task DeleteAsync(
            int purchaseOrderId);

        #endregion


    }
}