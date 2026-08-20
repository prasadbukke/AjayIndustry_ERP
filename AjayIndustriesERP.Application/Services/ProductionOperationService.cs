/*
============================================================
File: ProductionOperationService.cs

Purpose:
Implements Production Operation Master business rules.

Responsibilities:
- Generate Operation Code automatically.
- Normalize Operation information.
- Validate required fields.
- Validate Operation Type.
- Prevent duplicate active Operation Names.
- Create and update Operations.
- Soft-delete Operations.
- Restore deleted Operations.
- Provide Search + Pagination.

Operation Code:
AI/OPR/00001

Important:
- Setup Time and Cycle Time vary by Item and therefore
  belong to Item Process Routing.
- Machine assignment is not stored in Operation Master.
- Deleted Operation Codes are never reused.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Application.Services
{
    public class ProductionOperationService
        : IProductionOperationService
    {
        #region Fields

        private readonly IProductionOperationRepository
            _repository;

        #endregion


        #region Constructor

        public ProductionOperationService(
            IProductionOperationRepository repository)
        {
            _repository = repository;
        }

        #endregion


        #region Read Operations

        public async Task<List<ProductionOperation>>
            GetAllAsync()
        {
            return await _repository
                .GetAllAsync();
        }


        public async Task<ProductionOperation?>
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

        public async Task<PagedResult<ProductionOperation>>
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


        #region Create Operation

        public async Task<ProductionOperation>
            CreateAsync(
                ProductionOperation operation)
        {
            if (operation == null)
            {
                throw new BusinessException(
                    "Operation information is required.");
            }


            NormalizeOperation(
                operation);


            ValidateOperation(
                operation);


            await ValidateOperationNameAsync(
                operation.OperationName);


            operation.Code =
                await GenerateOperationCodeAsync();


            operation.IsActive =
                true;

            operation.IsDeleted =
                false;

            operation.CreatedOn =
                DateTime.UtcNow;

            operation.CreatedBy =
                "System";

            operation.ModifiedOn =
                null;

            operation.ModifiedBy =
                null;


            await _repository
                .AddAsync(operation);


            return operation;
        }

        #endregion


        #region Update Operation

        public async Task<ProductionOperation>
            UpdateAsync(
                ProductionOperation operation)
        {
            if (operation == null ||
                operation.Id <= 0)
            {
                throw new BusinessException(
                    "Invalid Production Operation.");
            }


            var existing =
                await _repository
                    .GetForUpdateAsync(
                        operation.Id);


            if (existing == null)
            {
                throw new BusinessException(
                    "Production Operation not found.");
            }


            NormalizeOperation(
                operation);


            ValidateOperation(
                operation);


            await ValidateOperationNameAsync(
                operation.OperationName,
                existing.Id);


            #region Operation Information

            existing.OperationName =
                operation.OperationName;

            existing.OperationType =
                operation.OperationType;

            existing.Description =
                operation.Description;

            #endregion


            #region Remarks

            existing.Remarks =
                operation.Remarks;

            #endregion


            #region Audit

            // Operation Code remains immutable.

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


        #region Delete Operation

        public async Task DeleteAsync(
            int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid Production Operation.");
            }


            var operation =
                await _repository
                    .GetForUpdateAsync(id);


            if (operation == null)
            {
                throw new BusinessException(
                    "Production Operation not found.");
            }


            /*
             * Phase 1:
             * Operation is soft-deleted.
             *
             * Later, once Item Process Routing exists,
             * deletion can be restricted if the Operation
             * is actively used in a routing.
             */

            operation.IsDeleted =
                true;

            operation.IsActive =
                false;

            operation.ModifiedOn =
                DateTime.UtcNow;

            operation.ModifiedBy =
                "System";


            await _repository
                .UpdateAsync(operation);
        }

        #endregion


        #region Deleted Operations

        public async Task<List<ProductionOperation>>
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
                    "Invalid Production Operation.");
            }


            var operation =
                await _repository
                    .GetDeletedForUpdateAsync(id);


            if (operation == null)
            {
                throw new BusinessException(
                    "Deleted Production Operation not found.");
            }


            /*
             * Another active Operation may have been created
             * with the same name after this record was deleted.
             *
             * Validate before restore.
             */

            await ValidateOperationNameAsync(
                operation.OperationName,
                operation.Id);


            operation.IsDeleted =
                false;

            operation.IsActive =
                true;

            operation.ModifiedOn =
                DateTime.UtcNow;

            operation.ModifiedBy =
                "System";


            await _repository
                .UpdateAsync(operation);
        }

        #endregion


        #region Business Validation

        private static void ValidateOperation(
            ProductionOperation operation)
        {
            if (string.IsNullOrWhiteSpace(
                operation.OperationName))
            {
                throw new BusinessException(
                    "Operation Name is required.");
            }


            if (operation.OperationName.Length >
                150)
            {
                throw new BusinessException(
                    "Operation Name cannot exceed 150 characters.");
            }


            if (!Enum.IsDefined(
                typeof(ProductionOperationType),
                operation.OperationType))
            {
                throw new BusinessException(
                    "Invalid Operation Type.");
            }


            if (operation.Description?.Length >
                500)
            {
                throw new BusinessException(
                    "Description cannot exceed 500 characters.");
            }


            if (operation.Remarks?.Length >
                1000)
            {
                throw new BusinessException(
                    "Remarks cannot exceed 1000 characters.");
            }
        }

        #endregion


        #region Duplicate Operation Validation

        private async Task ValidateOperationNameAsync(
            string operationName,
            int? excludeOperationId = null)
        {
            var exists =
                await _repository
                    .OperationNameExistsAsync(
                        operationName,
                        excludeOperationId);


            if (exists)
            {
                throw new BusinessException(
                    "An Operation with the same name already exists.");
            }
        }

        #endregion


        #region Normalization

        private static void NormalizeOperation(
            ProductionOperation operation)
        {
            operation.OperationName =
                operation.OperationName
                    ?.Trim()
                ?? string.Empty;


            operation.Description =
                NormalizeOptional(
                    operation.Description);


            operation.Remarks =
                NormalizeOptional(
                    operation.Remarks);
        }

        #endregion


        #region Operation Code Generation

        private async Task<string>
            GenerateOperationCodeAsync()
        {
            const string prefix =
                "AI/OPR/";


            var lastCode =
                await _repository
                    .GetLastOperationCodeAsync();


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
                    "Unable to generate Operation Code.");
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