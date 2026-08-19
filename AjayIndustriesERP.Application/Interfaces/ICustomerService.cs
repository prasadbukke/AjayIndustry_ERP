/*
============================================================
File: ICustomerService.cs

Purpose:
Defines business operations for Customer Master.

Responsibilities:
- Retrieve Customer records.
- Search and paginate Customer records.
- Create new Customers.
- Update existing Customers.
- Soft-delete Customers.
- Apply Customer Master business rules.

Important:
- Business rules are implemented in CustomerService.
- Database access is performed only through ICustomerRepository.
- Customer Code is generated automatically by the Service.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface ICustomerService
    {
        #region Read Operations

        Task<List<Customer>> GetAllAsync();

        Task<Customer?> GetByIdAsync(
            int id);

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


        #region Write Operations

        Task<Customer> CreateAsync(
            Customer customer);

        Task<Customer> UpdateAsync(
            Customer customer);

        Task DeleteAsync(
            int id);

        #endregion
    }
}