/*
==============================================================

File : SupplierService.cs

Purpose :
Contains Supplier Master business rules.

Features :
- SUP00001 automatic Supplier Code
- Supplier Name duplicate prevention
- GSTIN duplicate prevention
- Basic GSTIN / PAN validation
- Contact data normalization
- Soft Delete

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace AjayIndustriesERP.Application.Services
{
    /// <summary>
    /// Provides Supplier Master business operations.
    /// </summary>
    public class SupplierService :
        ISupplierService
    {
        private readonly ISupplierRepository
            _supplierRepository;

        public SupplierService(
            ISupplierRepository supplierRepository)
        {
            _supplierRepository =
                supplierRepository;
        }

        #region Read Operations

        public async Task<List<Supplier>>
            GetAllAsync()
        {
            return await _supplierRepository
                .GetAllAsync();
        }

        public async Task<Supplier?> GetByIdAsync(
            int supplierId)
        {
            return await _supplierRepository
                .GetByIdAsync(supplierId);
        }

        public async Task<List<Supplier>> SearchAsync(
            string searchText)
        {
            if (string.IsNullOrWhiteSpace(
                searchText))
            {
                return await _supplierRepository
                    .GetAllAsync();
            }

            return await _supplierRepository
                .SearchAsync(searchText);
        }

        public async Task<PagedResult<Supplier>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 10;
            }

            return await _supplierRepository
                .GetPagedAsync(
                    pageNumber,
                    pageSize);
        }

        #endregion

        #region Create Supplier

        public async Task CreateAsync(
            Supplier supplier)
        {
            NormalizeSupplier(supplier);

            ValidateSupplier(supplier);

            if (await _supplierRepository
                .ExistsByNameAsync(
                    supplier.SupplierName))
            {
                throw new BusinessException(
                    "Supplier Name already exists.");
            }

            if (!string.IsNullOrWhiteSpace(
                    supplier.Gstin) &&
                await _supplierRepository
                    .ExistsByGstinAsync(
                        supplier.Gstin))
            {
                throw new BusinessException(
                    "GSTIN is already registered with another Supplier.");
            }

            supplier.SupplierCode =
                await GenerateSupplierCodeAsync();

            supplier.CreatedOn =
                DateTime.UtcNow;

            supplier.CreatedBy =
                "System";

            supplier.IsDeleted =
                false;

            await _supplierRepository
                .AddAsync(supplier);

            await _supplierRepository
                .SaveChangesAsync();
        }

        #endregion

        #region Update Supplier

        public async Task UpdateAsync(
            Supplier supplier)
        {
            var existingSupplier =
                await _supplierRepository
                    .GetByIdAsync(
                        supplier.SupplierId);

            if (existingSupplier == null)
            {
                throw new BusinessException(
                    "Supplier not found.");
            }

            NormalizeSupplier(supplier);

            ValidateSupplier(supplier);

            if (await _supplierRepository
                .ExistsByNameAsync(
                    supplier.SupplierName,
                    supplier.SupplierId))
            {
                throw new BusinessException(
                    "Supplier Name already exists.");
            }

            if (!string.IsNullOrWhiteSpace(
                    supplier.Gstin) &&
                await _supplierRepository
                    .ExistsByGstinAsync(
                        supplier.Gstin,
                        supplier.SupplierId))
            {
                throw new BusinessException(
                    "GSTIN is already registered with another Supplier.");
            }

            /*
             * Supplier Code is intentionally preserved.
             * It is generated once and never edited.
             */
            existingSupplier.SupplierName =
                supplier.SupplierName;

            existingSupplier.ContactPerson =
                supplier.ContactPerson;

            existingSupplier.MobileNumber =
                supplier.MobileNumber;

            existingSupplier.AlternateMobileNumber =
                supplier.AlternateMobileNumber;

            existingSupplier.Email =
                supplier.Email;

            existingSupplier.Gstin =
                supplier.Gstin;

            existingSupplier.Pan =
                supplier.Pan;

            existingSupplier.AddressLine1 =
                supplier.AddressLine1;

            existingSupplier.AddressLine2 =
                supplier.AddressLine2;

            existingSupplier.City =
                supplier.City;

            existingSupplier.State =
                supplier.State;

            existingSupplier.Pincode =
                supplier.Pincode;

            existingSupplier.PaymentTermsDays =
                supplier.PaymentTermsDays;

            existingSupplier.Description =
                supplier.Description;

            existingSupplier.IsActive =
                supplier.IsActive;

            existingSupplier.ModifiedOn =
                DateTime.UtcNow;

            existingSupplier.ModifiedBy =
                "System";

            await _supplierRepository
                .UpdateAsync(
                    existingSupplier);

            await _supplierRepository
                .SaveChangesAsync();
        }

        #endregion

        #region Delete Supplier

        public async Task DeleteAsync(
            int supplierId)
        {
            var supplier =
                await _supplierRepository
                    .GetByIdAsync(
                        supplierId);

            if (supplier == null)
            {
                throw new BusinessException(
                    "Supplier not found.");
            }

            supplier.ModifiedOn =
                DateTime.UtcNow;

            supplier.ModifiedBy =
                "System";

            await _supplierRepository
                .DeleteAsync(supplier);

            await _supplierRepository
                .SaveChangesAsync();
        }

        #endregion

        #region Normalization

        private static void NormalizeSupplier(
            Supplier supplier)
        {
            supplier.SupplierName =
                NormalizeText(
                    supplier.SupplierName)
                ?? string.Empty;

            supplier.ContactPerson =
                NormalizeText(
                    supplier.ContactPerson);

            supplier.MobileNumber =
                NormalizeText(
                    supplier.MobileNumber);

            supplier.AlternateMobileNumber =
                NormalizeText(
                    supplier.AlternateMobileNumber);

            supplier.Email =
                NormalizeLowerText(
                    supplier.Email);

            supplier.Gstin =
                NormalizeUpperText(
                    supplier.Gstin);

            supplier.Pan =
                NormalizeUpperText(
                    supplier.Pan);

            supplier.AddressLine1 =
                NormalizeText(
                    supplier.AddressLine1);

            supplier.AddressLine2 =
                NormalizeText(
                    supplier.AddressLine2);

            supplier.City =
                NormalizeText(
                    supplier.City);

            supplier.State =
                NormalizeText(
                    supplier.State);

            supplier.Pincode =
                NormalizeText(
                    supplier.Pincode);

            supplier.Description =
                NormalizeText(
                    supplier.Description);

            if (supplier.PaymentTermsDays.HasValue &&
                supplier.PaymentTermsDays.Value < 0)
            {
                supplier.PaymentTermsDays = null;
            }
        }

        private static string? NormalizeText(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return Regex.Replace(
                value.Trim(),
                @"\s+",
                " ");
        }

        private static string? NormalizeUpperText(
            string? value)
        {
            var normalized =
                NormalizeText(value);

            return normalized?
                .ToUpperInvariant();
        }

        private static string? NormalizeLowerText(
            string? value)
        {
            var normalized =
                NormalizeText(value);

            return normalized?
                .ToLowerInvariant();
        }

        #endregion

        #region Validation

        private static void ValidateSupplier(
            Supplier supplier)
        {
            #region Supplier Name

            if (string.IsNullOrWhiteSpace(
                supplier.SupplierName))
            {
                throw new BusinessException(
                    "Supplier Name is required.");
            }

            if (supplier.SupplierName.Length > 150)
            {
                throw new BusinessException(
                    "Supplier Name cannot exceed 150 characters.");
            }

            #endregion

            #region Contact Person

            if (supplier.ContactPerson?.Length > 100)
            {
                throw new BusinessException(
                    "Contact Person cannot exceed 100 characters.");
            }

            #endregion

            #region Mobile

            ValidateMobileNumber(
                supplier.MobileNumber,
                "Mobile Number");

            ValidateMobileNumber(
                supplier.AlternateMobileNumber,
                "Alternate Mobile Number");

            #endregion

            #region Email

            if (!string.IsNullOrWhiteSpace(
                supplier.Email))
            {
                if (supplier.Email.Length > 150 ||
                    !IsValidEmail(
                        supplier.Email))
                {
                    throw new BusinessException(
                        "Please enter a valid Email address.");
                }
            }

            #endregion

            #region GSTIN

            if (!string.IsNullOrWhiteSpace(
                supplier.Gstin) &&
                !Regex.IsMatch(
                    supplier.Gstin,
                    @"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z][1-9A-Z]Z[0-9A-Z]$"))
            {
                throw new BusinessException(
                    "Please enter a valid 15-character GSTIN.");
            }

            #endregion

            #region PAN

            if (!string.IsNullOrWhiteSpace(
                supplier.Pan) &&
                !Regex.IsMatch(
                    supplier.Pan,
                    @"^[A-Z]{5}[0-9]{4}[A-Z]$"))
            {
                throw new BusinessException(
                    "Please enter a valid 10-character PAN.");
            }

            #endregion

            #region Address

            if (supplier.AddressLine1?.Length > 200 ||
                supplier.AddressLine2?.Length > 200)
            {
                throw new BusinessException(
                    "Address Line cannot exceed 200 characters.");
            }

            if (supplier.City?.Length > 100)
            {
                throw new BusinessException(
                    "City cannot exceed 100 characters.");
            }

            if (supplier.State?.Length > 100)
            {
                throw new BusinessException(
                    "State cannot exceed 100 characters.");
            }

            if (supplier.Pincode?.Length > 10)
            {
                throw new BusinessException(
                    "Pincode cannot exceed 10 characters.");
            }

            #endregion

            #region Payment Terms

            if (supplier.PaymentTermsDays.HasValue &&
                supplier.PaymentTermsDays.Value > 3650)
            {
                throw new BusinessException(
                    "Payment Terms Days cannot exceed 3650.");
            }

            #endregion

            #region Description

            if (supplier.Description?.Length > 500)
            {
                throw new BusinessException(
                    "Description cannot exceed 500 characters.");
            }

            #endregion
        }

        private static void ValidateMobileNumber(
            string? mobileNumber,
            string fieldName)
        {
            if (string.IsNullOrWhiteSpace(
                mobileNumber))
            {
                return;
            }

            if (mobileNumber.Length > 20)
            {
                throw new BusinessException(
                    $"{fieldName} cannot exceed 20 characters.");
            }

            /*
             * Allows:
             * 9876543210
             * +91 9876543210
             * 020-12345678
             */
            if (!Regex.IsMatch(
                mobileNumber,
                @"^[0-9+\-\s()]+$"))
            {
                throw new BusinessException(
                    $"Please enter a valid {fieldName}.");
            }
        }

        private static bool IsValidEmail(
            string email)
        {
            try
            {
                var address =
                    new MailAddress(email);

                return string.Equals(
                    address.Address,
                    email,
                    StringComparison
                        .OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Supplier Code Generation

        private async Task<string>
            GenerateSupplierCodeAsync()
        {
            var lastCode =
                await _supplierRepository
                    .GetLastSupplierCodeAsync();

            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(
                lastCode))
            {
                var numberPart =
                    lastCode
                        .Replace(
                            "SUP",
                            string.Empty,
                            StringComparison
                                .OrdinalIgnoreCase)
                        .Trim();

                if (int.TryParse(
                    numberPart,
                    out var lastNumber))
                {
                    nextNumber =
                        lastNumber + 1;
                }
            }

            var supplierCode =
                $"SUP{nextNumber:D5}";

            /*
             * Defensive collision check.
             * Deleted Supplier Codes are included.
             */
            while (await _supplierRepository
                .ExistsByCodeAsync(
                    supplierCode))
            {
                nextNumber++;

                supplierCode =
                    $"SUP{nextNumber:D5}";
            }

            return supplierCode;
        }

        #endregion
    }
}