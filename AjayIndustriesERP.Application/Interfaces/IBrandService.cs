using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IBrandService
    {
        Task<List<Brand>> GetAllAsync();

        Task<Brand?> GetByIdAsync(int brandId);

        Task CreateAsync(Brand brand);

        Task UpdateAsync(Brand brand);

        Task DeleteAsync(int brandId);

        Task<List<Brand>> SearchAsync(string searchText);

        Task<PagedResult<Brand>> GetPagedAsync(int pageNumber, int pageSize);
    }
}