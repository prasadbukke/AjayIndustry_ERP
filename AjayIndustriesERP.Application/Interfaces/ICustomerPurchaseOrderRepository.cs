/*
============================================================
File: ICustomerPurchaseOrderRepository.cs

Purpose:
Defines database operations required by the Customer Purchase
Order module.

Responsibilities:
- Retrieve Customer Purchase Orders.
- Retrieve Customer PO with Items for Details/Edit.
- Provide Search + Pagination.
- Load active Customers for Customer PO creation.
- Load active Items from existing Item Master.
- Detect duplicate Customer + Customer PO Number.
- Retrieve last generated Customer PO Code.
- Add and update Customer Purchase Orders.

Important:
- Business rules belong in CustomerPurchaseOrderService.
- Database access belongs only in Repository layer.
- Existing Customer Master and Item Master are reused.
- Production Pipeline data is not handled here.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface ICustomerPurchaseOrderRepository
    {
        #region Read Operations

        Task<List<CustomerPurchaseOrder>> GetAllAsync();

        Task<CustomerPurchaseOrder?> GetByIdAsync(
            int id);

        Task<CustomerPurchaseOrder?> GetForUpdateAsync(
            int id);

        Task<List<CustomerPurchaseOrder>>
    GetDeletedAsync();

        Task<CustomerPurchaseOrder?>
            GetDeletedForUpdateAsync(
                int id);

        #endregion


        #region Search And Pagination

        Task<PagedResult<CustomerPurchaseOrder>> GetPagedAsync(
            int pageNumber,
            int pageSize);

        Task<PagedResult<CustomerPurchaseOrder>> SearchPagedAsync(
            string searchText,
            int pageNumber,
            int pageSize);

        #endregion


        #region Customer Master Loading

        Task<List<Customer>> GetCustomersForOrderAsync();

        Task<Customer?> GetCustomerForOrderAsync(
            int customerId);

        #endregion


        #region Item Master Loading

        Task<List<Item>> GetItemsForOrderAsync();

        Task<Item?> GetItemForOrderAsync(
            int itemId);

        #endregion


        #region Duplicate Validation

        Task<bool> CustomerPurchaseOrderNumberExistsAsync(
            int customerId,
            string customerPurchaseOrderNumber,
            int? excludeCustomerPurchaseOrderId = null);

        #endregion


        #region Customer PO Code

        Task<string?> GetLastCustomerPurchaseOrderCodeAsync(
            string codePrefix);

        #endregion


        #region Write Operations

        Task AddAsync(
            CustomerPurchaseOrder customerPurchaseOrder);

        Task UpdateAsync(
            CustomerPurchaseOrder customerPurchaseOrder);

        #endregion

        #region Restore Support

        Task<CustomerPurchaseOrder?> GetAnyByIdForUpdateAsync(
            int id);

        #endregion
    }
}