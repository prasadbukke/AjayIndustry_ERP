/*
============================================================
File: MachineRepository.cs

Purpose:
Provides Entity Framework Core data access for Machine Master.

Responsibilities:
- Retrieve active Machines.
- Retrieve Machines for Details/Edit.
- Search and paginate Machine records.
- Retrieve soft-deleted Machines separately.
- Check duplicate active Machine Serial Numbers.
- Retrieve last generated Machine Code.
- Persist Machine changes.

Important:
- Main Machine Index displays only non-deleted Machines.
- Deleted Machines are displayed on a separate Deleted page.
- Deleted Machine Codes are included while finding the last
  generated Machine Code so codes are never reused.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;

using AjayIndustriesERP.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AjayIndustriesERP.Infrastructure.Repositories
{
    public class MachineRepository
        : IMachineRepository
    {
        #region Fields

        private readonly ApplicationDbContext _context;

        #endregion


        #region Constructor

        public MachineRepository(
            ApplicationDbContext context)
        {
            _context = context;
        }

        #endregion


        #region Read Operations

        public async Task<List<Machine>>
            GetAllAsync()
        {
            return await _context.Machines
                .AsNoTracking()
                .Where(x =>
                    !x.IsDeleted)
                .OrderBy(x =>
                    x.MachineName)
                .ThenBy(x =>
                    x.Code)
                .ToListAsync();
        }


        public async Task<Machine?>
            GetByIdAsync(
                int id)
        {
            return await _context.Machines
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }


        public async Task<Machine?>
            GetForUpdateAsync(
                int id)
        {
            return await _context.Machines
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    !x.IsDeleted);
        }

        #endregion


        #region Search And Pagination

        public async Task<PagedResult<Machine>>
            GetPagedAsync(
                int pageNumber,
                int pageSize)
        {
            var query =
                _context.Machines
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted);


            var totalRecords =
                await query.CountAsync();


            var machines =
                await query
                    .OrderBy(x =>
                        x.MachineName)
                    .ThenBy(x =>
                        x.Code)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(pageSize)
                    .ToListAsync();


            return new PagedResult<Machine>
            {
                Items = machines,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }


        public async Task<PagedResult<Machine>>
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
                _context.Machines
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDeleted &&
                        (
                            x.Code
                                .ToLower()
                                .Contains(search)

                            ||

                            x.MachineName
                                .ToLower()
                                .Contains(search)

                            ||

                            x.MachineType
                                .ToLower()
                                .Contains(search)

                            ||

                            (
                                x.Manufacturer != null &&
                                x.Manufacturer
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            (
                                x.Model != null &&
                                x.Model
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            (
                                x.SerialNumber != null &&
                                x.SerialNumber
                                    .ToLower()
                                    .Contains(search)
                            )

                            ||

                            (
                                x.Location != null &&
                                x.Location
                                    .ToLower()
                                    .Contains(search)
                            )
                        ));


            var totalRecords =
                await query.CountAsync();


            var machines =
                await query
                    .OrderBy(x =>
                        x.MachineName)
                    .ThenBy(x =>
                        x.Code)
                    .Skip(
                        (pageNumber - 1) *
                        pageSize)
                    .Take(pageSize)
                    .ToListAsync();


            return new PagedResult<Machine>
            {
                Items = machines,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords
            };
        }

        #endregion


        #region Deleted Machines

        public async Task<List<Machine>>
            GetDeletedAsync()
        {
            return await _context.Machines
                .AsNoTracking()
                .Where(x =>
                    x.IsDeleted)
                .OrderByDescending(x =>
                    x.ModifiedOn ?? x.CreatedOn)
                .ThenByDescending(x =>
                    x.Id)
                .ToListAsync();
        }


        public async Task<Machine?>
            GetDeletedForUpdateAsync(
                int id)
        {
            return await _context.Machines
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.IsDeleted);
        }

        #endregion


        #region Validation

        public async Task<bool>
            SerialNumberExistsAsync(
                string serialNumber,
                int? excludeMachineId = null)
        {
            var normalizedSerialNumber =
                serialNumber
                    .Trim()
                    .ToUpper();


            return await _context.Machines
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDeleted &&

                    x.SerialNumber != null &&

                    x.SerialNumber
                        .ToUpper() ==
                        normalizedSerialNumber &&

                    (
                        !excludeMachineId.HasValue ||
                        x.Id !=
                            excludeMachineId.Value
                    ));
        }

        #endregion


        #region Machine Code

        public async Task<string?>
            GetLastMachineCodeAsync()
        {
            const string prefix =
                "AI/MCH/";


            return await _context.Machines

                // IsDeleted intentionally not filtered.
                // Machine Codes must never be reused.

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
            Machine machine)
        {
            await _context.Machines
                .AddAsync(machine);


            await _context
                .SaveChangesAsync();
        }


        public async Task UpdateAsync(
            Machine machine)
        {
            await _context
                .SaveChangesAsync();
        }

        #endregion
    }
}