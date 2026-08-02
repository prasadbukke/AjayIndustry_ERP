using AjayIndustriesERP.Application.Common;
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

        Task<bool> ExistsByEmailAsync(string email);

        Task<bool> ExistsByEmailAsync(string email, int companyId);

        Task<bool> ExistsByPhoneAsync(string phoneNumber);

        Task<bool> ExistsByPhoneAsync(string phoneNumber, int companyId);

        Task<bool> ExistsByCompanyNameAsync(string companyName);
        Task<bool> ExistsByCompanyNameAsync(string companyName, int companyId);

        Task<bool> ExistsByPanAsync(string panNumber);
        Task<bool> ExistsByPanAsync(string panNumber, int companyId);

        Task<bool> ExistsByWebsiteAsync(string website);
        Task<bool> ExistsByWebsiteAsync(string website, int companyId);

        /// <summary>
        /// Searches companies by Company Code, Company Name or GST Number.
        /// </summary>
        Task<List<Company>> SearchAsync(string searchText);

        /// <summary>
        /// Returns paginated company list.
        /// </summary>
        Task<PagedResult<Company>> GetPagedAsync(int pageNumber, int pageSize);
        /// <summary>
        /// Returns last generated company code.
        /// </summary>
        Task<string?> GetLastCompanyCodeAsync();
        Task SaveChangesAsync();
    }
}