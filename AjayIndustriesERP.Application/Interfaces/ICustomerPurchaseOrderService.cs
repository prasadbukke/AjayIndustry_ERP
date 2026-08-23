/*
============================================================
File: ICustomerPurchaseOrderService.cs

Purpose:
Defines business operations for Customer Purchase Order module.

Responsibilities:
- Retrieve Customer Purchase Orders.
- Provide Search + Pagination.
- Load active Customer Master records.
- Load active Item Master records.
- Create Customer Purchase Orders.
- Update Draft Customer Purchase Orders.
- Confirm Draft Customer Purchase Orders.
- Soft-delete Draft Customer Purchase Orders.

Important:
- Business rules belong in CustomerPurchaseOrderService.
- Database access belongs in Repository layer.
- Production Pipeline is intentionally not handled here yet.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface ICustomerPurchaseOrderService
    {
        #region Read Operations

        Task<List<CustomerPurchaseOrder>>
            GetAllAsync();

        Task<CustomerPurchaseOrder?>
            GetByIdAsync(
                int id);

        #endregion


        #region Search And Pagination

        Task<PagedResult<CustomerPurchaseOrder>>
            GetPagedAsync(
                int pageNumber,
                int pageSize);

        Task<PagedResult<CustomerPurchaseOrder>>
            SearchPagedAsync(
                string searchText,
                int pageNumber,
                int pageSize);

        #endregion


        #region Customer Master

        Task<List<Customer>>
            GetCustomersForOrderAsync();

        #endregion


        #region Item Master

        Task<List<Item>>
            GetItemsForOrderAsync();

        Task<Item?>
            GetItemForOrderAsync(
                int itemId);

        #endregion

        #region Customer PO Validation

        /// <summary>
        /// Checks whether the Customer Purchase Order Number
        /// already exists for the selected Customer.
        ///
        /// Used by Create/Edit for live duplicate validation.
        /// </summary>
        Task<bool> CustomerPurchaseOrderNumberExistsAsync(
            int customerId,
            string customerPurchaseOrderNumber,
            int? excludeCustomerPurchaseOrderId = null);

        #endregion


        #region Write Operations

        Task<CustomerPurchaseOrder>
            CreateAsync(
                CustomerPurchaseOrder
                    customerPurchaseOrder);

        Task<CustomerPurchaseOrder>
            UpdateAsync(
                CustomerPurchaseOrder
                    customerPurchaseOrder);

        #endregion


        #region Workflow

        Task ConfirmAsync(
            int id);

        #endregion


        #region Delete

        Task DeleteAsync(
            int id);

        #endregion

        

        #region Deleted Orders

        Task<List<CustomerPurchaseOrder>>
            GetDeletedAsync();

        Task RestoreAsync(
            int id);

        #endregion
    }
}