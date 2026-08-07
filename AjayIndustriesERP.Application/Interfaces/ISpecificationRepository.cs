/*
==============================================================

File : ISpecificationRepository.cs

Purpose :
Defines database operations required for Specification Master.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    /// <summary>
    /// Defines repository operations for Specification Master.
    /// </summary>
    public interface ISpecificationRepository
    {
        #region Read Operations

        Task<List<Specification>> GetAllAsync();

        Task<Specification?> GetByIdAsync(
            int specificationId);

        Task<List<Specification>> SearchAsync(
            string searchText);

        Task<PagedResult<Specification>> GetPagedAsync(
            int pageNumber,
            int pageSize);

        #endregion

        #region Write Operations

        Task AddAsync(Specification specification);

        Task UpdateAsync(Specification specification);

        Task DeleteAsync(Specification specification);

        #endregion

        #region Duplicate Validation

        Task<bool> ExistsByCodeAsync(
            string specificationCode);

        Task<bool> ExistsByCodeAsync(
            string specificationCode,
            int specificationId);

        Task<bool> ExistsByNameAsync(
            string specificationName);

        Task<bool> ExistsByNameAsync(
            string specificationName,
            int specificationId);

        #endregion

        #region Code Generation

        Task<string?> GetLastSpecificationCodeAsync();

        #endregion

        #region Save Changes

        Task SaveChangesAsync();

        #endregion
    }
}