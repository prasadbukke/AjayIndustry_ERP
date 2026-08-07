/*
==============================================================

File : ShapeRepository.cs

Purpose :
Handles Shape Master database operations.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    /// <summary>
    /// Provides database operations for Shape Master.
    /// </summary>
    public class ShapeRepository : IShapeRepository
    {
        private readonly ApplicationDbContext _context;

        public ShapeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Read Operations

        public async Task<List<Shape>> GetAllAsync()
        {
            return await _context.Shapes
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.ShapeName)
                .ToListAsync();
        }

        public async Task<Shape?> GetByIdAsync(int shapeId)
        {
            return await _context.Shapes
                .FirstOrDefaultAsync(x =>
                    x.ShapeId == shapeId &&
                    !x.IsDeleted);
        }

        public async Task<List<Shape>> SearchAsync(
            string searchText)
        {
            var normalizedSearchText =
                searchText.Trim().ToLower();

            return await _context.Shapes
                .Where(x =>
                    !x.IsDeleted &&
                    (
                        x.ShapeCode.ToLower()
                            .Contains(normalizedSearchText) ||

                        x.ShapeName.ToLower()
                            .Contains(normalizedSearchText) ||

                        (
                            x.Description != null &&
                            x.Description.ToLower()
                                .Contains(normalizedSearchText)
                        )
                    ))
                .OrderBy(x => x.ShapeName)
                .ToListAsync();
        }

        public async Task<PagedResult<Shape>> GetPagedAsync(
            int pageNumber,
            int pageSize)
        {
            var query = _context.Shapes
                .Where(x => !x.IsDeleted);

            var totalRecords =
                await query.CountAsync();

            var shapes = await query
                .OrderBy(x => x.ShapeName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Shape>
            {
                Items = shapes,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        #endregion

        #region Write Operations

        public async Task AddAsync(Shape shape)
        {
            await _context.Shapes.AddAsync(shape);
        }

        public Task UpdateAsync(Shape shape)
        {
            _context.Shapes.Update(shape);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Shape shape)
        {
            shape.IsDeleted = true;

            _context.Shapes.Update(shape);

            return Task.CompletedTask;
        }

        #endregion

        #region Duplicate Validation

        public async Task<bool> ExistsByCodeAsync(
            string shapeCode)
        {
            var normalizedCode =
                shapeCode.Trim().ToLower();

            /*
             * Deleted records are included because
             * Shape Codes must never be reused.
             */
            return await _context.Shapes
                .AnyAsync(x =>
                    x.ShapeCode.ToLower() ==
                    normalizedCode);
        }

        public async Task<bool> ExistsByCodeAsync(
            string shapeCode,
            int shapeId)
        {
            var normalizedCode =
                shapeCode.Trim().ToLower();

            return await _context.Shapes
                .AnyAsync(x =>
                    x.ShapeCode.ToLower() ==
                    normalizedCode &&
                    x.ShapeId != shapeId);
        }

        public async Task<bool> ExistsByNameAsync(
            string shapeName)
        {
            var normalizedName =
                shapeName.Trim().ToLower();

            return await _context.Shapes
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.ShapeName.ToLower() ==
                    normalizedName);
        }

        public async Task<bool> ExistsByNameAsync(
            string shapeName,
            int shapeId)
        {
            var normalizedName =
                shapeName.Trim().ToLower();

            return await _context.Shapes
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.ShapeName.ToLower() ==
                    normalizedName &&
                    x.ShapeId != shapeId);
        }

        #endregion

        #region Shape Code Generation

        public async Task<string?> GetLastShapeCodeAsync()
        {
            /*
             * Deleted records are intentionally included.
             * This prevents an old Shape Code from being reused.
             */
            return await _context.Shapes
                .OrderByDescending(x => x.ShapeId)
                .Select(x => x.ShapeCode)
                .FirstOrDefaultAsync();
        }

        #endregion

        #region Save Changes

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}