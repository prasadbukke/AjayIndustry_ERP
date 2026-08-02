/*
==============================================================

File : EmployeeRepository.cs

Purpose :
Handles Employee database operations.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;

        public EmployeeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _context.Employees
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.FirstName)
                .ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(int employeeId)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(x =>
                    x.EmployeeId == employeeId &&
                    !x.IsDeleted);
        }

        public async Task AddAsync(Employee employee)
        {
            await _context.Employees.AddAsync(employee);
        }

        public Task UpdateAsync(Employee employee)
        {
            _context.Employees.Update(employee);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Employee employee)
        {
            employee.IsDeleted = true;

            _context.Employees.Update(employee);

            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByCodeAsync(string employeeCode)
        {
            return await _context.Employees.AnyAsync(x =>
                x.EmployeeCode == employeeCode &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Employees.AnyAsync(x =>
                x.Email == email &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByCodeAsync(string employeeCode, int employeeId)
        {
            return await _context.Employees.AnyAsync(x =>
                x.EmployeeCode == employeeCode &&
                x.EmployeeId != employeeId &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByEmailAsync(string email, int employeeId)
        {
            return await _context.Employees.AnyAsync(x =>
                x.Email == email &&
                x.EmployeeId != employeeId &&
                !x.IsDeleted);
        }

        public async Task<List<Employee>> SearchAsync(string searchText)
        {
            searchText = searchText.Trim().ToLower();

            return await _context.Employees
                .Where(x =>
                    !x.IsDeleted &&
                    (
                        x.EmployeeCode.ToLower().Contains(searchText) ||
                        x.FirstName.ToLower().Contains(searchText) ||
                        x.LastName.ToLower().Contains(searchText) ||
                        x.Email.ToLower().Contains(searchText)
                    ))
                .OrderBy(x => x.FirstName)
                .ToListAsync();
        }

        public async Task<PagedResult<Employee>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Employees
                .Where(x => !x.IsDeleted);

            var totalRecords = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.FirstName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Employee>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        public async Task<string?> GetLastEmployeeCodeAsync()
        {
            return await _context.Employees
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.EmployeeId)
                .Select(x => x.EmployeeCode)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsByMobileAsync(string mobileNumber)
        {
            return await _context.Employees.AnyAsync(x =>
                x.MobileNumber == mobileNumber &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByMobileAsync(string mobileNumber, int employeeId)
        {
            return await _context.Employees.AnyAsync(x =>
                x.MobileNumber == mobileNumber &&
                x.EmployeeId != employeeId &&
                !x.IsDeleted);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}