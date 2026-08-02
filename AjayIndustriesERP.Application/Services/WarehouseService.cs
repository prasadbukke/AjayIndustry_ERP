/*
==============================================================

File : WarehouseService.cs

Purpose :
Contains Warehouse business logic.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehouseService(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        public async Task<List<Warehouse>> GetAllAsync()
        {
            return await _warehouseRepository.GetAllAsync();
        }

        public async Task<Warehouse?> GetByIdAsync(int warehouseId)
        {
            return await _warehouseRepository.GetByIdAsync(warehouseId);
        }

        public async Task CreateAsync(Warehouse warehouse)
        {
            if (await _warehouseRepository.ExistsByCodeAsync(warehouse.WarehouseCode))
                throw new BusinessException("Warehouse Code already exists.");

            if (await _warehouseRepository.ExistsByNameAsync(warehouse.WarehouseName))
                throw new BusinessException("Warehouse Name already exists.");

            warehouse.CreatedOn = DateTime.UtcNow;
            warehouse.CreatedBy = "System";

            warehouse.WarehouseCode = await GenerateWarehouseCodeAsync();

            await _warehouseRepository.AddAsync(warehouse);

            await _warehouseRepository.SaveChangesAsync();
        }

        public async Task UpdateAsync(Warehouse warehouse)
        {
            var existingWarehouse =
                await _warehouseRepository.GetByIdAsync(warehouse.WarehouseId);

            if (existingWarehouse == null)
                throw new BusinessException("Warehouse not found.");

            if (await _warehouseRepository.ExistsByCodeAsync(
                warehouse.WarehouseCode,
                warehouse.WarehouseId))
            {
                throw new BusinessException("Warehouse Code already exists.");
            }

            if (await _warehouseRepository.ExistsByNameAsync(
                warehouse.WarehouseName,
                warehouse.WarehouseId))
            {
                throw new BusinessException("Warehouse Name already exists.");
            }

            existingWarehouse.WarehouseCode = warehouse.WarehouseCode;
            existingWarehouse.WarehouseName = warehouse.WarehouseName;
            existingWarehouse.Description = warehouse.Description;
            existingWarehouse.WarehouseType = warehouse.WarehouseType;
            existingWarehouse.IsDefault = warehouse.IsDefault;
            existingWarehouse.IsActive = warehouse.IsActive;

            existingWarehouse.ModifiedOn = DateTime.UtcNow;
            existingWarehouse.ModifiedBy = "System";

            await _warehouseRepository.UpdateAsync(existingWarehouse);

            await _warehouseRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int warehouseId)
        {
            var warehouse =
                await _warehouseRepository.GetByIdAsync(warehouseId);

            if (warehouse == null)
                throw new BusinessException("Warehouse not found.");

            warehouse.ModifiedOn = DateTime.UtcNow;
            warehouse.ModifiedBy = "System";

            await _warehouseRepository.DeleteAsync(warehouse);

            await _warehouseRepository.SaveChangesAsync();
        }

        public async Task<List<Warehouse>> SearchAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await _warehouseRepository.GetAllAsync();

            return await _warehouseRepository.SearchAsync(searchText);
        }

        public async Task<PagedResult<Warehouse>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _warehouseRepository.GetPagedAsync(pageNumber, pageSize);
        }

        #region Private Methods

        /// <summary>
        /// Generates Warehouse Code.
        /// Example:
        /// WH00001
        /// </summary>
        private async Task<string> GenerateWarehouseCodeAsync()
        {
            var lastCode =
                await _warehouseRepository.GetLastWarehouseCodeAsync();

            if (string.IsNullOrWhiteSpace(lastCode))
                return "WH00001";

            int number =
                int.Parse(lastCode.Replace("WH", ""));

            number++;

            return $"WH{number:D5}";
        }

        #endregion
    }
}