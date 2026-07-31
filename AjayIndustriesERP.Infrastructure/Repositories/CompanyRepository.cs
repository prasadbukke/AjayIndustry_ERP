using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApplicationDbContext _context;

        public CompanyRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Company>> GetAllAsync()
        {
            return await _context.Companies
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.CompanyName)
                .ToListAsync();
        }

        public async Task<Company?> GetByIdAsync(int companyId)
        {
            return await _context.Companies
                .FirstOrDefaultAsync(x =>
                    x.CompanyId == companyId &&
                    !x.IsDeleted);
        }

        public async Task AddAsync(Company company)
        {
            await _context.Companies.AddAsync(company);
        }

        public Task UpdateAsync(Company company)
        {
            _context.Companies.Update(company);

            return Task.CompletedTask;
        }

        public Task DeleteAsync(Company company)
        {
            company.IsDeleted = true;

            _context.Companies.Update(company);

            return Task.CompletedTask;
        }

        public async Task<bool> ExistsByCodeAsync(string companyCode)
        {
            return await _context.Companies.AnyAsync(x =>
                x.CompanyCode == companyCode &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByGstAsync(string gstNumber)
        {
            return await _context.Companies.AnyAsync(x =>
                x.GstNumber == gstNumber &&
                !x.IsDeleted);
        }

        /// <summary>
        /// Searches companies by Company Code, Company Name or GST Number.
        /// </summary>
        public async Task<List<Company>> SearchAsync(string searchText)
        {
            searchText = searchText.Trim().ToLower();

            return await _context.Companies
                .Where(x =>
                    !x.IsDeleted &&
                    (
                        x.CompanyCode.ToLower().Contains(searchText) ||
                        x.CompanyName.ToLower().Contains(searchText) ||
                        x.GstNumber.ToLower().Contains(searchText)
                    ))
                .OrderBy(x => x.CompanyName)
                .ToListAsync();
        }

        /// <summary>
        /// Returns paginated company list.
        /// </summary>
        public async Task<PagedResult<Company>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = _context.Companies
                .Where(x => !x.IsDeleted);

            var totalRecords = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.CompanyName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Company>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }


        /// <summary>
        /// Returns last generated company code.
        /// </summary>
        public async Task<string?> GetLastCompanyCodeAsync()
        {
            return await _context.Companies
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CompanyId)
                .Select(x => x.CompanyCode)
                .FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsByCodeAsync(string companyCode, int companyId)
        {
            return await _context.Companies.AnyAsync(x =>
                x.CompanyCode == companyCode &&
                x.CompanyId != companyId &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByGstAsync(string gstNumber, int companyId)
        {
            return await _context.Companies.AnyAsync(x =>
                x.GstNumber == gstNumber &&
                x.CompanyId != companyId &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _context.Companies.AnyAsync(x =>
                x.Email == email &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByEmailAsync(string email, int companyId)
        {
            return await _context.Companies.AnyAsync(x =>
                x.Email == email &&
                x.CompanyId != companyId &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByPhoneAsync(string phoneNumber)
        {
            return await _context.Companies.AnyAsync(x =>
                x.PhoneNumber == phoneNumber &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByPhoneAsync(string phoneNumber, int companyId)
        {
            return await _context.Companies.AnyAsync(x =>
                x.PhoneNumber == phoneNumber &&
                x.CompanyId != companyId &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByCompanyNameAsync(string companyName)
        {
            return await _context.Companies.AnyAsync(x =>
                x.CompanyName == companyName &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByCompanyNameAsync(string companyName, int companyId)
        {
            return await _context.Companies.AnyAsync(x =>
                x.CompanyName == companyName &&
                x.CompanyId != companyId &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByPanAsync(string panNumber)
        {
            return await _context.Companies.AnyAsync(x =>
                x.PanNumber == panNumber &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByPanAsync(string panNumber, int companyId)
        {
            return await _context.Companies.AnyAsync(x =>
                x.PanNumber == panNumber &&
                x.CompanyId != companyId &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByWebsiteAsync(string website)
        {
            return await _context.Companies.AnyAsync(x =>
                x.Website == website &&
                !x.IsDeleted);
        }

        public async Task<bool> ExistsByWebsiteAsync(string website, int companyId)
        {
            return await _context.Companies.AnyAsync(x =>
                x.Website == website &&
                x.CompanyId != companyId &&
                !x.IsDeleted);
        }
    }
}