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
    }
}