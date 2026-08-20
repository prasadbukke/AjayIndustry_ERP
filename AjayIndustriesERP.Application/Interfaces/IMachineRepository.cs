/*
============================================================
File: IMachineRepository.cs

Purpose:
Defines database operations required by Machine Master.

Responsibilities:
- Retrieve active Machines.
- Retrieve Machine Details/Edit records.
- Search and paginate Machines.
- Retrieve deleted Machines separately.
- Validate duplicate Serial Number.
- Retrieve last generated Machine Code.
- Add and update Machine records.
- Support soft-delete and restore workflows.

Important:
- Business rules belong in MachineService.
- Database access belongs only in Repository layer.
- Deleted Machine Codes are considered during code generation
  so Machine Codes are never reused.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IMachineRepository
    {
        #region Read Operations

        Task<List<Machine>> GetAllAsync();

        Task<Machine?> GetByIdAsync(
            int id);

        Task<Machine?> GetForUpdateAsync(
            int id);

        #endregion


        #region Search And Pagination

        Task<PagedResult<Machine>> GetPagedAsync(
            int pageNumber,
            int pageSize);

        Task<PagedResult<Machine>> SearchPagedAsync(
            string searchText,
            int pageNumber,
            int pageSize);

        #endregion


        #region Deleted Machines

        Task<List<Machine>> GetDeletedAsync();

        Task<Machine?> GetDeletedForUpdateAsync(
            int id);

        #endregion


        #region Validation

        Task<bool> SerialNumberExistsAsync(
            string serialNumber,
            int? excludeMachineId = null);

        #endregion


        #region Machine Code

        Task<string?> GetLastMachineCodeAsync();

        #endregion


        #region Write Operations

        Task AddAsync(
            Machine machine);

        Task UpdateAsync(
            Machine machine);

        #endregion
    }
}