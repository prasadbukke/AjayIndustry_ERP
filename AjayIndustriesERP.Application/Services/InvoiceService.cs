/*
============================================================
File: InvoiceService.cs

Module:
Invoice

Purpose:
Contains complete Invoice business logic.

Invoice Source Flow:

Customer Purchase Order
        ↓
Production Job
        ↓
Production Job Item
        ↓
Invoice Item

Invoice Source Identity:

ProductionJobId
        +
CustomerPurchaseOrderItemId

Important Business Rules:

- One Customer PO may have one PO-level Production Job.
- One Production Job may contain multiple Production Items.
- Invoice availability is calculated Item-wise.
- CompletedQuantity is the trusted invoiceable quantity.
- Current Production plan must be completed before that
  Production Item becomes invoiceable.

Invoiceable Production Item:

ProductionQuantity > 0
AND
CompletedQuantity > 0
AND
CompletedQuantity >= ProductionQuantity

- Draft + Finalized active Invoices reserve quantity.
- Deleted Invoices do NOT reserve quantity.
- PDI is NOT mandatory.
- Delivery Challan is NOT mandatory.
- Missing PDI / Delivery Challan is warning-only.
- Warning requires explicit confirmation before Create,
  Update or Finalize.
- Restore intentionally does not block on warning.
- Browser-posted source snapshots are NOT trusted.
- Browser-posted calculated amounts are NOT trusted.
- Customer / Company snapshots are captured on Create.
- Historical snapshots are not refreshed during normal Edit
  or Finalization.

Invoice Code:
AI/INV/{YY-YY}/{00001}

Example:
AI/INV/26-27/00001
============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using AjayIndustriesERP.Domain.Enums;
using System.Reflection;
using System.Text.Json;

namespace AjayIndustriesERP.Application.Services
{
    public class InvoiceService
        : IInvoiceService
    {
        #region Fields

        private readonly IInvoiceRepository
            _repository;

        private readonly IInvoicePdfGenerator
            _pdfGenerator;

        #endregion


        #region Constructor

        public InvoiceService(
            IInvoiceRepository repository,
            IInvoicePdfGenerator pdfGenerator)
        {
            _repository =
                repository;

            _pdfGenerator =
                pdfGenerator;
        }

        #endregion


        #region Read Operations

        public async Task<Invoice?>
            GetByIdAsync(
                int id)
        {
            if (id <= 0)
            {
                return null;
            }


            return await _repository
                .GetByIdAsync(
                    id);
        }

        #endregion


        #region Search And Pagination

        public async Task<PagedResult<Invoice>>
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


        public async Task<PagedResult<Invoice>>
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


        #region Customer Purchase Order Sources

        public async Task<List<CustomerPurchaseOrder>>
            GetCustomerPurchaseOrdersForInvoiceAsync()
        {
            return await _repository
                .GetCustomerPurchaseOrdersForInvoiceAsync();
        }


        public async Task<CustomerPurchaseOrder?>
            GetCustomerPurchaseOrderForInvoiceAsync(
                int customerPurchaseOrderId)
        {
            if (customerPurchaseOrderId <= 0)
            {
                return null;
            }


            return await _repository
                .GetCustomerPurchaseOrderForInvoiceAsync(
                    customerPurchaseOrderId);
        }

        #endregion


        #region Completed Production Job Sources

        public async Task<List<ProductionJob>>
            GetCompletedProductionJobsForInvoiceAsync(
                int customerPurchaseOrderId)
        {
            if (customerPurchaseOrderId <= 0)
            {
                return new List<ProductionJob>();
            }


            var jobs =
                await _repository
                    .GetCompletedProductionJobsForInvoiceAsync(
                        customerPurchaseOrderId);


            var result =
                new List<ProductionJob>();


            foreach (var productionJob
                in jobs)
            {
                var completedItems =
                    GetCompletedProductionItems(
                        productionJob);


                var hasAvailableItem =
                    false;


                foreach (var productionJobItem
                    in completedItems)
                {
                    var allocatedQuantity =
                        await _repository
                            .GetAllocatedInvoiceQuantityAsync(
                                productionJob.Id,
                                productionJobItem
                                    .CustomerPurchaseOrderItemId);


                    var remainingQuantity =
                        productionJobItem
                            .CompletedQuantity
                        -
                        allocatedQuantity;


                    if (remainingQuantity > 0)
                    {
                        hasAvailableItem =
                            true;

                        break;
                    }
                }


                if (hasAvailableItem)
                {
                    result.Add(
                        productionJob);
                }
            }


            return result;
        }


        public async Task<ProductionJob?>
            GetCompletedProductionJobForInvoiceAsync(
                int productionJobId)
        {
            if (productionJobId <= 0)
            {
                return null;
            }


            return await _repository
                .GetCompletedProductionJobForInvoiceAsync(
                    productionJobId);
        }

        #endregion


        #region Invoice Quantity Availability

        public async Task<decimal>
            GetRemainingInvoiceQuantityAsync(
                int productionJobId,
                int customerPurchaseOrderItemId,
                int? excludeInvoiceId = null)
        {
            if (productionJobId <= 0)
            {
                throw new BusinessException(
                    "Invalid Production Job.");
            }


            if (customerPurchaseOrderItemId <= 0)
            {
                throw new BusinessException(
                    "Invalid Customer Purchase Order Item.");
            }


            var productionJob =
                await ValidateCompletedProductionJobAsync(
                    productionJobId);


            var productionJobItem =
                ValidateCompletedProductionItem(
                    productionJob,
                    customerPurchaseOrderItemId);


            var allocatedQuantity =
                await _repository
                    .GetAllocatedInvoiceQuantityAsync(
                        productionJobId,
                        customerPurchaseOrderItemId,
                        excludeInvoiceId);


            var remainingQuantity =
                productionJobItem.CompletedQuantity
                -
                allocatedQuantity;


            return remainingQuantity < 0m
                ? 0m
                : remainingQuantity;
        }

        #endregion


        #region PDI And Delivery Challan Warning

        public async Task<List<int>>
            GetProductionJobIdsRequiringWarningAsync(
                IEnumerable<int> productionJobIds)
        {
            if (productionJobIds == null)
            {
                return new List<int>();
            }


            var jobIds =
                productionJobIds
                    .Where(x =>
                        x > 0)
                    .Distinct()
                    .ToList();


            var warningJobIds =
                new List<int>();


            foreach (var productionJobId
                in jobIds)
            {
                var productionJob =
                    await _repository
                        .GetCompletedProductionJobForInvoiceAsync(
                            productionJobId);


                if (productionJob == null)
                {
                    continue;
                }


                var completedItems =
                    GetCompletedProductionItems(
                        productionJob);


                var requiresWarning =
                    false;


                foreach (var productionJobItem
                    in completedItems)
                {
                    var customerPurchaseOrderItemId =
                        productionJobItem
                            .CustomerPurchaseOrderItemId;


                    var hasPdi =
                        await _repository
                            .HasFinalizedPdiAsync(
                                productionJob.Id,
                                customerPurchaseOrderItemId);


                    var hasDeliveryChallan =
                        await _repository
                            .HasDeliveryChallanAsync(
                                productionJob.Id,
                                customerPurchaseOrderItemId);


                    if (!hasPdi ||
                        !hasDeliveryChallan)
                    {
                        requiresWarning =
                            true;

                        break;
                    }
                }


                if (requiresWarning)
                {
                    warningJobIds.Add(
                        productionJobId);
                }
            }


            return warningJobIds;
        }

        #endregion


        #region Prepare Draft

        public async Task<Invoice?>
            PrepareDraftAsync(
                int customerPurchaseOrderId)
        {
            #region Customer PO

            var customerPurchaseOrder =
                await ValidateCustomerPurchaseOrderAsync(
                    customerPurchaseOrderId);

            #endregion


            #region Customer

            var customer =
                await ValidateCustomerAsync(
                    GetCustomerId(
                        customerPurchaseOrder));

            #endregion


            #region Company

            var company =
                await ValidateCompanyAsync();

            #endregion


            #region Header

            var invoiceDate =
                DateTime.Today;


            var invoice =
                new Invoice
                {
                    InvoiceDate =
                        invoiceDate,

                    Status =
                        InvoiceStatus.Draft,

                    CustomerId =
                        customer.Id,

                    CustomerName =
                        customer.CustomerName,

                    CustomerSnapshotJson =
                        SerializeScalarSnapshot(
                            customer),

                    BillingAddressLine1 =
                        customer.AddressLine1,

                    BillingAddressLine2 =
                        customer.AddressLine2,

                    BillingCity =
                        customer.City,

                    BillingDistrict =
                        customer.District,

                    BillingState =
                        customer.State,

                    BillingPincode =
                        customer.Pincode,

                    BillingCountry =
                        customer.Country,

                    CompanyId =
                        company.CompanyId,

                    CompanyName =
                        company.CompanyName,

                    CompanySnapshotJson =
                        SerializeScalarSnapshot(
                            company),

                    PaymentTerms =
                        customer.PaymentTerms,

                    CreditDays =
                        customer.CreditDays,

                    DueDate =
                        CalculateDueDate(
                            invoiceDate,
                            customer.CreditDays),

                    PlaceOfSupply =
                        customer.State,

                    InvoiceTermsAndConditions =
                        company
                            .InvoiceTermsAndConditions,

                    OtherCharges =
                        0m,

                    RoundOffAmount =
                        0m,

                    IsActive =
                        true,

                    IsDeleted =
                        false
                };


            invoice.IsInterState =
                DetermineInterStateForPreview(
                    company.State,
                    invoice.BillingState);

            #endregion


            #region Production Sources

            var productionJobs =
                await _repository
                    .GetCompletedProductionJobsForInvoiceAsync(
                        customerPurchaseOrderId);


            var sequenceNumber =
                1;


            foreach (var productionJob
                in productionJobs
                    .OrderBy(x =>
                        x.Id))
            {
                var completedItems =
                    GetCompletedProductionItems(
                        productionJob);


                foreach (var productionJobItem
                    in completedItems
                        .OrderBy(x =>
                            x.Id))
                {
                    var allocatedQuantity =
                        await _repository
                            .GetAllocatedInvoiceQuantityAsync(
                                productionJob.Id,
                                productionJobItem
                                    .CustomerPurchaseOrderItemId);


                    var availableQuantity =
                        productionJobItem
                            .CompletedQuantity
                        -
                        allocatedQuantity;


                    if (availableQuantity <= 0m)
                    {
                        continue;
                    }


                    var invoiceItem =
                        CreateProductionSourceSnapshot(
                            customerPurchaseOrder,
                            productionJob,
                            productionJobItem);


                    invoiceItem.SequenceNumber =
                        sequenceNumber++;


                    invoiceItem.InvoiceQuantity =
                        availableQuantity;


                    invoiceItem.Rate =
                        0m;

                    invoiceItem.DiscountPercent =
                        0m;

                    invoiceItem.GstRate =
                        18m;


                    CalculateLineAmounts(
                        invoiceItem,
                        invoice.IsInterState);


                    invoice.Items.Add(
                        invoiceItem);
                }
            }

            #endregion


            if (invoice.Items.Count == 0)
            {
                throw new BusinessException(
                    "Selected Customer Purchase Order has no Completed Production quantity available for Invoice.");
            }


            CalculateHeaderTotals(
                invoice);


            return invoice;
        }

        #endregion


        #region Create Invoice

        public async Task<Invoice>
            CreateAsync(
                Invoice invoice,
                bool confirmSourceWarning)
        {
            if (invoice == null)
            {
                throw new BusinessException(
                    "Invoice information is required.");
            }


            NormalizeHeader(
                invoice);


            ValidateHeader(
                invoice);


            ValidateSubmittedItems(
                invoice.Items);


            #region Resolve Customer PO

            var firstSubmittedItem =
                invoice.Items.First();


            var firstProductionJobId =
                GetRequiredProductionJobId(
                    firstSubmittedItem);


            var firstProductionJob =
                await ValidateCompletedProductionJobAsync(
                    firstProductionJobId);


            var customerPurchaseOrderId =
                GetCustomerPurchaseOrderId(
                    firstProductionJob);


            var customerPurchaseOrder =
                await ValidateCustomerPurchaseOrderAsync(
                    customerPurchaseOrderId);

            #endregion


            #region Customer / Company

            var customer =
                await ValidateCustomerAsync(
                    GetCustomerId(
                        customerPurchaseOrder));


            var company =
                await ValidateCompanyAsync();

            #endregion


            #region Header

            var newInvoice =
                new Invoice
                {
                    Code =
                        await GenerateInvoiceCodeAsync(
                            invoice.InvoiceDate),

                    InvoiceDate =
                        invoice.InvoiceDate,

                    Status =
                        InvoiceStatus.Draft,

                    CustomerId =
                        customer.Id,

                    CustomerName =
                        customer.CustomerName,

                    CustomerSnapshotJson =
                        SerializeScalarSnapshot(
                            customer),

                    CompanyId =
                        company.CompanyId,

                    CompanyName =
                        company.CompanyName,

                    CompanySnapshotJson =
                        SerializeScalarSnapshot(
                            company),

                    PaymentTerms =
                        customer.PaymentTerms,

                    CreditDays =
                        customer.CreditDays,

                    OtherCharges =
                        invoice.OtherCharges,

                    Remarks =
                        invoice.Remarks,

                    InvoiceTermsAndConditions =
                        string.IsNullOrWhiteSpace(
                            invoice.InvoiceTermsAndConditions)
                            ? company
                                .InvoiceTermsAndConditions
                            : invoice
                                .InvoiceTermsAndConditions,

                    IsActive =
                        true,

                    IsDeleted =
                        false,

                    CreatedOn =
                        DateTime.UtcNow,

                    CreatedBy =
                        "System"
                };


            ApplySubmittedOrMasterBillingAddress(
                newInvoice,
                invoice,
                customer);


            newInvoice.DueDate =
                CalculateDueDate(
                    newInvoice.InvoiceDate,
                    newInvoice.CreditDays);


            newInvoice.PlaceOfSupply =
                newInvoice.BillingState;


            var hasGst =
                invoice.Items.Any(x =>
                    x.GstRate > 0m);


            newInvoice.IsInterState =
                DetermineInterState(
                    company.State,
                    newInvoice.BillingState,
                    hasGst);

            #endregion


            #region Trusted Items

            var preparedItems =
                await PrepareTrustedItemsAsync(
                    invoice.Items,
                    customerPurchaseOrder,
                    customer.Id,
                    excludeInvoiceId:
                        null,
                    newInvoice.IsInterState);


            await EnsureSourceWarningConfirmedAsync(
                preparedItems,
                confirmSourceWarning);


            var sequenceNumber =
                1;


            foreach (var preparedItem
                in preparedItems)
            {
                preparedItem.SequenceNumber =
                    sequenceNumber++;

                preparedItem.IsActive =
                    true;

                preparedItem.IsDeleted =
                    false;

                preparedItem.CreatedOn =
                    DateTime.UtcNow;

                preparedItem.CreatedBy =
                    "System";


                newInvoice.Items.Add(
                    preparedItem);
            }

            #endregion


            CalculateHeaderTotals(
                newInvoice);


            await _repository
                .AddAsync(
                    newInvoice);


            return newInvoice;
        }

        #endregion


        #region Update Invoice

        public async Task<Invoice>
            UpdateAsync(
                Invoice invoice,
                bool confirmSourceWarning)
        {
            if (invoice == null ||
                invoice.Id <= 0)
            {
                throw new BusinessException(
                    "Invalid Invoice.");
            }


            var existing =
                await _repository
                    .GetForUpdateAsync(
                        invoice.Id);


            if (existing == null)
            {
                throw new BusinessException(
                    "Invoice not found.");
            }


            if (existing.Status !=
                InvoiceStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Invoice can be edited.");
            }


            NormalizeHeader(
                invoice);


            ValidateHeader(
                invoice);


            ValidateSubmittedItems(
                invoice.Items);


            #region Financial Year

            var oldFinancialYear =
                GetFinancialYear(
                    existing.InvoiceDate);


            var newFinancialYear =
                GetFinancialYear(
                    invoice.InvoiceDate);


            if (!string.Equals(
                oldFinancialYear,
                newFinancialYear,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException(
                    "Invoice Date cannot be changed to another Financial Year.");
            }

            #endregion


            #region Snapshots

            if (string.IsNullOrWhiteSpace(
                existing.CustomerSnapshotJson))
            {
                throw new BusinessException(
                    "Customer snapshot is missing from Invoice.");
            }


            if (string.IsNullOrWhiteSpace(
                existing.CompanySnapshotJson))
            {
                throw new BusinessException(
                    "Company snapshot is missing from Invoice.");
            }

            #endregion


            #region Customer PO Source

            var firstProductionJob =
                await ValidateCompletedProductionJobAsync(
                    GetRequiredProductionJobId(
                        invoice.Items.First()));


            var customerPurchaseOrder =
                await ValidateCustomerPurchaseOrderAsync(
                    GetCustomerPurchaseOrderId(
                        firstProductionJob));


            if (GetCustomerId(
                    customerPurchaseOrder) !=
                existing.CustomerId)
            {
                throw new BusinessException(
                    "Selected Customer Purchase Order does not belong to the Invoice Customer.");
            }

            #endregion


            #region GST

            var companySnapshot =
                ParseSnapshot(
                    existing.CompanySnapshotJson);


            var companyState =
                GetSnapshotString(
                    companySnapshot,
                    "State");


            var hasGst =
                invoice.Items.Any(x =>
                    x.GstRate > 0m);


            var isInterState =
                DetermineInterState(
                    companyState,
                    invoice.BillingState,
                    hasGst);

            #endregion


            #region Trusted Items

            var preparedItems =
                await PrepareTrustedItemsAsync(
                    invoice.Items,
                    customerPurchaseOrder,
                    existing.CustomerId,
                    existing.Id,
                    isInterState);


            await EnsureSourceWarningConfirmedAsync(
                preparedItems,
                confirmSourceWarning);

            #endregion


            #region Update Header

            existing.InvoiceDate =
                invoice.InvoiceDate;


            existing.DueDate =
                CalculateDueDate(
                    invoice.InvoiceDate,
                    existing.CreditDays);


            existing.BillingAddressLine1 =
                invoice.BillingAddressLine1;

            existing.BillingAddressLine2 =
                invoice.BillingAddressLine2;

            existing.BillingCity =
                invoice.BillingCity;

            existing.BillingDistrict =
                invoice.BillingDistrict;

            existing.BillingState =
                invoice.BillingState;

            existing.BillingPincode =
                invoice.BillingPincode;

            existing.BillingCountry =
                invoice.BillingCountry;


            existing.PlaceOfSupply =
                invoice.BillingState;


            existing.IsInterState =
                isInterState;


            existing.OtherCharges =
                invoice.OtherCharges;


            existing.InvoiceTermsAndConditions =
                invoice.InvoiceTermsAndConditions;


            existing.Remarks =
                invoice.Remarks;


            existing.ModifiedOn =
                DateTime.UtcNow;

            existing.ModifiedBy =
                "System";

            #endregion


            SynchronizePreparedItems(
                existing,
                preparedItems);


            CalculateHeaderTotals(
                existing);


            await _repository
                .UpdateAsync(
                    existing);


            return existing;
        }

        #endregion


        #region Finalize Invoice

        public async Task<Invoice>
            FinalizeAsync(
                int id,
                bool confirmSourceWarning)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid Invoice.");
            }


            var invoice =
                await _repository
                    .GetForUpdateAsync(
                        id);


            if (invoice == null)
            {
                throw new BusinessException(
                    "Invoice not found.");
            }


            if (invoice.Status !=
                InvoiceStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Invoice can be finalized.");
            }


            if (string.IsNullOrWhiteSpace(
                invoice.CustomerSnapshotJson))
            {
                throw new BusinessException(
                    "Customer snapshot is missing from Invoice.");
            }


            if (string.IsNullOrWhiteSpace(
                invoice.CompanySnapshotJson))
            {
                throw new BusinessException(
                    "Company snapshot is missing from Invoice.");
            }


            var activeItems =
                invoice.Items
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();


            if (activeItems.Count == 0)
            {
                throw new BusinessException(
                    "Invoice must contain at least one Item.");
            }


            #region GST

            var companySnapshot =
                ParseSnapshot(
                    invoice.CompanySnapshotJson);


            var companyState =
                GetSnapshotString(
                    companySnapshot,
                    "State");


            var hasGst =
                activeItems.Any(x =>
                    x.GstRate > 0m);


            invoice.IsInterState =
                DetermineInterState(
                    companyState,
                    invoice.BillingState,
                    hasGst);


            invoice.PlaceOfSupply =
                invoice.BillingState;

            #endregion


            #region Rebuild Submitted Items

            var submittedItems =
                activeItems
                    .Select(x =>
                        new InvoiceItem
                        {
                            ProductionJobId =
                                x.ProductionJobId,

                            CustomerPurchaseOrderItemId =
                                x.CustomerPurchaseOrderItemId,

                            InvoiceQuantity =
                                x.InvoiceQuantity,

                            Rate =
                                x.Rate,

                            DiscountPercent =
                                x.DiscountPercent,

                            GstRate =
                                x.GstRate
                        })
                    .ToList();

            #endregion


            var firstProductionJob =
                await ValidateCompletedProductionJobAsync(
                    GetRequiredProductionJobId(
                        submittedItems.First()));


            var customerPurchaseOrder =
                await ValidateCustomerPurchaseOrderAsync(
                    GetCustomerPurchaseOrderId(
                        firstProductionJob));


            if (GetCustomerId(
                    customerPurchaseOrder) !=
                invoice.CustomerId)
            {
                throw new BusinessException(
                    "Selected Customer Purchase Order does not belong to the Invoice Customer.");
            }


            var preparedItems =
                await PrepareTrustedItemsAsync(
                    submittedItems,
                    customerPurchaseOrder,
                    invoice.CustomerId,
                    invoice.Id,
                    invoice.IsInterState);


            await EnsureSourceWarningConfirmedAsync(
                preparedItems,
                confirmSourceWarning);


            SynchronizePreparedItems(
                invoice,
                preparedItems);


            CalculateHeaderTotals(
                invoice);


            invoice.Status =
                InvoiceStatus.Finalized;

            invoice.FinalizedOn =
                DateTime.UtcNow;

            invoice.FinalizedBy =
                "System";

            invoice.ModifiedOn =
                DateTime.UtcNow;

            invoice.ModifiedBy =
                "System";


            await _repository
                .UpdateAsync(
                    invoice);


            return invoice;
        }

        #endregion


        #region Delete Invoice

        public async Task DeleteAsync(
            int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid Invoice.");
            }


            var invoice =
                await _repository
                    .GetForUpdateAsync(
                        id);


            if (invoice == null)
            {
                throw new BusinessException(
                    "Invoice not found.");
            }


            if (invoice.Status !=
                InvoiceStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Invoice can be deleted.");
            }


            invoice.IsDeleted =
                true;

            invoice.IsActive =
                false;

            invoice.ModifiedOn =
                DateTime.UtcNow;

            invoice.ModifiedBy =
                "System";


            await _repository
                .UpdateAsync(
                    invoice);
        }

        #endregion


        #region Deleted Invoices

        public async Task<List<Invoice>>
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
                    "Invalid Invoice.");
            }


            var invoice =
                await _repository
                    .GetDeletedForUpdateAsync(
                        id);


            if (invoice == null)
            {
                throw new BusinessException(
                    "Deleted Invoice not found.");
            }


            if (invoice.Status !=
                InvoiceStatus.Draft)
            {
                throw new BusinessException(
                    "Only deleted Draft Invoice can be restored.");
            }


            var originalItems =
                invoice.Items
                    .Where(x =>
                        !x.IsDeleted)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();


            if (originalItems.Count == 0)
            {
                throw new BusinessException(
                    "Deleted Invoice has no Items to restore.");
            }


            #region GST

            var companySnapshot =
                ParseSnapshot(
                    invoice.CompanySnapshotJson);


            var companyState =
                GetSnapshotString(
                    companySnapshot,
                    "State");


            var hasGst =
                originalItems.Any(x =>
                    x.GstRate > 0m);


            var isInterState =
                DetermineInterState(
                    companyState,
                    invoice.BillingState,
                    hasGst);

            #endregion


            #region Rebuild Lines

            var submittedItems =
                originalItems
                    .Select(x =>
                        new InvoiceItem
                        {
                            ProductionJobId =
                                x.ProductionJobId,

                            CustomerPurchaseOrderItemId =
                                x.CustomerPurchaseOrderItemId,

                            InvoiceQuantity =
                                x.InvoiceQuantity,

                            Rate =
                                x.Rate,

                            DiscountPercent =
                                x.DiscountPercent,

                            GstRate =
                                x.GstRate
                        })
                    .ToList();

            #endregion


            var firstProductionJob =
                await ValidateCompletedProductionJobAsync(
                    GetRequiredProductionJobId(
                        submittedItems.First()));


            var customerPurchaseOrder =
                await ValidateCustomerPurchaseOrderAsync(
                    GetCustomerPurchaseOrderId(
                        firstProductionJob));


            if (GetCustomerId(
                    customerPurchaseOrder) !=
                invoice.CustomerId)
            {
                throw new BusinessException(
                    "Customer Purchase Order does not belong to the Invoice Customer.");
            }


            var preparedItems =
                await PrepareTrustedItemsAsync(
                    submittedItems,
                    customerPurchaseOrder,
                    invoice.CustomerId,
                    invoice.Id,
                    isInterState);


            /*
             * PDI / DC warning intentionally does not block
             * Draft restore.
             */

            SynchronizePreparedItems(
                invoice,
                preparedItems);


            invoice.IsDeleted =
                false;

            invoice.IsActive =
                true;

            invoice.IsInterState =
                isInterState;

            invoice.PlaceOfSupply =
                invoice.BillingState;

            invoice.ModifiedOn =
                DateTime.UtcNow;

            invoice.ModifiedBy =
                "System";


            CalculateHeaderTotals(
                invoice);


            await _repository
                .UpdateAsync(
                    invoice);
        }

        #endregion


        #region PDF

        public async Task<byte[]>
            GeneratePdfAsync(
                int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid Invoice.");
            }


            var invoice =
                await _repository
                    .GetByIdAsync(
                        id);


            if (invoice == null)
            {
                throw new BusinessException(
                    "Invoice not found.");
            }


            if (invoice.Status !=
                InvoiceStatus.Finalized)
            {
                throw new BusinessException(
                    "Invoice PDF can be generated only after Finalization.");
            }


            return _pdfGenerator
                .Generate(
                    invoice);
        }

        #endregion


        #region Trusted Item Preparation

        private async Task<List<InvoiceItem>>
            PrepareTrustedItemsAsync(
                IEnumerable<InvoiceItem> submittedItems,
                CustomerPurchaseOrder customerPurchaseOrder,
                int expectedCustomerId,
                int? excludeInvoiceId,
                bool isInterState)
        {
            var items =
                submittedItems
                    ?.ToList()
                ?? new List<InvoiceItem>();


            if (items.Count == 0)
            {
                throw new BusinessException(
                    "Invoice must contain at least one Item.");
            }


            foreach (var item
                in items)
            {
                NormalizeItem(
                    item);


                ValidateItem(
                    item);
            }


            ValidateDuplicateProductionItems(
                items);


            if (GetCustomerId(
                    customerPurchaseOrder) !=
                expectedCustomerId)
            {
                throw new BusinessException(
                    "Customer Purchase Order does not belong to the Invoice Customer.");
            }


            var preparedItems =
                new List<InvoiceItem>();


            foreach (var submittedItem
                in items)
            {
                #region Source Identity

                var productionJobId =
                    GetRequiredProductionJobId(
                        submittedItem);


                var customerPurchaseOrderItemId =
                    GetRequiredCustomerPurchaseOrderItemId(
                        submittedItem);

                #endregion


                #region Production Job

                var productionJob =
                    await ValidateCompletedProductionJobAsync(
                        productionJobId);


                if (GetCustomerPurchaseOrderId(
                        productionJob) !=
                    customerPurchaseOrder.Id)
                {
                    throw new BusinessException(
                        "All Production Items in one Invoice must belong to the selected Customer Purchase Order.");
                }

                #endregion


                #region Production Item

                var productionJobItem =
                    ValidateCompletedProductionItem(
                        productionJob,
                        customerPurchaseOrderItemId);

                #endregion


                #region Quantity

                var allocatedQuantity =
                    await _repository
                        .GetAllocatedInvoiceQuantityAsync(
                            productionJobId,
                            customerPurchaseOrderItemId,
                            excludeInvoiceId);


                var availableQuantity =
                    productionJobItem
                        .CompletedQuantity
                    -
                    allocatedQuantity;


                if (availableQuantity < 0m)
                {
                    availableQuantity =
                        0m;
                }


                if (submittedItem.InvoiceQuantity >
                    availableQuantity)
                {
                    throw new BusinessException(
                        $"Invoice Quantity for " +
                        $"{GetTrustedItemName(productionJobItem)} " +
                        $"cannot exceed available Production quantity " +
                        $"{availableQuantity:0.###} " +
                        $"{GetTrustedUnitName(productionJobItem)}.");
                }

                #endregion


                #region Trusted Snapshot

                var preparedItem =
                    CreateProductionSourceSnapshot(
                        customerPurchaseOrder,
                        productionJob,
                        productionJobItem);


                preparedItem.InvoiceQuantity =
                    submittedItem.InvoiceQuantity;


                preparedItem.Rate =
                    submittedItem.Rate;


                preparedItem.DiscountPercent =
                    submittedItem.DiscountPercent;


                preparedItem.GstRate =
                    submittedItem.GstRate;


                CalculateLineAmounts(
                    preparedItem,
                    isInterState);


                preparedItems.Add(
                    preparedItem);

                #endregion
            }


            return preparedItems;
        }

        #endregion


        #region Production Item Helpers

        private static List<ProductionJobItem>
            GetCompletedProductionItems(
                ProductionJob productionJob)
        {
            if (productionJob.Items == null)
            {
                return new List<ProductionJobItem>();
            }


            return productionJob
                .Items
                .Where(x =>
                    !x.IsDeleted
                    &&
                    x.IsActive
                    &&
                    x.ProductionQuantity > 0m
                    &&
                    x.CompletedQuantity > 0m
                    &&
                    x.CompletedQuantity >=
                        x.ProductionQuantity)
                .ToList();
        }


        private static ProductionJobItem
            ValidateCompletedProductionItem(
                ProductionJob productionJob,
                int customerPurchaseOrderItemId)
        {
            var productionJobItem =
                productionJob
                    .Items
                    .FirstOrDefault(x =>
                        x.CustomerPurchaseOrderItemId ==
                            customerPurchaseOrderItemId
                        &&
                        !x.IsDeleted
                        &&
                        x.IsActive);


            if (productionJobItem == null)
            {
                throw new BusinessException(
                    $"Selected Customer PO Item does not belong to Production Job {GetProductionJobDisplayCode(productionJob)}.");
            }


            if (productionJobItem.ProductionQuantity <= 0m)
            {
                throw new BusinessException(
                    $"Production Quantity is not configured for Item {GetTrustedItemName(productionJobItem)}.");
            }


            if (productionJobItem.CompletedQuantity <= 0m)
            {
                throw new BusinessException(
                    $"Item {GetTrustedItemName(productionJobItem)} has no completed Production quantity available for Invoice.");
            }


            if (productionJobItem.CompletedQuantity <
                productionJobItem.ProductionQuantity)
            {
                throw new BusinessException(
                    $"Current Production Quantity for Item {GetTrustedItemName(productionJobItem)} is not completed yet.");
            }


            return productionJobItem;
        }

        #endregion


        #region Production Source Snapshot

        private static InvoiceItem
            CreateProductionSourceSnapshot(
                CustomerPurchaseOrder customerPurchaseOrder,
                ProductionJob productionJob,
                ProductionJobItem productionJobItem)
        {
            var customerPurchaseOrderItemId =
                productionJobItem
                    .CustomerPurchaseOrderItemId;


            var purchaseOrderItem =
                customerPurchaseOrder
                    .Items
                    .FirstOrDefault(x =>
                        x.Id ==
                        customerPurchaseOrderItemId);


            if (purchaseOrderItem == null)
            {
                throw new BusinessException(
                    $"Customer Purchase Order Item for {GetTrustedItemName(productionJobItem)} was not found.");
            }


            var productionMasterItem =
                GetPropertyValue(
                    productionJobItem,
                    "Item");


            var purchaseOrderMasterItem =
                GetPropertyValue(
                    purchaseOrderItem,
                    "Item");


            var itemCode =
                FirstNonEmpty(
                    productionJobItem.ItemCode,

                    purchaseOrderItem.ItemCode,

                    GetStringProperty(
                        productionMasterItem,
                        "Code",
                        "ItemCode"),

                    GetStringProperty(
                        purchaseOrderMasterItem,
                        "Code",
                        "ItemCode"))

                ?? string.Empty;


            var itemName =
                FirstNonEmpty(
                    productionJobItem.ItemName,

                    purchaseOrderItem.ItemName,

                    GetStringProperty(
                        productionMasterItem,
                        "ItemName",
                        "Name"),

                    GetStringProperty(
                        purchaseOrderMasterItem,
                        "ItemName",
                        "Name"))

                ?? string.Empty;


            var unitName =
                FirstNonEmpty(
                    productionJobItem.UnitName,

                    purchaseOrderItem.UnitName,

                    GetStringProperty(
                        productionMasterItem,
                        "UnitName",
                        "UomName",
                        "UOMName"),

                    GetStringProperty(
                        purchaseOrderMasterItem,
                        "UnitName",
                        "UomName",
                        "UOMName"));


            var partNumber =
                FirstNonEmpty(
                    GetStringProperty(
                        productionJobItem,
                        "PartNumber"),

                    GetStringProperty(
                        purchaseOrderItem,
                        "PartNumber"),

                    GetStringProperty(
                        productionMasterItem,
                        "PartNumber"),

                    GetStringProperty(
                        purchaseOrderMasterItem,
                        "PartNumber"));


            var hsnNumber =
                FirstNonEmpty(
                    GetStringProperty(
                        productionJobItem,
                        "HsnNumber",
                        "HSNNumber",
                        "HsnCode",
                        "HSNCode"),

                    GetStringProperty(
                        purchaseOrderItem,
                        "HsnNumber",
                        "HSNNumber",
                        "HsnCode",
                        "HSNCode"),

                    GetStringProperty(
                        productionMasterItem,
                        "HsnNumber",
                        "HSNNumber",
                        "HsnCode",
                        "HSNCode"),

                    GetStringProperty(
                        purchaseOrderMasterItem,
                        "HsnNumber",
                        "HSNNumber",
                        "HsnCode",
                        "HSNCode"));


            var productReference =
                FirstNonEmpty(
                    GetStringProperty(
                        productionJobItem,
                        "ProductReference",
                        "ProductRef"),

                    GetStringProperty(
                        purchaseOrderItem,
                        "ProductReference",
                        "ProductRef"),

                    GetStringProperty(
                        productionMasterItem,
                        "ProductReference",
                        "ProductRef"),

                    GetStringProperty(
                        purchaseOrderMasterItem,
                        "ProductReference",
                        "ProductRef"));


            var customerItemCode =
                FirstNonEmpty(
                    purchaseOrderItem
                        .CustomerItemCode,

                    GetStringProperty(
                        productionJobItem,
                        "CustomerItemCode"));


            return new InvoiceItem
            {
                /*
                 * DC fields are optional historical
                 * traceability only.
                 */
                DeliveryChallanId =
                    null,

                DeliveryChallanCode =
                    null,

                DeliveryChallanItemId =
                    null,

                DeliveryChallanQuantity =
                    null,


                ProductReference =
                    productReference,


                ItemId =
                    productionJobItem.ItemId,

                ItemCode =
                    itemCode,

                ItemName =
                    itemName,

                PartNumber =
                    partNumber,

                CustomerItemCode =
                    customerItemCode,

                UnitName =
                    unitName,

                HsnNumber =
                    hsnNumber,


                CustomerPurchaseOrderItemId =
                    customerPurchaseOrderItemId,

                CustomerPurchaseOrderCode =
                    customerPurchaseOrder.Code,

                CustomerPurchaseOrderNumber =
                    customerPurchaseOrder
                        .CustomerPurchaseOrderNumber,


                ProductionJobId =
                    productionJob.Id,

                ProductionJobCode =
                    GetProductionJobDisplayCode(
                        productionJob)
            };
        }

        #endregion


        #region Synchronize Prepared Items

        private static void SynchronizePreparedItems(
            Invoice invoice,
            List<InvoiceItem> preparedItems)
        {
            /*
             * IMPORTANT:
             *
             * ProductionJobId alone is NOT unique anymore.
             *
             * One Job:
             *      Item A
             *      Item B
             *      Item C
             *
             * Therefore identity is:
             *
             * ProductionJobId
             * +
             * CustomerPurchaseOrderItemId
             */

            var retainedSources =
                new HashSet<(int ProductionJobId,
                             int CustomerPurchaseOrderItemId)>();


            var sequenceNumber =
                1;


            foreach (var preparedItem
                in preparedItems)
            {
                var productionJobId =
                    GetRequiredProductionJobId(
                        preparedItem);


                var customerPurchaseOrderItemId =
                    GetRequiredCustomerPurchaseOrderItemId(
                        preparedItem);


                var existingItem =
                    invoice.Items
                        .FirstOrDefault(x =>
                            x.ProductionJobId.HasValue
                            &&
                            x.ProductionJobId.Value ==
                                productionJobId
                            &&
                            x.CustomerPurchaseOrderItemId.HasValue
                            &&
                            x.CustomerPurchaseOrderItemId.Value ==
                                customerPurchaseOrderItemId);


                if (existingItem != null)
                {
                    CopyPreparedItem(
                        preparedItem,
                        existingItem);


                    existingItem.SequenceNumber =
                        sequenceNumber++;

                    existingItem.IsDeleted =
                        false;

                    existingItem.IsActive =
                        true;

                    existingItem.ModifiedOn =
                        DateTime.UtcNow;

                    existingItem.ModifiedBy =
                        "System";
                }
                else
                {
                    preparedItem.InvoiceId =
                        invoice.Id;

                    preparedItem.SequenceNumber =
                        sequenceNumber++;

                    preparedItem.IsDeleted =
                        false;

                    preparedItem.IsActive =
                        true;

                    preparedItem.CreatedOn =
                        DateTime.UtcNow;

                    preparedItem.CreatedBy =
                        "System";


                    invoice.Items.Add(
                        preparedItem);
                }


                retainedSources.Add(
                    (
                        productionJobId,
                        customerPurchaseOrderItemId
                    ));
            }


            foreach (var existingItem
                in invoice.Items
                    .Where(x =>
                        x.Id > 0)
                    .ToList())
            {
                if (
                    existingItem.ProductionJobId.HasValue
                    &&
                    existingItem.CustomerPurchaseOrderItemId
                        .HasValue
                    &&
                    retainedSources.Contains(
                        (
                            existingItem
                                .ProductionJobId
                                .Value,

                            existingItem
                                .CustomerPurchaseOrderItemId
                                .Value
                        ))
                )
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
        }

        #endregion


        #region Copy Prepared Item

        private static void CopyPreparedItem(
            InvoiceItem source,
            InvoiceItem target)
        {
            target.DeliveryChallanId =
                source.DeliveryChallanId;

            target.DeliveryChallanCode =
                source.DeliveryChallanCode;

            target.DeliveryChallanItemId =
                source.DeliveryChallanItemId;

            target.DeliveryChallanQuantity =
                source.DeliveryChallanQuantity;


            target.ProductReference =
                source.ProductReference;


            target.ItemId =
                source.ItemId;

            target.ItemCode =
                source.ItemCode;

            target.ItemName =
                source.ItemName;

            target.PartNumber =
                source.PartNumber;

            target.CustomerItemCode =
                source.CustomerItemCode;

            target.UnitName =
                source.UnitName;

            target.HsnNumber =
                source.HsnNumber;


            target.CustomerPurchaseOrderItemId =
                source.CustomerPurchaseOrderItemId;

            target.CustomerPurchaseOrderCode =
                source.CustomerPurchaseOrderCode;

            target.CustomerPurchaseOrderNumber =
                source.CustomerPurchaseOrderNumber;


            target.ProductionJobId =
                source.ProductionJobId;

            target.ProductionJobCode =
                source.ProductionJobCode;


            target.InvoiceQuantity =
                source.InvoiceQuantity;


            target.Rate =
                source.Rate;

            target.GrossAmount =
                source.GrossAmount;


            target.DiscountPercent =
                source.DiscountPercent;

            target.DiscountAmount =
                source.DiscountAmount;

            target.TaxableAmount =
                source.TaxableAmount;


            target.GstRate =
                source.GstRate;

            target.CgstRate =
                source.CgstRate;

            target.SgstRate =
                source.SgstRate;

            target.IgstRate =
                source.IgstRate;


            target.CgstAmount =
                source.CgstAmount;

            target.SgstAmount =
                source.SgstAmount;

            target.IgstAmount =
                source.IgstAmount;

            target.TotalTaxAmount =
                source.TotalTaxAmount;


            target.LineTotal =
                source.LineTotal;
        }

        #endregion


        #region Source Warning Validation

        private async Task
            EnsureSourceWarningConfirmedAsync(
                IEnumerable<InvoiceItem> items,
                bool confirmSourceWarning)
        {
            if (confirmSourceWarning)
            {
                return;
            }


            foreach (var item
                in items)
            {
                var productionJobId =
                    GetRequiredProductionJobId(
                        item);


                var customerPurchaseOrderItemId =
                    GetRequiredCustomerPurchaseOrderItemId(
                        item);


                var hasPdi =
                    await _repository
                        .HasFinalizedPdiAsync(
                            productionJobId,
                            customerPurchaseOrderItemId);


                var hasDeliveryChallan =
                    await _repository
                        .HasDeliveryChallanAsync(
                            productionJobId,
                            customerPurchaseOrderItemId);


                if (!hasPdi ||
                    !hasDeliveryChallan)
                {
                    throw new BusinessException(
                        "One or more selected Production Items do not have Finalized PDI or Delivery Challan. Please confirm the warning to continue with Invoice submission.");
                }
            }
        }

        #endregion


        #region Customer PO Validation

        private async Task<CustomerPurchaseOrder>
            ValidateCustomerPurchaseOrderAsync(
                int customerPurchaseOrderId)
        {
            if (customerPurchaseOrderId <= 0)
            {
                throw new BusinessException(
                    "Please select a Customer Purchase Order.");
            }


            var customerPurchaseOrder =
                await _repository
                    .GetCustomerPurchaseOrderForInvoiceAsync(
                        customerPurchaseOrderId);


            if (customerPurchaseOrder == null)
            {
                throw new BusinessException(
                    "Selected Customer Purchase Order is not available for Invoice.");
            }


            if (customerPurchaseOrder.IsDeleted)
            {
                throw new BusinessException(
                    "Selected Customer Purchase Order is deleted.");
            }


            if (!customerPurchaseOrder.IsActive)
            {
                throw new BusinessException(
                    "Selected Customer Purchase Order is inactive.");
            }


            if (customerPurchaseOrder.CustomerId <= 0)
            {
                throw new BusinessException(
                    "Customer Purchase Order Customer is invalid.");
            }


            return customerPurchaseOrder;
        }

        #endregion


        #region Production Job Validation

        private async Task<ProductionJob>
            ValidateCompletedProductionJobAsync(
                int productionJobId)
        {
            if (productionJobId <= 0)
            {
                throw new BusinessException(
                    "Invalid Production Job.");
            }


            var productionJob =
                await _repository
                    .GetCompletedProductionJobForInvoiceAsync(
                        productionJobId);


            if (productionJob == null)
            {
                throw new BusinessException(
                    "Selected Production Job is not available for Invoice.");
            }


            if (productionJob.IsDeleted)
            {
                throw new BusinessException(
                    "Selected Production Job is deleted.");
            }


            if (!productionJob.IsActive)
            {
                throw new BusinessException(
                    "Selected Production Job is inactive.");
            }


            if (productionJob.Status ==
                ProductionJobStatus.Cancelled)
            {
                throw new BusinessException(
                    "Cancelled Production Job cannot be invoiced.");
            }


            return productionJob;
        }

        #endregion


        #region Required Source Ids

        private static int
            GetRequiredProductionJobId(
                InvoiceItem item)
        {
            if (
                !item.ProductionJobId.HasValue
                ||
                item.ProductionJobId.Value <= 0
            )
            {
                throw new BusinessException(
                    "Invalid Production Job.");
            }


            return item
                .ProductionJobId
                .Value;
        }


        private static int
            GetRequiredCustomerPurchaseOrderItemId(
                InvoiceItem item)
        {
            if (
                !item.CustomerPurchaseOrderItemId.HasValue
                ||
                item.CustomerPurchaseOrderItemId.Value <= 0
            )
            {
                throw new BusinessException(
                    "Invalid Customer Purchase Order Item.");
            }


            return item
                .CustomerPurchaseOrderItemId
                .Value;
        }

        #endregion


        #region Production Source Information

        private static int
            GetCustomerPurchaseOrderId(
                ProductionJob productionJob)
        {
            if (productionJob.CustomerPurchaseOrderId > 0)
            {
                return productionJob
                    .CustomerPurchaseOrderId;
            }


            if (productionJob.CustomerPurchaseOrder != null &&
                productionJob.CustomerPurchaseOrder.Id > 0)
            {
                return productionJob
                    .CustomerPurchaseOrder
                    .Id;
            }


            throw new BusinessException(
                $"Customer Purchase Order reference is missing from Production Job {GetProductionJobDisplayCode(productionJob)}.");
        }


        private static int
            GetCustomerId(
                CustomerPurchaseOrder customerPurchaseOrder)
        {
            if (customerPurchaseOrder.CustomerId > 0)
            {
                return customerPurchaseOrder
                    .CustomerId;
            }


            if (customerPurchaseOrder.Customer != null)
            {
                return customerPurchaseOrder
                    .Customer
                    .Id;
            }


            return 0;
        }


        private static string
            GetProductionJobDisplayCode(
                ProductionJob productionJob)
        {
            return string.IsNullOrWhiteSpace(
                productionJob.Code)
                ? productionJob.Id.ToString()
                : productionJob.Code;
        }


        private static string
            GetTrustedItemName(
                ProductionJobItem productionJobItem)
        {
            return FirstNonEmpty(
                productionJobItem.ItemName,
                productionJobItem.ItemCode,
                productionJobItem.Id.ToString())

                ?? "Item";
        }


        private static string
            GetTrustedUnitName(
                ProductionJobItem productionJobItem)
        {
            return productionJobItem.UnitName
                ?? string.Empty;
        }

        #endregion


        #region Reflection Helpers

        private static object?
            GetPropertyValue(
                object? source,
                params string[] propertyNames)
        {
            if (source == null ||
                propertyNames == null)
            {
                return null;
            }


            var sourceType =
                source.GetType();


            foreach (var propertyName
                in propertyNames)
            {
                var property =
                    sourceType.GetProperty(
                        propertyName,
                        BindingFlags.Public |
                        BindingFlags.Instance |
                        BindingFlags.IgnoreCase);


                if (property == null ||
                    !property.CanRead)
                {
                    continue;
                }


                return property.GetValue(
                    source);
            }


            return null;
        }


        private static string?
            GetStringProperty(
                object? source,
                params string[] propertyNames)
        {
            var value =
                GetPropertyValue(
                    source,
                    propertyNames);


            return value?.ToString();
        }


        private static string?
            FirstNonEmpty(
                params string?[] values)
        {
            return values
                .FirstOrDefault(x =>
                    !string.IsNullOrWhiteSpace(
                        x));
        }

        #endregion


        #region Line Calculation

        private static void CalculateLineAmounts(
            InvoiceItem item,
            bool isInterState)
        {
            item.GrossAmount =
                RoundAmount(
                    item.InvoiceQuantity *
                    item.Rate);


            item.DiscountAmount =
                RoundAmount(
                    item.GrossAmount *
                    item.DiscountPercent /
                    100m);


            item.TaxableAmount =
                RoundAmount(
                    item.GrossAmount -
                    item.DiscountAmount);


            item.CgstRate =
                0m;

            item.SgstRate =
                0m;

            item.IgstRate =
                0m;


            item.CgstAmount =
                0m;

            item.SgstAmount =
                0m;

            item.IgstAmount =
                0m;


            if (item.GstRate > 0m)
            {
                if (isInterState)
                {
                    item.IgstRate =
                        item.GstRate;


                    item.IgstAmount =
                        RoundAmount(
                            item.TaxableAmount *
                            item.IgstRate /
                            100m);
                }
                else
                {
                    var halfGst =
                        item.GstRate /
                        2m;


                    item.CgstRate =
                        halfGst;

                    item.SgstRate =
                        halfGst;


                    item.CgstAmount =
                        RoundAmount(
                            item.TaxableAmount *
                            item.CgstRate /
                            100m);


                    item.SgstAmount =
                        RoundAmount(
                            item.TaxableAmount *
                            item.SgstRate /
                            100m);
                }
            }


            item.TotalTaxAmount =
                RoundAmount(
                    item.CgstAmount +
                    item.SgstAmount +
                    item.IgstAmount);


            item.LineTotal =
                RoundAmount(
                    item.TaxableAmount +
                    item.TotalTaxAmount);
        }

        #endregion


        #region Header Calculation

        private static void CalculateHeaderTotals(
            Invoice invoice)
        {
            var activeItems =
                invoice.Items
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .ToList();


            invoice.GrossAmount =
                RoundAmount(
                    activeItems.Sum(x =>
                        x.GrossAmount));


            invoice.DiscountAmount =
                RoundAmount(
                    activeItems.Sum(x =>
                        x.DiscountAmount));


            invoice.TaxableAmount =
                RoundAmount(
                    activeItems.Sum(x =>
                        x.TaxableAmount));


            invoice.CgstAmount =
                RoundAmount(
                    activeItems.Sum(x =>
                        x.CgstAmount));


            invoice.SgstAmount =
                RoundAmount(
                    activeItems.Sum(x =>
                        x.SgstAmount));


            invoice.IgstAmount =
                RoundAmount(
                    activeItems.Sum(x =>
                        x.IgstAmount));


            var beforeRoundOff =
                RoundAmount(
                    invoice.TaxableAmount +
                    invoice.CgstAmount +
                    invoice.SgstAmount +
                    invoice.IgstAmount +
                    invoice.OtherCharges);


            var roundedPayable =
                Math.Round(
                    beforeRoundOff,
                    0,
                    MidpointRounding
                        .AwayFromZero);


            invoice.RoundOffAmount =
                RoundAmount(
                    roundedPayable -
                    beforeRoundOff);


            invoice.GrandTotal =
                RoundAmount(
                    beforeRoundOff +
                    invoice.RoundOffAmount);
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
                    "Invalid Customer.");
            }


            var customer =
                await _repository
                    .GetCustomerForInvoiceAsync(
                        customerId);


            if (customer == null ||
                customer.IsDeleted)
            {
                throw new BusinessException(
                    "Customer does not exist.");
            }


            if (!customer.IsActive)
            {
                throw new BusinessException(
                    "Customer is inactive.");
            }


            return customer;
        }

        #endregion


        #region Company Validation

        private async Task<Company>
            ValidateCompanyAsync()
        {
            var company =
                await _repository
                    .GetCompanyForInvoiceAsync();


            if (company == null ||
                company.IsDeleted)
            {
                throw new BusinessException(
                    "Active Company / Workshop is not configured.");
            }


            if (!company.IsActive)
            {
                throw new BusinessException(
                    "Company / Workshop is inactive.");
            }


            return company;
        }

        #endregion


        #region Billing Address

        private static void
            ApplySubmittedOrMasterBillingAddress(
                Invoice target,
                Invoice submitted,
                Customer customer)
        {
            if (HasAnyBillingAddress(
                submitted))
            {
                target.BillingAddressLine1 =
                    submitted.BillingAddressLine1;

                target.BillingAddressLine2 =
                    submitted.BillingAddressLine2;

                target.BillingCity =
                    submitted.BillingCity;

                target.BillingDistrict =
                    submitted.BillingDistrict;

                target.BillingState =
                    submitted.BillingState;

                target.BillingPincode =
                    submitted.BillingPincode;

                target.BillingCountry =
                    submitted.BillingCountry;


                return;
            }


            target.BillingAddressLine1 =
                customer.AddressLine1;

            target.BillingAddressLine2 =
                customer.AddressLine2;

            target.BillingCity =
                customer.City;

            target.BillingDistrict =
                customer.District;

            target.BillingState =
                customer.State;

            target.BillingPincode =
                customer.Pincode;

            target.BillingCountry =
                customer.Country;
        }


        private static bool HasAnyBillingAddress(
            Invoice invoice)
        {
            return
                !string.IsNullOrWhiteSpace(
                    invoice.BillingAddressLine1)

                ||

                !string.IsNullOrWhiteSpace(
                    invoice.BillingAddressLine2)

                ||

                !string.IsNullOrWhiteSpace(
                    invoice.BillingCity)

                ||

                !string.IsNullOrWhiteSpace(
                    invoice.BillingDistrict)

                ||

                !string.IsNullOrWhiteSpace(
                    invoice.BillingState)

                ||

                !string.IsNullOrWhiteSpace(
                    invoice.BillingPincode)

                ||

                !string.IsNullOrWhiteSpace(
                    invoice.BillingCountry);
        }

        #endregion


        #region GST Type

        private static bool DetermineInterState(
            string? companyState,
            string? billingState,
            bool hasGst)
        {
            var normalizedCompanyState =
                NormalizeComparableText(
                    companyState);


            var normalizedBillingState =
                NormalizeComparableText(
                    billingState);


            if (!hasGst)
            {
                if (
                    string.IsNullOrWhiteSpace(
                        normalizedCompanyState)
                    ||
                    string.IsNullOrWhiteSpace(
                        normalizedBillingState)
                )
                {
                    return false;
                }


                return !string.Equals(
                    normalizedCompanyState,
                    normalizedBillingState,
                    StringComparison.OrdinalIgnoreCase);
            }


            if (string.IsNullOrWhiteSpace(
                normalizedCompanyState))
            {
                throw new BusinessException(
                    "Company State is required to calculate GST.");
            }


            if (string.IsNullOrWhiteSpace(
                normalizedBillingState))
            {
                throw new BusinessException(
                    "Customer Billing State is required to calculate GST.");
            }


            return !string.Equals(
                normalizedCompanyState,
                normalizedBillingState,
                StringComparison.OrdinalIgnoreCase);
        }


        private static bool
            DetermineInterStateForPreview(
                string? companyState,
                string? billingState)
        {
            var normalizedCompanyState =
                NormalizeComparableText(
                    companyState);


            var normalizedBillingState =
                NormalizeComparableText(
                    billingState);


            if (
                string.IsNullOrWhiteSpace(
                    normalizedCompanyState)
                ||
                string.IsNullOrWhiteSpace(
                    normalizedBillingState)
            )
            {
                return false;
            }


            return !string.Equals(
                normalizedCompanyState,
                normalizedBillingState,
                StringComparison.OrdinalIgnoreCase);
        }

        #endregion


        #region Due Date

        private static DateTime?
            CalculateDueDate(
                DateTime invoiceDate,
                int? creditDays)
        {
            if (!creditDays.HasValue ||
                creditDays.Value < 0)
            {
                return null;
            }


            return invoiceDate
                .Date
                .AddDays(
                    creditDays.Value);
        }

        #endregion


        #region Invoice Code

        private async Task<string>
            GenerateInvoiceCodeAsync(
                DateTime invoiceDate)
        {
            var financialYear =
                GetFinancialYear(
                    invoiceDate);


            var prefix =
                $"AI/INV/{financialYear}/";


            var lastCode =
                await _repository
                    .GetLastCodeAsync(
                        prefix);


            var nextNumber =
                1;


            if (!string.IsNullOrWhiteSpace(
                lastCode))
            {
                var numberPart =
                    lastCode.Length >
                    prefix.Length
                        ? lastCode.Substring(
                            prefix.Length)
                        : string.Empty;


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


        #region Snapshot Serialization

        private static string
            SerializeScalarSnapshot(
                object master)
        {
            var snapshot =
                new Dictionary<string, object?>();


            var properties =
                master
                    .GetType()
                    .GetProperties(
                        BindingFlags.Public |
                        BindingFlags.Instance);


            foreach (var property
                in properties)
            {
                if (!property.CanRead)
                {
                    continue;
                }


                if (!IsSnapshotScalarType(
                    property.PropertyType))
                {
                    continue;
                }


                snapshot[property.Name] =
                    property.GetValue(
                        master);
            }


            return JsonSerializer
                .Serialize(
                    snapshot);
        }


        private static bool IsSnapshotScalarType(
            Type type)
        {
            var actualType =
                Nullable.GetUnderlyingType(
                    type)
                ?? type;


            return
                actualType.IsEnum
                ||
                actualType.IsPrimitive
                ||
                actualType == typeof(string)
                ||
                actualType == typeof(decimal)
                ||
                actualType == typeof(DateTime)
                ||
                actualType == typeof(DateTimeOffset)
                ||
                actualType == typeof(TimeSpan)
                ||
                actualType == typeof(Guid);
        }

        #endregion


        #region Snapshot Read Helpers

        private static Dictionary<string, JsonElement>
            ParseSnapshot(
                string? snapshotJson)
        {
            if (string.IsNullOrWhiteSpace(
                snapshotJson))
            {
                return new Dictionary<
                    string,
                    JsonElement>();
            }


            try
            {
                return JsonSerializer
                    .Deserialize<
                        Dictionary<
                            string,
                            JsonElement>>(
                                snapshotJson)

                    ?? new Dictionary<
                        string,
                        JsonElement>();
            }
            catch (JsonException)
            {
                return new Dictionary<
                    string,
                    JsonElement>();
            }
        }


        private static string?
            GetSnapshotString(
                Dictionary<string, JsonElement> snapshot,
                string propertyName)
        {
            if (!snapshot.TryGetValue(
                propertyName,
                out var value))
            {
                return null;
            }


            if (value.ValueKind ==
                JsonValueKind.Null)
            {
                return null;
            }


            if (value.ValueKind ==
                JsonValueKind.String)
            {
                return value.GetString();
            }


            return value.ToString();
        }

        #endregion


        #region Normalization

        private static void NormalizeHeader(
            Invoice invoice)
        {
            invoice.BillingAddressLine1 =
                NormalizeOptional(
                    invoice.BillingAddressLine1);

            invoice.BillingAddressLine2 =
                NormalizeOptional(
                    invoice.BillingAddressLine2);

            invoice.BillingCity =
                NormalizeOptional(
                    invoice.BillingCity);

            invoice.BillingDistrict =
                NormalizeOptional(
                    invoice.BillingDistrict);

            invoice.BillingState =
                NormalizeOptional(
                    invoice.BillingState);

            invoice.BillingPincode =
                NormalizeOptional(
                    invoice.BillingPincode);

            invoice.BillingCountry =
                NormalizeOptional(
                    invoice.BillingCountry);

            invoice.InvoiceTermsAndConditions =
                NormalizeOptional(
                    invoice.InvoiceTermsAndConditions);

            invoice.Remarks =
                NormalizeOptional(
                    invoice.Remarks);
        }


        private static void NormalizeItem(
            InvoiceItem item)
        {
            /*
             * Source snapshot fields are intentionally
             * not normalized here because they are rebuilt
             * from trusted Production / Customer PO records.
             */
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
            NormalizeComparableText(
                string? value)
        {
            return NormalizeOptional(
                value)?
                .ToUpperInvariant();
        }

        #endregion


        #region Header Validation

        private static void ValidateHeader(
            Invoice invoice)
        {
            if (invoice.InvoiceDate ==
                default)
            {
                throw new BusinessException(
                    "Invoice Date is required.");
            }


            if (invoice.OtherCharges < 0m)
            {
                throw new BusinessException(
                    "Other Charges cannot be negative.");
            }


            if (invoice.BillingAddressLine1?.Length >
                500)
            {
                throw new BusinessException(
                    "Billing Address Line 1 cannot exceed 500 characters.");
            }


            if (invoice.BillingAddressLine2?.Length >
                500)
            {
                throw new BusinessException(
                    "Billing Address Line 2 cannot exceed 500 characters.");
            }


            if (invoice.BillingCity?.Length >
                150)
            {
                throw new BusinessException(
                    "Billing City cannot exceed 150 characters.");
            }


            if (invoice.BillingDistrict?.Length >
                150)
            {
                throw new BusinessException(
                    "Billing District cannot exceed 150 characters.");
            }


            if (invoice.BillingState?.Length >
                150)
            {
                throw new BusinessException(
                    "Billing State cannot exceed 150 characters.");
            }


            if (invoice.BillingPincode?.Length >
                20)
            {
                throw new BusinessException(
                    "Billing Pincode cannot exceed 20 characters.");
            }


            if (invoice.BillingCountry?.Length >
                100)
            {
                throw new BusinessException(
                    "Billing Country cannot exceed 100 characters.");
            }


            if (invoice.InvoiceTermsAndConditions?.Length >
                4000)
            {
                throw new BusinessException(
                    "Invoice Terms & Conditions cannot exceed 4000 characters.");
            }


            if (invoice.Remarks?.Length >
                2000)
            {
                throw new BusinessException(
                    "Invoice Remarks cannot exceed 2000 characters.");
            }


            if (invoice.Items == null ||
                invoice.Items.Count == 0)
            {
                throw new BusinessException(
                    "Invoice must contain at least one Item.");
            }
        }

        #endregion


        #region Item Validation

        private static void ValidateItem(
            InvoiceItem item)
        {
            if (
                !item.ProductionJobId.HasValue
                ||
                item.ProductionJobId.Value <= 0
            )
            {
                throw new BusinessException(
                    "Invalid Production Job.");
            }


            if (
                !item.CustomerPurchaseOrderItemId.HasValue
                ||
                item.CustomerPurchaseOrderItemId.Value <= 0
            )
            {
                throw new BusinessException(
                    "Invalid Customer Purchase Order Item.");
            }


            if (item.InvoiceQuantity <= 0m)
            {
                throw new BusinessException(
                    "Invoice Quantity must be greater than zero.");
            }


            if (item.Rate <= 0m)
            {
                throw new BusinessException(
                    "Invoice Rate must be greater than zero.");
            }


            if (
                item.DiscountPercent < 0m
                ||
                item.DiscountPercent > 100m
            )
            {
                throw new BusinessException(
                    "Discount percentage must be between 0 and 100.");
            }


            if (
                item.GstRate < 0m
                ||
                item.GstRate > 100m
            )
            {
                throw new BusinessException(
                    "GST percentage must be between 0 and 100.");
            }
        }

        #endregion


        #region Submitted Item Validation

        private static void ValidateSubmittedItems(
            ICollection<InvoiceItem> items)
        {
            if (items == null ||
                items.Count == 0)
            {
                throw new BusinessException(
                    "Invoice must contain at least one Item.");
            }


            foreach (var item
                in items)
            {
                ValidateItem(
                    item);
            }


            ValidateDuplicateProductionItems(
                items);
        }


        private static void
            ValidateDuplicateProductionItems(
                IEnumerable<InvoiceItem> items)
        {
            var duplicate =
                items
                    .Where(x =>
                        x.ProductionJobId.HasValue
                        &&
                        x.ProductionJobId.Value > 0
                        &&
                        x.CustomerPurchaseOrderItemId.HasValue
                        &&
                        x.CustomerPurchaseOrderItemId.Value > 0)
                    .GroupBy(x =>
                        new
                        {
                            ProductionJobId =
                                x.ProductionJobId!.Value,

                            CustomerPurchaseOrderItemId =
                                x.CustomerPurchaseOrderItemId!.Value
                        })
                    .FirstOrDefault(x =>
                        x.Count() > 1);


            if (duplicate != null)
            {
                throw new BusinessException(
                    "The same Production Item cannot be added more than once in one Invoice.");
            }
        }

        #endregion


        #region Pagination

        private static void NormalizePagination(
            ref int pageNumber,
            ref int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber =
                    1;
            }


            if (
                pageSize != 10
                &&
                pageSize != 25
                &&
                pageSize != 50
            )
            {
                pageSize =
                    10;
            }
        }

        #endregion


        #region Amount Helper

        private static decimal RoundAmount(
            decimal value)
        {
            return Math.Round(
                value,
                2,
                MidpointRounding
                    .AwayFromZero);
        }

        #endregion
    }
}