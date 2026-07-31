/*
==============================================================

File : IEmployeeService.cs

Purpose :
Defines Employee business operations.

==============================================================
*/

using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<Employee>> GetAllAsync();

        Task<Employee?> GetByIdAsync(int employeeId);

        Task CreateAsync(Employee employee);

        Task UpdateAsync(Employee employee);

        Task DeleteAsync(int employeeId);

        Task<List<Employee>> SearchAsync(string searchText);

        Task<List<Employee>> GetPagedAsync(int pageNumber, int pageSize);
    }
}