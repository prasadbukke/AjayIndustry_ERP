/*
============================================================
File: MachineService.cs

Purpose:
Implements Machine Master business rules.

Responsibilities:
- Generate Machine Code automatically.
- Normalize Machine information.
- Validate required fields.
- Validate Machine Status.
- Prevent duplicate active Serial Numbers.
- Create and update Machine records.
- Soft-delete Machine records.
- Restore deleted Machine records.
- Preserve Machine operational Status during restore.
- Provide Search + Pagination.

Machine Code:
AI/MCH/00001

Important:
- ERP is not physically connected to Machines.
- Machine Status is manually maintained by ERP users.
- Production Job Step status belongs to Production module.
- Deleted Machine Codes are never reused.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Application.Services
{
    public class MachineService
        : IMachineService
    {
        #region Fields

        private readonly IMachineRepository _repository;

        #endregion


        #region Constructor

        public MachineService(
            IMachineRepository repository)
        {
            _repository = repository;
        }

        #endregion


        #region Read Operations

        public async Task<List<Machine>>
            GetAllAsync()
        {
            return await _repository
                .GetAllAsync();
        }


        public async Task<Machine?>
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

        public async Task<PagedResult<Machine>>
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


        #region Create Machine

        public async Task<Machine>
            CreateAsync(
                Machine machine)
        {
            if (machine == null)
            {
                throw new BusinessException(
                    "Machine information is required.");
            }


            NormalizeMachine(
                machine);


            ValidateMachine(
                machine);


            await ValidateSerialNumberAsync(
                machine.SerialNumber);


            machine.Code =
                await GenerateMachineCodeAsync();


            machine.IsActive =
                true;

            machine.IsDeleted =
                false;

            machine.CreatedOn =
                DateTime.UtcNow;

            machine.CreatedBy =
                "System";

            machine.ModifiedOn =
                null;

            machine.ModifiedBy =
                null;


            await _repository
                .AddAsync(machine);


            return machine;
        }

        #endregion


        #region Update Machine

        public async Task<Machine>
            UpdateAsync(
                Machine machine)
        {
            if (machine == null ||
                machine.Id <= 0)
            {
                throw new BusinessException(
                    "Invalid Machine.");
            }


            var existing =
                await _repository
                    .GetForUpdateAsync(
                        machine.Id);


            if (existing == null)
            {
                throw new BusinessException(
                    "Machine not found.");
            }


            NormalizeMachine(
                machine);


            ValidateMachine(
                machine);


            await ValidateSerialNumberAsync(
                machine.SerialNumber,
                existing.Id);


            #region Machine Information

            existing.MachineName =
                machine.MachineName;

            existing.MachineType =
                machine.MachineType;

            #endregion


            #region Manufacturer Information

            existing.Manufacturer =
                machine.Manufacturer;

            existing.Model =
                machine.Model;

            existing.SerialNumber =
                machine.SerialNumber;

            #endregion


            #region Capacity And Location

            existing.Capacity =
                machine.Capacity;

            existing.Location =
                machine.Location;

            #endregion


            #region Operational Status

            existing.Status =
                machine.Status;

            #endregion


            #region Remarks

            existing.Remarks =
                machine.Remarks;

            #endregion


            #region Audit

            // Machine Code remains immutable.

            existing.ModifiedOn =
                DateTime.UtcNow;

            existing.ModifiedBy =
                "System";

            #endregion


            await _repository
                .UpdateAsync(existing);


            return existing;
        }

        #endregion


        #region Delete Machine

        public async Task DeleteAsync(
            int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid Machine.");
            }


            var machine =
                await _repository
                    .GetForUpdateAsync(id);


            if (machine == null)
            {
                throw new BusinessException(
                    "Machine not found.");
            }


            machine.IsDeleted =
                true;

            machine.IsActive =
                false;

            machine.ModifiedOn =
                DateTime.UtcNow;

            machine.ModifiedBy =
                "System";


            await _repository
                .UpdateAsync(machine);
        }

        #endregion


        #region Deleted Machines

        public async Task<List<Machine>>
            GetDeletedAsync()
        {
            return await _repository
                .GetDeletedAsync();
        }


        public async Task RestoreAsync(
            int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid Machine.");
            }


            var machine =
                await _repository
                    .GetDeletedForUpdateAsync(id);


            if (machine == null)
            {
                throw new BusinessException(
                    "Deleted Machine not found.");
            }


            /*
             * Serial Number may have been reused by another
             * active Machine after this Machine was deleted.
             *
             * Validate before restore to avoid DB unique
             * index failure.
             */

            await ValidateSerialNumberAsync(
                machine.SerialNumber,
                machine.Id);


            machine.IsDeleted =
                false;

            machine.IsActive =
                true;

            /*
             * Operational Status is preserved.
             *
             * Example:
             * Machine was Maintenance before delete
             * → Restore
             * → Status remains Maintenance.
             */

            machine.ModifiedOn =
                DateTime.UtcNow;

            machine.ModifiedBy =
                "System";


            await _repository
                .UpdateAsync(machine);
        }

        #endregion


        #region Business Validation

        private static void ValidateMachine(
            Machine machine)
        {
            if (string.IsNullOrWhiteSpace(
                machine.MachineName))
            {
                throw new BusinessException(
                    "Machine Name is required.");
            }


            if (machine.MachineName.Length >
                200)
            {
                throw new BusinessException(
                    "Machine Name cannot exceed 200 characters.");
            }


            if (string.IsNullOrWhiteSpace(
                machine.MachineType))
            {
                throw new BusinessException(
                    "Machine Type is required.");
            }


            if (machine.MachineType.Length >
                100)
            {
                throw new BusinessException(
                    "Machine Type cannot exceed 100 characters.");
            }


            if (!Enum.IsDefined(
                typeof(MachineStatus),
                machine.Status))
            {
                throw new BusinessException(
                    "Invalid Machine Status.");
            }


            if (machine.Manufacturer?.Length >
                150)
            {
                throw new BusinessException(
                    "Manufacturer cannot exceed 150 characters.");
            }


            if (machine.Model?.Length >
                150)
            {
                throw new BusinessException(
                    "Model cannot exceed 150 characters.");
            }


            if (machine.SerialNumber?.Length >
                100)
            {
                throw new BusinessException(
                    "Serial Number cannot exceed 100 characters.");
            }


            if (machine.Capacity?.Length >
                250)
            {
                throw new BusinessException(
                    "Capacity cannot exceed 250 characters.");
            }


            if (machine.Location?.Length >
                150)
            {
                throw new BusinessException(
                    "Location cannot exceed 150 characters.");
            }


            if (machine.Remarks?.Length >
                1000)
            {
                throw new BusinessException(
                    "Remarks cannot exceed 1000 characters.");
            }
        }

        #endregion


        #region Serial Number Validation

        private async Task ValidateSerialNumberAsync(
            string? serialNumber,
            int? excludeMachineId = null)
        {
            if (string.IsNullOrWhiteSpace(
                serialNumber))
            {
                return;
            }


            var exists =
                await _repository
                    .SerialNumberExistsAsync(
                        serialNumber,
                        excludeMachineId);


            if (exists)
            {
                throw new BusinessException(
                    "A Machine with the same Serial Number already exists.");
            }
        }

        #endregion


        #region Machine Normalization

        private static void NormalizeMachine(
            Machine machine)
        {
            machine.MachineName =
                machine.MachineName
                    ?.Trim()
                ?? string.Empty;


            machine.MachineType =
                machine.MachineType
                    ?.Trim()
                ?? string.Empty;


            machine.Manufacturer =
                NormalizeOptional(
                    machine.Manufacturer);


            machine.Model =
                NormalizeOptional(
                    machine.Model);


            machine.SerialNumber =
                NormalizeOptionalUpper(
                    machine.SerialNumber);


            machine.Capacity =
                NormalizeOptional(
                    machine.Capacity);


            machine.Location =
                NormalizeOptional(
                    machine.Location);


            machine.Remarks =
                NormalizeOptional(
                    machine.Remarks);
        }

        #endregion


        #region Machine Code Generation

        private async Task<string>
            GenerateMachineCodeAsync()
        {
            const string prefix =
                "AI/MCH/";


            var lastCode =
                await _repository
                    .GetLastMachineCodeAsync();


            if (string.IsNullOrWhiteSpace(
                lastCode))
            {
                return
                    $"{prefix}00001";
            }


            var numberPart =
                lastCode.Substring(
                    prefix.Length);


            if (!int.TryParse(
                numberPart,
                out var lastNumber))
            {
                throw new BusinessException(
                    "Unable to generate Machine Code.");
            }


            var nextNumber =
                lastNumber + 1;


            return
                $"{prefix}{nextNumber:00000}";
        }

        #endregion


        #region Helpers

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