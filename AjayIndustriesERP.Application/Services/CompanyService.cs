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

using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;

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
            

            if (await _companyRepository.ExistsByGstAsync(company.GstNumber))
                throw new Exception("GST Number already exists.");

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

            if (existingCompany == null)
                throw new Exception("Company not found.");

            if (await _companyRepository.ExistsByCodeAsync(company.CompanyCode, company.CompanyId))
                throw new Exception("Company Code already exists.");

            if (await _companyRepository.ExistsByGstAsync(company.GstNumber, company.CompanyId))
                throw new Exception("GST Number already exists.");

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
                throw new Exception("Company not found.");

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
        public async Task<List<Company>> GetPagedAsync(int pageNumber, int pageSize)
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