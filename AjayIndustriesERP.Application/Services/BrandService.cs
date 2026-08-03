/*
==============================================================

File : BrandService.cs

Purpose :
Contains Item Brand business logic.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Services
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _brandRepository;

        public BrandService(IBrandRepository BrandRepository)
        {
            _brandRepository = BrandRepository;
        }

        public async Task<List<Brand>> GetAllAsync()
        {
            return await _brandRepository.GetAllAsync();
        }

        public async Task<Brand?> GetByIdAsync(int BrandId)
        {
            return await _brandRepository.GetByIdAsync(BrandId);
        }

        public async Task CreateAsync(Brand Brand)
        {
            if (await _brandRepository.ExistsByCodeAsync(Brand.BrandCode))
                throw new BusinessException("Brand Code already exists.");

            if (await _brandRepository.ExistsByNameAsync(Brand.BrandName))
                throw new BusinessException("Brand Name already exists.");

            Brand.CreatedOn = DateTime.UtcNow;
            Brand.CreatedBy = "System";

            Brand.BrandCode = await GenerateBrandCodeAsync();

            await _brandRepository.AddAsync(Brand);

            await _brandRepository.SaveChangesAsync();
        }

        public async Task UpdateAsync(Brand Brand)
        {
            var existingBrand =
                await _brandRepository.GetByIdAsync(Brand.BrandId);

            if (existingBrand == null)
                throw new BusinessException("Brand not found.");

            if (await _brandRepository.ExistsByCodeAsync(
                Brand.BrandCode,
                Brand.BrandId))
            {
                throw new BusinessException("Brand Code already exists.");
            }

            if (await _brandRepository.ExistsByNameAsync(
                Brand.BrandName,
                Brand.BrandId))
            {
                throw new BusinessException("Brand Name already exists.");
            }

            existingBrand.BrandCode = Brand.BrandCode;
            existingBrand.BrandName = Brand.BrandName;
            existingBrand.Description = Brand.Description;
            existingBrand.IsActive = Brand.IsActive;

            existingBrand.ModifiedOn = DateTime.UtcNow;
            existingBrand.ModifiedBy = "System";

            await _brandRepository.UpdateAsync(existingBrand);

            await _brandRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int BrandId)
        {
            var Brand =
                await _brandRepository.GetByIdAsync(BrandId);

            if (Brand == null)
                throw new BusinessException("Brand not found.");

            Brand.ModifiedOn = DateTime.UtcNow;
            Brand.ModifiedBy = "System";

            await _brandRepository.DeleteAsync(Brand);

            await _brandRepository.SaveChangesAsync();
        }

        public async Task<List<Brand>> SearchAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await _brandRepository.GetAllAsync();

            return await _brandRepository.SearchAsync(searchText);
        }

        public async Task<PagedResult<Brand>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _brandRepository.GetPagedAsync(pageNumber, pageSize);
        }

        #region Private Methods

        /// <summary>
        /// Generates Brand Code.
        /// Example:
        /// BRD00001
        /// </summary>
        private async Task<string> GenerateBrandCodeAsync()
        {
            var lastCode =
                await _brandRepository.GetLastBrandCodeAsync();

            if (string.IsNullOrWhiteSpace(lastCode))
                return "BRD00001";

            int number =
                int.Parse(lastCode.Replace("BRD", ""));

            number++;

            return $"BRD{number:D5}";
        }

        #endregion
    }
}