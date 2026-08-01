/*
==============================================================

File : IUomService.cs

Purpose :
Represents UOM Service.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IUomService
    {
        Task<List<Uom>> GetAllAsync();

        Task<Uom?> GetByIdAsync(int uomId);

        Task CreateAsync(Uom uom);

        Task UpdateAsync(Uom uom);

        Task DeleteAsync(int uomId);

        Task<List<Uom>> SearchAsync(string searchText);

        Task<PagedResult<Uom>> GetPagedAsync(int pageNumber, int pageSize);
    }
}