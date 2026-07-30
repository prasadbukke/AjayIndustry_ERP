using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Interfaces
{
    public interface ICompanyRepository
    {
        Task<List<Company>> GetAllAsync();

        Task<Company?> GetByIdAsync(int companyId);

        Task AddAsync(Company company);

        Task UpdateAsync(Company company);

        Task DeleteAsync(Company company);

        Task<bool> ExistsByCodeAsync(string companyCode);

        Task<bool> ExistsByGstAsync(string gstNumber);

        Task<bool> ExistsByCodeAsync(string companyCode, int companyId);

        Task<bool> ExistsByGstAsync(string gstNumber, int companyId);

        Task SaveChangesAsync();
    }
}