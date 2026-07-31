/*
==============================================================================
File        : CompanyService.cs

Module      : Company

Purpose     : Contains complete Company business logic.

Flow

MVC View
    ↓
Controller
    ↓
Service
    ↓
Repository
    ↓
SQL Server

Author      : Prasad Bukke
Project     : Ajay Industries ERP

==============================================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Application.Exceptions;

namespace AjayIndustriesERP.Application.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyService(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
        }

        /// <summary>
        /// Returns all active companies.
        /// </summary>
        public async Task<List<Company>> GetAllAsync()
        {
            return await _companyRepository.GetAllAsync();
        }

        /// <summary>
        /// Returns company by id.
        /// </summary>
        public async Task<Company?> GetByIdAsync(int companyId)
        {
            return await _companyRepository.GetByIdAsync(companyId);
        }

        /// <summary>
        /// Creates new company.
        /// </summary>
        public async Task CreateAsync(Company company)
        {
            company.CompanyName = company.CompanyName.Trim();
            company.GstNumber = company.GstNumber.Trim().ToUpper();

            if (!string.IsNullOrWhiteSpace(company.PanNumber))
                company.PanNumber = company.PanNumber.Trim().ToUpper();

            if (!string.IsNullOrWhiteSpace(company.PhoneNumber))
                company.PhoneNumber = company.PhoneNumber.Trim();

            if (!string.IsNullOrWhiteSpace(company.Email))
                company.Email = company.Email.Trim().ToLower();

            if (!string.IsNullOrWhiteSpace(company.Website))
                company.Website = company.Website.Trim();

            if (!string.IsNullOrWhiteSpace(company.ContactPerson))
                company.ContactPerson = company.ContactPerson.Trim();

            if (!string.IsNullOrWhiteSpace(company.Address))
                company.Address = company.Address.Trim();

            if (!string.IsNullOrWhiteSpace(company.City))
                company.City = company.City.Trim();

            if (!string.IsNullOrWhiteSpace(company.State))
                company.State = company.State.Trim();

            if (!string.IsNullOrWhiteSpace(company.Country))
                company.Country = company.Country.Trim();

            if (!string.IsNullOrWhiteSpace(company.PostalCode))
                company.PostalCode = company.PostalCode.Trim();

            if (await _companyRepository.ExistsByCompanyNameAsync(company.CompanyName))
                throw new BusinessException("Company Name already exists.");

            if (!string.IsNullOrWhiteSpace(company.PanNumber) &&
                await _companyRepository.ExistsByPanAsync(company.PanNumber))
            {
                throw new BusinessException("PAN Number already exists.");
            }

            if (!string.IsNullOrWhiteSpace(company.Website) &&
                await _companyRepository.ExistsByWebsiteAsync(company.Website))
            {
                throw new BusinessException("Website already exists.");
            }

            if (await _companyRepository.ExistsByGstAsync(company.GstNumber))
                throw new BusinessException("GST Number already exists.");

            if (!string.IsNullOrWhiteSpace(company.Email) &&
    await _companyRepository.ExistsByEmailAsync(company.Email, company.CompanyId))
            {
                throw new BusinessException("Email already exists.");
            }

            if (!string.IsNullOrWhiteSpace(company.PhoneNumber) &&
                await _companyRepository.ExistsByPhoneAsync(company.PhoneNumber, company.CompanyId))
            {
                throw new BusinessException("Phone Number already exists.");
            }

            company.CreatedOn = DateTime.UtcNow;
            company.CreatedBy = "System";

            company.CompanyCode = await GenerateCompanyCodeAsync();

            await _companyRepository.AddAsync(company);

            await _companyRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Updates company.
        /// </summary>
        public async Task UpdateAsync(Company company)
        {
            var existingCompany = await _companyRepository.GetByIdAsync(company.CompanyId);

            company.CompanyName = company.CompanyName.Trim();
            company.GstNumber = company.GstNumber.Trim().ToUpper();

            if (!string.IsNullOrWhiteSpace(company.PanNumber))
                company.PanNumber = company.PanNumber.Trim().ToUpper();

            if (!string.IsNullOrWhiteSpace(company.PhoneNumber))
                company.PhoneNumber = company.PhoneNumber.Trim();

            if (!string.IsNullOrWhiteSpace(company.Email))
                company.Email = company.Email.Trim().ToLower();

            if (!string.IsNullOrWhiteSpace(company.Website))
                company.Website = company.Website.Trim();

            if (!string.IsNullOrWhiteSpace(company.ContactPerson))
                company.ContactPerson = company.ContactPerson.Trim();

            if (!string.IsNullOrWhiteSpace(company.Address))
                company.Address = company.Address.Trim();

            if (!string.IsNullOrWhiteSpace(company.City))
                company.City = company.City.Trim();

            if (!string.IsNullOrWhiteSpace(company.State))
                company.State = company.State.Trim();

            if (!string.IsNullOrWhiteSpace(company.Country))
                company.Country = company.Country.Trim();

            if (!string.IsNullOrWhiteSpace(company.PostalCode))
                company.PostalCode = company.PostalCode.Trim();

            if (existingCompany == null)
                throw new BusinessException("Company not found.");

            if (await _companyRepository.ExistsByCodeAsync(company.CompanyCode, company.CompanyId))
                throw new BusinessException("Company Code already exists.");

            if (await _companyRepository.ExistsByCompanyNameAsync(
                company.CompanyName,
                company.CompanyId))
            {
                throw new BusinessException("Company Name already exists.");
            }

            if (!string.IsNullOrWhiteSpace(company.PanNumber) &&
                await _companyRepository.ExistsByPanAsync(
                    company.PanNumber,
                    company.CompanyId))
            {
                throw new BusinessException("PAN Number already exists.");
            }

            if (!string.IsNullOrWhiteSpace(company.Website) &&
                await _companyRepository.ExistsByWebsiteAsync(
                    company.Website,
                    company.CompanyId))
            {
                throw new BusinessException("Website already exists.");
            }

            if (await _companyRepository.ExistsByGstAsync(company.GstNumber, company.CompanyId))
                throw new BusinessException("GST Number already exists.");

            if (!string.IsNullOrWhiteSpace(company.Email) &&
    await _companyRepository.ExistsByEmailAsync(company.Email))
            {
                throw new BusinessException("Email already exists.");
            }

            if (!string.IsNullOrWhiteSpace(company.PhoneNumber) &&
                await _companyRepository.ExistsByPhoneAsync(company.PhoneNumber))
            {
                throw new BusinessException("Phone Number already exists.");
            }

            existingCompany.CompanyCode = company.CompanyCode;
            existingCompany.CompanyName = company.CompanyName;
            existingCompany.GstNumber = company.GstNumber;
            existingCompany.PanNumber = company.PanNumber;
            existingCompany.PhoneNumber = company.PhoneNumber;
            existingCompany.Email = company.Email;
            existingCompany.Website = company.Website;
            existingCompany.ContactPerson = company.ContactPerson;
            existingCompany.Address = company.Address;
            existingCompany.City = company.City;
            existingCompany.State = company.State;
            existingCompany.Country = company.Country;
            existingCompany.PostalCode = company.PostalCode;
            existingCompany.IsActive = company.IsActive;

            existingCompany.ModifiedOn = DateTime.UtcNow;
            existingCompany.ModifiedBy = "System";

            await _companyRepository.UpdateAsync(existingCompany);

            await _companyRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Soft deletes company.
        /// </summary>
        public async Task DeleteAsync(int companyId)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);

            if (company == null)
                throw new BusinessException("Company not found.");

            company.IsDeleted = true;
            company.ModifiedOn = DateTime.UtcNow;
            company.ModifiedBy = "System";

            await _companyRepository.DeleteAsync(company);





            await _companyRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Searches companies.
        /// </summary>
        public async Task<List<Company>> SearchAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return await _companyRepository.GetAllAsync();

            return await _companyRepository.SearchAsync(searchText);
        }


        /// <summary>
        /// Returns paginated company list.
        /// </summary>
        public async Task<PagedResult<Company>> GetPagedAsync(int pageNumber, int pageSize)
        {
            return await _companyRepository.GetPagedAsync(pageNumber, pageSize);
        }

        #region Private Methods

        /// <summary>
        /// Generates next company code.
        /// Example:
        /// CMP00001
        /// CMP00002
        /// </summary>
        private async Task<string> GenerateCompanyCodeAsync()
        {
            var lastCode = await _companyRepository.GetLastCompanyCodeAsync();

            if (string.IsNullOrWhiteSpace(lastCode))
                return "CMP00001";

            int number = int.Parse(lastCode.Replace("CMP", ""));

            number++;

            return $"CMP{number:D5}";
        }

        #endregion
    }
}