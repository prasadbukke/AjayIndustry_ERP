/*
==============================================================

File : PurchaseOrderService.cs

Purpose :
Contains Purchase Order business rules.

Responsibilities :
- Financial Year PO Number generation
- Company validation and snapshot
- Supplier validation and snapshot
- Item validation and snapshot
- Drawing validation and snapshot
- Purchase GST calculation
- Transport / Other Charges
- Grand Total calculation
- Purchase Order Terms & Conditions snapshot
- Draft update rules
- Soft Delete

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using System.Text.RegularExpressions;

namespace AjayIndustriesERP.Application.Services
{
    public class PurchaseOrderService :
        IPurchaseOrderService
    {
        private readonly IPurchaseOrderRepository
            _purchaseOrderRepository;

        private readonly ICompanyRepository
            _companyRepository;

        private readonly ISupplierRepository
            _supplierRepository;

        private readonly IItemRepository
            _itemRepository;

        private readonly IDrawingRepository
            _drawingRepository;


        public PurchaseOrderService(
            IPurchaseOrderRepository purchaseOrderRepository,
            ICompanyRepository companyRepository,
            ISupplierRepository supplierRepository,
            IItemRepository itemRepository,
            IDrawingRepository drawingRepository)
        {
            _purchaseOrderRepository =
                purchaseOrderRepository;

            _companyRepository =
                companyRepository;

            _supplierRepository =
                supplierRepository;

            _itemRepository =
                itemRepository;

            _drawingRepository =
                drawingRepository;
        }


        // =====================================================
        // REGION 1 — Read Operations
        // =====================================================

        #region Read Operations

        public async Task<List<PurchaseOrder>>
            GetAllAsync()
        {
            return await _purchaseOrderRepository
                .GetAllAsync();
        }


        public async Task<PurchaseOrder?>
            GetByIdAsync(
                int purchaseOrderId)
        {
            return await _purchaseOrderRepository
                .GetByIdAsync(
                    purchaseOrderId);
        }


        public async Task<List<PurchaseOrder>>
            SearchAsync(
                string searchText)
        {
            if (string.IsNullOrWhiteSpace(
                searchText))
            {
                return await _purchaseOrderRepository
                    .GetAllAsync();
            }

            return await _purchaseOrderRepository
                .SearchAsync(
                    searchText);
        }


        public async Task<PagedResult<PurchaseOrder>>
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

            return await _purchaseOrderRepository
                .GetPagedAsync(
                    pageNumber,
                    pageSize);
        }


        public async Task<bool>
            IsIntraStateAsync(
                int companyId,
                int supplierId)
        {
            var company =
                await ValidateCompanyAsync(
                    companyId);

            var supplier =
                await ValidateSupplierAsync(
                    supplierId);

            return DetermineIntraState(
                company,
                supplier);
        }

        #endregion


        // =====================================================
        // REGION 2 — Create Purchase Order
        // =====================================================

        #region Create Purchase Order

        public async Task CreateAsync(
            PurchaseOrder purchaseOrder)
        {
            NormalizePurchaseOrder(
                purchaseOrder);

            ValidatePurchaseOrder(
                purchaseOrder);


            var company =
                await ValidateCompanyAsync(
                    purchaseOrder.CompanyId);

            var supplier =
                await ValidateSupplierAsync(
                    purchaseOrder.SupplierId);


            purchaseOrder.Code =
                await GeneratePurchaseOrderCodeAsync(
                    purchaseOrder.PODate);


            ApplyCompanySnapshot(
                purchaseOrder,
                company);

            ApplySupplierSnapshot(
                purchaseOrder,
                supplier);


            var isIntraState =
                ResolveTaxType(
                    company,
                    supplier,
                    purchaseOrder.Items);


            await PrepareItemsAsync(
                purchaseOrder,
                isIntraState,
                isCreate: true);


            CalculateHeaderTotals(
                purchaseOrder);


            purchaseOrder.Status =
                PurchaseOrderStatus.Draft;

            purchaseOrder.IsActive =
                true;

            purchaseOrder.IsDeleted =
                false;

            purchaseOrder.CreatedOn =
                DateTime.UtcNow;

            purchaseOrder.CreatedBy =
                "System";


            await _purchaseOrderRepository
                .AddAsync(
                    purchaseOrder);

            await _purchaseOrderRepository
                .SaveChangesAsync();
        }

        #endregion


        // =====================================================
        // REGION 3 — Update Purchase Order
        // =====================================================

        #region Update Purchase Order

        public async Task UpdateAsync(
            PurchaseOrder purchaseOrder)
        {
            var existingPurchaseOrder =
                await _purchaseOrderRepository
                    .GetByIdAsync(
                        purchaseOrder.Id);


            if (existingPurchaseOrder == null)
            {
                throw new BusinessException(
                    "Purchase Order not found.");
            }


            if (existingPurchaseOrder.Status !=
                PurchaseOrderStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Purchase Orders can be edited.");
            }


            NormalizePurchaseOrder(
                purchaseOrder);

            ValidatePurchaseOrder(
                purchaseOrder);


            /*
             * PO Number contains Financial Year.
             *
             * PO Date can change only within
             * the same Financial Year.
             */
            var oldFinancialYear =
                GetFinancialYear(
                    existingPurchaseOrder.PODate);

            var newFinancialYear =
                GetFinancialYear(
                    purchaseOrder.PODate);


            if (!string.Equals(
                oldFinancialYear,
                newFinancialYear,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException(
                    "PO Date cannot be changed to another Financial Year.");
            }


            var company =
                await ValidateCompanyAsync(
                    purchaseOrder.CompanyId);

            var supplier =
                await ValidateSupplierAsync(
                    purchaseOrder.SupplierId);


            var submittedItems =
                purchaseOrder.Items
                    .ToList();


            if (submittedItems.Count == 0)
            {
                throw new BusinessException(
                    "At least one Item is required.");
            }


            var isIntraState =
                ResolveTaxType(
                    company,
                    supplier,
                    submittedItems);


            /*
             * Prepare posted Item rows separately.
             */
            var temporaryPurchaseOrder =
                new PurchaseOrder
                {
                    Items =
                        submittedItems
                };


            await PrepareItemsAsync(
                temporaryPurchaseOrder,
                isIntraState,
                isCreate: false);


            var preparedItems =
                temporaryPurchaseOrder.Items
                    .ToList();


            // =================================================
            // Update Header
            // =================================================

            existingPurchaseOrder.PODate =
                purchaseOrder.PODate;

            existingPurchaseOrder.ExpectedDeliveryDate =
                purchaseOrder.ExpectedDeliveryDate;

            existingPurchaseOrder.CompanyId =
                company.CompanyId;

            existingPurchaseOrder.SupplierId =
                supplier.SupplierId;

            existingPurchaseOrder.DeliveryAddress =
                purchaseOrder.DeliveryAddress;

            existingPurchaseOrder.PaymentTerms =
                purchaseOrder.PaymentTerms;

            existingPurchaseOrder.DeliveryTerms =
                purchaseOrder.DeliveryTerms;

            existingPurchaseOrder.Remarks =
                purchaseOrder.Remarks;

            existingPurchaseOrder.TransportCharges =
                purchaseOrder.TransportCharges;

            existingPurchaseOrder.OtherCharges =
                purchaseOrder.OtherCharges;


            /*
             * Purchase Order does not use Round Off.
             */
            existingPurchaseOrder.RoundOffAmount =
                0;


            existingPurchaseOrder.IsActive =
                purchaseOrder.IsActive;


            /*
             * Refresh Company/Supplier snapshots
             * while PO is still Draft.
             */
            ApplyCompanySnapshot(
                existingPurchaseOrder,
                company);

            ApplySupplierSnapshot(
                existingPurchaseOrder,
                supplier);


            // =================================================
            // Synchronize Items
            // =================================================

            var existingItems =
                existingPurchaseOrder.Items
                    .Where(x =>
                        !x.IsDeleted)
                    .ToList();


            var retainedItemIds =
                new HashSet<int>();


            foreach (var preparedItem
                in preparedItems)
            {
                PurchaseOrderItem?
                    existingItem = null;


                if (preparedItem.Id > 0)
                {
                    existingItem =
                        existingItems
                            .FirstOrDefault(x =>
                                x.Id ==
                                preparedItem.Id);


                    if (existingItem == null)
                    {
                        throw new BusinessException(
                            "Invalid Purchase Order Item record.");
                    }
                }


                if (existingItem != null)
                {
                    CopyPurchaseOrderItem(
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


                preparedItem.PurchaseOrderId =
                    existingPurchaseOrder.Id;

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


                existingPurchaseOrder.Items
                    .Add(
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


            CalculateHeaderTotals(
                existingPurchaseOrder);


            existingPurchaseOrder.ModifiedOn =
                DateTime.UtcNow;

            existingPurchaseOrder.ModifiedBy =
                "System";


            await _purchaseOrderRepository
                .UpdateAsync(
                    existingPurchaseOrder);

            await _purchaseOrderRepository
                .SaveChangesAsync();
        }

        #endregion


        // =====================================================
        // REGION 4 — Purchase Order Workflow
        // =====================================================

        #region Purchase Order Workflow

        public async Task ConfirmAsync(
            int purchaseOrderId)
        {
            var purchaseOrder =
                await _purchaseOrderRepository
                    .GetByIdAsync(
                        purchaseOrderId);


            if (purchaseOrder == null)
            {
                throw new BusinessException(
                    "Purchase Order not found.");
            }


            if (purchaseOrder.Status !=
                PurchaseOrderStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Purchase Orders can be confirmed.");
            }


            if (purchaseOrder.Items == null ||
                !purchaseOrder.Items.Any(x =>
                    !x.IsDeleted))
            {
                throw new BusinessException(
                    "Purchase Order must contain at least one Item.");
            }


            purchaseOrder.Status =
                PurchaseOrderStatus.Confirmed;

            purchaseOrder.ConfirmedOn =
                DateTime.UtcNow;

            purchaseOrder.ModifiedOn =
                DateTime.UtcNow;

            purchaseOrder.ModifiedBy =
                "System";


            await _purchaseOrderRepository
                .UpdateAsync(
                    purchaseOrder);

            await _purchaseOrderRepository
                .SaveChangesAsync();
        }


        public async Task MarkAsSentAsync(
            int purchaseOrderId)
        {
            var purchaseOrder =
                await _purchaseOrderRepository
                    .GetByIdAsync(
                        purchaseOrderId);


            if (purchaseOrder == null)
            {
                throw new BusinessException(
                    "Purchase Order not found.");
            }


            if (purchaseOrder.Status !=
                PurchaseOrderStatus.Confirmed)
            {
                throw new BusinessException(
                    "Only Confirmed Purchase Orders can be marked as Sent.");
            }


            purchaseOrder.Status =
                PurchaseOrderStatus.Sent;

            purchaseOrder.SentToSupplierOn =
                DateTime.UtcNow;

            purchaseOrder.ModifiedOn =
                DateTime.UtcNow;

            purchaseOrder.ModifiedBy =
                "System";


            await _purchaseOrderRepository
                .UpdateAsync(
                    purchaseOrder);

            await _purchaseOrderRepository
                .SaveChangesAsync();
        }

        #endregion


        // =====================================================
        // REGION 5 — Delete Purchase Order
        // =====================================================

        #region Delete Purchase Order

        public async Task DeleteAsync(
            int purchaseOrderId)
        {
            var purchaseOrder =
                await _purchaseOrderRepository
                    .GetByIdAsync(
                        purchaseOrderId);


            if (purchaseOrder == null)
            {
                throw new BusinessException(
                    "Purchase Order not found.");
            }


            if (purchaseOrder.Status !=
                PurchaseOrderStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Purchase Orders can be deleted. " +
                    "Confirmed Purchase Orders must be cancelled.");
            }


            purchaseOrder.ModifiedOn =
                DateTime.UtcNow;

            purchaseOrder.ModifiedBy =
                "System";


            await _purchaseOrderRepository
                .DeleteAsync(
                    purchaseOrder);

            await _purchaseOrderRepository
                .SaveChangesAsync();
        }

        #endregion


        // =====================================================
        // REGION 6 — Prepare Items
        // =====================================================

        #region Prepare Items

        private async Task PrepareItemsAsync(
            PurchaseOrder purchaseOrder,
            bool isIntraState,
            bool isCreate)
        {
            if (purchaseOrder.Items == null ||
                purchaseOrder.Items.Count == 0)
            {
                throw new BusinessException(
                    "At least one Item is required.");
            }


            foreach (var purchaseOrderItem
                in purchaseOrder.Items)
            {
                var originalId =
                    purchaseOrderItem.Id;


                NormalizePurchaseOrderItem(
                    purchaseOrderItem);


                ValidatePurchaseOrderItem(
                    purchaseOrderItem);


                var item =
                    await _itemRepository
                        .GetByIdAsync(
                            purchaseOrderItem.ItemId);


                if (item == null ||
                    item.IsDeleted)
                {
                    throw new BusinessException(
                        "Selected Item does not exist.");
                }


                if (!item.IsActive)
                {
                    throw new BusinessException(
                        $"Item {item.ItemCode} - " +
                        $"{item.ItemName} is inactive.");
                }


                /*
                 * Item Snapshot
                 */
                purchaseOrderItem.ItemCode =
                    item.ItemCode;

                purchaseOrderItem.ItemName =
                    item.ItemName;

                purchaseOrderItem.Description =
                    item.Description;

                purchaseOrderItem.UnitName =
                    item.Uom?.UomName;

                purchaseOrderItem.Specification =
                    BuildSpecificationSnapshot(
                        item);


                /*
                 * Purchase Order Discount is not used.
                 *
                 * Columns remain in DB for compatibility,
                 * but values always stay zero.
                 */
                purchaseOrderItem.DiscountPercent =
                    0;

                purchaseOrderItem.DiscountAmount =
                    0;


                // =============================================
                // Drawing Snapshot
                // =============================================

                if (purchaseOrderItem.DrawingId
                    .HasValue)
                {
                    var drawing =
                        await _drawingRepository
                            .GetByIdAsync(
                                purchaseOrderItem
                                    .DrawingId.Value);


                    if (drawing == null ||
                        drawing.IsDeleted)
                    {
                        throw new BusinessException(
                            $"Selected Drawing for Item " +
                            $"{item.ItemCode} does not exist.");
                    }


                    if (drawing.ItemId !=
                        item.ItemId)
                    {
                        throw new BusinessException(
                            $"Selected Drawing does not belong to " +
                            $"Item {item.ItemCode} - {item.ItemName}.");
                    }


                    if (!drawing.IsActive)
                    {
                        throw new BusinessException(
                            $"Selected Drawing revision " +
                            $"{drawing.RevisionNumber} is not Current.");
                    }


                    purchaseOrderItem.DrawingNumber =
                        drawing.DrawingNumber;

                    purchaseOrderItem.DrawingRevision =
                        drawing.RevisionNumber;
                }
                else
                {
                    purchaseOrderItem.DrawingNumber =
                        null;

                    purchaseOrderItem.DrawingRevision =
                        null;
                }


                CalculateLineAmounts(
                    purchaseOrderItem,
                    isIntraState);


                if (isCreate)
                {
                    purchaseOrderItem.Code =
                        GenerateLineCode();

                    purchaseOrderItem.IsActive =
                        true;

                    purchaseOrderItem.IsDeleted =
                        false;

                    purchaseOrderItem.CreatedOn =
                        DateTime.UtcNow;

                    purchaseOrderItem.CreatedBy =
                        "System";
                }
                else
                {
                    /*
                     * Preserve submitted Id so Update can
                     * map back to existing child record.
                     */
                    purchaseOrderItem.Id =
                        originalId;
                }
            }
        }

        #endregion


        // =====================================================
        // REGION 7 — Line Calculation
        // =====================================================

        #region Line Calculation

        private static void CalculateLineAmounts(
            PurchaseOrderItem item,
            bool isIntraState)
        {
            /*
             * Purchase Order calculation:
             *
             * Quantity × Rate
             * = Taxable Amount
             */

            var grossAmount =
                item.Quantity *
                item.UnitPrice;


            /*
             * Discount is intentionally disabled
             * in Purchase Order.
             */
            item.DiscountPercent =
                0;

            item.DiscountAmount =
                0;


            item.TaxableAmount =
                RoundAmount(
                    grossAmount);


            /*
             * Reset GST amounts before recalculation.
             */
            item.CGSTAmount =
                0;

            item.SGSTAmount =
                0;

            item.IGSTAmount =
                0;


            if (item.GSTPercent > 0)
            {
                /*
                 * Same State
                 * GST => CGST + SGST
                 */
                if (isIntraState)
                {
                    var halfGST =
                        item.GSTPercent /
                        2m;


                    item.CGSTAmount =
                        RoundAmount(
                            item.TaxableAmount *
                            halfGST /
                            100m);


                    item.SGSTAmount =
                        RoundAmount(
                            item.TaxableAmount *
                            halfGST /
                            100m);
                }

                /*
                 * Different State
                 * GST => IGST
                 */
                else
                {
                    item.IGSTAmount =
                        RoundAmount(
                            item.TaxableAmount *
                            item.GSTPercent /
                            100m);
                }
            }


            item.LineTotal =
                RoundAmount(
                    item.TaxableAmount +
                    item.CGSTAmount +
                    item.SGSTAmount +
                    item.IGSTAmount);
        }

        #endregion


        // =====================================================
        // REGION 8 — Header Calculation
        // =====================================================

        #region Header Calculation

        private static void CalculateHeaderTotals(
            PurchaseOrder purchaseOrder)
        {
            var activeItems =
                purchaseOrder.Items
                    .Where(x =>
                        !x.IsDeleted)
                    .ToList();


            purchaseOrder.SubTotal =
                RoundAmount(
                    activeItems.Sum(x =>
                        x.Quantity *
                        x.UnitPrice));


            /*
             * Discount is not applicable
             * in Purchase Order.
             */
            purchaseOrder.DiscountAmount =
                0;


            purchaseOrder.TaxableAmount =
                RoundAmount(
                    activeItems.Sum(x =>
                        x.TaxableAmount));


            purchaseOrder.CGSTAmount =
                RoundAmount(
                    activeItems.Sum(x =>
                        x.CGSTAmount));


            purchaseOrder.SGSTAmount =
                RoundAmount(
                    activeItems.Sum(x =>
                        x.SGSTAmount));


            purchaseOrder.IGSTAmount =
                RoundAmount(
                    activeItems.Sum(x =>
                        x.IGSTAmount));


            /*
             * Round Off is not applicable
             * in Purchase Order.
             */
            purchaseOrder.RoundOffAmount =
                0;


            /*
             * Current ERP Rule:
             *
             * Item Taxable
             * + Item GST
             * + Transport Charges
             * + Other Charges
             * = Grand Total
             *
             * No separate GST on Transport /
             * Other Charges currently.
             */
            purchaseOrder.GrandTotal =
                RoundAmount(
                    purchaseOrder.TaxableAmount +
                    purchaseOrder.CGSTAmount +
                    purchaseOrder.SGSTAmount +
                    purchaseOrder.IGSTAmount +
                    purchaseOrder.TransportCharges +
                    purchaseOrder.OtherCharges);
        }

        #endregion


        // =====================================================
        // REGION 9 — Company Validation / Snapshot
        // =====================================================

        #region Company Validation / Snapshot

        private async Task<Company>
            ValidateCompanyAsync(
                int companyId)
        {
            if (companyId <= 0)
            {
                throw new BusinessException(
                    "Please select a Company.");
            }


            var company =
                await _companyRepository
                    .GetByIdAsync(
                        companyId);


            if (company == null ||
                company.IsDeleted)
            {
                throw new BusinessException(
                    "Selected Company does not exist.");
            }


            if (!company.IsActive)
            {
                throw new BusinessException(
                    "Selected Company is inactive.");
            }


            return company;
        }


        private static void ApplyCompanySnapshot(
            PurchaseOrder purchaseOrder,
            Company company)
        {
            purchaseOrder.CompanyName =
                company.CompanyName;

            purchaseOrder.CompanyGSTIN =
                company.GstNumber;

            purchaseOrder.CompanyState =
                company.State;

            purchaseOrder.CompanyPhone =
                company.PhoneNumber;

            purchaseOrder.CompanyEmail =
                company.Email;


            purchaseOrder.CompanyAddress =
                BuildAddress(
                    company.Address,
                    null,
                    company.City,
                    company.State,
                    company.PostalCode,
                    company.Country);


            /*
             * Standard Purchase Order Terms &
             * Conditions Snapshot.
             *
             * Once PO is confirmed, it cannot be edited,
             * therefore this snapshot is preserved.
             */
            purchaseOrder.TermsAndConditions =
                company
                    .PurchaseOrderTermsAndConditions;


            if (string.IsNullOrWhiteSpace(
                purchaseOrder.DeliveryAddress))
            {
                purchaseOrder.DeliveryAddress =
                    purchaseOrder.CompanyAddress;
            }
        }

        #endregion


        // =====================================================
        // REGION 10 — Supplier Validation / Snapshot
        // =====================================================

        #region Supplier Validation / Snapshot

        private async Task<Supplier>
            ValidateSupplierAsync(
                int supplierId)
        {
            if (supplierId <= 0)
            {
                throw new BusinessException(
                    "Please select a Supplier.");
            }


            var supplier =
                await _supplierRepository
                    .GetByIdAsync(
                        supplierId);


            if (supplier == null ||
                supplier.IsDeleted)
            {
                throw new BusinessException(
                    "Selected Supplier does not exist.");
            }


            if (!supplier.IsActive)
            {
                throw new BusinessException(
                    "Selected Supplier is inactive.");
            }


            return supplier;
        }


        private static void ApplySupplierSnapshot(
            PurchaseOrder purchaseOrder,
            Supplier supplier)
        {
            purchaseOrder.SupplierName =
                supplier.SupplierName;

            purchaseOrder.SupplierGSTIN =
                supplier.Gstin;

            purchaseOrder.SupplierContactPerson =
                supplier.ContactPerson;

            purchaseOrder.SupplierPhone =
                supplier.MobileNumber;

            purchaseOrder.SupplierEmail =
                supplier.Email;


            purchaseOrder.SupplierAddress =
                BuildAddress(
                    supplier.AddressLine1,
                    supplier.AddressLine2,
                    supplier.City,
                    supplier.State,
                    supplier.Pincode,
                    null);


            if (string.IsNullOrWhiteSpace(
                    purchaseOrder.PaymentTerms) &&
                supplier.PaymentTermsDays.HasValue)
            {
                purchaseOrder.PaymentTerms =
                    $"{supplier.PaymentTermsDays.Value} Days";
            }
        }

        #endregion


        // =====================================================
        // REGION 11 — GST Type
        // =====================================================

        #region GST Type

        private static bool ResolveTaxType(
            Company company,
            Supplier supplier,
            IEnumerable<PurchaseOrderItem> items)
        {
            var hasGST =
                items.Any(x =>
                    x.GSTPercent > 0);


            /*
             * When no GST is applicable,
             * split type has no financial impact.
             */
            if (!hasGST)
            {
                return true;
            }


            return DetermineIntraState(
                company,
                supplier);
        }


        private static bool DetermineIntraState(
            Company company,
            Supplier supplier)
        {
            /*
             * IMPORTANT:
             *
             * GSTIN is optional in this ERP.
             *
             * Tax type is determined only from:
             *
             * Company.State
             * Supplier.State
             */

            var companyState =
                NormalizeComparableText(
                    company.State);

            var supplierState =
                NormalizeComparableText(
                    supplier.State);


            if (string.IsNullOrWhiteSpace(
                companyState))
            {
                throw new BusinessException(
                    "Company State is required to calculate GST.");
            }


            if (string.IsNullOrWhiteSpace(
                supplierState))
            {
                throw new BusinessException(
                    "Supplier State is required to calculate GST.");
            }


            return string.Equals(
                companyState,
                supplierState,
                StringComparison.OrdinalIgnoreCase);
        }

        #endregion


        // =====================================================
        // REGION 12 — PO Number Generation
        // =====================================================

        #region PO Number Generation

        private async Task<string>
            GeneratePurchaseOrderCodeAsync(
                DateTime poDate)
        {
            var financialYear =
                GetFinancialYear(
                    poDate);


            var prefix =
                $"AI/PO/{financialYear}/";


            var lastCode =
                await _purchaseOrderRepository
                    .GetLastPurchaseOrderCodeAsync(
                        prefix);


            var nextNumber =
                1;


            if (!string.IsNullOrWhiteSpace(
                lastCode))
            {
                var numberPart =
                    lastCode
                        .Replace(
                            prefix,
                            string.Empty,
                            StringComparison
                                .OrdinalIgnoreCase)
                        .Trim();


                if (int.TryParse(
                    numberPart,
                    out var lastNumber))
                {
                    nextNumber =
                        lastNumber +
                        1;
                }
            }


            var purchaseOrderCode =
                $"{prefix}{nextNumber:D5}";


            /*
             * Defensive collision check.
             *
             * Deleted PO numbers are also never reused.
             */
            while (await _purchaseOrderRepository
                .ExistsByCodeAsync(
                    purchaseOrderCode))
            {
                nextNumber++;


                purchaseOrderCode =
                    $"{prefix}{nextNumber:D5}";
            }


            return purchaseOrderCode;
        }


        private static string GetFinancialYear(
            DateTime date)
        {
            int startYear;
            int endYear;


            if (date.Month >= 4)
            {
                startYear =
                    date.Year;

                endYear =
                    date.Year +
                    1;
            }
            else
            {
                startYear =
                    date.Year -
                    1;

                endYear =
                    date.Year;
            }


            return
                $"{startYear % 100:D2}-" +
                $"{endYear % 100:D2}";
        }

        #endregion


        // =====================================================
        // REGION 13 — Specification Snapshot
        // =====================================================

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
                        !string.IsNullOrWhiteSpace(
                            x))
                    .ToList();


            return rows.Count == 0
                ? null
                : string.Join(
                    " | ",
                    rows);
        }

        #endregion


        // =====================================================
        // REGION 14 — Copy Item
        // =====================================================

        #region Copy Item

        private static void CopyPurchaseOrderItem(
            PurchaseOrderItem source,
            PurchaseOrderItem target)
        {
            target.ItemId =
                source.ItemId;

            target.ItemCode =
                source.ItemCode;

            target.ItemName =
                source.ItemName;

            target.Description =
                source.Description;

            target.Specification =
                source.Specification;

            target.UnitName =
                source.UnitName;

            target.HSNCode =
                source.HSNCode;

            target.DrawingId =
                source.DrawingId;

            target.DrawingNumber =
                source.DrawingNumber;

            target.DrawingRevision =
                source.DrawingRevision;

            target.Quantity =
                source.Quantity;

            target.UnitPrice =
                source.UnitPrice;


            /*
             * Purchase Order does not use Discount.
             */
            target.DiscountPercent =
                0;

            target.DiscountAmount =
                0;


            target.TaxableAmount =
                source.TaxableAmount;

            target.GSTPercent =
                source.GSTPercent;

            target.CGSTAmount =
                source.CGSTAmount;

            target.SGSTAmount =
                source.SGSTAmount;

            target.IGSTAmount =
                source.IGSTAmount;

            target.LineTotal =
                source.LineTotal;

            target.RequiredDate =
                source.RequiredDate;

            target.Remarks =
                source.Remarks;

            target.IsActive =
                true;

            target.IsDeleted =
                false;
        }

        #endregion


        // =====================================================
        // REGION 15 — Normalization
        // =====================================================

        #region Normalization

        private static void NormalizePurchaseOrder(
            PurchaseOrder purchaseOrder)
        {
            purchaseOrder.DeliveryAddress =
                NormalizeText(
                    purchaseOrder.DeliveryAddress);

            purchaseOrder.PaymentTerms =
                NormalizeText(
                    purchaseOrder.PaymentTerms);

            purchaseOrder.DeliveryTerms =
                NormalizeText(
                    purchaseOrder.DeliveryTerms);

            purchaseOrder.Remarks =
                NormalizeText(
                    purchaseOrder.Remarks);


            /*
             * Purchase Order Round Off is disabled.
             */
            purchaseOrder.RoundOffAmount =
                0;
        }


        private static void NormalizePurchaseOrderItem(
            PurchaseOrderItem item)
        {
            item.HSNCode =
                NormalizeUpperText(
                    item.HSNCode);

            item.Remarks =
                NormalizeText(
                    item.Remarks);


            /*
             * Ignore any Discount value posted from
             * UI / API / old client.
             */
            item.DiscountPercent =
                0;

            item.DiscountAmount =
                0;
        }


        private static string?
            NormalizeText(
                string? value)
        {
            if (string.IsNullOrWhiteSpace(
                value))
            {
                return null;
            }


            return Regex.Replace(
                value.Trim(),
                @"\s+",
                " ");
        }


        private static string?
            NormalizeUpperText(
                string? value)
        {
            return NormalizeText(
                value)?
                .ToUpperInvariant();
        }


        private static string?
            NormalizeComparableText(
                string? value)
        {
            return NormalizeText(
                value)?
                .ToUpperInvariant();
        }

        #endregion


        // =====================================================
        // REGION 16 — Validation
        // =====================================================

        #region Validation

        private static void ValidatePurchaseOrder(
            PurchaseOrder purchaseOrder)
        {
            if (purchaseOrder.CompanyId <=
                0)
            {
                throw new BusinessException(
                    "Please select a Company.");
            }


            if (purchaseOrder.SupplierId <=
                0)
            {
                throw new BusinessException(
                    "Please select a Supplier.");
            }


            if (purchaseOrder.PODate ==
                default)
            {
                throw new BusinessException(
                    "PO Date is required.");
            }


            if (purchaseOrder
                    .ExpectedDeliveryDate
                    .HasValue &&
                purchaseOrder
                    .ExpectedDeliveryDate
                    .Value.Date <
                purchaseOrder.PODate.Date)
            {
                throw new BusinessException(
                    "Expected Delivery Date cannot be before PO Date.");
            }


            if (purchaseOrder.TransportCharges <
                0)
            {
                throw new BusinessException(
                    "Transport Charges cannot be negative.");
            }


            if (purchaseOrder.OtherCharges <
                0)
            {
                throw new BusinessException(
                    "Other Charges cannot be negative.");
            }


            if (purchaseOrder.PaymentTerms?.Length >
                500)
            {
                throw new BusinessException(
                    "Payment Terms cannot exceed 500 characters.");
            }


            if (purchaseOrder.DeliveryTerms?.Length >
                500)
            {
                throw new BusinessException(
                    "Delivery Terms cannot exceed 500 characters.");
            }


            if (purchaseOrder.Remarks?.Length >
                1000)
            {
                throw new BusinessException(
                    "Remarks cannot exceed 1000 characters.");
            }


            if (purchaseOrder.Items == null ||
                purchaseOrder.Items.Count ==
                0)
            {
                throw new BusinessException(
                    "At least one Item is required.");
            }
        }


        private static void ValidatePurchaseOrderItem(
            PurchaseOrderItem item)
        {
            if (item.ItemId <= 0)
            {
                throw new BusinessException(
                    "Please select an Item.");
            }


            if (item.Quantity <= 0)
            {
                throw new BusinessException(
                    "Item Quantity must be greater than zero.");
            }


            if (item.UnitPrice < 0)
            {
                throw new BusinessException(
                    "Item Rate cannot be negative.");
            }


            /*
             * Discount validation removed because
             * Discount is not used in Purchase Order.
             */


            if (item.GSTPercent < 0 ||
                item.GSTPercent > 100)
            {
                throw new BusinessException(
                    "GST percentage must be between 0 and 100.");
            }


            if (item.HSNCode?.Length >
                50)
            {
                throw new BusinessException(
                    "HSN Code cannot exceed 50 characters.");
            }


            if (item.Remarks?.Length >
                500)
            {
                throw new BusinessException(
                    "Item Remarks cannot exceed 500 characters.");
            }
        }

        #endregion


        // =====================================================
        // REGION 17 — Helpers
        // =====================================================

        #region Helpers

        private static decimal RoundAmount(
            decimal value)
        {
            return Math.Round(
                value,
                2,
                MidpointRounding
                    .AwayFromZero);
        }


        private static string GenerateLineCode()
        {
            /*
             * Internal child row code.
             *
             * Independent from display sequence,
             * therefore deleted codes are never reused.
             */
            return
                $"POL{Guid.NewGuid()
                    .ToString("N")
                    .Substring(
                        0,
                        12)
                    .ToUpperInvariant()}";
        }


        private static string? BuildAddress(
            string? line1,
            string? line2,
            string? city,
            string? state,
            string? postalCode,
            string? country)
        {
            var parts =
                new[]
                {
                    line1,
                    line2,
                    city,
                    state,
                    postalCode,
                    country
                }
                .Where(x =>
                    !string.IsNullOrWhiteSpace(
                        x))
                .Select(x =>
                    x!.Trim())
                .ToList();


            return parts.Count == 0
                ? null
                : string.Join(
                    ", ",
                    parts);
        }

        #endregion
    }
}