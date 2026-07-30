using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface ICompanyService
    {
        Task<List<Company>> GetAllAsync();

        Task<Company?> GetByIdAsync(int companyId);

        Task CreateAsync(Company company);

        Task UpdateAsync(Company company);

        Task DeleteAsync(int companyId);

        /// <summary>
        /// Searches companies.
        /// </summary>
        Task<List<Company>> SearchAsync(string searchText);

        /// <summary>
        /// Returns paginated company list.
        /// </summary>
        Task<List<Company>> GetPagedAsync(int pageNumber, int pageSize);
    }
}