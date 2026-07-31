/*
==============================================================

File : EmployeeService.cs

Purpose :
Contains Employee business logic.

Flow

Controller
    ↓
Service
    ↓
Repository
    ↓
Database

==============================================================
*/

using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<List<Employee>> GetAllAsync()
        {
            return await _employeeRepository.GetAllAsync();
        }

        public async Task<Employee?> GetByIdAsync(int employeeId)
        {
            return await _employeeRepository.GetByIdAsync(employeeId);
        }

        public async Task CreateAsync(Employee employee)
        {
            if (await _employeeRepository.ExistsByCodeAsync(employee.EmployeeCode))
                throw new Exception("Employee Code already exists.");

            if (await _employeeRepository.ExistsByEmailAsync(employee.Email))
                throw new Exception("Email already exists.");

            if (await _employeeRepository.ExistsByMobileAsync(employee.MobileNumber))
                throw new Exception("Mobile Number already exists.");

            employee.EmployeeCode = await GenerateEmployeeCodeAsync();

            await _employeeRepository.AddAsync(employee);

            await _employeeRepository.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            var existingEmployee =
                await _employeeRepository.GetByIdAsync(employee.EmployeeId);

            if (existingEmployee == null)
                throw new Exception("Employee not found.");

            if (await _employeeRepository.ExistsByCodeAsync(employee.EmployeeCode, employee.EmployeeId))
                throw new Exception("Employee Code already exists.");

            if (await _employeeRepository.ExistsByEmailAsync(employee.Email, employee.EmployeeId))
                throw new Exception("Email already exists.");

            if (await _employeeRepository.ExistsByMobileAsync(employee.MobileNumber, employee.EmployeeId))
                throw new Exception("Mobile Number already exists.");

            existingEmployee.FirstName = employee.FirstName;
            existingEmployee.LastName = employee.LastName;
            existingEmployee.Gender = employee.Gender;
            existingEmployee.DateOfBirth = employee.DateOfBirth;
            existingEmployee.MobileNumber = employee.MobileNumber;
            existingEmployee.Email = employee.Email;
            existingEmployee.Address = employee.Address;
            existingEmployee.JoiningDate = employee.JoiningDate;
            existingEmployee.Salary = employee.Salary;
            existingEmployee.IsActive = employee.IsActive;

            existingEmployee.ModifiedOn = DateTime.UtcNow;
            existingEmployee.ModifiedBy = "System";

            await _employeeRepository.UpdateAsync(existingEmployee);

            await _employeeRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int employeeId)
        {
            var employee =
                await _employeeRepository.GetByIdAsync(employeeId);

            if (employee == null)
                throw new Exception("Employee not found.");

            employee.ModifiedOn = DateTime.UtcNow;
            employee.ModifiedBy = "System";

            await _employeeRepository.DeleteAsync(employee);

            await _employeeRepository.SaveChangesAsync();
        }

        public async Task<List<Employee>> SearchAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await _employeeRepository.GetAllAsync();

            return await _employeeRepository.SearchAsync(searchText);
        }

        public async Task<List<Employee>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _employeeRepository.GetPagedAsync(pageNumber, pageSize);
        }

        #region Private Methods

        /// <summary>
        /// Generates Employee Code.
        /// Example:
        /// EMP00001
        /// </summary>
        private async Task<string> GenerateEmployeeCodeAsync()
        {
            var lastCode = await _employeeRepository.GetLastEmployeeCodeAsync();

            if (string.IsNullOrWhiteSpace(lastCode))
                return "EMP00001";

            int number = int.Parse(lastCode.Replace("EMP", ""));

            number++;

            return $"EMP{number:D5}";
        }

        #endregion
    }
}