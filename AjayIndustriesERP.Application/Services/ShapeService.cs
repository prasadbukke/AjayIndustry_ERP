/*
==============================================================

File : ShapeService.cs

Purpose :
Contains Shape Master business rules and operations.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using System.Text.RegularExpressions;

namespace AjayIndustriesERP.Application.Services
{
    /// <summary>
    /// Provides business operations for Shape Master.
    /// </summary>
    public class ShapeService : IShapeService
    {
        private readonly IShapeRepository _shapeRepository;

        public ShapeService(
            IShapeRepository shapeRepository)
        {
            _shapeRepository = shapeRepository;
        }

        #region Read Operations

        public async Task<List<Shape>> GetAllAsync()
        {
            return await _shapeRepository.GetAllAsync();
        }

        public async Task<Shape?> GetByIdAsync(
            int shapeId)
        {
            return await _shapeRepository
                .GetByIdAsync(shapeId);
        }

        public async Task<List<Shape>> SearchAsync(
            string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await _shapeRepository.GetAllAsync();
            }

            return await _shapeRepository
                .SearchAsync(searchText);
        }

        public async Task<PagedResult<Shape>> GetPagedAsync(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 10;
            }

            return await _shapeRepository.GetPagedAsync(
                pageNumber,
                pageSize);
        }

        #endregion

        #region Create Shape

        public async Task CreateAsync(Shape shape)
        {
            NormalizeShape(shape);

            ValidateShape(shape);

            if (await _shapeRepository.ExistsByNameAsync(
                shape.ShapeName))
            {
                throw new BusinessException(
                    "Shape Name already exists.");
            }

            shape.ShapeCode =
                await GenerateShapeCodeAsync();

            shape.CreatedOn = DateTime.UtcNow;
            shape.CreatedBy = "System";

            await _shapeRepository.AddAsync(shape);

            await _shapeRepository.SaveChangesAsync();
        }

        #endregion

        #region Update Shape

        public async Task UpdateAsync(Shape shape)
        {
            var existingShape =
                await _shapeRepository.GetByIdAsync(
                    shape.ShapeId);

            if (existingShape == null)
            {
                throw new BusinessException(
                    "Shape not found.");
            }

            NormalizeShape(shape);

            ValidateShape(shape);

            if (await _shapeRepository.ExistsByNameAsync(
                shape.ShapeName,
                shape.ShapeId))
            {
                throw new BusinessException(
                    "Shape Name already exists.");
            }

            /*
             * Auto-generated Shape Code is preserved.
             */
            existingShape.ShapeName =
                shape.ShapeName;

            existingShape.Description =
                shape.Description;

            existingShape.IsActive =
                shape.IsActive;

            existingShape.ModifiedOn =
                DateTime.UtcNow;

            existingShape.ModifiedBy =
                "System";

            await _shapeRepository.UpdateAsync(
                existingShape);

            await _shapeRepository.SaveChangesAsync();
        }

        #endregion

        #region Delete Shape

        public async Task DeleteAsync(int shapeId)
        {
            var shape =
                await _shapeRepository.GetByIdAsync(
                    shapeId);

            if (shape == null)
            {
                throw new BusinessException(
                    "Shape not found.");
            }

            shape.ModifiedOn =
                DateTime.UtcNow;

            shape.ModifiedBy =
                "System";

            await _shapeRepository.DeleteAsync(shape);

            await _shapeRepository.SaveChangesAsync();
        }

        #endregion

        #region Normalization and Validation

        private static void NormalizeShape(Shape shape)
        {
            shape.ShapeName =
                NormalizeDisplayValue(shape.ShapeName);

            shape.Description =
                string.IsNullOrWhiteSpace(shape.Description)
                    ? null
                    : NormalizeDisplayValue(
                        shape.Description);
        }

        private static string NormalizeDisplayValue(
            string value)
        {
            var trimmedValue =
                value.Trim();

            return Regex.Replace(
                trimmedValue,
                @"\s+",
                " ");
        }

        private static void ValidateShape(Shape shape)
        {
            if (string.IsNullOrWhiteSpace(
                shape.ShapeName))
            {
                throw new BusinessException(
                    "Shape Name is required.");
            }

            if (shape.ShapeName.Length > 100)
            {
                throw new BusinessException(
                    "Shape Name cannot exceed 100 characters.");
            }

            if (shape.Description?.Length > 500)
            {
                throw new BusinessException(
                    "Description cannot exceed 500 characters.");
            }
        }

        #endregion

        #region Shape Code Generation

        private async Task<string> GenerateShapeCodeAsync()
        {
            var lastCode =
                await _shapeRepository
                    .GetLastShapeCodeAsync();

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastCode))
            {
                var numberPart = lastCode
                    .Replace(
                        "SHP",
                        string.Empty,
                        StringComparison.OrdinalIgnoreCase)
                    .Trim();

                if (int.TryParse(
                    numberPart,
                    out var lastNumber))
                {
                    nextNumber =
                        lastNumber + 1;
                }
            }

            var shapeCode =
                $"SHP{nextNumber:D5}";

            /*
             * Additional protection against code collision.
             */
            while (await _shapeRepository
                .ExistsByCodeAsync(shapeCode))
            {
                nextNumber++;

                shapeCode =
                    $"SHP{nextNumber:D5}";
            }

            return shapeCode;
        }

        #endregion
    }
}