/*
============================================================
File: ICustomerRepository.cs

Purpose:
Defines database operations required by Customer Master.

Responsibilities:
- Retrieve active customers.
- Retrieve a customer by Id.
- Search customers.
- Provide paginated customer data.
- Detect duplicate GSTIN.
- Generate the next Customer Code safely.
- Add, update and soft-delete customer records.

Important:
- This interface contains database operation contracts only.
- Business rules belong in CustomerService.
- Actual EF Core implementation belongs in Infrastructure.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface ICustomerRepository
    {
        #region Read Operations

        Task<List<Customer>> GetAllAsync();

        Task<Customer?> GetByIdAsync(int id);

        Task<Customer?> GetForUpdateAsync(int id);

        #endregion


        #region Search And Pagination

        Task<List<Customer>> SearchAsync(
            string searchText);

        Task<PagedResult<Customer>> GetPagedAsync(
            int pageNumber,
            int pageSize);

        Task<PagedResult<Customer>> SearchPagedAsync(
    string searchText,
    int pageNumber,
    int pageSize);

        #endregion


        #region Validation

        Task<bool> GSTINExistsAsync(
            string gstin,
            int? excludeCustomerId = null);

        Task<bool> EmailExistsAsync(
            string email,
            int? excludeCustomerId = null);

        Task<bool> MobileNumberExistsAsync(
            string mobileNumber,
            int? excludeCustomerId = null);

        #endregion


        #region Customer Code

        Task<string?> GetLastCustomerCodeAsync();

        #endregion


        #region Write Operations

        Task AddAsync(Customer customer);

        Task UpdateAsync(Customer customer);

        #endregion
    }
}