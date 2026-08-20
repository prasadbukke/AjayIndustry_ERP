/*
============================================================
File: IMachineService.cs

Purpose:
Defines Machine Master business operations.

Responsibilities:
- Retrieve Machine records.
- Search and paginate Machines.
- Create Machines.
- Update Machines.
- Soft-delete Machines.
- Retrieve deleted Machines.
- Restore deleted Machines.

Important:
- Machine operational Status is manually maintained.
- Business rules belong in MachineService.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IMachineService
    {
        #region Read Operations

        Task<List<Machine>> GetAllAsync();

        Task<Machine?> GetByIdAsync(
            int id);

        #endregion


        #region Search And Pagination

        Task<PagedResult<Machine>> SearchPagedAsync(
            string searchText,
            int pageNumber,
            int pageSize);

        #endregion


        #region Write Operations

        Task<Machine> CreateAsync(
            Machine machine);

        Task<Machine> UpdateAsync(
            Machine machine);

        #endregion


        #region Delete And Restore

        Task DeleteAsync(
            int id);

        Task<List<Machine>> GetDeletedAsync();

        Task RestoreAsync(
            int id);

        #endregion
    }
}