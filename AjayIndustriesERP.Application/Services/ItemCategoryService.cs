/*
==============================================================

File : ItemCategoryService.cs

Purpose :
Contains Item Category business logic.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Services
{
    public class ItemCategoryService : IItemCategoryService
    {
        private readonly IItemCategoryRepository _itemCategoryRepository;

        public ItemCategoryService(IItemCategoryRepository itemCategoryRepository)
        {
            _itemCategoryRepository = itemCategoryRepository;
        }

        public async Task<List<ItemCategory>> GetAllAsync()
        {
            return await _itemCategoryRepository.GetAllAsync();
        }

        public async Task<ItemCategory?> GetByIdAsync(int itemCategoryId)
        {
            return await _itemCategoryRepository.GetByIdAsync(itemCategoryId);
        }

        public async Task CreateAsync(ItemCategory itemCategory)
        {
            itemCategory.CategoryName =
                itemCategory.CategoryName.Trim();

            itemCategory.Description =
                string.IsNullOrWhiteSpace(itemCategory.Description)
                    ? null
                    : itemCategory.Description.Trim();

            if (await _itemCategoryRepository.ExistsByNameAsync(
                itemCategory.CategoryName))
            {
                throw new BusinessException(
                    "Category Name already exists.");
            }

            itemCategory.CategoryCode =
                await GenerateCategoryCodeAsync();

            itemCategory.CreatedOn = DateTime.UtcNow;
            itemCategory.CreatedBy = "System";

            await _itemCategoryRepository.AddAsync(itemCategory);

            await _itemCategoryRepository.SaveChangesAsync();
        }

        public async Task UpdateAsync(ItemCategory itemCategory)
        {
            var existingCategory =
                await _itemCategoryRepository.GetByIdAsync(itemCategory.ItemCategoryId);

            if (existingCategory == null)
                throw new BusinessException("Category not found.");

            if (await _itemCategoryRepository.ExistsByCodeAsync(
                itemCategory.CategoryCode,
                itemCategory.ItemCategoryId))
            {
                throw new BusinessException("Category Code already exists.");
            }

            if (await _itemCategoryRepository.ExistsByNameAsync(
                itemCategory.CategoryName,
                itemCategory.ItemCategoryId))
            {
                throw new BusinessException("Category Name already exists.");
            }

            existingCategory.CategoryCode = itemCategory.CategoryCode;
            existingCategory.CategoryName = itemCategory.CategoryName;
            existingCategory.Description = itemCategory.Description;
            existingCategory.IsActive = itemCategory.IsActive;

            existingCategory.ModifiedOn = DateTime.UtcNow;
            existingCategory.ModifiedBy = "System";

            await _itemCategoryRepository.UpdateAsync(existingCategory);

            await _itemCategoryRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int itemCategoryId)
        {
            var itemCategory =
                await _itemCategoryRepository.GetByIdAsync(itemCategoryId);

            if (itemCategory == null)
                throw new BusinessException("Category not found.");

            itemCategory.ModifiedOn = DateTime.UtcNow;
            itemCategory.ModifiedBy = "System";

            await _itemCategoryRepository.DeleteAsync(itemCategory);

            await _itemCategoryRepository.SaveChangesAsync();
        }

        public async Task<List<ItemCategory>> SearchAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await _itemCategoryRepository.GetAllAsync();

            return await _itemCategoryRepository.SearchAsync(searchText);
        }

        public async Task<PagedResult<ItemCategory>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _itemCategoryRepository.GetPagedAsync(pageNumber, pageSize);
        }

        #region Private Methods

        /// <summary>
        /// Generates Category Code.
        /// Example:
        /// CAT00001
        /// </summary>
        private async Task<string> GenerateCategoryCodeAsync()
        {
            var lastCode =
                await _itemCategoryRepository.GetLastCategoryCodeAsync();

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastCode))
            {
                var numberPart = lastCode
                    .Replace("CAT", string.Empty)
                    .Trim();

                if (int.TryParse(numberPart, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            var categoryCode = $"CAT{nextNumber:D5}";

            while (await _itemCategoryRepository.ExistsByCodeAsync(categoryCode))
            {
                nextNumber++;

                categoryCode = $"CAT{nextNumber:D5}";
            }

            return categoryCode;
        }

        #endregion
    }
}