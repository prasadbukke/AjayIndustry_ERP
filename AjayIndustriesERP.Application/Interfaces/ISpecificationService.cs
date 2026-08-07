/*
==============================================================

File : ISpecificationService.cs

Purpose :
Defines business operations for Specification Master.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    /// <summary>
    /// Defines business operations for Specification Master.
    /// </summary>
    public interface ISpecificationService
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

        Task CreateAsync(
            Specification specification);

        Task UpdateAsync(
            Specification specification);

        Task DeleteAsync(
            int specificationId);

        #endregion
    }
}