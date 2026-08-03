using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface IBrandRepository
    {
        Task<List<Brand>> GetAllAsync();

        Task<Brand?> GetByIdAsync(int brandId);

        Task AddAsync(Brand brand);

        Task UpdateAsync(Brand brand);

        Task DeleteAsync(Brand brand);

        Task<bool> ExistsByCodeAsync(string brandCode);

        Task<bool> ExistsByCodeAsync(string brandCode, int brandId);

        Task<bool> ExistsByNameAsync(string brandName);

        Task<bool> ExistsByNameAsync(string brandName, int brandId);

        Task<List<Brand>> SearchAsync(string searchText);

        Task<PagedResult<Brand>> GetPagedAsync(int pageNumber, int pageSize);

        Task<string?> GetLastBrandCodeAsync();

        Task SaveChangesAsync();
    }
}