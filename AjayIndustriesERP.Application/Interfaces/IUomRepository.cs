/*
==============================================================

File : IUomRepository.cs

Purpose :
Represents UOM Repository.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IUomRepository
    {
        Task<List<Uom>> GetAllAsync();

        Task<Uom?> GetByIdAsync(int uomId);

        Task AddAsync(Uom uom);

        Task UpdateAsync(Uom uom);

        Task DeleteAsync(Uom uom);

        Task<bool> ExistsByCodeAsync(string uomCode);

        Task<bool> ExistsByCodeAsync(string uomCode, int uomId);

        Task<bool> ExistsByNameAsync(string uomName);

        Task<bool> ExistsByNameAsync(string uomName, int uomId);

        /// <summary>
        /// Searches UOM by Code or Name.
        /// </summary>
        Task<List<Uom>> SearchAsync(string searchText);

        /// <summary>
        /// Returns paginated UOM list.
        /// </summary>
        Task<PagedResult<Uom>> GetPagedAsync(int pageNumber, int pageSize);

        /// <summary>
        /// Returns last generated UOM code.
        /// </summary>
        Task<string?> GetLastUomCodeAsync();

        Task SaveChangesAsync();
    }
}