/*
============================================================
File: CustomerRepository.cs

Purpose:
Provides Entity Framework Core data access for Customer Master.

Responsibilities:
- Retrieve active customers.
- Retrieve customers for Details and Edit.
- Search customers using common business fields.
- Provide paginated Customer Index data.
- Check duplicate GSTIN.
- Retrieve the last generated Customer Code.
- Persist Customer changes.

Important:
- Database access belongs only in Repository layer.
- Business validations belong in CustomerService.
- Soft-deleted customers are excluded from normal queries.
- Deleted Customer Codes are still considered while generating
  future codes so that old codes are never reused.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;

using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class CustomerRepository
        : ICustomerRepository
    {
        #region Fields

        private readonly ApplicationDbContext _context;

        #endregion


        #region Constructor

        public CustomerRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        #endregion


        #region Read Operations

        public async Task<List<Customer>> GetAllAsync()
        {
            return await _context.Customers
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive)
                .OrderBy(x =>
                    x.CustomerName)
                .ThenBy(x =>
                    x.Code)
                .ToListAsync();
        }


        public async Task<Customer?> GetByIdAsync(
            int id)
        {
            return await _context.Customers
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }


        public async Task<Customer?> GetForUpdateAsync(
            int id)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        #endregion


        #region Search Operations

        public async Task<List<Customer>> SearchAsync(
            string searchText)
        {
            if (string.IsNullOrWhiteSpace(
                searchText))
            {
                return await GetAllAsync();
            }

            var search =
                searchText
                    .Trim()
                    .ToLower();


            return await _context.Customers
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted &&
                    x.IsActive &&
                    (
                        x.Code
                            .ToLower()
                            .Contains(search)

                        ||

                        x.CustomerName
                            .ToLower()
                            .Contains(search)

                        ||

                        (
                            x.LegalName != null &&
                            x.LegalName
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.GSTIN != null &&
                            x.GSTIN
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.ContactPerson != null &&
                            x.ContactPerson
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        (
                            x.MobileNumber != null &&
                            x.MobileNumber
                                .ToLower()
                                .Contains(search)
                        )

                        ||

                        x.City
                            .ToLower()
                            .Contains(search)

                        ||

                        x.State
                            .ToLower()
                            .Contains(search)
                    ))
                .OrderBy(x =>
                    x.CustomerName)
                .ThenBy(x =>
                    x.Code)
                .ToListAsync();
        }

        #endregion


        #region Pagination

        public async Task<PagedResult<Customer>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            var query =
                _context.Customers
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive);


            var totalRecords =
                await query.CountAsync();


            var customers =
                await query
                    .OrderBy(x =>
                        x.CustomerName)
                    .ThenBy(x =>
                        x.Code)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(pageSize)
                    .ToListAsync();


            return new PagedResult<Customer>
            {
                Items = customers,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        public async Task<PagedResult<Customer>>
    SearchPagedAsync(
        string searchText,
        int pageNumber,
        int pageSize)
        {
            var search =
                searchText
                    .Trim()
                    .ToLower();


            var query =
                _context.Customers
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive &&
                        (
                            x.Code
                                .ToLower()
                                .Contains(search)

                            ||

                            x.CustomerName
                                .ToLower()
                                .Contains(search)

                            ||

                            (
                                x.LegalName != null &&
                                x.LegalName
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            (
                                x.GSTIN != null &&
                                x.GSTIN
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            (
                                x.ContactPerson != null &&
                                x.ContactPerson
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            (
                                x.MobileNumber != null &&
                                x.MobileNumber
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            x.City
                                .ToLower()
                                .Contains(search)

                            ||

                            x.State
                                .ToLower()
                                .Contains(search)
                        ));


            var totalRecords =
                await query.CountAsync();


            var customers =
                await query
                    .OrderBy(x =>
                        x.CustomerName)
                    .ThenBy(x =>
                        x.Code)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(pageSize)
                    .ToListAsync();


            return new PagedResult<Customer>
            {
                Items = customers,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        #endregion


        #region Validation

        public async Task<bool> GSTINExistsAsync(
            string gstin,
            int? excludeCustomerId = null)
        {
            var normalizedGSTIN =
                gstin
                    .Trim()
                    .ToUpper();


            return await _context.Customers
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.GSTIN != null &&
                    x.GSTIN.ToUpper() ==
                        normalizedGSTIN &&
                    (
                        !excludeCustomerId.HasValue ||
                        x.Id != excludeCustomerId.Value
                    ));
        }

        public async Task<bool> EmailExistsAsync(
    string email,
    int? excludeCustomerId = null)
        {
            var normalizedEmail =
                email
                    .Trim()
                    .ToLower();


            return await _context.Customers
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.Email != null &&
                    x.Email.ToLower() ==
                        normalizedEmail &&
                    (
                        !excludeCustomerId.HasValue ||
                        x.Id != excludeCustomerId.Value
                    ));
        }


        public async Task<bool> MobileNumberExistsAsync(
            string mobileNumber,
            int? excludeCustomerId = null)
        {
            var normalizedMobile =
                mobileNumber.Trim();


            return await _context.Customers
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    (
                        x.MobileNumber ==
                            normalizedMobile

                        ||

                        x.AlternateMobileNumber ==
                            normalizedMobile
                    )
                    &&
                    (
                        !excludeCustomerId.HasValue ||
                        x.Id != excludeCustomerId.Value
                    ));
        }

        #endregion


        #region Customer Code Generation

        public async Task<string?>
            GetLastCustomerCodeAsync()
        {
            const string prefix =
                "AI/CUS/";


            return await _context.Customers

                // IsDeleted intentionally not filtered here.
                // Old customer codes must never be reused.

                .Where(x =>
                    x.Code.StartsWith(prefix))
                .OrderByDescending(x =>
                    x.Id)
                .Select(x =>
                    x.Code)
                .FirstOrDefaultAsync();
        }

        #endregion


        #region Write Operations

        public async Task AddAsync(
            Customer customer)
        {
            await _context.Customers
                .AddAsync(customer);

            await _context
                .SaveChangesAsync();
        }


        public async Task UpdateAsync(
            Customer customer)
        {
            await _context
                .SaveChangesAsync();
        }

        #endregion
    }
}