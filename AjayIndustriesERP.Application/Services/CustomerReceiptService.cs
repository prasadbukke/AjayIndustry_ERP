/*
============================================================
File: CustomerReceiptService.cs

Module:
Customer Receipt

Purpose:
Contains complete Customer Receipt business logic.

Responsibilities:
- Search and paginate Customer Receipts.
- Load active Customers.
- Load Finalized Invoices having outstanding balance.
- Calculate trusted Invoice outstanding.
- Generate financial-year based Receipt Code.
- Capture Customer / Company historical snapshots.
- Create Draft Customer Receipt.
- Update Draft Customer Receipt.
- Synchronize Invoice allocations.
- Validate partial / full payments.
- Finalize Customer Receipt.
- Soft-delete Draft Receipt.
- Restore Draft Receipt after revalidation.
- Generate Finalized Customer Receipt PDF.

Receipt Code:
AI/CR/{YY-YY}/{00001}

Example:
AI/CR/26-27/00001

Important:
- Only Finalized Invoices can receive payment.
- One Receipt can be allocated against multiple Invoices.
- All Invoices on one Receipt must belong to the same Customer.
- Browser-posted Invoice totals / outstanding snapshots
  are NOT trusted.
- Browser-posted AlreadyReceivedAmount and
  BalanceAfterReceipt are NOT trusted.
- Only AllocatedAmount is a commercial user input.
- Only Finalized Customer Receipts affect Invoice outstanding.
- Draft Receipts do NOT reserve Invoice outstanding.
- Finalize always revalidates the current live outstanding.
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
    public class CustomerReceiptService
        : ICustomerReceiptService
    {
        #region Fields

        private readonly
            ICustomerReceiptRepository
            _repository;


        private readonly
            ICustomerReceiptPdfGenerator
            _pdfGenerator;

        #endregion


        #region Constructor

        public CustomerReceiptService(
            ICustomerReceiptRepository repository,
            ICustomerReceiptPdfGenerator pdfGenerator)
        {
            _repository =
                repository;


            _pdfGenerator =
                pdfGenerator;
        }

        #endregion


        #region Read

        public async Task<CustomerReceipt?>
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


        #region Pagination

        public async Task<PagedResult<CustomerReceipt>>
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


        public async Task<PagedResult<CustomerReceipt>>
            SearchPagedAsync(
                string? searchTerm,
                int pageNumber,
                int pageSize)
        {
            NormalizePagination(
                ref pageNumber,
                ref pageSize);


            if (string.IsNullOrWhiteSpace(
                searchTerm))
            {
                return await _repository
                    .GetPagedAsync(
                        pageNumber,
                        pageSize);
            }


            return await _repository
                .SearchPagedAsync(
                    searchTerm.Trim(),
                    pageNumber,
                    pageSize);
        }


        public async Task<PagedResult<CustomerReceipt>>
            GetDeletedPagedAsync(
                int pageNumber,
                int pageSize)
        {
            NormalizePagination(
                ref pageNumber,
                ref pageSize);


            return await _repository
                .GetDeletedPagedAsync(
                    pageNumber,
                    pageSize);
        }


        public async Task<PagedResult<CustomerReceipt>>
            SearchDeletedPagedAsync(
                string? searchTerm,
                int pageNumber,
                int pageSize)
        {
            NormalizePagination(
                ref pageNumber,
                ref pageSize);


            if (string.IsNullOrWhiteSpace(
                searchTerm))
            {
                return await _repository
                    .GetDeletedPagedAsync(
                        pageNumber,
                        pageSize);
            }


            return await _repository
                .SearchDeletedPagedAsync(
                    searchTerm.Trim(),
                    pageNumber,
                    pageSize);
        }

        #endregion


        #region Customer

        public async Task<List<Customer>>
            GetCustomersForReceiptAsync()
        {
            return await _repository
                .GetCustomersForReceiptAsync();
        }

        #endregion


        #region Outstanding Invoices

        public async Task<List<Invoice>>
            GetOutstandingInvoicesForCustomerAsync(
                int customerId,
                int? excludeCustomerReceiptId = null)
        {
            #region Customer Validation

            await ValidateCustomerAsync(
                customerId);

            #endregion


            #region Finalized Invoices

            var invoices =
                await _repository
                    .GetFinalizedInvoicesForReceiptAsync(
                        customerId);

            #endregion


            var result =
                new List<Invoice>();


            foreach (var invoice
                in invoices)
            {
                var outstanding =
                    await CalculateOutstandingAsync(
                        invoice,
                        excludeCustomerReceiptId);


                if (outstanding > 0)
                {
                    result.Add(
                        invoice);
                }
            }


            return result;
        }


        public async Task<decimal>
            GetInvoiceOutstandingAsync(
                int invoiceId,
                int? excludeCustomerReceiptId = null)
        {
            if (invoiceId <= 0)
            {
                throw new BusinessException(
                    "Invalid Invoice.");
            }


            var invoice =
                await _repository
                    .GetFinalizedInvoiceForReceiptAsync(
                        invoiceId);


            if (invoice == null)
            {
                throw new BusinessException(
                    "Invoice is not available for Customer Receipt.");
            }


            return await CalculateOutstandingAsync(
                invoice,
                excludeCustomerReceiptId);
        }

        #endregion


        #region Create

        public async Task<CustomerReceipt>
            CreateAsync(
                CustomerReceipt customerReceipt)
        {
            if (customerReceipt == null)
            {
                throw new BusinessException(
                    "Customer Receipt information is required.");
            }


            #region Normalize / Validate Header

            NormalizeHeader(
                customerReceipt);


            ValidateHeader(
                customerReceipt);

            #endregion


            #region Submitted Allocations

            var submittedAllocations =
                GetSubmittedAllocations(
                    customerReceipt);


            ValidateSubmittedAllocations(
                submittedAllocations);

            #endregion


            #region Customer

            var customer =
                await ValidateCustomerAsync(
                    customerReceipt.CustomerId);

            #endregion


            #region Company

            var company =
                await ValidateCompanyAsync();

            #endregion


            #region Trusted Allocations

            var trustedAllocations =
                await PrepareTrustedAllocationsAsync(
                    submittedAllocations,
                    customer.Id,
                    excludeCustomerReceiptId:
                        null);

            #endregion


            #region Total Validation

            ValidateReceiptTotal(
                customerReceipt.TotalReceivedAmount,
                trustedAllocations);

            #endregion


            #region Trusted Header

            var newReceipt =
                new CustomerReceipt
                {
                    Code =
                        await GenerateReceiptCodeAsync(
                            customerReceipt.ReceiptDate),

                    ReceiptDate =
                        customerReceipt
                            .ReceiptDate
                            .Date,


                    CustomerId =
                        customer.Id,

                    CustomerCode =
                        GetStringPropertyValue(
                            customer,
                            "Code",
                            "CustomerCode"),

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


                    PaymentMode =
                        customerReceipt.PaymentMode,

                    ReferenceNumber =
                        customerReceipt.ReferenceNumber,

                    ChequeNumber =
                        customerReceipt.ChequeNumber,

                    ChequeDate =
                        customerReceipt.ChequeDate,

                    BankName =
                        customerReceipt.BankName,


                    TotalReceivedAmount =
                        RoundMoney(
                            trustedAllocations
                                .Sum(x =>
                                    x.AllocatedAmount)),


                    Remarks =
                        customerReceipt.Remarks,


                    Status =
                        CustomerReceiptStatus.Draft,


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


            #region Add Trusted Allocations

            var sequenceNumber =
                1;


            foreach (var allocation
                in trustedAllocations)
            {
                allocation.Id =
                    0;

                allocation.SequenceNumber =
                    sequenceNumber++;

                allocation.IsActive =
                    true;

                allocation.IsDeleted =
                    false;

                allocation.CreatedOn =
                    DateTime.UtcNow;

                allocation.CreatedBy =
                    "System";


                newReceipt.Allocations.Add(
                    allocation);
            }

            #endregion


            #region Save

            await _repository
                .AddAsync(
                    newReceipt);

            #endregion


            return newReceipt;
        }

        #endregion


        #region Update

        public async Task<CustomerReceipt>
            UpdateAsync(
                CustomerReceipt customerReceipt)
        {
            if (customerReceipt == null ||
                customerReceipt.Id <= 0)
            {
                throw new BusinessException(
                    "Invalid Customer Receipt.");
            }


            #region Load Existing

            var existing =
                await _repository
                    .GetForUpdateAsync(
                        customerReceipt.Id);


            if (existing == null ||
                existing.IsDeleted)
            {
                throw new BusinessException(
                    "Customer Receipt not found.");
            }

            #endregion


            #region Draft Only

            if (existing.Status !=
                CustomerReceiptStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Customer Receipt can be edited.");
            }

            #endregion


            #region Normalize / Validate Header

            NormalizeHeader(
                customerReceipt);


            ValidateHeader(
                customerReceipt);

            #endregion


            #region Financial Year Protection

            var oldFinancialYear =
                GetFinancialYear(
                    existing.ReceiptDate);


            var newFinancialYear =
                GetFinancialYear(
                    customerReceipt.ReceiptDate);


            if (!string.Equals(
                oldFinancialYear,
                newFinancialYear,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException(
                    "Receipt Date cannot be changed to another Financial Year.");
            }

            #endregion


            #region Submitted Allocations

            var submittedAllocations =
                GetSubmittedAllocations(
                    customerReceipt);


            ValidateSubmittedAllocations(
                submittedAllocations);


            ValidateSubmittedAllocationRecordIds(
                existing,
                submittedAllocations);

            #endregion


            #region Customer

            var customer =
                await ValidateCustomerAsync(
                    customerReceipt.CustomerId);

            #endregion


            #region Trusted Allocations

            var trustedAllocations =
                await PrepareTrustedAllocationsAsync(
                    submittedAllocations,
                    customer.Id,
                    existing.Id);

            #endregion


            #region Total Validation

            ValidateReceiptTotal(
                customerReceipt.TotalReceivedAmount,
                trustedAllocations);

            #endregion


            #region Header Update

            var customerChanged =
                existing.CustomerId !=
                customer.Id;


            existing.ReceiptDate =
                customerReceipt
                    .ReceiptDate
                    .Date;


            existing.CustomerId =
                customer.Id;

            existing.CustomerCode =
                GetStringPropertyValue(
                    customer,
                    "Code",
                    "CustomerCode");

            existing.CustomerName =
                customer.CustomerName;


            if (customerChanged ||
                string.IsNullOrWhiteSpace(
                    existing.CustomerSnapshotJson))
            {
                existing.CustomerSnapshotJson =
                    SerializeScalarSnapshot(
                        customer);
            }


            existing.PaymentMode =
                customerReceipt.PaymentMode;

            existing.ReferenceNumber =
                customerReceipt.ReferenceNumber;

            existing.ChequeNumber =
                customerReceipt.ChequeNumber;

            existing.ChequeDate =
                customerReceipt.ChequeDate;

            existing.BankName =
                customerReceipt.BankName;


            existing.TotalReceivedAmount =
                RoundMoney(
                    trustedAllocations
                        .Sum(x =>
                            x.AllocatedAmount));


            existing.Remarks =
                customerReceipt.Remarks;


            existing.ModifiedOn =
                DateTime.UtcNow;

            existing.ModifiedBy =
                "System";

            #endregion


            #region Synchronize Allocations

            SynchronizeAllocations(
                existing,
                trustedAllocations);

            #endregion


            #region Save

            await _repository
                .UpdateAsync(
                    existing);

            #endregion


            return existing;
        }

        #endregion


        #region Finalize

        public async Task<CustomerReceipt>
            FinalizeAsync(
                int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid Customer Receipt.");
            }


            #region Load Existing

            var existing =
                await _repository
                    .GetForUpdateAsync(
                        id);


            if (existing == null ||
                existing.IsDeleted)
            {
                throw new BusinessException(
                    "Customer Receipt not found.");
            }

            #endregion


            #region Draft Only

            if (existing.Status !=
                CustomerReceiptStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Customer Receipt can be finalized.");
            }

            #endregion


            #region Header Revalidation

            NormalizeHeader(
                existing);


            ValidateHeader(
                existing);

            #endregion


            #region Customer Revalidation

            var customer =
                await ValidateCustomerAsync(
                    existing.CustomerId);

            #endregion


            #region Active Allocations

            var activeAllocations =
                existing.Allocations
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();


            ValidateSubmittedAllocations(
                activeAllocations);

            #endregion


            #region Rebuild Trusted Allocation Snapshots

            var trustedAllocations =
                await PrepareTrustedAllocationsAsync(
                    activeAllocations,
                    customer.Id,
                    existing.Id);

            #endregion


            #region Total Revalidation

            ValidateReceiptTotal(
                existing.TotalReceivedAmount,
                trustedAllocations);

            #endregion


            #region Apply Latest Trusted Amounts

            ApplyTrustedAllocationValues(
                existing,
                trustedAllocations);

            #endregion


            #region Finalize

            existing.TotalReceivedAmount =
                RoundMoney(
                    trustedAllocations
                        .Sum(x =>
                            x.AllocatedAmount));


            existing.Status =
                CustomerReceiptStatus.Finalized;


            existing.FinalizedOn =
                DateTime.UtcNow;

            existing.FinalizedBy =
                "System";


            existing.ModifiedOn =
                DateTime.UtcNow;

            existing.ModifiedBy =
                "System";

            #endregion


            #region Save

            await _repository
                .UpdateAsync(
                    existing);

            #endregion


            return existing;
        }

        #endregion


        #region Delete

        public async Task DeleteAsync(
            int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid Customer Receipt.");
            }


            var existing =
                await _repository
                    .GetForUpdateAsync(
                        id);


            if (existing == null ||
                existing.IsDeleted)
            {
                throw new BusinessException(
                    "Customer Receipt not found.");
            }


            if (existing.Status !=
                CustomerReceiptStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Customer Receipt can be deleted.");
            }


            /*
             * Child allocations are intentionally left unchanged.
             *
             * Parent soft-delete is enough because:
             * - Draft Receipt has no financial effect.
             * - Finalized allocation calculation also checks
             *   CustomerReceipt.IsDeleted.
             *
             * Keeping child flags unchanged allows accurate
             * Restore without reviving previously removed lines.
             */
            existing.IsDeleted =
                true;

            existing.IsActive =
                false;


            existing.ModifiedOn =
                DateTime.UtcNow;

            existing.ModifiedBy =
                "System";


            await _repository
                .UpdateAsync(
                    existing);
        }

        #endregion


        #region Restore

        public async Task RestoreAsync(
            int id)
        {
            if (id <= 0)
            {
                throw new BusinessException(
                    "Invalid Customer Receipt.");
            }


            #region Load Deleted Receipt

            var existing =
                await _repository
                    .GetForUpdateAsync(
                        id);


            if (existing == null ||
                !existing.IsDeleted)
            {
                throw new BusinessException(
                    "Deleted Customer Receipt not found.");
            }

            #endregion


            #region Draft Only

            if (existing.Status !=
                CustomerReceiptStatus.Draft)
            {
                throw new BusinessException(
                    "Only Draft Customer Receipt can be restored.");
            }

            #endregion


            #region Customer

            var customer =
                await ValidateCustomerAsync(
                    existing.CustomerId);

            #endregion


            #region Active Allocations

            var activeAllocations =
                existing.Allocations
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();


            ValidateSubmittedAllocations(
                activeAllocations);

            #endregion


            #region Revalidate Outstanding

            var trustedAllocations =
                await PrepareTrustedAllocationsAsync(
                    activeAllocations,
                    customer.Id,
                    existing.Id);


            ValidateReceiptTotal(
                existing.TotalReceivedAmount,
                trustedAllocations);


            ApplyTrustedAllocationValues(
                existing,
                trustedAllocations);

            #endregion


            #region Restore

            existing.IsDeleted =
                false;

            existing.IsActive =
                true;


            existing.ModifiedOn =
                DateTime.UtcNow;

            existing.ModifiedBy =
                "System";

            #endregion


            await _repository
                .UpdateAsync(
                    existing);
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
                    "Invalid Customer Receipt.");
            }


            var customerReceipt =
                await _repository
                    .GetByIdAsync(
                        id);


            if (customerReceipt == null)
            {
                throw new BusinessException(
                    "Customer Receipt not found.");
            }


            if (customerReceipt.Status !=
                CustomerReceiptStatus.Finalized)
            {
                throw new BusinessException(
                    "Only Finalized Customer Receipt can be downloaded as PDF.");
            }


            return _pdfGenerator
                .Generate(
                    customerReceipt);
        }

        #endregion


        #region Trusted Allocation Preparation

        private async Task<List<CustomerReceiptAllocation>>
            PrepareTrustedAllocationsAsync(
                IEnumerable<CustomerReceiptAllocation>
                    submittedAllocations,
                int expectedCustomerId,
                int? excludeCustomerReceiptId)
        {
            var trustedAllocations =
                new List<CustomerReceiptAllocation>();


            var sequenceNumber =
                1;


            foreach (var submitted
                in submittedAllocations)
            {
                #region Invoice

                var invoice =
                    await _repository
                        .GetFinalizedInvoiceForReceiptAsync(
                            submitted.InvoiceId);


                if (invoice == null)
                {
                    throw new BusinessException(
                        "One or more selected Invoices are not available for payment.");
                }

                #endregion


                #region Customer Ownership

                if (invoice.CustomerId !=
                    expectedCustomerId)
                {
                    throw new BusinessException(
                        $"Invoice {invoice.Code} does not belong to the selected Customer.");
                }

                #endregion


                #region Current Finalized Allocation

                var alreadyReceived =
                    await _repository
                        .GetFinalizedAllocatedAmountAsync(
                            invoice.Id,
                            excludeCustomerReceiptId);


                alreadyReceived =
                    RoundMoney(
                        alreadyReceived);

                #endregion


                #region Outstanding

                var outstanding =
                    RoundMoney(
                        invoice.GrandTotal -
                        alreadyReceived);


                if (outstanding < 0)
                {
                    outstanding =
                        0;
                }


                if (outstanding <= 0)
                {
                    throw new BusinessException(
                        $"Invoice {invoice.Code} is already fully paid.");
                }

                #endregion


                #region Current Allocation

                var allocatedAmount =
                    RoundMoney(
                        submitted.AllocatedAmount);


                if (allocatedAmount <= 0)
                {
                    throw new BusinessException(
                        $"Allocated Amount for Invoice {invoice.Code} must be greater than zero.");
                }


                if (allocatedAmount >
                    outstanding)
                {
                    throw new BusinessException(
                        $"Allocated Amount for Invoice {invoice.Code} cannot exceed current outstanding amount {outstanding:0.00}.");
                }

                #endregion


                #region Trusted Snapshot

                trustedAllocations.Add(
                    new CustomerReceiptAllocation
                    {
                        Id =
                            submitted.Id,

                        SequenceNumber =
                            sequenceNumber++,


                        InvoiceId =
                            invoice.Id,

                        InvoiceCode =
                            invoice.Code,

                        InvoiceDate =
                            invoice.InvoiceDate,

                        InvoiceGrandTotal =
                            RoundMoney(
                                invoice.GrandTotal),


                        AlreadyReceivedAmount =
                            alreadyReceived,

                        AllocatedAmount =
                            allocatedAmount,

                        BalanceAfterReceipt =
                            RoundMoney(
                                outstanding -
                                allocatedAmount),


                        IsActive =
                            true,

                        IsDeleted =
                            false
                    });

                #endregion
            }


            return trustedAllocations;
        }

        #endregion


        #region Allocation Synchronization

        private static void SynchronizeAllocations(
            CustomerReceipt existing,
            List<CustomerReceiptAllocation>
                trustedAllocations)
        {
            var retainedIds =
                new HashSet<int>();


            foreach (var trusted
                in trustedAllocations)
            {
                CustomerReceiptAllocation?
                    existingAllocation =
                        null;


                if (trusted.Id > 0)
                {
                    existingAllocation =
                        existing.Allocations
                            .FirstOrDefault(x =>
                                x.Id ==
                                trusted.Id);


                    if (existingAllocation == null)
                    {
                        throw new BusinessException(
                            "Invalid Customer Receipt Allocation record.");
                    }
                }


                if (existingAllocation != null)
                {
                    existingAllocation.SequenceNumber =
                        trusted.SequenceNumber;


                    existingAllocation.InvoiceId =
                        trusted.InvoiceId;

                    existingAllocation.InvoiceCode =
                        trusted.InvoiceCode;

                    existingAllocation.InvoiceDate =
                        trusted.InvoiceDate;

                    existingAllocation.InvoiceGrandTotal =
                        trusted.InvoiceGrandTotal;


                    existingAllocation.AlreadyReceivedAmount =
                        trusted.AlreadyReceivedAmount;

                    existingAllocation.AllocatedAmount =
                        trusted.AllocatedAmount;

                    existingAllocation.BalanceAfterReceipt =
                        trusted.BalanceAfterReceipt;


                    existingAllocation.IsActive =
                        true;

                    existingAllocation.IsDeleted =
                        false;


                    existingAllocation.ModifiedOn =
                        DateTime.UtcNow;

                    existingAllocation.ModifiedBy =
                        "System";


                    retainedIds.Add(
                        existingAllocation.Id);


                    continue;
                }


                var newAllocation =
                    new CustomerReceiptAllocation
                    {
                        SequenceNumber =
                            trusted.SequenceNumber,


                        InvoiceId =
                            trusted.InvoiceId,

                        InvoiceCode =
                            trusted.InvoiceCode,

                        InvoiceDate =
                            trusted.InvoiceDate,

                        InvoiceGrandTotal =
                            trusted.InvoiceGrandTotal,


                        AlreadyReceivedAmount =
                            trusted.AlreadyReceivedAmount,

                        AllocatedAmount =
                            trusted.AllocatedAmount,

                        BalanceAfterReceipt =
                            trusted.BalanceAfterReceipt,


                        IsActive =
                            true,

                        IsDeleted =
                            false,


                        CreatedOn =
                            DateTime.UtcNow,

                        CreatedBy =
                            "System"
                    };


                existing.Allocations.Add(
                    newAllocation);
            }


            #region Soft Delete Removed Allocations

            foreach (var allocation
                in existing.Allocations
                    .Where(x =>
                        x.Id > 0 &&
                        !x.IsDeleted &&
                        x.IsActive &&
                        !retainedIds.Contains(
                            x.Id))
                    .ToList())
            {
                allocation.IsActive =
                    false;

                allocation.IsDeleted =
                    true;


                allocation.ModifiedOn =
                    DateTime.UtcNow;

                allocation.ModifiedBy =
                    "System";
            }

            #endregion
        }


        private static void
            ApplyTrustedAllocationValues(
                CustomerReceipt existing,
                List<CustomerReceiptAllocation>
                    trustedAllocations)
        {
            foreach (var trusted
                in trustedAllocations)
            {
                var existingAllocation =
                    existing.Allocations
                        .FirstOrDefault(x =>
                            !x.IsDeleted &&
                            x.IsActive &&
                            x.InvoiceId ==
                                trusted.InvoiceId);


                if (existingAllocation == null)
                {
                    throw new BusinessException(
                        "Customer Receipt Allocation not found.");
                }


                existingAllocation.SequenceNumber =
                    trusted.SequenceNumber;


                existingAllocation.InvoiceCode =
                    trusted.InvoiceCode;

                existingAllocation.InvoiceDate =
                    trusted.InvoiceDate;

                existingAllocation.InvoiceGrandTotal =
                    trusted.InvoiceGrandTotal;


                existingAllocation.AlreadyReceivedAmount =
                    trusted.AlreadyReceivedAmount;

                existingAllocation.AllocatedAmount =
                    trusted.AllocatedAmount;

                existingAllocation.BalanceAfterReceipt =
                    trusted.BalanceAfterReceipt;


                existingAllocation.ModifiedOn =
                    DateTime.UtcNow;

                existingAllocation.ModifiedBy =
                    "System";
            }
        }

        #endregion


        #region Submitted Allocation Validation

        private static List<CustomerReceiptAllocation>
            GetSubmittedAllocations(
                CustomerReceipt customerReceipt)
        {
            if (customerReceipt.Allocations == null)
            {
                return new List<CustomerReceiptAllocation>();
            }


            return customerReceipt
                .Allocations
                .Where(x =>
                    !x.IsDeleted)
                .ToList();
        }


        private static void
            ValidateSubmittedAllocations(
                ICollection<CustomerReceiptAllocation>
                    allocations)
        {
            if (allocations == null ||
                allocations.Count == 0)
            {
                throw new BusinessException(
                    "At least one Invoice allocation is required.");
            }


            if (allocations.Any(x =>
                x.InvoiceId <= 0))
            {
                throw new BusinessException(
                    "Invalid Invoice allocation found.");
            }


            if (allocations.Any(x =>
                x.AllocatedAmount <= 0))
            {
                throw new BusinessException(
                    "Allocated Amount must be greater than zero.");
            }


            var duplicateInvoice =
                allocations
                    .GroupBy(x =>
                        x.InvoiceId)
                    .FirstOrDefault(x =>
                        x.Count() > 1);


            if (duplicateInvoice != null)
            {
                throw new BusinessException(
                    "The same Invoice cannot be allocated more than once in one Customer Receipt.");
            }


            var duplicateRecordId =
                allocations
                    .Where(x =>
                        x.Id > 0)
                    .GroupBy(x =>
                        x.Id)
                    .FirstOrDefault(x =>
                        x.Count() > 1);


            if (duplicateRecordId != null)
            {
                throw new BusinessException(
                    "Duplicate Customer Receipt Allocation record found.");
            }
        }


        private static void
            ValidateSubmittedAllocationRecordIds(
                CustomerReceipt existing,
                IEnumerable<CustomerReceiptAllocation>
                    submittedAllocations)
        {
            foreach (var submitted
                in submittedAllocations
                    .Where(x =>
                        x.Id > 0))
            {
                var existingAllocation =
                    existing.Allocations
                        .FirstOrDefault(x =>
                            x.Id ==
                            submitted.Id &&
                            !x.IsDeleted &&
                            x.IsActive);


                if (existingAllocation == null)
                {
                    throw new BusinessException(
                        "Invalid Customer Receipt Allocation record.");
                }


                /*
                 * Existing row ID cannot be reused to point
                 * to another Invoice.
                 *
                 * Changing an Invoice creates a new allocation
                 * row and soft-deletes the old allocation.
                 */
                if (existingAllocation.InvoiceId !=
                    submitted.InvoiceId)
                {
                    throw new BusinessException(
                        "Customer Receipt Allocation Invoice cannot be changed for an existing row.");
                }
            }
        }

        #endregion


        #region Receipt Total Validation

        private static void ValidateReceiptTotal(
            decimal submittedTotal,
            IEnumerable<CustomerReceiptAllocation>
                trustedAllocations)
        {
            var totalReceivedAmount =
                RoundMoney(
                    submittedTotal);


            if (totalReceivedAmount <= 0)
            {
                throw new BusinessException(
                    "Total Received Amount must be greater than zero.");
            }


            var allocationTotal =
                RoundMoney(
                    trustedAllocations
                        .Sum(x =>
                            x.AllocatedAmount));


            if (totalReceivedAmount !=
                allocationTotal)
            {
                throw new BusinessException(
                    $"Total Received Amount {totalReceivedAmount:0.00} must match total Invoice allocation amount {allocationTotal:0.00}.");
            }
        }

        #endregion


        #region Outstanding Calculation

        private async Task<decimal>
            CalculateOutstandingAsync(
                Invoice invoice,
                int? excludeCustomerReceiptId)
        {
            var allocatedAmount =
                await _repository
                    .GetFinalizedAllocatedAmountAsync(
                        invoice.Id,
                        excludeCustomerReceiptId);


            var outstanding =
                RoundMoney(
                    invoice.GrandTotal -
                    allocatedAmount);


            return outstanding < 0
                ? 0
                : outstanding;
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
                    "Customer is required.");
            }


            var customer =
                await _repository
                    .GetCustomerForReceiptAsync(
                        customerId);


            if (customer == null)
            {
                throw new BusinessException(
                    "Selected Customer is not available.");
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
                    .GetCompanyForReceiptAsync();


            if (company == null)
            {
                throw new BusinessException(
                    "Active Company / Workshop information is required.");
            }


            return company;
        }

        #endregion


        #region Header Normalization

        private static void NormalizeHeader(
            CustomerReceipt customerReceipt)
        {
            customerReceipt.ReceiptDate =
                customerReceipt
                    .ReceiptDate
                    .Date;


            customerReceipt.ReferenceNumber =
                NormalizeOptional(
                    customerReceipt.ReferenceNumber);


            customerReceipt.ChequeNumber =
                NormalizeOptional(
                    customerReceipt.ChequeNumber);


            customerReceipt.BankName =
                NormalizeOptional(
                    customerReceipt.BankName);


            customerReceipt.Remarks =
                NormalizeOptional(
                    customerReceipt.Remarks);


            customerReceipt.TotalReceivedAmount =
                RoundMoney(
                    customerReceipt.TotalReceivedAmount);


            NormalizePaymentModeFields(
                customerReceipt);
        }

        #endregion


        #region Header Validation

        private static void ValidateHeader(
            CustomerReceipt customerReceipt)
        {
            if (customerReceipt.CustomerId <= 0)
            {
                throw new BusinessException(
                    "Customer is required.");
            }


            if (customerReceipt.ReceiptDate ==
                DateTime.MinValue)
            {
                throw new BusinessException(
                    "Receipt Date is required.");
            }


            if (!Enum.IsDefined(
                typeof(PaymentMode),
                customerReceipt.PaymentMode))
            {
                throw new BusinessException(
                    "Valid Payment Mode is required.");
            }


            if (customerReceipt.TotalReceivedAmount <= 0)
            {
                throw new BusinessException(
                    "Total Received Amount must be greater than zero.");
            }


            #region Payment Mode Validation

            switch (customerReceipt.PaymentMode)
            {
                case PaymentMode.Cash:
                    break;


                case PaymentMode.Cheque:

                    if (string.IsNullOrWhiteSpace(
                        customerReceipt.ChequeNumber))
                    {
                        throw new BusinessException(
                            "Cheque Number is required for Cheque payment.");
                    }


                    if (!customerReceipt.ChequeDate.HasValue)
                    {
                        throw new BusinessException(
                            "Cheque Date is required for Cheque payment.");
                    }


                    if (string.IsNullOrWhiteSpace(
                        customerReceipt.BankName))
                    {
                        throw new BusinessException(
                            "Bank Name is required for Cheque payment.");
                    }

                    break;


                case PaymentMode.NEFT:
                case PaymentMode.RTGS:
                case PaymentMode.IMPS:
                case PaymentMode.UPI:
                case PaymentMode.BankTransfer:

                    if (string.IsNullOrWhiteSpace(
                        customerReceipt.ReferenceNumber))
                    {
                        throw new BusinessException(
                            "Transaction / Reference Number is required for the selected Payment Mode.");
                    }

                    break;


                case PaymentMode.Other:
                    break;


                default:

                    throw new BusinessException(
                        "Valid Payment Mode is required.");
            }

            #endregion


            #region Length Validation

            if (customerReceipt.ReferenceNumber?.Length >
                100)
            {
                throw new BusinessException(
                    "Transaction / Reference Number cannot exceed 100 characters.");
            }


            if (customerReceipt.ChequeNumber?.Length >
                50)
            {
                throw new BusinessException(
                    "Cheque Number cannot exceed 50 characters.");
            }


            if (customerReceipt.BankName?.Length >
                200)
            {
                throw new BusinessException(
                    "Bank Name cannot exceed 200 characters.");
            }


            if (customerReceipt.Remarks?.Length >
                1000)
            {
                throw new BusinessException(
                    "Remarks cannot exceed 1000 characters.");
            }

            #endregion
        }

        #endregion


        #region Payment Mode Normalization

        private static void NormalizePaymentModeFields(
            CustomerReceipt customerReceipt)
        {
            switch (customerReceipt.PaymentMode)
            {
                case PaymentMode.Cash:

                    customerReceipt.ReferenceNumber =
                        null;

                    customerReceipt.ChequeNumber =
                        null;

                    customerReceipt.ChequeDate =
                        null;

                    customerReceipt.BankName =
                        null;

                    break;


                case PaymentMode.Cheque:

                    customerReceipt.ReferenceNumber =
                        null;

                    break;


                case PaymentMode.NEFT:
                case PaymentMode.RTGS:
                case PaymentMode.IMPS:
                case PaymentMode.UPI:
                case PaymentMode.BankTransfer:

                    customerReceipt.ChequeNumber =
                        null;

                    customerReceipt.ChequeDate =
                        null;

                    break;


                case PaymentMode.Other:

                    customerReceipt.ChequeNumber =
                        null;

                    customerReceipt.ChequeDate =
                        null;

                    break;
            }
        }

        #endregion


        #region Receipt Code Generation

        private async Task<string>
            GenerateReceiptCodeAsync(
                DateTime receiptDate)
        {
            var financialYear =
                GetFinancialYear(
                    receiptDate);


            var prefix =
                $"AI/CR/{financialYear}/";


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

                actualType == typeof(string)

                ||

                actualType == typeof(bool)

                ||

                actualType == typeof(byte)

                ||

                actualType == typeof(short)

                ||

                actualType == typeof(int)

                ||

                actualType == typeof(long)

                ||

                actualType == typeof(float)

                ||

                actualType == typeof(double)

                ||

                actualType == typeof(decimal)

                ||

                actualType == typeof(DateTime)

                ||

                actualType == typeof(DateOnly)

                ||

                actualType == typeof(Guid);
        }

        #endregion


        #region Reflection Helper

        private static string?
            GetStringPropertyValue(
                object source,
                params string[] propertyNames)
        {
            foreach (var propertyName
                in propertyNames)
            {
                var property =
                    source
                        .GetType()
                        .GetProperty(
                            propertyName,
                            BindingFlags.Public |
                            BindingFlags.Instance |
                            BindingFlags.IgnoreCase);


                if (property == null ||
                    !property.CanRead)
                {
                    continue;
                }


                var value =
                    property.GetValue(
                        source);


                if (value == null)
                {
                    continue;
                }


                var text =
                    value.ToString();


                if (!string.IsNullOrWhiteSpace(
                    text))
                {
                    return text.Trim();
                }
            }


            return null;
        }

        #endregion


        #region Common Helpers

        private static decimal RoundMoney(
            decimal amount)
        {
            return Math.Round(
                amount,
                2,
                MidpointRounding.AwayFromZero);
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


        private static void NormalizePagination(
            ref int pageNumber,
            ref int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber =
                    1;
            }


            if (pageSize != 10 &&
                pageSize != 25 &&
                pageSize != 50)
            {
                pageSize =
                    10;
            }
        }

        #endregion
    }
}