/*
==============================================================

File : SpecificationService.cs

Purpose :
Contains Specification Master business rules and operations.

Examples :
- Diameter
- Thickness
- Length
- Width
- Grade
- Hardness
- Finish

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
    /// Provides business operations for Specification Master.
    /// </summary>
    public class SpecificationService :
        ISpecificationService
    {
        private readonly ISpecificationRepository
            _specificationRepository;

        public SpecificationService(
            ISpecificationRepository specificationRepository)
        {
            _specificationRepository =
                specificationRepository;
        }

        #region Read Operations

        public async Task<List<Specification>> GetAllAsync()
        {
            return await _specificationRepository
                .GetAllAsync();
        }

        public async Task<Specification?> GetByIdAsync(
            int specificationId)
        {
            return await _specificationRepository
                .GetByIdAsync(specificationId);
        }

        public async Task<List<Specification>> SearchAsync(
            string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await _specificationRepository
                    .GetAllAsync();
            }

            return await _specificationRepository
                .SearchAsync(searchText);
        }

        public async Task<PagedResult<Specification>>
            GetPagedAsync(
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

            return await _specificationRepository
                .GetPagedAsync(
                    pageNumber,
                    pageSize);
        }

        #endregion

        #region Create Specification

        public async Task CreateAsync(
            Specification specification)
        {
            NormalizeSpecification(
                specification);

            ValidateSpecification(
                specification);

            if (await _specificationRepository
                .ExistsByNameAsync(
                    specification.SpecificationName))
            {
                throw new BusinessException(
                    "Specification Name already exists.");
            }

            specification.SpecificationCode =
                await GenerateSpecificationCodeAsync();

            specification.CreatedOn =
                DateTime.UtcNow;

            specification.CreatedBy =
                "System";

            await _specificationRepository
                .AddAsync(specification);

            await _specificationRepository
                .SaveChangesAsync();
        }

        #endregion

        #region Update Specification

        public async Task UpdateAsync(
            Specification specification)
        {
            var existingSpecification =
                await _specificationRepository
                    .GetByIdAsync(
                        specification.SpecificationId);

            if (existingSpecification == null)
            {
                throw new BusinessException(
                    "Specification not found.");
            }

            NormalizeSpecification(
                specification);

            ValidateSpecification(
                specification);

            if (await _specificationRepository
                .ExistsByNameAsync(
                    specification.SpecificationName,
                    specification.SpecificationId))
            {
                throw new BusinessException(
                    "Specification Name already exists.");
            }

            /*
             * Auto-generated Specification Code
             * is never modified during Edit.
             */
            existingSpecification.SpecificationName =
                specification.SpecificationName;

            existingSpecification.Description =
                specification.Description;

            existingSpecification.IsActive =
                specification.IsActive;

            existingSpecification.ModifiedOn =
                DateTime.UtcNow;

            existingSpecification.ModifiedBy =
                "System";

            await _specificationRepository
                .UpdateAsync(
                    existingSpecification);

            await _specificationRepository
                .SaveChangesAsync();
        }

        #endregion

        #region Delete Specification

        public async Task DeleteAsync(
            int specificationId)
        {
            var specification =
                await _specificationRepository
                    .GetByIdAsync(
                        specificationId);

            if (specification == null)
            {
                throw new BusinessException(
                    "Specification not found.");
            }

            specification.ModifiedOn =
                DateTime.UtcNow;

            specification.ModifiedBy =
                "System";

            await _specificationRepository
                .DeleteAsync(specification);

            await _specificationRepository
                .SaveChangesAsync();
        }

        #endregion

        #region Validation and Normalization

        private static void NormalizeSpecification(
            Specification specification)
        {
            specification.SpecificationName =
                NormalizeDisplayValue(
                    specification.SpecificationName);

            specification.Description =
                string.IsNullOrWhiteSpace(
                    specification.Description)
                    ? null
                    : NormalizeDisplayValue(
                        specification.Description);
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

        private static void ValidateSpecification(
            Specification specification)
        {
            if (string.IsNullOrWhiteSpace(
                specification.SpecificationName))
            {
                throw new BusinessException(
                    "Specification Name is required.");
            }

            if (specification.SpecificationName.Length > 100)
            {
                throw new BusinessException(
                    "Specification Name cannot exceed 100 characters.");
            }

            if (specification.Description?.Length > 500)
            {
                throw new BusinessException(
                    "Description cannot exceed 500 characters.");
            }
        }

        #endregion

        #region Specification Code Generation

        private async Task<string>
            GenerateSpecificationCodeAsync()
        {
            var lastCode =
                await _specificationRepository
                    .GetLastSpecificationCodeAsync();

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastCode))
            {
                var numberPart = lastCode
                    .Replace(
                        "SPC",
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

            var specificationCode =
                $"SPC{nextNumber:D5}";

            while (await _specificationRepository
                .ExistsByCodeAsync(
                    specificationCode))
            {
                nextNumber++;

                specificationCode =
                    $"SPC{nextNumber:D5}";
            }

            return specificationCode;
        }

        #endregion
    }
}