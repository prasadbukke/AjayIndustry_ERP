/*
==============================================================

File : ItemService.cs

Purpose :
Contains Item Master business rules and operations.

Responsibilities :
- Item validation
- Item Code generation
- Item CRUD
- Item Specification validation
- Item Specification synchronization
- Complete Item configuration duplicate validation
- Soft delete

Duplicate Rule :
ItemName + Shape + Specifications

Specification comparison includes:
- SpecificationId
- SpecificationValue
- Specification UOM

Specification row order is ignored.

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
    /// Provides Item Master business operations.
    /// </summary>
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;

        private readonly IItemSpecificationRepository
            _itemSpecificationRepository;

        private readonly ISpecificationService
            _specificationService;

        private readonly IUomService
            _uomService;

        public ItemService(
            IItemRepository itemRepository,
            IItemSpecificationRepository itemSpecificationRepository,
            ISpecificationService specificationService,
            IUomService uomService)
        {
            _itemRepository =
                itemRepository;

            _itemSpecificationRepository =
                itemSpecificationRepository;

            _specificationService =
                specificationService;

            _uomService =
                uomService;
        }

        #region Read Operations

        public async Task<List<Item>> GetAllAsync()
        {
            return await _itemRepository
                .GetAllAsync();
        }

        public async Task<Item?> GetByIdAsync(
            int itemId)
        {
            return await _itemRepository
                .GetByIdAsync(itemId);
        }

        public async Task<List<Item>> SearchAsync(
            string searchText)
        {
            if (string.IsNullOrWhiteSpace(
                searchText))
            {
                return await _itemRepository
                    .GetAllAsync();
            }

            return await _itemRepository
                .SearchAsync(searchText);
        }

        public async Task<PagedResult<Item>>
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

            return await _itemRepository
                .GetPagedAsync(
                    pageNumber,
                    pageSize);
        }

        public async Task<List<ItemSpecification>>
            GetSpecificationsAsync(
                int itemId)
        {
            if (itemId <= 0)
            {
                return new List<ItemSpecification>();
            }

            return await _itemSpecificationRepository
                .GetByItemIdAsync(itemId);
        }

        #endregion

        #region Create Item

        public async Task CreateAsync(
            Item item)
        {
            NormalizeItem(item);

            NormalizeItemSpecifications(
                item.ItemSpecifications);

            ValidateItem(item);

            await ValidateItemSpecificationsAsync(
                item.ItemSpecifications);

            /*
             * Item Name alone is NOT unique.
             *
             * Final duplicate validation:
             * ItemName + Shape + Specifications.
             */
            await ValidateDuplicateConfigurationAsync(
                item);

            item.ItemCode =
                await GenerateItemCodeAsync();

            item.CreatedOn =
                DateTime.UtcNow;

            item.CreatedBy =
                "System";

            foreach (var itemSpecification
                in item.ItemSpecifications)
            {
                itemSpecification.Item =
                    item;

                itemSpecification.CreatedOn =
                    DateTime.UtcNow;

                itemSpecification.CreatedBy =
                    "System";

                itemSpecification.IsActive =
                    true;

                itemSpecification.IsDeleted =
                    false;
            }

            await _itemRepository
                .AddAsync(item);

            /*
             * Parent and child rows are saved together
             * through the same scoped DbContext.
             */
            await _itemRepository
                .SaveChangesAsync();
        }

        #endregion

        #region Update Item

        public async Task UpdateAsync(
            Item item)
        {
            var existingItem =
                await _itemRepository
                    .GetByIdAsync(
                        item.ItemId);

            if (existingItem == null)
            {
                throw new BusinessException(
                    "Item not found.");
            }

            NormalizeItem(item);

            NormalizeItemSpecifications(
                item.ItemSpecifications);

            ValidateItem(item);

            await ValidateItemSpecificationsAsync(
                item.ItemSpecifications);

            /*
             * Exclude the Item currently being edited
             * from duplicate comparison.
             */
            await ValidateDuplicateConfigurationAsync(
                item,
                item.ItemId);

            #region Update Main Item

            existingItem.ItemName =
                item.ItemName;

            existingItem.Description =
                item.Description;

            existingItem.ItemCategoryId =
                item.ItemCategoryId;

            existingItem.BrandId =
                item.BrandId;

            existingItem.UomId =
                item.UomId;

            existingItem.ShapeId =
                item.ShapeId;

            existingItem.IsActive =
                item.IsActive;

            existingItem.ModifiedOn =
                DateTime.UtcNow;

            existingItem.ModifiedBy =
                "System";

            await _itemRepository
                .UpdateAsync(existingItem);

            #endregion

            #region Synchronize Specifications

            await SynchronizeItemSpecificationsAsync(
                existingItem.ItemId,
                item.ItemSpecifications);

            #endregion

            await _itemSpecificationRepository
                .SaveChangesAsync();
        }

        #endregion

        #region Delete Item

        public async Task DeleteAsync(
            int itemId)
        {
            var item =
                await _itemRepository
                    .GetByIdAsync(itemId);

            if (item == null)
            {
                throw new BusinessException(
                    "Item not found.");
            }

            var itemSpecifications =
                await _itemSpecificationRepository
                    .GetByItemIdAsync(itemId);

            var modifiedOn =
                DateTime.UtcNow;

            foreach (var specification
                in itemSpecifications)
            {
                specification.ModifiedOn =
                    modifiedOn;

                specification.ModifiedBy =
                    "System";
            }

            await _itemSpecificationRepository
                .SoftDeleteRangeAsync(
                    itemSpecifications);

            item.ModifiedOn =
                modifiedOn;

            item.ModifiedBy =
                "System";

            await _itemRepository
                .DeleteAsync(item);

            await _itemSpecificationRepository
                .SaveChangesAsync();
        }

        #endregion

        #region Duplicate Item Configuration

        /// <summary>
        /// Blocks only an identical Item configuration.
        ///
        /// Same Item Name with another Shape or different
        /// Specifications is allowed.
        /// </summary>
        private async Task
            ValidateDuplicateConfigurationAsync(
                Item item,
                int? excludedItemId = null)
        {
            var sameNameItems =
                await _itemRepository
                    .GetByNameForDuplicateCheckAsync(
                        item.ItemName,
                        excludedItemId);

            foreach (var existingItem
                in sameNameItems)
            {
                var isSameConfiguration =
                    ItemConfigurationSimilarityHelper
                        .IsSameConfiguration(
                            existingItem,
                            item);

                if (!isSameConfiguration)
                {
                    continue;
                }

                throw new BusinessException(
                    $"The same Item configuration already exists as " +
                    $"{existingItem.ItemCode} - {existingItem.ItemName}.");
            }
        }

        #endregion

        #region Item Specification Synchronization

        private async Task
            SynchronizeItemSpecificationsAsync(
                int itemId,
                ICollection<ItemSpecification>
                    postedSpecifications)
        {
            var existingSpecifications =
                await _itemSpecificationRepository
                    .GetByItemIdAsync(itemId);

            var retainedIds =
                new HashSet<int>();

            foreach (var postedRow
                in postedSpecifications)
            {
                ItemSpecification?
                    existingRow = null;

                if (postedRow.ItemSpecificationId > 0)
                {
                    existingRow =
                        existingSpecifications
                            .FirstOrDefault(x =>
                                x.ItemSpecificationId ==
                                postedRow
                                    .ItemSpecificationId);

                    if (existingRow == null)
                    {
                        throw new BusinessException(
                            "Invalid Item Specification record.");
                    }
                }
                else
                {
                    existingRow =
                        existingSpecifications
                            .FirstOrDefault(x =>
                                x.SpecificationId ==
                                postedRow
                                    .SpecificationId &&
                                !retainedIds.Contains(
                                    x.ItemSpecificationId));
                }

                if (existingRow != null)
                {
                    existingRow.SpecificationId =
                        postedRow.SpecificationId;

                    existingRow.SpecificationValue =
                        postedRow.SpecificationValue;

                    existingRow.UomId =
                        postedRow.UomId;

                    existingRow.SortOrder =
                        postedRow.SortOrder;

                    existingRow.IsActive =
                        true;

                    existingRow.IsDeleted =
                        false;

                    existingRow.ModifiedOn =
                        DateTime.UtcNow;

                    existingRow.ModifiedBy =
                        "System";

                    retainedIds.Add(
                        existingRow
                            .ItemSpecificationId);

                    await _itemSpecificationRepository
                        .UpdateAsync(existingRow);

                    continue;
                }

                var newSpecification =
                    new ItemSpecification
                    {
                        ItemId =
                            itemId,

                        SpecificationId =
                            postedRow.SpecificationId,

                        SpecificationValue =
                            postedRow.SpecificationValue,

                        UomId =
                            postedRow.UomId,

                        SortOrder =
                            postedRow.SortOrder,

                        IsActive =
                            true,

                        IsDeleted =
                            false,

                        CreatedOn =
                            DateTime.UtcNow,

                        CreatedBy =
                            "System"
                    };

                await _itemSpecificationRepository
                    .AddAsync(
                        newSpecification);
            }

            var removedSpecifications =
                existingSpecifications
                    .Where(x =>
                        !retainedIds.Contains(
                            x.ItemSpecificationId))
                    .ToList();

            foreach (var removedRow
                in removedSpecifications)
            {
                removedRow.ModifiedOn =
                    DateTime.UtcNow;

                removedRow.ModifiedBy =
                    "System";
            }

            await _itemSpecificationRepository
                .SoftDeleteRangeAsync(
                    removedSpecifications);
        }

        #endregion

        #region Item Specification Validation

        private async Task
            ValidateItemSpecificationsAsync(
                ICollection<ItemSpecification>
                    itemSpecifications)
        {
            if (itemSpecifications.Count == 0)
            {
                return;
            }

            var duplicateSpecification =
                itemSpecifications
                    .GroupBy(x =>
                        x.SpecificationId)
                    .FirstOrDefault(x =>
                        x.Key > 0 &&
                        x.Count() > 1);

            if (duplicateSpecification != null)
            {
                throw new BusinessException(
                    "The same Specification cannot be added more than once to an Item.");
            }

            var specificationMasters =
                await _specificationService
                    .GetAllAsync();

            var validSpecificationIds =
                specificationMasters
                    .Select(x =>
                        x.SpecificationId)
                    .ToHashSet();

            var uomMasters =
                await _uomService
                    .GetAllAsync();

            var validUomIds =
                uomMasters
                    .Select(x =>
                        x.UomId)
                    .ToHashSet();

            foreach (var row
                in itemSpecifications)
            {
                if (row.SpecificationId <= 0)
                {
                    throw new BusinessException(
                        "Please select a Specification.");
                }

                if (!validSpecificationIds.Contains(
                    row.SpecificationId))
                {
                    throw new BusinessException(
                        "Selected Specification does not exist.");
                }

                if (string.IsNullOrWhiteSpace(
                    row.SpecificationValue))
                {
                    throw new BusinessException(
                        "Specification Value is required.");
                }

                if (row.SpecificationValue.Length > 200)
                {
                    throw new BusinessException(
                        "Specification Value cannot exceed 200 characters.");
                }

                if (row.UomId.HasValue &&
                    !validUomIds.Contains(
                        row.UomId.Value))
                {
                    throw new BusinessException(
                        "Selected Specification UOM does not exist.");
                }
            }
        }

        #endregion

        #region Item Validation and Normalization

        private static void NormalizeItem(
            Item item)
        {
            item.ItemName =
                NormalizeDisplayValue(
                    item.ItemName);

            item.Description =
                string.IsNullOrWhiteSpace(
                    item.Description)
                    ? null
                    : NormalizeDisplayValue(
                        item.Description);

            if (!item.ShapeId.HasValue ||
                item.ShapeId.Value <= 0)
            {
                item.ShapeId = null;
            }
        }

        private static void NormalizeItemSpecifications(
            ICollection<ItemSpecification>
                itemSpecifications)
        {
            var sortOrder = 1;

            foreach (var row
                in itemSpecifications)
            {
                row.SpecificationValue =
                    NormalizeDisplayValue(
                        row.SpecificationValue);

                if (!row.UomId.HasValue ||
                    row.UomId.Value <= 0)
                {
                    row.UomId = null;
                }

                row.SortOrder =
                    sortOrder;

                sortOrder++;
            }
        }

        private static string NormalizeDisplayValue(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return Regex.Replace(
                value.Trim(),
                @"\s+",
                " ");
        }

        private static void ValidateItem(
            Item item)
        {
            if (string.IsNullOrWhiteSpace(
                item.ItemName))
            {
                throw new BusinessException(
                    "Item Name is required.");
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
                throw new BusinessException(
                    "Please select a Category.");
            }

            if (item.BrandId <= 0)
            {
                throw new BusinessException(
                    "Please select a Brand.");
            }

            if (item.UomId <= 0)
            {
                throw new BusinessException(
                    "Please select a UOM.");
            }
        }

        #endregion

        #region Item Code Generation

        private async Task<string>
            GenerateItemCodeAsync()
        {
            var lastCode =
                await _itemRepository
                    .GetLastItemCodeAsync();

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(
                lastCode))
            {
                var numberPart =
                    lastCode
                        .Replace(
                            "ITM",
                            string.Empty,
                            StringComparison
                                .OrdinalIgnoreCase)
                        .Trim();

                if (int.TryParse(
                    numberPart,
                    out var lastNumber))
                {
                    nextNumber =
                        lastNumber + 1;
                }
            }

            var itemCode =
                $"ITM{nextNumber:D5}";

            while (await _itemRepository
                .ExistsByCodeAsync(
                    itemCode))
            {
                nextNumber++;

                itemCode =
                    $"ITM{nextNumber:D5}";
            }

            return itemCode;
        }

        #endregion
    }
}