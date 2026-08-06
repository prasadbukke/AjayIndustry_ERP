/*
==============================================================

File : ItemService.cs

Purpose :
Contains Item Master business rules and operations.

Notes :
- Stock is managed in the Inventory module.
- Warehouse-wise stock is managed separately.
- GST and pricing are not part of Item Master.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Services
{
    /// <summary>
    /// Provides business operations for Item Master.
    /// </summary>
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;

        public ItemService(IItemRepository itemRepository)
        {
            _itemRepository = itemRepository;
        }

        #region Read Operations

        public async Task<List<Item>> GetAllAsync()
        {
            return await _itemRepository.GetAllAsync();
        }

        public async Task<Item?> GetByIdAsync(int itemId)
        {
            return await _itemRepository.GetByIdAsync(itemId);
        }

        public async Task<List<Item>> SearchAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                return await _itemRepository.GetAllAsync();
            }

            return await _itemRepository.SearchAsync(searchText);
        }

        public async Task<PagedResult<Item>> GetPagedAsync(
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

            return await _itemRepository.GetPagedAsync(
                pageNumber,
                pageSize);
        }

        #endregion

        #region Create Item

        public async Task CreateAsync(Item item)
        {
            NormalizeItem(item);

            ValidateItem(item);

            if (await _itemRepository.ExistsByNameAsync(item.ItemName))
            {
                throw new BusinessException(
                    "An item with the same name already exists.");
            }

            item.ItemCode = await GenerateItemCodeAsync();

            item.CreatedOn = DateTime.UtcNow;
            item.CreatedBy = "System";

            await _itemRepository.AddAsync(item);
            await _itemRepository.SaveChangesAsync();
        }

        #endregion

        #region Update Item

        public async Task UpdateAsync(Item item)
        {
            var existingItem =
                await _itemRepository.GetByIdAsync(item.ItemId);

            if (existingItem == null)
            {
                throw new BusinessException("Item not found.");
            }

            NormalizeItem(item);

            ValidateItem(item);

            if (await _itemRepository.ExistsByNameAsync(
                item.ItemName,
                item.ItemId))
            {
                throw new BusinessException(
                    "An item with the same name already exists.");
            }

            /*
             * Item Code is not modified during Edit.
             * Existing auto-generated code is preserved.
             */
            existingItem.ItemName = item.ItemName;
            existingItem.Description = item.Description;
            existingItem.ItemCategoryId = item.ItemCategoryId;
            existingItem.BrandId = item.BrandId;
            existingItem.UomId = item.UomId;
            existingItem.IsActive = item.IsActive;

            existingItem.ModifiedOn = DateTime.UtcNow;
            existingItem.ModifiedBy = "System";

            await _itemRepository.UpdateAsync(existingItem);
            await _itemRepository.SaveChangesAsync();
        }

        #endregion

        #region Delete Item

        public async Task DeleteAsync(int itemId)
        {
            var item = await _itemRepository.GetByIdAsync(itemId);

            if (item == null)
            {
                throw new BusinessException("Item not found.");
            }

            item.ModifiedOn = DateTime.UtcNow;
            item.ModifiedBy = "System";

            await _itemRepository.DeleteAsync(item);
            await _itemRepository.SaveChangesAsync();
        }

        #endregion

        #region Private Validation Methods

        private static void NormalizeItem(Item item)
        {
            item.ItemName = item.ItemName.Trim();

            item.Description = string.IsNullOrWhiteSpace(item.Description)
                ? null
                : item.Description.Trim();
        }

        private static void ValidateItem(Item item)
        {
            if (string.IsNullOrWhiteSpace(item.ItemName))
            {
                throw new BusinessException("Item Name is required.");
            }

            if (item.ItemName.Length > 150)
            {
                throw new BusinessException(
                    "Item Name cannot exceed 150 characters.");
            }

            if (item.Description?.Length > 500)
            {
                throw new BusinessException(
                    "Description cannot exceed 500 characters.");
            }

            if (item.ItemCategoryId <= 0)
            {
                throw new BusinessException("Please select a Category.");
            }

            if (item.BrandId <= 0)
            {
                throw new BusinessException("Please select a Brand.");
            }

            if (item.UomId <= 0)
            {
                throw new BusinessException("Please select a UOM.");
            }
        }

        #endregion

        #region Item Code Generation

        private async Task<string> GenerateItemCodeAsync()
        {
            var lastCode =
                await _itemRepository.GetLastItemCodeAsync();

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(lastCode))
            {
                var numberPart = lastCode
                    .Replace("ITM", string.Empty)
                    .Trim();

                if (int.TryParse(numberPart, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            var itemCode = $"ITM{nextNumber:D5}";

            /*
             * Extra safety against duplicate Item Codes.
             */
            while (await _itemRepository.ExistsByCodeAsync(itemCode))
            {
                nextNumber++;

                itemCode = $"ITM{nextNumber:D5}";
            }

            return itemCode;
        }

        #endregion
    }
}