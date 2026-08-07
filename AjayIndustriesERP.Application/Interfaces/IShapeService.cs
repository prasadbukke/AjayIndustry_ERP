/*
==============================================================

File : IShapeService.cs

Purpose :
Defines business operations for Shape Master.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    /// <summary>
    /// Defines business operations for Shape Master.
    /// </summary>
    public interface IShapeService
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

        Task CreateAsync(Shape shape);

        Task UpdateAsync(Shape shape);

        Task DeleteAsync(int shapeId);

        #endregion
    }
}