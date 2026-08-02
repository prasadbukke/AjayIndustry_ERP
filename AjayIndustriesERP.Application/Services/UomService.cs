/*
==============================================================

File : UomService.cs

Purpose :
Contains UOM business logic.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Services
{
    public class UomService : IUomService
    {
        private readonly IUomRepository _uomRepository;

        public UomService(IUomRepository uomRepository)
        {
            _uomRepository = uomRepository;
        }

        public async Task<List<Uom>> GetAllAsync()
        {
            return await _uomRepository.GetAllAsync();
        }

        public async Task<Uom?> GetByIdAsync(int uomId)
        {
            return await _uomRepository.GetByIdAsync(uomId);
        }

        
        public async Task CreateAsync(Uom uom)
        {
            uom.UomCode = uom.UomCode.Trim().ToUpper();
            uom.UomName = uom.UomName.Trim();

            if (await _uomRepository.ExistsByCodeAsync(uom.UomCode))
                throw new BusinessException("UOM Code already exists.");

            if (await _uomRepository.ExistsByNameAsync(uom.UomName))
                throw new BusinessException("UOM Name already exists.");

            await _uomRepository.AddAsync(uom);

            await _uomRepository.SaveChangesAsync();
        }
        public async Task UpdateAsync(Uom uom)
        {
            var existingUom = await _uomRepository.GetByIdAsync(uom.UomId);

            if (existingUom == null)
                throw new BusinessException("UOM not found.");

            uom.UomCode = uom.UomCode.Trim().ToUpper();
            uom.UomName = uom.UomName.Trim();

            if (await _uomRepository.ExistsByCodeAsync(uom.UomCode, uom.UomId))
                throw new BusinessException("UOM Code already exists.");

            if (await _uomRepository.ExistsByNameAsync(uom.UomName, uom.UomId))
                throw new BusinessException("UOM Name already exists.");

            existingUom.UomCode = uom.UomCode;
            existingUom.UomName = uom.UomName;
            existingUom.Description = uom.Description;

            existingUom.ModifiedOn = DateTime.UtcNow;
            existingUom.ModifiedBy = "System";

            await _uomRepository.UpdateAsync(existingUom);

            await _uomRepository.SaveChangesAsync();
        }
        public async Task DeleteAsync(int uomId)
        {
            var uom = await _uomRepository.GetByIdAsync(uomId);

            if (uom == null)
                throw new BusinessException("UOM not found.");

            uom.ModifiedOn = DateTime.UtcNow;
            uom.ModifiedBy = "System";

            await _uomRepository.DeleteAsync(uom);

            await _uomRepository.SaveChangesAsync();
        }

        public async Task<List<Uom>> SearchAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await _uomRepository.GetAllAsync();

            return await _uomRepository.SearchAsync(searchText);
        }

        public async Task<PagedResult<Uom>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _uomRepository.GetPagedAsync(pageNumber, pageSize);
        }
    }
}