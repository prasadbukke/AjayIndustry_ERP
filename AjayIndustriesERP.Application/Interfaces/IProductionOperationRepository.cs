/*
============================================================
File: IProductionOperationRepository.cs

Purpose:
Defines database operations required by Production Operation Master.

Responsibilities:
- Retrieve active Production Operations.
- Retrieve Production Operation Details/Edit records.
- Search and paginate Production Operations.
- Retrieve deleted Production Operations separately.
- Validate duplicate Operation Name.
- Retrieve last generated Operation Code.
- Add and update Production Operation records.
- Support soft-delete and restore workflows.

Important:
- Business rules belong in ProductionOperationService.
- Database access belongs only in Repository layer.
- Deleted Operation Codes are considered during code generation
  so Operation Codes are never reused.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IProductionOperationRepository
    {
        #region Read Operations

        Task<List<ProductionOperation>> GetAllAsync();

        Task<ProductionOperation?> GetByIdAsync(
            int id);

        Task<ProductionOperation?> GetForUpdateAsync(
            int id);

        #endregion


        #region Search And Pagination

        Task<PagedResult<ProductionOperation>> GetPagedAsync(
            int pageNumber,
            int pageSize);

        Task<PagedResult<ProductionOperation>> SearchPagedAsync(
            string searchText,
            int pageNumber,
            int pageSize);

        #endregion


        #region Deleted Operations

        Task<List<ProductionOperation>> GetDeletedAsync();

        Task<ProductionOperation?> GetDeletedForUpdateAsync(
            int id);

        #endregion


        #region Validation

        Task<bool> OperationNameExistsAsync(
            string operationName,
            int? excludeOperationId = null);

        #endregion


        #region Operation Code

        Task<string?> GetLastOperationCodeAsync();

        #endregion


        #region Write Operations

        Task AddAsync(
            ProductionOperation operation);

        Task UpdateAsync(
            ProductionOperation operation);

        #endregion
    }
}