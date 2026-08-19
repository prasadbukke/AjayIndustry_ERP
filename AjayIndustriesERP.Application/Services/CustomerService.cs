/*
============================================================
File: CustomerService.cs

Purpose:
Implements Customer Master business rules.

Responsibilities:
- Retrieve Customer records.
- Search and paginate Customers.
- Generate Customer Code automatically.
- Normalize Customer data before saving.
- Validate required Customer information.
- Prevent duplicate GSTIN.
- Create and update Customer records.
- Soft-delete Customer records.

Important:
- All Customer business rules belong in this Service.
- Database access is performed only through ICustomerRepository.
- Customer Code format:
      AI/CUS/00001
      AI/CUS/00002
      AI/CUS/00003
- Customer Codes are never reused.
- GSTIN is optional.
- Soft delete is used instead of physical database deletion.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;

namespace AjayIndustriesERP.Application.Services
{
    public class CustomerService
        : ICustomerService
    {
        #region Fields

        private readonly ICustomerRepository _repository;

        #endregion


        #region Constructor

        public CustomerService(
            ICustomerRepository repository)
        {
            _repository = repository;
        }

        #endregion


        #region Read Operations

        public async Task<List<Customer>>
            GetAllAsync()
        {
            return await _repository
                .GetAllAsync();
        }


        public async Task<Customer?>
            GetByIdAsync(
                int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await _repository
                .GetByIdAsync(id);
        }

        #endregion


        #region Search And Pagination

        public async Task<List<Customer>>
            SearchAsync(
                string searchText)
        {
            if (string.IsNullOrWhiteSpace(
                searchText))
            {
                return await _repository
                    .GetAllAsync();
            }


            return await _repository
                .SearchAsync(
                    searchText.Trim());
        }


        public async Task<PagedResult<Customer>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            NormalizePagination(
                ref pageNumber,
                ref pageSize);


            return await _repository
                .GetPagedAsync(
                    pageNumber,
                    pageSize);
        }


        public async Task<PagedResult<Customer>>
            SearchPagedAsync(
                string searchText,
                int pageNumber,
                int pageSize)
        {
            NormalizePagination(
                ref pageNumber,
                ref pageSize);


            if (string.IsNullOrWhiteSpace(
                searchText))
            {
                return await _repository
                    .GetPagedAsync(
                        pageNumber,
                        pageSize);
            }


            return await _repository
                .SearchPagedAsync(
                    searchText.Trim(),
                    pageNumber,
                    pageSize);
        }

        #endregion


        #region Create Customer

        public async Task<Customer>
            CreateAsync(
                Customer customer)
        {
            if (customer == null)
            {
                throw new BusinessException(
                    "Customer information is required.");
            }


            NormalizeCustomer(customer);

            ValidateCustomer(customer);

            await ValidateUniqueFieldsAsync(
    customer);


            customer.Code =
                await GenerateCustomerCodeAsync();


            customer.IsActive = true;
            customer.IsDeleted = false;

            customer.CreatedOn =
                DateTime.UtcNow;

            customer.CreatedBy =
                "System";


            customer.ModifiedOn = null;
            customer.ModifiedBy = null;


            await _repository
                .AddAsync(customer);


            return customer;
        }

        #endregion


        #region Update Customer

        public async Task<Customer>
            UpdateAsync(
                Customer customer)
        {
            if (customer == null ||
                customer.Id <= 0)
            {
                throw new BusinessException(
                    "Invalid customer.");
            }


            var existing =
                await _repository
                    .GetForUpdateAsync(
                        customer.Id);


            if (existing == null)
            {
                throw new BusinessException(
                    "Customer not found.");
            }


            NormalizeCustomer(customer);

            ValidateCustomer(customer);


            await ValidateUniqueFieldsAsync(
    customer,
    existing.Id);


            // =================================================
            // CUSTOMER CODE
            // =================================================
            //
            // Customer Code is immutable after creation.
            // Never trust Code posted back from the UI.
            // =================================================

            var originalCode =
                existing.Code;


            // =================================================
            // CUSTOMER INFORMATION
            // =================================================

            existing.CustomerName =
                customer.CustomerName;

            existing.LegalName =
                customer.LegalName;


            // =================================================
            // TAX INFORMATION
            // =================================================

            existing.GSTIN =
                customer.GSTIN;

            existing.PAN =
                customer.PAN;


            // =================================================
            // PRIMARY CONTACT
            // =================================================

            existing.ContactPerson =
                customer.ContactPerson;

            existing.MobileNumber =
                customer.MobileNumber;

            existing.AlternateMobileNumber =
                customer.AlternateMobileNumber;

            existing.Email =
                customer.Email;


            // =================================================
            // PRIMARY ADDRESS
            // =================================================

            existing.AddressLine1 =
                customer.AddressLine1;

            existing.AddressLine2 =
                customer.AddressLine2;

            existing.City =
                customer.City;

            existing.District =
                customer.District;

            existing.State =
                customer.State;

            existing.Pincode =
                customer.Pincode;

            existing.Country =
                customer.Country;


            // =================================================
            // COMMERCIAL INFORMATION
            // =================================================

            existing.PaymentTerms =
                customer.PaymentTerms;

            existing.CreditDays =
                customer.CreditDays;


            // =================================================
            // OTHER INFORMATION
            // =================================================

            existing.Website =
                customer.Website;

            existing.Remarks =
                customer.Remarks;


            // =================================================
            // AUDIT
            // =================================================

            existing.Code =
                originalCode;

            existing.ModifiedOn =
                DateTime.UtcNow;

            existing.ModifiedBy =
                "System";


            await _repository
                .UpdateAsync(existing);


            return existing;
        }

        #endregion


        #region Delete Customer

        public async Task DeleteAsync(
            int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid customer.");
            }


            var customer =
                await _repository
                    .GetForUpdateAsync(id);


            if (customer == null)
            {
                throw new BusinessException(
                    "Customer not found.");
            }


            customer.IsDeleted = true;
            customer.IsActive = false;

            customer.ModifiedOn =
                DateTime.UtcNow;

            customer.ModifiedBy =
                "System";


            await _repository
                .UpdateAsync(customer);
        }

        #endregion


        #region Business Validation

        private static void ValidateCustomer(
            Customer customer)
        {
            if (string.IsNullOrWhiteSpace(
                customer.CustomerName))
            {
                throw new BusinessException(
                    "Customer Name is required.");
            }


            if (string.IsNullOrWhiteSpace(
                customer.AddressLine1))
            {
                throw new BusinessException(
                    "Address Line 1 is required.");
            }


            if (string.IsNullOrWhiteSpace(
                customer.City))
            {
                throw new BusinessException(
                    "City is required.");
            }


            if (string.IsNullOrWhiteSpace(
                customer.State))
            {
                throw new BusinessException(
                    "State is required.");
            }


            if (string.IsNullOrWhiteSpace(
                customer.Pincode))
            {
                throw new BusinessException(
                    "Pincode is required.");
            }


            if (string.IsNullOrWhiteSpace(
                customer.Country))
            {
                throw new BusinessException(
                    "Country is required.");
            }


            if (customer.CreditDays.HasValue &&
                customer.CreditDays.Value < 0)
            {
                throw new BusinessException(
                    "Credit Days cannot be negative.");
            }

            #region Mobile Validation

            if (!string.IsNullOrWhiteSpace(
                    customer.MobileNumber) &&
                !string.IsNullOrWhiteSpace(
                    customer.AlternateMobileNumber) &&
                customer.MobileNumber ==
                    customer.AlternateMobileNumber)
            {
                throw new BusinessException(
                    "Mobile Number and Alternate Mobile Number cannot be the same.");
            }

            #endregion
        }


        private async Task ValidateUniqueFieldsAsync(
    Customer customer,
    int? excludeCustomerId = null)
        {
            #region GSTIN Duplicate

            if (!string.IsNullOrWhiteSpace(
                customer.GSTIN))
            {
                var gstinExists =
                    await _repository
                        .GSTINExistsAsync(
                            customer.GSTIN,
                            excludeCustomerId);


                if (gstinExists)
                {
                    throw new BusinessException(
                        "A customer with the same GSTIN already exists.");
                }
            }

            #endregion


            #region Email Duplicate

            if (!string.IsNullOrWhiteSpace(
                customer.Email))
            {
                var emailExists =
                    await _repository
                        .EmailExistsAsync(
                            customer.Email,
                            excludeCustomerId);


                if (emailExists)
                {
                    throw new BusinessException(
                        "A customer with the same Email Address already exists.");
                }
            }

            #endregion


            #region Primary Mobile Duplicate

            if (!string.IsNullOrWhiteSpace(
                customer.MobileNumber))
            {
                var mobileExists =
                    await _repository
                        .MobileNumberExistsAsync(
                            customer.MobileNumber,
                            excludeCustomerId);


                if (mobileExists)
                {
                    throw new BusinessException(
                        "A customer with the same Mobile Number already exists.");
                }
            }

            #endregion


            #region Alternate Mobile Duplicate

            if (!string.IsNullOrWhiteSpace(
                customer.AlternateMobileNumber))
            {
                var alternateExists =
                    await _repository
                        .MobileNumberExistsAsync(
                            customer.AlternateMobileNumber,
                            excludeCustomerId);


                if (alternateExists)
                {
                    throw new BusinessException(
                        "A customer with the same Alternate Mobile Number already exists.");
                }
            }

            #endregion
        }

        #endregion


        #region Data Normalization

        private static void NormalizeCustomer(
            Customer customer)
        {
            customer.CustomerName =
                customer.CustomerName
                    ?.Trim()
                ?? string.Empty;


            customer.LegalName =
                NormalizeOptional(
                    customer.LegalName);


            // =================================================
            // TAX
            // =================================================

            customer.GSTIN =
                NormalizeOptionalUpper(
                    customer.GSTIN);

            customer.PAN =
                NormalizeOptionalUpper(
                    customer.PAN);


            // =================================================
            // CONTACT
            // =================================================

            customer.ContactPerson =
                NormalizeOptional(
                    customer.ContactPerson);

            customer.MobileNumber =
                NormalizeOptional(
                    customer.MobileNumber);

            customer.AlternateMobileNumber =
                NormalizeOptional(
                    customer.AlternateMobileNumber);

            customer.Email =
    string.IsNullOrWhiteSpace(
        customer.Email)
        ? null
        : customer.Email
            .Trim()
            .ToLowerInvariant();


            // =================================================
            // ADDRESS
            // =================================================

            customer.AddressLine1 =
                customer.AddressLine1
                    ?.Trim()
                ?? string.Empty;

            customer.AddressLine2 =
                NormalizeOptional(
                    customer.AddressLine2);

            customer.City =
                customer.City
                    ?.Trim()
                ?? string.Empty;

            customer.District =
                NormalizeOptional(
                    customer.District);

            customer.State =
                customer.State
                    ?.Trim()
                ?? string.Empty;

            customer.Pincode =
                customer.Pincode
                    ?.Trim()
                ?? string.Empty;

            customer.Country =
                string.IsNullOrWhiteSpace(
                    customer.Country)
                    ? "India"
                    : customer.Country.Trim();


            // =================================================
            // COMMERCIAL
            // =================================================

            customer.PaymentTerms =
                NormalizeOptional(
                    customer.PaymentTerms);


            // =================================================
            // OTHER
            // =================================================

            customer.Website =
                NormalizeOptional(
                    customer.Website);

            customer.Remarks =
                NormalizeOptional(
                    customer.Remarks);
        }


        private static string?
            NormalizeOptional(
                string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? null
                : value.Trim();
        }


        private static string?
            NormalizeOptionalUpper(
                string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? null
                : value
                    .Trim()
                    .ToUpperInvariant();
        }

        #endregion


        #region Customer Code Generation

        private async Task<string>
            GenerateCustomerCodeAsync()
        {
            const string prefix =
                "AI/CUS/";


            var lastCode =
                await _repository
                    .GetLastCustomerCodeAsync();


            if (string.IsNullOrWhiteSpace(
                lastCode))
            {
                return $"{prefix}00001";
            }


            var numberPart =
                lastCode
                    .Substring(prefix.Length);


            if (!int.TryParse(
                numberPart,
                out var lastNumber))
            {
                throw new BusinessException(
                    "Unable to generate Customer Code.");
            }


            var nextNumber =
                lastNumber + 1;


            return
                $"{prefix}{nextNumber:00000}";
        }

        #endregion


        #region Pagination Helpers

        private static void NormalizePagination(
            ref int pageNumber,
            ref int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }


            if (pageSize != 10 &&
                pageSize != 25 &&
                pageSize != 50)
            {
                pageSize = 10;
            }
        }

        #endregion
    }
}