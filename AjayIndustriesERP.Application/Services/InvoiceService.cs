/*
============================================================
File: InvoiceService.cs

Module:
Invoice

Purpose:
Contains complete Invoice business logic.

Responsibilities:
- Search and paginate Invoices.
- Load Customer Purchase Orders available for Invoice.
- Load Completed Production Jobs for selected Customer PO.
- Calculate remaining invoiceable Production quantity.
- Check PDI / Delivery Challan warning status.
- Prepare unsaved Invoice Draft from Customer PO.
- Create trusted Invoice from Completed Production Jobs.
- Capture Customer / Company historical snapshots.
- Auto-load Customer Billing Address.
- Auto-load Payment Terms / Credit Days.
- Auto-load Company Invoice Terms.
- Calculate Rate / Discount / GST.
- Calculate CGST + SGST or IGST.
- Calculate Invoice header totals and Round Off.
- Update Draft Invoice.
- Finalize Invoice.
- Soft-delete Draft Invoice.
- Restore Draft Invoice after quantity validation.
- Generate Finalized Invoice PDF.

Invoice Code:
AI/INV/{YY-YY}/{00001}

Example:
AI/INV/26-27/00001

Important:
- New trusted Invoice source flow:
  Customer PO → Completed Production Job → Invoice.
- Delivery Challan is NOT mandatory for Invoice.
- PDI is NOT mandatory for Invoice.
- Missing PDI / Delivery Challan requires warning
  confirmation only.
- Browser-posted source snapshot values are NOT trusted.
- Browser-posted calculated amounts are NOT trusted.
- Rate, Discount % and GST % are user-entered commercial
  inputs and are validated by this Service.
- Draft + Finalized active Invoices reserve Production
  quantity.
- Deleted Invoices do not reserve Production quantity.
- One Invoice uses Production Jobs belonging to one
  Customer Purchase Order.
- Customer / Company JSON snapshots are captured on Create.
- Customer / Company JSON snapshots are NOT refreshed on
  normal Draft Edit or Finalization.
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


            var availableJobs =
                new List<ProductionJob>();


            foreach (var job
                in jobs)
            {
                var productionQuantity =
                    GetCompletedProductionQuantity(
                        job);


                if (productionQuantity <= 0)
                {
                    continue;
                }


                var allocatedQuantity =
                    await _repository
                        .GetAllocatedInvoiceQuantityAsync(
                            job.Id);


                var remainingQuantity =
                    productionQuantity -
                    allocatedQuantity;


                if (remainingQuantity > 0)
                {
                    availableJobs.Add(
                        job);
                }
            }


            return availableJobs;
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
                int? excludeInvoiceId = null)
        {
            if (productionJobId <= 0)
            {
                throw new BusinessException(
                    "Invalid Production Job.");
            }


            var productionJob =
                await ValidateCompletedProductionJobAsync(
                    productionJobId);


            var productionQuantity =
                GetCompletedProductionQuantity(
                    productionJob);


            var allocatedQuantity =
                await _repository
                    .GetAllocatedInvoiceQuantityAsync(
                        productionJobId,
                        excludeInvoiceId);


            var remainingQuantity =
                productionQuantity -
                allocatedQuantity;


            return remainingQuantity < 0
                ? 0
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
                var hasPdi =
                    await _repository
                        .HasFinalizedPdiAsync(
                            productionJobId);


                var hasDeliveryChallan =
                    await _repository
                        .HasDeliveryChallanAsync(
                            productionJobId);


                if (!hasPdi ||
                    !hasDeliveryChallan)
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
            #region Validate Customer Purchase Order

            var customerPurchaseOrder =
                await ValidateCustomerPurchaseOrderAsync(
                    customerPurchaseOrderId);

            #endregion


            #region Validate Customer

            var customerId =
                GetCustomerId(
                    customerPurchaseOrder);


            var customer =
                await ValidateCustomerAsync(
                    customerId);

            #endregion


            #region Validate Company

            var company =
                await ValidateCompanyAsync();

            #endregion


            #region Prepare Header

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
                        0,

                    RoundOffAmount =
                        0,

                    IsActive =
                        true,

                    IsDeleted =
                        false
                };

            #endregion


            #region GST Type Preview

            invoice.IsInterState =
                DetermineInterStateForPreview(
                    company.State,
                    invoice.BillingState);

            #endregion


            #region Load Completed Production Jobs

            var productionJobs =
                await _repository
                    .GetCompletedProductionJobsForInvoiceAsync(
                        customerPurchaseOrderId);

            #endregion


            #region Prepare Available Items

            var sequenceNumber =
                1;


            foreach (var productionJob
                in productionJobs
                    .OrderBy(x =>
                        x.Id))
            {
                var productionQuantity =
                    GetCompletedProductionQuantity(
                        productionJob);


                if (productionQuantity <= 0)
                {
                    continue;
                }


                var allocatedQuantity =
                    await _repository
                        .GetAllocatedInvoiceQuantityAsync(
                            productionJob.Id);


                var availableQuantity =
                    productionQuantity -
                    allocatedQuantity;


                if (availableQuantity <= 0)
                {
                    continue;
                }


                var invoiceItem =
                    CreateProductionSourceSnapshot(
                        customerPurchaseOrder,
                        productionJob);


                invoiceItem.SequenceNumber =
                    sequenceNumber++;


                invoiceItem.InvoiceQuantity =
                    availableQuantity;


                invoiceItem.Rate =
                    0;

                invoiceItem.DiscountPercent =
                    0;

                invoiceItem.GstRate =
                    18;


                CalculateLineAmounts(
                    invoiceItem,
                    invoice.IsInterState);


                invoice.Items.Add(
                    invoiceItem);
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


            #region Normalize / Validate Header

            NormalizeHeader(
                invoice);


            ValidateHeader(
                invoice);

            #endregion


            #region Validate Submitted Lines

            ValidateSubmittedItems(
                invoice.Items);

            #endregion


            #region Trusted First Production Source

            var firstSubmittedItem =
                invoice.Items
                    .First();


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


            #region Customer Master Snapshot

            var customerId =
                GetCustomerId(
                    customerPurchaseOrder);


            var customer =
                await ValidateCustomerAsync(
                    customerId);


            var company =
                await ValidateCompanyAsync();

            #endregion


            #region Prepare Trusted Header

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
                            invoice
                                .InvoiceTermsAndConditions)
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

            #endregion


            #region Billing Address

            ApplySubmittedOrMasterBillingAddress(
                newInvoice,
                invoice,
                customer);

            #endregion


            #region Payment / Supply

            newInvoice.DueDate =
                CalculateDueDate(
                    newInvoice.InvoiceDate,
                    newInvoice.CreditDays);


            newInvoice.PlaceOfSupply =
                newInvoice.BillingState;

            #endregion


            #region GST Type

            var hasGst =
                invoice.Items.Any(x =>
                    x.GstRate > 0);


            newInvoice.IsInterState =
                DetermineInterState(
                    company.State,
                    newInvoice.BillingState,
                    hasGst);

            #endregion


            #region Prepare Trusted Items

            var preparedItems =
                await PrepareTrustedItemsAsync(
                    invoice.Items,
                    customerPurchaseOrder,
                    expectedCustomerId:
                        customer.Id,
                    excludeInvoiceId:
                        null,
                    isInterState:
                        newInvoice.IsInterState);

            #endregion


            #region Source Warning Confirmation

            await EnsureSourceWarningConfirmedAsync(
                preparedItems,
                confirmSourceWarning);

            #endregion


            #region Add Prepared Items

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


            #region Financial Totals

            CalculateHeaderTotals(
                newInvoice);

            #endregion


            #region Save

            await _repository
                .AddAsync(
                    newInvoice);

            #endregion


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


            #region Load Existing

            var existing =
                await _repository
                    .GetForUpdateAsync(
                        invoice.Id);


            if (existing == null)
            {
                throw new BusinessException(
                    "Invoice not found.");
            }

            #endregion


            #region Draft Only Rule

            if (existing.Status !=
                InvoiceStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Invoice can be edited.");
            }

            #endregion


            #region Normalize / Validate

            NormalizeHeader(
                invoice);


            ValidateHeader(
                invoice);


            ValidateSubmittedItems(
                invoice.Items);

            #endregion


            #region Financial Year Protection

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


            #region Snapshot Validation

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


            #region Validate Customer PO Source

            var firstSubmittedItem =
                invoice.Items
                    .First();


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


            var purchaseOrderCustomerId =
                GetCustomerId(
                    customerPurchaseOrder);


            if (purchaseOrderCustomerId !=
                existing.CustomerId)
            {
                throw new BusinessException(
                    "Selected Customer Purchase Order does not belong to the Invoice Customer.");
            }

            #endregion


            #region GST Type From Saved Company Snapshot

            var companySnapshot =
                ParseSnapshot(
                    existing.CompanySnapshotJson);


            var companyState =
                GetSnapshotString(
                    companySnapshot,
                    "State");


            var hasGst =
                invoice.Items.Any(x =>
                    x.GstRate > 0);


            var isInterState =
                DetermineInterState(
                    companyState,
                    invoice.BillingState,
                    hasGst);

            #endregion


            #region Prepare Trusted Items

            var preparedItems =
                await PrepareTrustedItemsAsync(
                    invoice.Items,
                    customerPurchaseOrder,
                    expectedCustomerId:
                        existing.CustomerId,
                    excludeInvoiceId:
                        existing.Id,
                    isInterState:
                        isInterState);

            #endregion


            #region Source Warning Confirmation

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


            #region Synchronize Items

            SynchronizePreparedItems(
                existing,
                preparedItems);

            #endregion


            #region Financial Totals

            CalculateHeaderTotals(
                existing);

            #endregion


            #region Save

            await _repository
                .UpdateAsync(
                    existing);

            #endregion


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


            #region Load Existing

            var invoice =
                await _repository
                    .GetForUpdateAsync(
                        id);


            if (invoice == null)
            {
                throw new BusinessException(
                    "Invoice not found.");
            }

            #endregion


            #region Draft Only Rule

            if (invoice.Status !=
                InvoiceStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Invoice can be finalized.");
            }

            #endregion


            #region Snapshot Validation

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

            #endregion


            #region Active Item Validation

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

            #endregion


            #region GST Type From Historical Snapshot

            var companySnapshot =
                ParseSnapshot(
                    invoice.CompanySnapshotJson);


            var companyState =
                GetSnapshotString(
                    companySnapshot,
                    "State");


            var hasGst =
                activeItems.Any(x =>
                    x.GstRate > 0);


            invoice.IsInterState =
                DetermineInterState(
                    companyState,
                    invoice.BillingState,
                    hasGst);


            invoice.PlaceOfSupply =
                invoice.BillingState;

            #endregion


            #region Rebuild Submitted Production Lines

            var submittedItems =
                activeItems
                    .Select(x =>
                        new InvoiceItem
                        {
                            ProductionJobId =
                                x.ProductionJobId,

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


            #region Validate Customer PO Source

            var firstSubmittedProductionJobId =
                GetRequiredProductionJobId(
                    submittedItems.First());


            var firstProductionJob =
                await ValidateCompletedProductionJobAsync(
                    firstSubmittedProductionJobId);


            var customerPurchaseOrderId =
                GetCustomerPurchaseOrderId(
                    firstProductionJob);


            var customerPurchaseOrder =
                await ValidateCustomerPurchaseOrderAsync(
                    customerPurchaseOrderId);


            var purchaseOrderCustomerId =
                GetCustomerId(
                    customerPurchaseOrder);


            if (purchaseOrderCustomerId !=
                invoice.CustomerId)
            {
                throw new BusinessException(
                    "Selected Customer Purchase Order does not belong to the Invoice Customer.");
            }

            #endregion


            #region Revalidate Trusted Sources

            var preparedItems =
                await PrepareTrustedItemsAsync(
                    submittedItems,
                    customerPurchaseOrder,
                    expectedCustomerId:
                        invoice.CustomerId,
                    excludeInvoiceId:
                        invoice.Id,
                    isInterState:
                        invoice.IsInterState);

            #endregion


            #region Source Warning Confirmation

            await EnsureSourceWarningConfirmedAsync(
                preparedItems,
                confirmSourceWarning);

            #endregion


            #region Synchronize Trusted Sources

            SynchronizePreparedItems(
                invoice,
                preparedItems);

            #endregion


            #region Recalculate Financials

            CalculateHeaderTotals(
                invoice);

            #endregion


            #region Finalize

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

            #endregion


            #region Save

            await _repository
                .UpdateAsync(
                    invoice);

            #endregion


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


            #region Load Deleted Invoice

            var invoice =
                await _repository
                    .GetDeletedForUpdateAsync(
                        id);


            if (invoice == null)
            {
                throw new BusinessException(
                    "Deleted Invoice not found.");
            }

            #endregion


            #region Draft Only Rule

            if (invoice.Status !=
                InvoiceStatus.Draft)
            {
                throw new BusinessException(
                    "Only deleted Draft Invoice can be restored.");
            }

            #endregion


            #region Active Historical Lines

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

            #endregion


            #region GST Type

            var companySnapshot =
                ParseSnapshot(
                    invoice.CompanySnapshotJson);


            var companyState =
                GetSnapshotString(
                    companySnapshot,
                    "State");


            var hasGst =
                originalItems.Any(x =>
                    x.GstRate > 0);


            var isInterState =
                DetermineInterState(
                    companyState,
                    invoice.BillingState,
                    hasGst);

            #endregion


            #region Rebuild Submitted Production Lines

            var submittedItems =
                originalItems
                    .Select(x =>
                        new InvoiceItem
                        {
                            ProductionJobId =
                                x.ProductionJobId,

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


            #region Validate Customer PO Source

            var firstSubmittedProductionJobId =
                GetRequiredProductionJobId(
                    submittedItems.First());


            var firstProductionJob =
                await ValidateCompletedProductionJobAsync(
                    firstSubmittedProductionJobId);


            var customerPurchaseOrderId =
                GetCustomerPurchaseOrderId(
                    firstProductionJob);


            var customerPurchaseOrder =
                await ValidateCustomerPurchaseOrderAsync(
                    customerPurchaseOrderId);


            var purchaseOrderCustomerId =
                GetCustomerId(
                    customerPurchaseOrder);


            if (purchaseOrderCustomerId !=
                invoice.CustomerId)
            {
                throw new BusinessException(
                    "Customer Purchase Order does not belong to the Invoice Customer.");
            }

            #endregion


            #region Revalidate Quantities

            var preparedItems =
                await PrepareTrustedItemsAsync(
                    submittedItems,
                    customerPurchaseOrder,
                    expectedCustomerId:
                        invoice.CustomerId,
                    excludeInvoiceId:
                        invoice.Id,
                    isInterState:
                        isInterState);

            #endregion


            #region Restore

            /*
             * PDI / Delivery Challan status intentionally
             * does NOT block Draft restore.
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

            #endregion
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
                    .ToList();


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


            ValidateDuplicateProductionJobs(
                items);


            var purchaseOrderCustomerId =
                GetCustomerId(
                    customerPurchaseOrder);


            if (purchaseOrderCustomerId !=
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
                #region Production Job Id

                var productionJobId =
                    GetRequiredProductionJobId(
                        submittedItem);

                #endregion


                #region Load Trusted Production Job

                var productionJob =
                    await ValidateCompletedProductionJobAsync(
                        productionJobId);

                #endregion


                #region Customer PO Ownership

                var productionJobPurchaseOrderId =
                    GetCustomerPurchaseOrderId(
                        productionJob);


                if (productionJobPurchaseOrderId !=
                    customerPurchaseOrder.Id)
                {
                    throw new BusinessException(
                        "All Production Jobs in one Invoice must belong to the selected Customer Purchase Order.");
                }

                #endregion


                #region Completed Production Quantity

                var productionQuantity =
                    GetCompletedProductionQuantity(
                        productionJob);


                if (productionQuantity <= 0)
                {
                    throw new BusinessException(
                        $"Production Job {GetProductionJobDisplayCode(productionJob)} has no completed quantity available for Invoice.");
                }

                #endregion


                #region Quantity Allocation

                var allocatedQuantity =
                    await _repository
                        .GetAllocatedInvoiceQuantityAsync(
                            productionJobId,
                            excludeInvoiceId);


                var availableQuantity =
                    productionQuantity -
                    allocatedQuantity;


                if (availableQuantity < 0)
                {
                    availableQuantity =
                        0;
                }


                if (submittedItem.InvoiceQuantity >
                    availableQuantity)
                {
                    var itemName =
                        GetTrustedItemName(
                            customerPurchaseOrder,
                            productionJob);


                    var unitName =
                        GetTrustedUnitName(
                            customerPurchaseOrder,
                            productionJob);


                    throw new BusinessException(
                        $"Invoice Quantity for " +
                        $"{itemName} cannot exceed " +
                        $"available Production quantity " +
                        $"{availableQuantity:0.###} " +
                        $"{unitName}.");
                }

                #endregion


                #region Trusted Snapshot

                var preparedItem =
                    CreateProductionSourceSnapshot(
                        customerPurchaseOrder,
                        productionJob);


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


        #region Create Production Source Snapshot

        private static InvoiceItem
            CreateProductionSourceSnapshot(
                CustomerPurchaseOrder customerPurchaseOrder,
                ProductionJob productionJob)
        {
            var customerPurchaseOrderItemId =
                GetCustomerPurchaseOrderItemId(
                    productionJob);


            var purchaseOrderItem =
                customerPurchaseOrder
                    .Items
                    .FirstOrDefault(x =>
                        GetIntProperty(
                            x,
                            "Id") ==
                        customerPurchaseOrderItemId);


            if (purchaseOrderItem == null)
            {
                throw new BusinessException(
                    $"Customer Purchase Order Item for Production Job {GetProductionJobDisplayCode(productionJob)} was not found.");
            }


            var productionMasterItem =
                GetPropertyValue(
                    productionJob,
                    "Item");


            var purchaseOrderMasterItem =
                GetPropertyValue(
                    purchaseOrderItem,
                    "Item");


            var itemId =
                GetIntProperty(
                    productionJob,
                    "ItemId")

                ??

                GetIntProperty(
                    purchaseOrderItem,
                    "ItemId")

                ??

                GetIntProperty(
                    productionMasterItem,
                    "Id")

                ??

                GetIntProperty(
                    purchaseOrderMasterItem,
                    "Id")

                ??

                0;


            var itemCode =
                FirstNonEmpty(
                    GetStringProperty(
                        productionJob,
                        "ItemCode"),

                    GetStringProperty(
                        purchaseOrderItem,
                        "ItemCode"),

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
                    GetStringProperty(
                        productionJob,
                        "ItemName"),

                    GetStringProperty(
                        purchaseOrderItem,
                        "ItemName"),

                    GetStringProperty(
                        productionMasterItem,
                        "ItemName",
                        "Name"),

                    GetStringProperty(
                        purchaseOrderMasterItem,
                        "ItemName",
                        "Name"))

                ?? string.Empty;


            var partNumber =
                FirstNonEmpty(
                    GetStringProperty(
                        productionJob,
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


            var customerItemCode =
                FirstNonEmpty(
                    GetStringProperty(
                        productionJob,
                        "CustomerItemCode"),

                    GetStringProperty(
                        purchaseOrderItem,
                        "CustomerItemCode"));


            var unitName =
                FirstNonEmpty(
                    GetStringProperty(
                        productionJob,
                        "UnitName",
                        "UomName",
                        "UOMName"),

                    GetStringProperty(
                        purchaseOrderItem,
                        "UnitName",
                        "UomName",
                        "UOMName"),

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


            var hsnNumber =
                FirstNonEmpty(
                    GetStringProperty(
                        productionJob,
                        "HsnNumber",
                        "HSNNumber",
                        "HsnCode"),

                    GetStringProperty(
                        purchaseOrderItem,
                        "HsnNumber",
                        "HSNNumber",
                        "HsnCode"),

                    GetStringProperty(
                        productionMasterItem,
                        "HsnNumber",
                        "HSNNumber",
                        "HsnCode"),

                    GetStringProperty(
                        purchaseOrderMasterItem,
                        "HsnNumber",
                        "HSNNumber",
                        "HsnCode"));


            var productReference =
                FirstNonEmpty(
                    GetStringProperty(
                        productionJob,
                        "ProductReference",
                        "ProductRef"),

                    GetStringProperty(
                        purchaseOrderItem,
                        "ProductReference",
                        "ProductRef"));


            return new InvoiceItem
            {
                /*
                 * Delivery Challan fields remain only
                 * optional historical fields.
                 *
                 * New Invoice source is ProductionJobId.
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
                    itemId,

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
                    GetCustomerPurchaseOrderCode(
                        customerPurchaseOrder),

                CustomerPurchaseOrderNumber =
                    GetCustomerPurchaseOrderNumber(
                        customerPurchaseOrder),


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
            var retainedProductionJobIds =
                new HashSet<int>();


            var sequenceNumber =
                1;


            foreach (var preparedItem
                in preparedItems)
            {
                var preparedProductionJobId =
                    GetRequiredProductionJobId(
                        preparedItem);


                var existingItem =
                    invoice.Items
                        .FirstOrDefault(x =>
                            x.ProductionJobId
                                .HasValue &&
                            x.ProductionJobId
                                .Value ==
                                preparedProductionJobId);


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


                    retainedProductionJobIds.Add(
                        preparedProductionJobId);


                    continue;
                }


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


                retainedProductionJobIds.Add(
                    preparedProductionJobId);
            }


            foreach (var existingItem
                in invoice.Items
                    .Where(x =>
                        x.Id > 0)
                    .ToList())
            {
                if (
                    existingItem
                        .ProductionJobId
                        .HasValue

                    &&

                    retainedProductionJobIds
                        .Contains(
                            existingItem
                                .ProductionJobId
                                .Value)
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
            /*
             * Missing PDI / DC is warning-only.
             *
             * If user already confirmed, continue.
             */
            if (confirmSourceWarning)
            {
                return;
            }


            var productionJobIds =
                items
                    .Select(
                        GetRequiredProductionJobId)
                    .Distinct()
                    .ToList();


            var warningJobIds =
                await GetProductionJobIdsRequiringWarningAsync(
                    productionJobIds);


            if (warningJobIds.Count == 0)
            {
                return;
            }


            throw new BusinessException(
                "One or more selected Production Jobs do not have Finalized PDI or Delivery Challan. Please confirm the warning to continue with Invoice submission.");
        }

        #endregion


        #region Customer Purchase Order Validation

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


            var isDeleted =
                GetBoolProperty(
                    customerPurchaseOrder,
                    "IsDeleted");


            if (isDeleted == true)
            {
                throw new BusinessException(
                    "Selected Customer Purchase Order is deleted.");
            }


            var isActive =
                GetBoolProperty(
                    customerPurchaseOrder,
                    "IsActive");


            if (isActive == false)
            {
                throw new BusinessException(
                    "Selected Customer Purchase Order is inactive.");
            }


            if (GetCustomerId(
                    customerPurchaseOrder) <= 0)
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
                    "Selected Production Job is not available for Invoice or Production is not completed.");
            }


            var isDeleted =
                GetBoolProperty(
                    productionJob,
                    "IsDeleted");


            if (isDeleted == true)
            {
                throw new BusinessException(
                    "Selected Production Job is deleted.");
            }


            var isActive =
                GetBoolProperty(
                    productionJob,
                    "IsActive");


            if (isActive == false)
            {
                throw new BusinessException(
                    "Selected Production Job is inactive.");
            }


            return productionJob;
        }

        #endregion


        #region Required Production Job Id

        private static int
            GetRequiredProductionJobId(
                InvoiceItem item)
        {
            if (
                !item.ProductionJobId
                    .HasValue

                ||

                item.ProductionJobId
                    .Value <= 0
            )
            {
                throw new BusinessException(
                    "Invalid Production Job.");
            }


            return item
                .ProductionJobId
                .Value;
        }

        #endregion


        #region Production Source Helpers

        private static int
            GetCustomerPurchaseOrderId(
                ProductionJob productionJob)
        {
            /*
             * First support direct FK when present.
             */
            var directCustomerPurchaseOrderId =
                GetIntProperty(
                    productionJob,
                    "CustomerPurchaseOrderId",
                    "CustomerPOId",
                    "PurchaseOrderId");


            if (
                directCustomerPurchaseOrderId
                    .HasValue

                &&

                directCustomerPurchaseOrderId
                    .Value > 0
            )
            {
                return directCustomerPurchaseOrderId
                    .Value;
            }


            /*
             * Actual current relationship:
             *
             * ProductionJob
             *   → CustomerPurchaseOrderItem
             *   → CustomerPurchaseOrder
             */
            var customerPurchaseOrderItem =
                GetPropertyValue(
                    productionJob,
                    "CustomerPurchaseOrderItem");


            var purchaseOrderIdFromItem =
                GetIntProperty(
                    customerPurchaseOrderItem,
                    "CustomerPurchaseOrderId",
                    "CustomerPOId",
                    "PurchaseOrderId");


            if (
                purchaseOrderIdFromItem
                    .HasValue

                &&

                purchaseOrderIdFromItem
                    .Value > 0
            )
            {
                return purchaseOrderIdFromItem
                    .Value;
            }


            var customerPurchaseOrder =
                GetPropertyValue(
                    customerPurchaseOrderItem,
                    "CustomerPurchaseOrder",
                    "PurchaseOrder");


            var purchaseOrderId =
                GetIntProperty(
                    customerPurchaseOrder,
                    "Id");


            if (
                purchaseOrderId
                    .HasValue

                &&

                purchaseOrderId
                    .Value > 0
            )
            {
                return purchaseOrderId
                    .Value;
            }


            throw new BusinessException(
                $"Customer Purchase Order reference is missing from Production Job {GetProductionJobDisplayCode(productionJob)}.");
        }


        private static int
            GetCustomerPurchaseOrderItemId(
                ProductionJob productionJob)
        {
            var directItemId =
                GetIntProperty(
                    productionJob,
                    "CustomerPurchaseOrderItemId",
                    "CustomerPOItemId",
                    "PurchaseOrderItemId");


            if (
                directItemId
                    .HasValue

                &&

                directItemId
                    .Value > 0
            )
            {
                return directItemId
                    .Value;
            }


            var customerPurchaseOrderItem =
                GetPropertyValue(
                    productionJob,
                    "CustomerPurchaseOrderItem");


            var navigationItemId =
                GetIntProperty(
                    customerPurchaseOrderItem,
                    "Id");


            if (
                navigationItemId
                    .HasValue

                &&

                navigationItemId
                    .Value > 0
            )
            {
                return navigationItemId
                    .Value;
            }


            throw new BusinessException(
                $"Customer Purchase Order Item reference is missing from Production Job {GetProductionJobDisplayCode(productionJob)}.");
        }


        private static decimal
            GetCompletedProductionQuantity(
                ProductionJob productionJob)
        {
            /*
             * Current ProductionJob uses JobQuantity.
             *
             * Additional names are kept as safe fallback
             * without requiring entity redesign.
             */
            var quantity =
                GetDecimalProperty(
                    productionJob,
                    "JobQuantity",
                    "CompletedQuantity",
                    "ProducedQuantity",
                    "ProductionQuantity",
                    "CompletedQty",
                    "ProducedQty",
                    "Quantity");


            return quantity ?? 0m;
        }


        private static string
            GetProductionJobDisplayCode(
                ProductionJob productionJob)
        {
            return FirstNonEmpty(
                GetStringProperty(
                    productionJob,
                    "Code",
                    "ProductionJobCode",
                    "JobCode"),

                productionJob.Id
                    .ToString())

                ?? productionJob.Id
                    .ToString();
        }


        private static int
            GetCustomerId(
                CustomerPurchaseOrder customerPurchaseOrder)
        {
            var customerId =
                GetIntProperty(
                    customerPurchaseOrder,
                    "CustomerId");


            if (
                customerId.HasValue &&
                customerId.Value > 0
            )
            {
                return customerId.Value;
            }


            var customer =
                GetPropertyValue(
                    customerPurchaseOrder,
                    "Customer");


            var customerNavigationId =
                GetIntProperty(
                    customer,
                    "Id");


            return customerNavigationId
                ?? 0;
        }


        private static string?
            GetCustomerPurchaseOrderCode(
                CustomerPurchaseOrder customerPurchaseOrder)
        {
            return GetStringProperty(
                customerPurchaseOrder,
                "Code",
                "CustomerPurchaseOrderCode",
                "PurchaseOrderCode",
                "POCode");
        }


        private static string?
            GetCustomerPurchaseOrderNumber(
                CustomerPurchaseOrder customerPurchaseOrder)
        {
            return GetStringProperty(
                customerPurchaseOrder,
                "PurchaseOrderNumber",
                "CustomerPurchaseOrderNumber",
                "PONumber",
                "PoNumber",
                "OrderNumber");
        }


        private static string
            GetTrustedItemName(
                CustomerPurchaseOrder customerPurchaseOrder,
                ProductionJob productionJob)
        {
            var purchaseOrderItem =
                GetPurchaseOrderItem(
                    customerPurchaseOrder,
                    productionJob);


            var productionMasterItem =
                GetPropertyValue(
                    productionJob,
                    "Item");


            var purchaseOrderMasterItem =
                GetPropertyValue(
                    purchaseOrderItem,
                    "Item");


            return FirstNonEmpty(
                GetStringProperty(
                    productionJob,
                    "ItemName"),

                GetStringProperty(
                    purchaseOrderItem,
                    "ItemName"),

                GetStringProperty(
                    productionMasterItem,
                    "ItemName",
                    "Name"),

                GetStringProperty(
                    purchaseOrderMasterItem,
                    "ItemName",
                    "Name"),

                GetProductionJobDisplayCode(
                    productionJob))

                ?? "Item";
        }


        private static string
            GetTrustedUnitName(
                CustomerPurchaseOrder customerPurchaseOrder,
                ProductionJob productionJob)
        {
            var purchaseOrderItem =
                GetPurchaseOrderItem(
                    customerPurchaseOrder,
                    productionJob);


            var productionMasterItem =
                GetPropertyValue(
                    productionJob,
                    "Item");


            var purchaseOrderMasterItem =
                GetPropertyValue(
                    purchaseOrderItem,
                    "Item");


            return FirstNonEmpty(
                GetStringProperty(
                    productionJob,
                    "UnitName",
                    "UomName",
                    "UOMName"),

                GetStringProperty(
                    purchaseOrderItem,
                    "UnitName",
                    "UomName",
                    "UOMName"),

                GetStringProperty(
                    productionMasterItem,
                    "UnitName",
                    "UomName",
                    "UOMName"),

                GetStringProperty(
                    purchaseOrderMasterItem,
                    "UnitName",
                    "UomName",
                    "UOMName"))

                ?? string.Empty;
        }


        private static object?
            GetPurchaseOrderItem(
                CustomerPurchaseOrder customerPurchaseOrder,
                ProductionJob productionJob)
        {
            var itemId =
                GetCustomerPurchaseOrderItemId(
                    productionJob);


            return customerPurchaseOrder
                .Items
                .FirstOrDefault(x =>
                    GetIntProperty(
                        x,
                        "Id") ==
                    itemId);
        }

        #endregion


        #region Reflection Source Helpers

        private static object?
            GetPropertyValue(
                object? source,
                params string[] propertyNames)
        {
            if (source == null)
            {
                return null;
            }


            var type =
                source.GetType();


            foreach (var propertyName
                in propertyNames)
            {
                var property =
                    type.GetProperty(
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


            if (value == null)
            {
                return null;
            }


            var text =
                value.ToString();


            return string.IsNullOrWhiteSpace(
                text)
                ? null
                : text.Trim();
        }


        private static int?
            GetIntProperty(
                object? source,
                params string[] propertyNames)
        {
            var value =
                GetPropertyValue(
                    source,
                    propertyNames);


            if (value == null)
            {
                return null;
            }


            try
            {
                return Convert.ToInt32(
                    value);
            }
            catch
            {
                return null;
            }
        }


        private static decimal?
            GetDecimalProperty(
                object? source,
                params string[] propertyNames)
        {
            var value =
                GetPropertyValue(
                    source,
                    propertyNames);


            if (value == null)
            {
                return null;
            }


            try
            {
                return Convert.ToDecimal(
                    value);
            }
            catch
            {
                return null;
            }
        }


        private static bool?
            GetBoolProperty(
                object? source,
                params string[] propertyNames)
        {
            var value =
                GetPropertyValue(
                    source,
                    propertyNames);


            if (value == null)
            {
                return null;
            }


            try
            {
                return Convert.ToBoolean(
                    value);
            }
            catch
            {
                return null;
            }
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
            #region Gross

            item.GrossAmount =
                RoundAmount(
                    item.InvoiceQuantity *
                    item.Rate);

            #endregion


            #region Discount

            item.DiscountAmount =
                RoundAmount(
                    item.GrossAmount *
                    item.DiscountPercent /
                    100m);


            item.TaxableAmount =
                RoundAmount(
                    item.GrossAmount -
                    item.DiscountAmount);

            #endregion


            #region Reset GST

            item.CgstRate =
                0;

            item.SgstRate =
                0;

            item.IgstRate =
                0;


            item.CgstAmount =
                0;

            item.SgstAmount =
                0;

            item.IgstAmount =
                0;

            #endregion


            #region GST

            if (item.GstRate > 0)
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

            #endregion


            #region Total Tax

            item.TotalTaxAmount =
                RoundAmount(
                    item.CgstAmount +
                    item.SgstAmount +
                    item.IgstAmount);

            #endregion


            #region Line Total

            item.LineTotal =
                RoundAmount(
                    item.TaxableAmount +
                    item.TotalTaxAmount);

            #endregion
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
            var hasSubmittedAddress =
                HasAnyBillingAddress(
                    submitted);


            if (hasSubmittedAddress)
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
                    StringComparison
                        .OrdinalIgnoreCase);
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
                StringComparison
                    .OrdinalIgnoreCase);
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
            if (!creditDays.HasValue)
            {
                return null;
            }


            if (creditDays.Value < 0)
            {
                return null;
            }


            return invoiceDate.Date
                .AddDays(
                    creditDays.Value);
        }

        #endregion


        #region Invoice Code Generation

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
                        ? lastCode
                            .Substring(
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

                actualType ==
                    typeof(string)

                ||

                actualType ==
                    typeof(bool)

                ||

                actualType ==
                    typeof(byte)

                ||

                actualType ==
                    typeof(short)

                ||

                actualType ==
                    typeof(int)

                ||

                actualType ==
                    typeof(long)

                ||

                actualType ==
                    typeof(float)

                ||

                actualType ==
                    typeof(double)

                ||

                actualType ==
                    typeof(decimal)

                ||

                actualType ==
                    typeof(DateTime)

                ||

                actualType ==
                    typeof(DateTimeOffset)

                ||

                actualType ==
                    typeof(TimeSpan)

                ||

                actualType ==
                    typeof(Guid);
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
             * Product / Item / PO / Production snapshots
             * are rebuilt from trusted source records.
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


            if (invoice.OtherCharges < 0)
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
                !item.ProductionJobId
                    .HasValue

                ||

                item.ProductionJobId
                    .Value <= 0
            )
            {
                throw new BusinessException(
                    "Invalid Production Job.");
            }


            if (item.InvoiceQuantity <= 0)
            {
                throw new BusinessException(
                    "Invoice Quantity must be greater than zero.");
            }


            if (item.Rate <= 0)
            {
                throw new BusinessException(
                    "Invoice Rate must be greater than zero.");
            }


            if (
                item.DiscountPercent < 0
                ||
                item.DiscountPercent > 100
            )
            {
                throw new BusinessException(
                    "Discount percentage must be between 0 and 100.");
            }


            if (
                item.GstRate < 0
                ||
                item.GstRate > 100
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


            ValidateDuplicateProductionJobs(
                items);
        }


        private static void
            ValidateDuplicateProductionJobs(
                IEnumerable<InvoiceItem> items)
        {
            var duplicate =
                items
                    .Where(x =>
                        x.ProductionJobId
                            .HasValue &&
                        x.ProductionJobId
                            .Value > 0)
                    .GroupBy(x =>
                        x.ProductionJobId
                            .Value)
                    .FirstOrDefault(x =>
                        x.Count() > 1);


            if (duplicate != null)
            {
                throw new BusinessException(
                    "The same Production Job cannot be added more than once in one Invoice.");
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