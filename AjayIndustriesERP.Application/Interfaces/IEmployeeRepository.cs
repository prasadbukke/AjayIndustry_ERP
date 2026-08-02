/*
==============================================================

File : IEmployeeRepository.cs

Purpose :
Defines Employee repository operations.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllAsync();

        Task<Employee?> GetByIdAsync(int employeeId);

        Task AddAsync(Employee employee);

        Task UpdateAsync(Employee employee);

        Task DeleteAsync(Employee employee);

        Task<bool> ExistsByCodeAsync(string employeeCode);

        Task<bool> ExistsByEmailAsync(string email);

        Task<bool> ExistsByCodeAsync(string employeeCode, int employeeId);

        Task<bool> ExistsByEmailAsync(string email, int employeeId);

        Task<bool> ExistsByMobileAsync(string mobileNumber);

        Task<bool> ExistsByMobileAsync(string mobileNumber, int employeeId);

        Task<List<Employee>> SearchAsync(string searchText);

        Task<PagedResult<Employee>> GetPagedAsync(int pageNumber, int pageSize);
        Task<string?> GetLastEmployeeCodeAsync();

        Task SaveChangesAsync();
    }
}