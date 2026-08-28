/*
==============================================================================
File        : CompanyService.cs

Module      : Company

Purpose     :
Contains complete Company Master business logic.

Responsibilities:
- Company CRUD business rules.
- Company master normalization.
- Duplicate validation.
- ISO certification persistence.
- Primary Bank Details persistence.
- Purchase Order Terms persistence.
- Invoice Terms persistence.

Important:
- Database access belongs to CompanyRepository.
- Controller must not contain Company business logic.
- Bank details currently represent one primary bank account.
==============================================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Services
{
    public class CompanyService
        : ICompanyService
    {
        #region Fields

        private readonly ICompanyRepository
            _companyRepository;

        #endregion


        #region Constructor

        public CompanyService(
            ICompanyRepository companyRepository)
        {
            _companyRepository =
                companyRepository;
        }

        #endregion


        #region Read Operations

        public async Task<List<Company>>
            GetAllAsync()
        {
            return await _companyRepository
                .GetAllAsync();
        }


        public async Task<Company?>
            GetByIdAsync(
                int companyId)
        {
            return await _companyRepository
                .GetByIdAsync(
                    companyId);
        }

        #endregion


        #region Create Company

        public async Task CreateAsync(
            Company company)
        {
            #region Normalize

            NormalizeCompany(
                company);

            #endregion


            #region Duplicate Validation

            if (
                await _companyRepository
                    .ExistsByCompanyNameAsync(
                        company.CompanyName)
            )
            {
                throw new BusinessException(
                    "Company Name already exists.");
            }


            if (
                !string.IsNullOrWhiteSpace(
                    company.PanNumber) &&
                await _companyRepository
                    .ExistsByPanAsync(
                        company.PanNumber)
            )
            {
                throw new BusinessException(
                    "PAN Number already exists.");
            }


            if (
                !string.IsNullOrWhiteSpace(
                    company.Website) &&
                await _companyRepository
                    .ExistsByWebsiteAsync(
                        company.Website)
            )
            {
                throw new BusinessException(
                    "Website already exists.");
            }


            if (
                await _companyRepository
                    .ExistsByGstAsync(
                        company.GstNumber)
            )
            {
                throw new BusinessException(
                    "GST Number already exists.");
            }


            if (
                !string.IsNullOrWhiteSpace(
                    company.Email) &&
                await _companyRepository
                    .ExistsByEmailAsync(
                        company.Email,
                        company.CompanyId)
            )
            {
                throw new BusinessException(
                    "Email already exists.");
            }


            if (
                !string.IsNullOrWhiteSpace(
                    company.PhoneNumber) &&
                await _companyRepository
                    .ExistsByPhoneAsync(
                        company.PhoneNumber,
                        company.CompanyId)
            )
            {
                throw new BusinessException(
                    "Phone Number already exists.");
            }

            #endregion


            #region Audit / Code

            company.CreatedOn =
                DateTime.UtcNow;

            company.CreatedBy =
                "System";

            company.CompanyCode =
                await GenerateCompanyCodeAsync();

            #endregion


            #region Save

            await _companyRepository
                .AddAsync(
                    company);

            await _companyRepository
                .SaveChangesAsync();

            #endregion
        }

        #endregion


        #region Update Company

        public async Task UpdateAsync(
            Company company)
        {
            #region Load Existing Company

            var existingCompany =
                await _companyRepository
                    .GetByIdAsync(
                        company.CompanyId);


            if (existingCompany == null)
            {
                throw new BusinessException(
                    "Company not found.");
            }

            #endregion


            #region Normalize

            NormalizeCompany(
                company);

            #endregion


            #region Duplicate Validation

            if (
                await _companyRepository
                    .ExistsByCodeAsync(
                        company.CompanyCode,
                        company.CompanyId)
            )
            {
                throw new BusinessException(
                    "Company Code already exists.");
            }


            if (
                await _companyRepository
                    .ExistsByCompanyNameAsync(
                        company.CompanyName,
                        company.CompanyId)
            )
            {
                throw new BusinessException(
                    "Company Name already exists.");
            }


            if (
                !string.IsNullOrWhiteSpace(
                    company.PanNumber) &&
                await _companyRepository
                    .ExistsByPanAsync(
                        company.PanNumber,
                        company.CompanyId)
            )
            {
                throw new BusinessException(
                    "PAN Number already exists.");
            }


            if (
                !string.IsNullOrWhiteSpace(
                    company.Website) &&
                await _companyRepository
                    .ExistsByWebsiteAsync(
                        company.Website,
                        company.CompanyId)
            )
            {
                throw new BusinessException(
                    "Website already exists.");
            }


            if (
                await _companyRepository
                    .ExistsByGstAsync(
                        company.GstNumber,
                        company.CompanyId)
            )
            {
                throw new BusinessException(
                    "GST Number already exists.");
            }


            if (
                !string.IsNullOrWhiteSpace(
                    company.Email) &&
                await _companyRepository
                    .ExistsByEmailAsync(
                        company.Email)
            )
            {
                throw new BusinessException(
                    "Email already exists.");
            }


            if (
                !string.IsNullOrWhiteSpace(
                    company.PhoneNumber) &&
                await _companyRepository
                    .ExistsByPhoneAsync(
                        company.PhoneNumber)
            )
            {
                throw new BusinessException(
                    "Phone Number already exists.");
            }

            #endregion


            #region Basic Information

            existingCompany.CompanyCode =
                company.CompanyCode;

            existingCompany.CompanyName =
                company.CompanyName;

            #endregion


            #region Statutory Information

            existingCompany.GstNumber =
                company.GstNumber;

            existingCompany.PanNumber =
                company.PanNumber;

            #endregion


            #region ISO Certification

            existingCompany.IsoCertificationNumber =
                company.IsoCertificationNumber;

            #endregion


            #region Contact Information

            existingCompany.PhoneNumber =
                company.PhoneNumber;

            existingCompany.Email =
                company.Email;

            existingCompany.Website =
                company.Website;

            existingCompany.ContactPerson =
                company.ContactPerson;

            #endregion


            #region Address

            existingCompany.Address =
                company.Address;

            existingCompany.City =
                company.City;

            existingCompany.State =
                company.State;

            existingCompany.Country =
                company.Country;

            existingCompany.PostalCode =
                company.PostalCode;

            #endregion


            #region Bank Details

            existingCompany.BankName =
                company.BankName;

            existingCompany.BankAccountHolderName =
                company.BankAccountHolderName;

            existingCompany.BankAccountNumber =
                company.BankAccountNumber;

            existingCompany.BankIfscCode =
                company.BankIfscCode;

            existingCompany.BankBranchName =
                company.BankBranchName;

            existingCompany.BankAccountType =
                company.BankAccountType;

            #endregion


            #region Terms And Conditions

            existingCompany.PurchaseOrderTermsAndConditions =
                company.PurchaseOrderTermsAndConditions;

            existingCompany.InvoiceTermsAndConditions =
                company.InvoiceTermsAndConditions;

            #endregion


            #region Status / Audit

            existingCompany.IsActive =
                company.IsActive;

            existingCompany.ModifiedOn =
                DateTime.UtcNow;

            existingCompany.ModifiedBy =
                "System";

            #endregion


            #region Save

            await _companyRepository
                .UpdateAsync(
                    existingCompany);

            await _companyRepository
                .SaveChangesAsync();

            #endregion
        }

        #endregion


        #region Delete Company

        public async Task DeleteAsync(
            int companyId)
        {
            var company =
                await _companyRepository
                    .GetByIdAsync(
                        companyId);


            if (company == null)
            {
                throw new BusinessException(
                    "Company not found.");
            }


            company.IsDeleted =
                true;

            company.ModifiedOn =
                DateTime.UtcNow;

            company.ModifiedBy =
                "System";


            await _companyRepository
                .DeleteAsync(
                    company);


            await _companyRepository
                .SaveChangesAsync();
        }

        #endregion


        #region Search

        public async Task<List<Company>>
            SearchAsync(
                string searchText)
        {
            if (string.IsNullOrWhiteSpace(
                searchText))
            {
                return await _companyRepository
                    .GetAllAsync();
            }


            return await _companyRepository
                .SearchAsync(
                    searchText);
        }


        public async Task<PagedResult<Company>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            return await _companyRepository
                .GetPagedAsync(
                    pageNumber,
                    pageSize);
        }

        #endregion


        #region Company Normalization

        private static void NormalizeCompany(
            Company company)
        {
            #region Required Values

            company.CompanyName =
                company.CompanyName
                    .Trim();

            company.GstNumber =
                company.GstNumber
                    .Trim()
                    .ToUpperInvariant();

            #endregion


            #region Statutory Information

            company.PanNumber =
                NormalizeUpper(
                    company.PanNumber);

            company.IsoCertificationNumber =
                NormalizeText(
                    company.IsoCertificationNumber);

            #endregion


            #region Contact Information

            company.PhoneNumber =
                NormalizeText(
                    company.PhoneNumber);

            company.Email =
                NormalizeLower(
                    company.Email);

            company.Website =
                NormalizeText(
                    company.Website);

            company.ContactPerson =
                NormalizeText(
                    company.ContactPerson);

            #endregion


            #region Address

            company.Address =
                NormalizeText(
                    company.Address);

            company.City =
                NormalizeText(
                    company.City);

            company.State =
                NormalizeText(
                    company.State);

            company.Country =
                NormalizeText(
                    company.Country);

            company.PostalCode =
                NormalizeText(
                    company.PostalCode);

            #endregion


            #region Bank Details

            company.BankName =
                NormalizeText(
                    company.BankName);

            company.BankAccountHolderName =
                NormalizeText(
                    company.BankAccountHolderName);

            company.BankAccountNumber =
                NormalizeText(
                    company.BankAccountNumber);

            company.BankIfscCode =
                NormalizeUpper(
                    company.BankIfscCode);

            company.BankBranchName =
                NormalizeText(
                    company.BankBranchName);

            company.BankAccountType =
                NormalizeText(
                    company.BankAccountType);

            #endregion


            #region Terms And Conditions

            company.PurchaseOrderTermsAndConditions =
                NormalizeText(
                    company.PurchaseOrderTermsAndConditions);

            company.InvoiceTermsAndConditions =
                NormalizeText(
                    company.InvoiceTermsAndConditions);

            #endregion
        }

        #endregion


        #region Text Helpers

        private static string?
            NormalizeText(
                string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? null
                : value.Trim();
        }


        private static string?
            NormalizeUpper(
                string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? null
                : value
                    .Trim()
                    .ToUpperInvariant();
        }


        private static string?
            NormalizeLower(
                string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? null
                : value
                    .Trim()
                    .ToLowerInvariant();
        }

        #endregion


        #region Company Code Generation

        /// <summary>
        /// Generates next Company Code.
        /// Example:
        /// CMP00001
        /// CMP00002
        /// </summary>
        private async Task<string>
            GenerateCompanyCodeAsync()
        {
            var lastCode =
                await _companyRepository
                    .GetLastCompanyCodeAsync();


            if (string.IsNullOrWhiteSpace(
                lastCode))
            {
                return "CMP00001";
            }


            var number =
                int.Parse(
                    lastCode.Replace(
                        "CMP",
                        string.Empty));


            number++;


            return
                $"CMP{number:D5}";
        }

        #endregion
    }
}