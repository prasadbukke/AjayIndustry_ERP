/*
==============================================================

File : IShapeRepository.cs

Purpose :
Defines database operations required for Shape Master.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    /// <summary>
    /// Defines repository operations for Shape Master.
    /// </summary>
    public interface IShapeRepository
    {
        #region Read Operations

        Task<List<Shape>> GetAllAsync();

        Task<Shape?> GetByIdAsync(int shapeId);

        Task<List<Shape>> SearchAsync(string searchText);

        Task<PagedResult<Shape>> GetPagedAsync(
            int pageNumber,
            int pageSize);

        #endregion

        #region Write Operations

        Task AddAsync(Shape shape);

        Task UpdateAsync(Shape shape);

        Task DeleteAsync(Shape shape);

        #endregion

        #region Duplicate Validation

        Task<bool> ExistsByCodeAsync(string shapeCode);

        Task<bool> ExistsByCodeAsync(
            string shapeCode,
            int shapeId);

        Task<bool> ExistsByNameAsync(string shapeName);

        Task<bool> ExistsByNameAsync(
            string shapeName,
            int shapeId);

        #endregion

        #region Shape Code Generation

        Task<string?> GetLastShapeCodeAsync();

        #endregion

        #region Save Changes

        Task SaveChangesAsync();

        #endregion
    }
}