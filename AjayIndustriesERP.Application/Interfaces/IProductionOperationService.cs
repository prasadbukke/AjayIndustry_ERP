/*
============================================================
File: IProductionOperationService.cs

Purpose:
Defines Production Operation Master business operations.

Responsibilities:
- Retrieve Production Operations.
- Search and paginate Operations.
- Create Operations.
- Update Operations.
- Soft-delete Operations.
- Retrieve deleted Operations.
- Restore deleted Operations.

Important:
- Setup Time and Cycle Time do not belong here.
- Machine assignment does not belong here.
- Those concepts will be handled by Item Process Routing.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IProductionOperationService
    {
        #region Read Operations

        Task<List<ProductionOperation>> GetAllAsync();

        Task<ProductionOperation?> GetByIdAsync(
            int id);

        #endregion


        #region Search And Pagination

        Task<PagedResult<ProductionOperation>> SearchPagedAsync(
            string searchText,
            int pageNumber,
            int pageSize);

        #endregion


        #region Write Operations

        Task<ProductionOperation> CreateAsync(
            ProductionOperation operation);

        Task<ProductionOperation> UpdateAsync(
            ProductionOperation operation);

        #endregion


        #region Delete And Restore

        Task DeleteAsync(
            int id);

        Task<List<ProductionOperation>> GetDeletedAsync();

        Task RestoreAsync(
            int id);

        #endregion
    }
}