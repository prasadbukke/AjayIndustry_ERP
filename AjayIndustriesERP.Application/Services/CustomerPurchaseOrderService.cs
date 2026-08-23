/*
============================================================
File: CustomerPurchaseOrderService.cs

Purpose:
Contains Customer Purchase Order business rules.

Responsibilities:
- Generate Financial Year based Customer PO Code.
- Validate Customer Master.
- Prevent duplicate Customer + Customer PO Number.
- Validate Customer PO header information.
- Validate Customer PO Items.
- Load trusted Item Master information.
- Load current Customer Drawing using Customer + Item.
- Create historical Item snapshots.
- Create Customer Drawing Number / Revision snapshots.
- Create Draft Customer Purchase Orders.
- Update Draft Customer Purchase Orders.
- Synchronize Customer PO Item rows.
- Confirm Draft Customer Purchase Orders.
- Soft-delete Customer Purchase Orders.
- Restore deleted Customer Purchase Orders.

Internal Customer PO Code:
AI/CPO/26-27/00001

Important Business Rules:
- Customer PO Number is required.
- Same Customer + Same Customer PO Number is not allowed.
- Existing Item Master must be used.
- Customer / Item snapshots posted from browser are NOT trusted.
- Customer Drawing Number / Revision posted from browser are NOT trusted.
- Customer Drawing snapshot is resolved from Customer + Item.
- Only Draft Customer POs can be edited.
- Same Item cannot appear more than once on one Customer PO.
- Production Machine / Pipeline information is NOT stored here.
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Application.Services
{
    public class CustomerPurchaseOrderService
        : ICustomerPurchaseOrderService
    {
        #region Fields

        private readonly
            ICustomerPurchaseOrderRepository
            _repository;

        private readonly
            ICustomerDrawingService
            _customerDrawingService;

        #endregion


        #region Constructor

        public CustomerPurchaseOrderService(
            ICustomerPurchaseOrderRepository repository,
            ICustomerDrawingService customerDrawingService)
        {
            _repository =
                repository;

            _customerDrawingService =
                customerDrawingService;
        }

        #endregion


        #region Read Operations

        public async Task<List<CustomerPurchaseOrder>>
            GetAllAsync()
        {
            return await _repository
                .GetAllAsync();
        }


        public async Task<CustomerPurchaseOrder?>
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

        public async Task<PagedResult<CustomerPurchaseOrder>>
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


        public async Task<PagedResult<CustomerPurchaseOrder>>
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


        #region Customer Master

        public async Task<List<Customer>>
            GetCustomersForOrderAsync()
        {
            return await _repository
                .GetCustomersForOrderAsync();
        }

        #endregion


        #region Item Master

        public async Task<List<Item>>
            GetItemsForOrderAsync()
        {
            return await _repository
                .GetItemsForOrderAsync();
        }


        public async Task<Item?>
            GetItemForOrderAsync(
                int itemId)
        {
            if (itemId <= 0)
            {
                return null;
            }


            return await _repository
                .GetItemForOrderAsync(
                    itemId);
        }

        #endregion


        #region Create Customer Purchase Order

        public async Task<CustomerPurchaseOrder>
            CreateAsync(
                CustomerPurchaseOrder
                    customerPurchaseOrder)
        {
            if (customerPurchaseOrder == null)
            {
                throw new BusinessException(
                    "Customer Purchase Order information is required.");
            }


            NormalizeHeader(
                customerPurchaseOrder);


            ValidateHeader(
                customerPurchaseOrder);


            #region Validate Customer

            var customer =
                await ValidateCustomerAsync(
                    customerPurchaseOrder.CustomerId);

            #endregion


            #region Duplicate Customer PO Number

            await ValidateDuplicateCustomerPONumberAsync(
                customerPurchaseOrder.CustomerId,
                customerPurchaseOrder
                    .CustomerPurchaseOrderNumber);

            #endregion


            #region Generate Internal Code

            var customerPurchaseOrderCode =
                await GenerateCustomerPurchaseOrderCodeAsync(
                    customerPurchaseOrder.ReceivedDate);

            #endregion


            #region Create Trusted Header

            var newCustomerPurchaseOrder =
                new CustomerPurchaseOrder
                {
                    Code =
                        customerPurchaseOrderCode,

                    CustomerId =
                        customer.Id,

                    CustomerName =
                        customer.CustomerName,

                    CustomerPurchaseOrderNumber =
                        customerPurchaseOrder
                            .CustomerPurchaseOrderNumber,

                    CustomerPurchaseOrderDate =
                        customerPurchaseOrder
                            .CustomerPurchaseOrderDate,

                    ReceivedDate =
                        customerPurchaseOrder
                            .ReceivedDate,

                    RequiredDeliveryDate =
                        customerPurchaseOrder
                            .RequiredDeliveryDate,

                    Priority =
                        customerPurchaseOrder.Priority,

                    Status =
                        CustomerPurchaseOrderStatus.Draft,

                    CustomerReference =
                        customerPurchaseOrder
                            .CustomerReference,

                    Remarks =
                        customerPurchaseOrder.Remarks,

                    IsActive =
                        true,

                    IsDeleted =
                        false,

                    CreatedOn =
                        DateTime.UtcNow,

                    CreatedBy =
                        "System"
                };

            #endregion


            #region Prepare Item Snapshots

            if (customerPurchaseOrder.Items == null ||
                customerPurchaseOrder.Items.Count == 0)
            {
                throw new BusinessException(
                    "At least one Item is required.");
            }


            ValidateDuplicateItems(
                customerPurchaseOrder.Items);


            foreach (var submittedItem
                in customerPurchaseOrder.Items)
            {
                var preparedItem =
                    await PrepareTrustedItemAsync(
                        submittedItem,
                        customer.Id,
                        isNewLine: true);


                newCustomerPurchaseOrder.Items
                    .Add(preparedItem);
            }

            #endregion


            await _repository
                .AddAsync(
                    newCustomerPurchaseOrder);


            return newCustomerPurchaseOrder;
        }

        #endregion


        #region Update Customer Purchase Order

        public async Task<CustomerPurchaseOrder>
            UpdateAsync(
                CustomerPurchaseOrder
                    customerPurchaseOrder)
        {
            if (customerPurchaseOrder == null ||
                customerPurchaseOrder.Id <= 0)
            {
                throw new BusinessException(
                    "Invalid Customer Purchase Order.");
            }


            var existing =
                await _repository
                    .GetForUpdateAsync(
                        customerPurchaseOrder.Id);


            if (existing == null)
            {
                throw new BusinessException(
                    "Customer Purchase Order not found.");
            }


            #region Draft Only Rule

            if (existing.Status !=
                CustomerPurchaseOrderStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Customer Purchase Orders can be edited.");
            }

            #endregion


            NormalizeHeader(
                customerPurchaseOrder);


            ValidateHeader(
                customerPurchaseOrder);


            #region Financial Year Protection

            var oldFinancialYear =
                GetFinancialYear(
                    existing.ReceivedDate);


            var newFinancialYear =
                GetFinancialYear(
                    customerPurchaseOrder.ReceivedDate);


            if (!string.Equals(
                oldFinancialYear,
                newFinancialYear,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException(
                    "Received Date cannot be changed to another Financial Year.");
            }

            #endregion


            #region Validate Customer

            var customer =
                await ValidateCustomerAsync(
                    customerPurchaseOrder.CustomerId);

            #endregion


            #region Duplicate Customer PO Number

            await ValidateDuplicateCustomerPONumberAsync(
                customer.Id,
                customerPurchaseOrder
                    .CustomerPurchaseOrderNumber,
                existing.Id);

            #endregion


            #region Validate Submitted Lines

            if (customerPurchaseOrder.Items == null ||
                customerPurchaseOrder.Items.Count == 0)
            {
                throw new BusinessException(
                    "At least one Item is required.");
            }


            ValidateDuplicateSubmittedLineIds(
                customerPurchaseOrder.Items);


            ValidateDuplicateItems(
                customerPurchaseOrder.Items);

            #endregion


            #region Update Header

            existing.CustomerId =
                customer.Id;

            existing.CustomerName =
                customer.CustomerName;

            existing.CustomerPurchaseOrderNumber =
                customerPurchaseOrder
                    .CustomerPurchaseOrderNumber;

            existing.CustomerPurchaseOrderDate =
                customerPurchaseOrder
                    .CustomerPurchaseOrderDate;

            existing.ReceivedDate =
                customerPurchaseOrder
                    .ReceivedDate;

            existing.RequiredDeliveryDate =
                customerPurchaseOrder
                    .RequiredDeliveryDate;

            existing.Priority =
                customerPurchaseOrder.Priority;

            existing.CustomerReference =
                customerPurchaseOrder
                    .CustomerReference;

            existing.Remarks =
                customerPurchaseOrder.Remarks;

            existing.ModifiedOn =
                DateTime.UtcNow;

            existing.ModifiedBy =
                "System";

            #endregion


            #region Synchronize Items

            var existingItems =
                existing.Items
                    .Where(x =>
                        !x.IsDeleted)
                    .ToList();


            var retainedItemIds =
                new HashSet<int>();


            foreach (var submittedItem
                in customerPurchaseOrder.Items)
            {
                var preparedItem =
                    await PrepareTrustedItemAsync(
                        submittedItem,
                        customer.Id,
                        isNewLine:
                            submittedItem.Id <= 0);


                CustomerPurchaseOrderItem?
                    existingItem = null;


                if (submittedItem.Id > 0)
                {
                    existingItem =
                        existingItems
                            .FirstOrDefault(x =>
                                x.Id ==
                                submittedItem.Id);


                    if (existingItem == null)
                    {
                        throw new BusinessException(
                            "Invalid Customer Purchase Order Item record.");
                    }
                }


                if (existingItem != null)
                {
                    CopyPreparedItem(
                        preparedItem,
                        existingItem);


                    existingItem.ModifiedOn =
                        DateTime.UtcNow;

                    existingItem.ModifiedBy =
                        "System";


                    retainedItemIds.Add(
                        existingItem.Id);


                    continue;
                }


                preparedItem.CustomerPurchaseOrderId =
                    existing.Id;

                preparedItem.Code =
                    GenerateLineCode();

                preparedItem.IsActive =
                    true;

                preparedItem.IsDeleted =
                    false;

                preparedItem.CreatedOn =
                    DateTime.UtcNow;

                preparedItem.CreatedBy =
                    "System";


                existing.Items.Add(
                    preparedItem);
            }


            foreach (var existingItem
                in existingItems)
            {
                if (retainedItemIds.Contains(
                    existingItem.Id))
                {
                    continue;
                }


                existingItem.IsDeleted =
                    true;

                existingItem.IsActive =
                    false;

                existingItem.ModifiedOn =
                    DateTime.UtcNow;

                existingItem.ModifiedBy =
                    "System";
            }

            #endregion


            await _repository
                .UpdateAsync(existing);


            return existing;
        }

        #endregion


        #region Confirm Workflow

        public async Task ConfirmAsync(
            int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid Customer Purchase Order.");
            }


            var customerPurchaseOrder =
                await _repository
                    .GetForUpdateAsync(id);


            if (customerPurchaseOrder == null)
            {
                throw new BusinessException(
                    "Customer Purchase Order not found.");
            }


            if (customerPurchaseOrder.Status !=
                CustomerPurchaseOrderStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Customer Purchase Orders can be confirmed.");
            }


            if (customerPurchaseOrder.Items == null ||
                !customerPurchaseOrder.Items.Any(x =>
                    !x.IsDeleted &&
                    x.IsActive))
            {
                throw new BusinessException(
                    "Customer Purchase Order must contain at least one Item.");
            }


            customerPurchaseOrder.Status =
                CustomerPurchaseOrderStatus.Confirmed;


            customerPurchaseOrder.ModifiedOn =
                DateTime.UtcNow;

            customerPurchaseOrder.ModifiedBy =
                "System";


            await _repository
                .UpdateAsync(
                    customerPurchaseOrder);
        }

        #endregion


        #region Delete Customer Purchase Order

        public async Task DeleteAsync(
            int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid Customer Purchase Order.");
            }


            var customerPurchaseOrder =
                await _repository
                    .GetForUpdateAsync(id);


            if (customerPurchaseOrder == null)
            {
                throw new BusinessException(
                    "Customer Purchase Order not found.");
            }


            /*
             * Customer PO may be deleted even after Confirm.
             *
             * Delete is a soft delete.
             * Transaction Status itself is preserved.
             *
             * Example:
             *
             * Confirmed PO deleted
             *     ↓
             * Restore
             *     ↓
             * Status remains Confirmed
             */

            customerPurchaseOrder.IsDeleted =
                true;

            customerPurchaseOrder.IsActive =
                false;

            customerPurchaseOrder.ModifiedOn =
                DateTime.UtcNow;

            customerPurchaseOrder.ModifiedBy =
                "System";


            foreach (var item
                in customerPurchaseOrder.Items)
            {
                item.IsDeleted =
                    true;

                item.IsActive =
                    false;

                item.ModifiedOn =
                    DateTime.UtcNow;

                item.ModifiedBy =
                    "System";
            }


            await _repository
                .UpdateAsync(
                    customerPurchaseOrder);
        }

        #endregion


        #region Deleted Customer Purchase Orders

        public async Task<List<CustomerPurchaseOrder>>
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
                    "Invalid Customer Purchase Order.");
            }


            var customerPurchaseOrder =
                await _repository
                    .GetDeletedForUpdateAsync(id);


            if (customerPurchaseOrder == null)
            {
                throw new BusinessException(
                    "Deleted Customer Purchase Order not found.");
            }


            /*
             * Restore preserves original transaction status.
             *
             * Draft     → restored as Draft
             * Confirmed → restored as Confirmed
             */

            customerPurchaseOrder.IsDeleted =
                false;

            customerPurchaseOrder.IsActive =
                true;

            customerPurchaseOrder.ModifiedOn =
                DateTime.UtcNow;

            customerPurchaseOrder.ModifiedBy =
                "System";


            foreach (var item
                in customerPurchaseOrder.Items)
            {
                item.IsDeleted =
                    false;

                item.IsActive =
                    true;

                item.ModifiedOn =
                    DateTime.UtcNow;

                item.ModifiedBy =
                    "System";
            }


            await _repository
                .UpdateAsync(
                    customerPurchaseOrder);
        }

        #endregion


        #region Prepare Trusted Item

        private async Task<CustomerPurchaseOrderItem>
            PrepareTrustedItemAsync(
                CustomerPurchaseOrderItem submittedItem,
                int customerId,
                bool isNewLine)
        {
            if (submittedItem == null)
            {
                throw new BusinessException(
                    "Invalid Customer Purchase Order Item.");
            }


            if (customerId <= 0)
            {
                throw new BusinessException(
                    "Invalid Customer for Customer Purchase Order Item.");
            }


            NormalizeItem(
                submittedItem);


            ValidateItem(
                submittedItem);


            #region Trusted Item Master

            var item =
                await _repository
                    .GetItemForOrderAsync(
                        submittedItem.ItemId);


            if (item == null ||
                item.IsDeleted ||
                !item.IsActive)
            {
                throw new BusinessException(
                    "Selected Item does not exist or is inactive.");
            }


            var unitName =
                item.Uom?.UomName;


            if (string.IsNullOrWhiteSpace(
                unitName))
            {
                throw new BusinessException(
                    $"UOM is not configured for Item " +
                    $"{item.ItemCode} - {item.ItemName}.");
            }

            #endregion


            #region Trusted Customer Drawing

            /*
             * Customer Drawing is resolved from:
             *
             * Customer + Item
             *
             * Browser-posted CustomerDrawingNumber
             * and Revision are NOT trusted.
             *
             * GetByCustomerAndItemAsync returns the
             * current active Customer Drawing revision.
             */

            var currentCustomerDrawing =
                await _customerDrawingService
                    .GetByCustomerAndItemAsync(
                        customerId,
                        item.ItemId);


            var customerDrawingNumber =
                currentCustomerDrawing?
                    .DrawingNumber;


            var customerDrawingRevision =
                currentCustomerDrawing?
                    .RevisionNumber;

            #endregion


            #region Prepare Snapshot

            var preparedItem =
                new CustomerPurchaseOrderItem
                {
                    Id =
                        submittedItem.Id,

                    ItemId =
                        item.ItemId,

                    ItemCode =
                        item.ItemCode,

                    ItemName =
                        item.ItemName,

                    Specification =
                        BuildSpecificationSnapshot(
                            item),

                    UnitName =
                        unitName,

                    CustomerItemCode =
                        submittedItem
                            .CustomerItemCode,

                    /*
                     * Historical Customer Drawing snapshot.
                     *
                     * Example:
                     *
                     * Customer Drawing Master:
                     * CD-001 / RV-03
                     *
                     * Customer PO Item Snapshot:
                     * CustomerDrawingNumber = CD-001
                     * Revision              = RV-03
                     */

                    CustomerDrawingNumber =
                        customerDrawingNumber,

                    Revision =
                        customerDrawingRevision,

                    OrderedQuantity =
                        submittedItem
                            .OrderedQuantity,

                    RequiredDeliveryDate =
                        submittedItem
                            .RequiredDeliveryDate,

                    Priority =
                        submittedItem.Priority,

                    Remarks =
                        submittedItem.Remarks
                };


            if (isNewLine)
            {
                preparedItem.Code =
                    GenerateLineCode();

                preparedItem.IsActive =
                    true;

                preparedItem.IsDeleted =
                    false;

                preparedItem.CreatedOn =
                    DateTime.UtcNow;

                preparedItem.CreatedBy =
                    "System";
            }


            return preparedItem;

            #endregion
        }

        #endregion


        #region Customer Validation

        private async Task<Customer>
            ValidateCustomerAsync(
                int customerId)
        {
            if (customerId <= 0)
            {
                throw new BusinessException(
                    "Please select a Customer.");
            }


            var customer =
                await _repository
                    .GetCustomerForOrderAsync(
                        customerId);


            if (customer == null ||
                customer.IsDeleted ||
                !customer.IsActive)
            {
                throw new BusinessException(
                    "Selected Customer does not exist or is inactive.");
            }


            return customer;
        }

        #endregion


        #region Duplicate Customer PO Validation

        private async Task
            ValidateDuplicateCustomerPONumberAsync(
                int customerId,
                string customerPurchaseOrderNumber,
                int? excludeId = null)
        {
            var exists =
                await _repository
                    .CustomerPurchaseOrderNumberExistsAsync(
                        customerId,
                        customerPurchaseOrderNumber,
                        excludeId);


            if (exists)
            {
                throw new BusinessException(
                    $"Customer PO Number " +
                    $"'{customerPurchaseOrderNumber}' " +
                    $"already exists for the selected Customer.");
            }
        }

        #endregion


        #region Header Validation

        private static void ValidateHeader(
            CustomerPurchaseOrder
                customerPurchaseOrder)
        {
            if (customerPurchaseOrder.CustomerId <= 0)
            {
                throw new BusinessException(
                    "Please select a Customer.");
            }


            if (string.IsNullOrWhiteSpace(
                customerPurchaseOrder
                    .CustomerPurchaseOrderNumber))
            {
                throw new BusinessException(
                    "Customer PO Number is required.");
            }


            if (customerPurchaseOrder
                    .CustomerPurchaseOrderNumber
                    .Length > 100)
            {
                throw new BusinessException(
                    "Customer PO Number cannot exceed 100 characters.");
            }


            if (customerPurchaseOrder
                    .CustomerPurchaseOrderDate ==
                default)
            {
                throw new BusinessException(
                    "Customer PO Date is required.");
            }


            if (customerPurchaseOrder
                    .ReceivedDate ==
                default)
            {
                throw new BusinessException(
                    "PO Received Date is required.");
            }


            if (customerPurchaseOrder
                    .RequiredDeliveryDate ==
                default)
            {
                throw new BusinessException(
                    "Required Delivery Date is required.");
            }


            if (!Enum.IsDefined(
                typeof(
                    CustomerPurchaseOrderPriority),
                customerPurchaseOrder.Priority))
            {
                throw new BusinessException(
                    "Invalid Customer PO Priority.");
            }


            if (customerPurchaseOrder
                    .CustomerReference
                    ?.Length > 200)
            {
                throw new BusinessException(
                    "Customer Reference cannot exceed 200 characters.");
            }


            if (customerPurchaseOrder
                    .Remarks
                    ?.Length > 1000)
            {
                throw new BusinessException(
                    "Remarks cannot exceed 1000 characters.");
            }


            if (customerPurchaseOrder.Items == null ||
                customerPurchaseOrder.Items.Count == 0)
            {
                throw new BusinessException(
                    "At least one Item is required.");
            }
        }

        #endregion


        #region Item Validation

        private static void ValidateItem(
            CustomerPurchaseOrderItem item)
        {
            if (item.ItemId <= 0)
            {
                throw new BusinessException(
                    "Please select an Item.");
            }


            if (item.OrderedQuantity <= 0)
            {
                throw new BusinessException(
                    "Ordered Quantity must be greater than zero.");
            }


            if (item.CustomerItemCode?.Length >
                100)
            {
                throw new BusinessException(
                    "Customer Item Code cannot exceed 100 characters.");
            }


            /*
             * CustomerDrawingNumber and Revision
             * are intentionally NOT validated here.
             *
             * Browser-posted values are ignored.
             * Trusted values come from Customer Drawing Master.
             */


            if (item.Priority.HasValue &&
                !Enum.IsDefined(
                    typeof(
                        CustomerPurchaseOrderPriority),
                    item.Priority.Value))
            {
                throw new BusinessException(
                    "Invalid Item Priority.");
            }


            if (item.Remarks?.Length >
                1000)
            {
                throw new BusinessException(
                    "Item Remarks cannot exceed 1000 characters.");
            }
        }

        #endregion


        #region Header Normalization

        private static void NormalizeHeader(
            CustomerPurchaseOrder
                customerPurchaseOrder)
        {
            customerPurchaseOrder
                .CustomerPurchaseOrderNumber =
                customerPurchaseOrder
                    .CustomerPurchaseOrderNumber
                    ?.Trim()
                ?? string.Empty;


            customerPurchaseOrder.CustomerReference =
                NormalizeOptional(
                    customerPurchaseOrder
                        .CustomerReference);


            customerPurchaseOrder.Remarks =
                NormalizeOptional(
                    customerPurchaseOrder.Remarks);
        }

        #endregion


        #region Item Normalization

        private static void NormalizeItem(
            CustomerPurchaseOrderItem item)
        {
            item.CustomerItemCode =
                NormalizeOptional(
                    item.CustomerItemCode);


            /*
             * CustomerDrawingNumber and Revision
             * are not normalized because posted values
             * are not trusted or used.
             */


            item.Remarks =
                NormalizeOptional(
                    item.Remarks);
        }

        #endregion


        #region Specification Snapshot

        private static string?
            BuildSpecificationSnapshot(
                Item item)
        {
            var rows =
                item.ItemSpecifications
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SortOrder)
                    .Select(x =>
                    {
                        var specificationName =
                            x.Specification?
                                .SpecificationName;


                        var value =
                            x.SpecificationValue;


                        var uom =
                            x.Uom?
                                .UomName;


                        var valueWithUom =
                            string.IsNullOrWhiteSpace(
                                uom)
                                ? value
                                : $"{value} {uom}";


                        return string.IsNullOrWhiteSpace(
                            specificationName)
                                ? valueWithUom
                                : $"{specificationName}: " +
                                  $"{valueWithUom}";
                    })
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x))
                    .ToList();


            return rows.Count == 0
                ? null
                : string.Join(
                    " | ",
                    rows);
        }

        #endregion


        #region Copy Prepared Item

        private static void CopyPreparedItem(
            CustomerPurchaseOrderItem source,
            CustomerPurchaseOrderItem target)
        {
            target.ItemId =
                source.ItemId;

            target.ItemCode =
                source.ItemCode;

            target.ItemName =
                source.ItemName;

            target.Specification =
                source.Specification;

            target.UnitName =
                source.UnitName;

            target.CustomerItemCode =
                source.CustomerItemCode;

            /*
             * Trusted Customer Drawing snapshot.
             */

            target.CustomerDrawingNumber =
                source.CustomerDrawingNumber;

            target.Revision =
                source.Revision;

            target.OrderedQuantity =
                source.OrderedQuantity;

            target.RequiredDeliveryDate =
                source.RequiredDeliveryDate;

            target.Priority =
                source.Priority;

            target.Remarks =
                source.Remarks;

            target.IsActive =
                true;

            target.IsDeleted =
                false;
        }

        #endregion


        #region Duplicate Submitted Line Validation

        private static void
            ValidateDuplicateSubmittedLineIds(
                ICollection<CustomerPurchaseOrderItem>
                    items)
        {
            var duplicateLineId =
                items
                    .Where(x =>
                        x.Id > 0)
                    .GroupBy(x =>
                        x.Id)
                    .FirstOrDefault(x =>
                        x.Count() > 1);


            if (duplicateLineId != null)
            {
                throw new BusinessException(
                    "Duplicate Customer Purchase Order Item record found.");
            }
        }

        #endregion


        #region Customer PO Code Generation

        private async Task<string>
            GenerateCustomerPurchaseOrderCodeAsync(
                DateTime receivedDate)
        {
            var financialYear =
                GetFinancialYear(
                    receivedDate);


            var prefix =
                $"AI/CPO/{financialYear}/";


            var lastCode =
                await _repository
                    .GetLastCustomerPurchaseOrderCodeAsync(
                        prefix);


            var nextNumber =
                1;


            if (!string.IsNullOrWhiteSpace(
                lastCode))
            {
                var numberPart =
                    lastCode
                        .Substring(
                            prefix.Length);


                if (int.TryParse(
                    numberPart,
                    out var lastNumber))
                {
                    nextNumber =
                        lastNumber + 1;
                }
            }


            return
                $"{prefix}" +
                $"{nextNumber:00000}";
        }

        #endregion


        #region Financial Year

        private static string GetFinancialYear(
            DateTime date)
        {
            var startYear =
                date.Month >= 4
                    ? date.Year
                    : date.Year - 1;


            var endYear =
                startYear + 1;


            return
                $"{startYear % 100:00}-" +
                $"{endYear % 100:00}";
        }

        #endregion


        #region Line Code

        private static string GenerateLineCode()
        {
            /*
             * Independent internal line code.
             *
             * It does not depend on visible row sequence,
             * therefore deleted row codes are never reused.
             */

            return
                $"CPOI{Guid.NewGuid()
                    .ToString("N")
                    .Substring(0, 12)
                    .ToUpperInvariant()}";
        }

        #endregion


        #region Optional Text Helper

        private static string?
            NormalizeOptional(
                string? value)
        {
            return string.IsNullOrWhiteSpace(
                value)
                ? null
                : value.Trim();
        }

        #endregion


        #region Duplicate Item Validation

        private static void ValidateDuplicateItems(
            IEnumerable<CustomerPurchaseOrderItem> items)
        {
            var duplicateItem =
                items
                    .Where(x =>
                        x.ItemId > 0)
                    .GroupBy(x =>
                        x.ItemId)
                    .FirstOrDefault(x =>
                        x.Count() > 1);


            if (duplicateItem != null)
            {
                throw new BusinessException(
                    "The same Item cannot be selected more than once in a Customer Purchase Order.");
            }
        }

        #endregion


        #region Pagination Helper

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


        #region Customer PO Validation

        public async Task<bool>
            CustomerPurchaseOrderNumberExistsAsync(
                int customerId,
                string customerPurchaseOrderNumber,
                int? excludeCustomerPurchaseOrderId = null)
        {
            if (customerId <= 0 ||
                string.IsNullOrWhiteSpace(
                    customerPurchaseOrderNumber))
            {
                return false;
            }


            return await _repository
                .CustomerPurchaseOrderNumberExistsAsync(
                    customerId,
                    customerPurchaseOrderNumber.Trim(),
                    excludeCustomerPurchaseOrderId);
        }

        #endregion
    }
}