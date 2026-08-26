/*
============================================================
File: DeliveryChallanService.cs

Purpose:
Implements Delivery Challan / Dispatch business rules.

Responsibilities:
- Read and search Delivery Challans.
- Load Finalized PDI Reports available for dispatch.
- Calculate remaining dispatch quantity.
- Prepare Delivery Challan Draft from Finalized PDI.
- Rebuild trusted PDI snapshots.
- Auto-load Customer Master information.
- Store extensible Customer Master JSON snapshot.
- Auto-load Company / Workshop Master information.
- Store extensible Company Master JSON snapshot.
- Preserve editable Customer delivery address.
- Preserve manually entered Product ID.
- Preserve manually entered HSN No.
- Preserve manually entered L.P.G. No.
- Validate Dispatch Quantity.
- Support multiple Challan Items.
- Enforce same Customer across one Challan.
- Create and update Draft Challans.
- Finalize and lock Challans.
- Soft-delete and restore Draft Challans.
- Generate sequential Challan Code.
- Generate Finalized Delivery Challan PDF.

Important:
- Finalized PDI is the trusted dispatch source.
- Browser-posted PDI snapshot values are NOT trusted.
- Customer and Company Master snapshots are captured when
  the Challan is created.
- Customer delivery address is separately editable.
- Master snapshots are not refreshed during Edit/Finalize,
  preserving historical document data.
- Scalar master properties are serialized generically so
  future scalar fields added to Customer / Company Master
  automatically become part of new JSON snapshots.
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
    public class DeliveryChallanService
        : IDeliveryChallanService
    {
        #region Fields

        private readonly IDeliveryChallanRepository
            _repository;

        private readonly IDeliveryChallanPdfGenerator
            _pdfGenerator;

        #endregion


        #region Constructor

        public DeliveryChallanService(
            IDeliveryChallanRepository repository,
            IDeliveryChallanPdfGenerator pdfGenerator)
        {
            _repository =
                repository;

            _pdfGenerator =
                pdfGenerator;
        }

        #endregion


        #region Read Operations

        public async Task<DeliveryChallan?>
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


        public async Task<PagedResult<DeliveryChallan>>
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


        #region Finalized PDI Source

        public async Task<List<PreDispatchInspection>>
            GetFinalizedPdisForDispatchAsync()
        {
            var pdiReports =
                await _repository
                    .GetFinalizedPdisForDispatchAsync();

            var availablePdis =
                new List<PreDispatchInspection>();

            foreach (var pdi
                in pdiReports)
            {
                var allocatedQuantity =
                    await _repository
                        .GetAllocatedDispatchQuantityAsync(
                            pdi.Id);

                var remainingQuantity =
                    pdi.AcceptedQuantity -
                    allocatedQuantity;

                if (remainingQuantity <= 0)
                {
                    continue;
                }

                availablePdis.Add(
                    pdi);
            }

            return availablePdis;
        }


        public async Task<PreDispatchInspection?>
            GetFinalizedPdiForDispatchAsync(
                int preDispatchInspectionId)
        {
            if (preDispatchInspectionId <= 0)
            {
                return null;
            }

            return await _repository
                .GetFinalizedPdiForDispatchAsync(
                    preDispatchInspectionId);
        }


        public async Task<decimal>
            GetRemainingDispatchQuantityAsync(
                int preDispatchInspectionId,
                int? excludeDeliveryChallanId = null)
        {
            var pdi =
                await _repository
                    .GetFinalizedPdiForDispatchAsync(
                        preDispatchInspectionId);

            if (pdi == null)
            {
                throw new BusinessException(
                    "Finalized PDI Report is not available for dispatch.");
            }

            var allocatedQuantity =
                await _repository
                    .GetAllocatedDispatchQuantityAsync(
                        preDispatchInspectionId,
                        excludeDeliveryChallanId);

            var remainingQuantity =
                pdi.AcceptedQuantity -
                allocatedQuantity;

            return remainingQuantity < 0
                ? 0
                : remainingQuantity;
        }

        #endregion


        #region Prepare Draft Source

        public async Task<DeliveryChallan?>
            PrepareDraftAsync(
                int preDispatchInspectionId)
        {
            #region Load Finalized PDI

            var pdi =
                await _repository
                    .GetFinalizedPdiForDispatchAsync(
                        preDispatchInspectionId);

            if (pdi == null)
            {
                return null;
            }

            #endregion


            #region Remaining Quantity

            var remainingQuantity =
                await GetRemainingDispatchQuantityAsync(
                    preDispatchInspectionId);

            if (remainingQuantity <= 0)
            {
                throw new BusinessException(
                    "The complete PDI Accepted Quantity is already allocated to Delivery Challans.");
            }

            #endregion


            #region Prepare Header

            var deliveryChallan =
                new DeliveryChallan
                {
                    ChallanDate =
                        DateTime.Today,

                    Status =
                        DeliveryChallanStatus.Draft,

                    CustomerId =
                        pdi.CustomerId,

                    CustomerName =
                        pdi.CustomerName,

                    IsActive =
                        true,

                    IsDeleted =
                        false
                };

            #endregion


            #region Load Master Information

            await ApplyInitialMasterSnapshotsAsync(
                deliveryChallan,
                pdi.CustomerId);

            #endregion


            #region Prepare First Dispatch Line

            deliveryChallan.Items.Add(
                CreateTrustedItem(
                    pdi,
                    sequenceNumber: 1,
                    dispatchQuantity:
                        remainingQuantity,
                    productReference:
                        null,
                    hsnNumber:
                        null));

            #endregion

            return deliveryChallan;
        }

        #endregion


        #region Create

        public async Task<DeliveryChallan>
            CreateAsync(
                DeliveryChallan deliveryChallan)
        {
            #region Basic Validation

            if (deliveryChallan == null)
            {
                throw new BusinessException(
                    "Delivery Challan information is required.");
            }

            if (
                deliveryChallan.Items == null ||
                deliveryChallan.Items.Count == 0
            )
            {
                throw new BusinessException(
                    "At least one dispatch item is required.");
            }

            #endregion


            #region Prepare Header

            var prepared =
                new DeliveryChallan
                {
                    ChallanDate =
                        deliveryChallan.ChallanDate,

                    Status =
                        DeliveryChallanStatus.Draft,

                    LpgNumber =
                        NormalizeLpgNumber(
                            deliveryChallan.LpgNumber),

                    /*
                     * Address is user editable.
                     * Preserve submitted values.
                     */
                    CustomerAddressLine1 =
                        NormalizeOptional(
                            deliveryChallan
                                .CustomerAddressLine1),

                    CustomerAddressLine2 =
                        NormalizeOptional(
                            deliveryChallan
                                .CustomerAddressLine2),

                    CustomerCity =
                        NormalizeOptional(
                            deliveryChallan
                                .CustomerCity),

                    CustomerDistrict =
                        NormalizeOptional(
                            deliveryChallan
                                .CustomerDistrict),

                    CustomerState =
                        NormalizeOptional(
                            deliveryChallan
                                .CustomerState),

                    CustomerPincode =
                        NormalizeOptional(
                            deliveryChallan
                                .CustomerPincode),

                    CustomerCountry =
                        NormalizeOptional(
                            deliveryChallan
                                .CustomerCountry),

                    TransporterName =
                        NormalizeOptional(
                            deliveryChallan.TransporterName),

                    VehicleNumber =
                        NormalizeOptional(
                            deliveryChallan.VehicleNumber),

                    TransportReference =
                        NormalizeOptional(
                            deliveryChallan.TransportReference),

                    DispatchFrom =
                        NormalizeOptional(
                            deliveryChallan.DispatchFrom),

                    Destination =
                        NormalizeOptional(
                            deliveryChallan.Destination),

                    Remarks =
                        NormalizeOptional(
                            deliveryChallan.Remarks),

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


            #region Header Validation

            ValidateHeader(
                prepared);

            #endregion


            #region Validate Duplicate PDI

            ValidateDuplicatePdis(
                deliveryChallan.Items);

            #endregion


            #region Prepare Trusted Items

            var sequenceNumber =
                1;

            int? challanCustomerId =
                null;

            string? challanCustomerName =
                null;

            foreach (var submittedItem
                in deliveryChallan.Items)
            {
                if (submittedItem.PreDispatchInspectionId <= 0)
                {
                    throw new BusinessException(
                        $"PDI Report is required for Dispatch Line {sequenceNumber}.");
                }

                var pdi =
                    await LoadAndValidatePdiAsync(
                        submittedItem
                            .PreDispatchInspectionId);

                ValidateAndSetCustomer(
                    pdi,
                    ref challanCustomerId,
                    ref challanCustomerName);

                var remainingQuantity =
                    await GetRemainingDispatchQuantityAsync(
                        pdi.Id);

                ValidateDispatchQuantity(
                    submittedItem.DispatchQuantity,
                    remainingQuantity,
                    pdi);

                prepared.Items.Add(
                    CreateTrustedItem(
                        pdi,
                        sequenceNumber,
                        submittedItem.DispatchQuantity,
                        submittedItem.ProductReference,
                        submittedItem.HsnNumber));

                sequenceNumber++;
            }

            #endregion


            #region Apply Trusted Customer

            prepared.CustomerId =
                challanCustomerId
                ?? throw new BusinessException(
                    "Customer information could not be determined.");

            prepared.CustomerName =
                challanCustomerName
                ?? string.Empty;

            #endregion


            #region Capture Master Snapshots

            await ApplyInitialMasterSnapshotsAsync(
                prepared,
                prepared.CustomerId,
                preserveSubmittedAddress:
                    true);

            #endregion


            #region Validate Prepared Header

            ValidateHeader(
                prepared);

            #endregion


            #region Generate Challan Code

            prepared.Code =
                await GenerateCodeAsync();

            #endregion


            #region Save

            await _repository
                .AddAsync(
                    prepared);

            #endregion

            return prepared;
        }

        #endregion


        #region Update Draft

        public async Task<DeliveryChallan>
            UpdateAsync(
                DeliveryChallan deliveryChallan)
        {
            #region Basic Validation

            if (
                deliveryChallan == null ||
                deliveryChallan.Id <= 0
            )
            {
                throw new BusinessException(
                    "Invalid Delivery Challan.");
            }

            if (
                deliveryChallan.Items == null ||
                deliveryChallan.Items.Count == 0
            )
            {
                throw new BusinessException(
                    "At least one dispatch item is required.");
            }

            #endregion


            #region Load Existing Challan

            var existing =
                await _repository
                    .GetForUpdateAsync(
                        deliveryChallan.Id);

            if (existing == null)
            {
                throw new BusinessException(
                    "Delivery Challan not found.");
            }

            if (
                existing.Status !=
                DeliveryChallanStatus.Draft
            )
            {
                throw new BusinessException(
                    "Only Draft Delivery Challan can be edited.");
            }

            #endregion


            #region Duplicate PDI Validation

            ValidateDuplicatePdis(
                deliveryChallan.Items);

            #endregion


            #region Update Header

            existing.ChallanDate =
                deliveryChallan.ChallanDate;

            existing.LpgNumber =
                NormalizeLpgNumber(
                    deliveryChallan.LpgNumber);


            /*
             * Customer address is intentionally editable.
             *
             * CustomerSnapshotJson and CompanySnapshotJson
             * are intentionally NOT refreshed here.
             */

            existing.CustomerAddressLine1 =
                NormalizeOptional(
                    deliveryChallan
                        .CustomerAddressLine1);

            existing.CustomerAddressLine2 =
                NormalizeOptional(
                    deliveryChallan
                        .CustomerAddressLine2);

            existing.CustomerCity =
                NormalizeOptional(
                    deliveryChallan
                        .CustomerCity);

            existing.CustomerDistrict =
                NormalizeOptional(
                    deliveryChallan
                        .CustomerDistrict);

            existing.CustomerState =
                NormalizeOptional(
                    deliveryChallan
                        .CustomerState);

            existing.CustomerPincode =
                NormalizeOptional(
                    deliveryChallan
                        .CustomerPincode);

            existing.CustomerCountry =
                NormalizeOptional(
                    deliveryChallan
                        .CustomerCountry);


            existing.TransporterName =
                NormalizeOptional(
                    deliveryChallan.TransporterName);

            existing.VehicleNumber =
                NormalizeOptional(
                    deliveryChallan.VehicleNumber);

            existing.TransportReference =
                NormalizeOptional(
                    deliveryChallan.TransportReference);

            existing.DispatchFrom =
                NormalizeOptional(
                    deliveryChallan.DispatchFrom);

            existing.Destination =
                NormalizeOptional(
                    deliveryChallan.Destination);

            existing.Remarks =
                NormalizeOptional(
                    deliveryChallan.Remarks);

            existing.ModifiedOn =
                DateTime.UtcNow;

            existing.ModifiedBy =
                "System";

            #endregion


            #region Header Validation

            ValidateHeader(
                existing);

            #endregion


            #region Prepare Trusted Submitted Items

            var preparedItems =
                new List<PreparedChallanItem>();

            int? challanCustomerId =
                null;

            string? challanCustomerName =
                null;

            var sequenceNumber =
                1;

            foreach (var submittedItem
                in deliveryChallan.Items)
            {
                if (submittedItem.PreDispatchInspectionId <= 0)
                {
                    throw new BusinessException(
                        $"PDI Report is required for Dispatch Line {sequenceNumber}.");
                }

                var pdi =
                    await LoadAndValidatePdiAsync(
                        submittedItem
                            .PreDispatchInspectionId);

                ValidateAndSetCustomer(
                    pdi,
                    ref challanCustomerId,
                    ref challanCustomerName);

                var remainingQuantity =
                    await GetRemainingDispatchQuantityAsync(
                        pdi.Id,
                        existing.Id);

                ValidateDispatchQuantity(
                    submittedItem.DispatchQuantity,
                    remainingQuantity,
                    pdi);

                preparedItems.Add(
                    new PreparedChallanItem
                    {
                        SubmittedItem =
                            submittedItem,

                        Pdi =
                            pdi,

                        SequenceNumber =
                            sequenceNumber
                    });

                sequenceNumber++;
            }

            #endregion


            #region Validate Customer Consistency

            if (
                challanCustomerId.HasValue &&
                existing.CustomerId !=
                challanCustomerId.Value
            )
            {
                throw new BusinessException(
                    "Customer of an existing Delivery Challan cannot be changed.");
            }

            #endregion


            #region Preserve Customer Snapshot Identity

            /*
             * Historical CustomerName and JSON snapshot
             * remain unchanged during Draft edits.
             *
             * PDI lines must continue belonging to the
             * original Challan Customer.
             */

            #endregion


            #region Synchronize Items

            SynchronizeItems(
                existing,
                preparedItems);

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

        public async Task<DeliveryChallan>
            FinalizeAsync(
                int id)
        {
            #region Load Challan

            var deliveryChallan =
                await _repository
                    .GetForUpdateAsync(
                        id);

            if (deliveryChallan == null)
            {
                throw new BusinessException(
                    "Delivery Challan not found.");
            }

            if (
                deliveryChallan.Status !=
                DeliveryChallanStatus.Draft
            )
            {
                throw new BusinessException(
                    "Only Draft Delivery Challan can be finalized.");
            }

            #endregion


            #region Header Validation

            ValidateHeader(
                deliveryChallan);

            #endregion


            #region Master Snapshot Validation

            if (string.IsNullOrWhiteSpace(
                deliveryChallan
                    .CustomerSnapshotJson))
            {
                throw new BusinessException(
                    "Customer Master snapshot is missing from this Delivery Challan.");
            }

            if (string.IsNullOrWhiteSpace(
                deliveryChallan
                    .CompanySnapshotJson))
            {
                throw new BusinessException(
                    "Company / Workshop snapshot is missing from this Delivery Challan.");
            }

            #endregion


            #region Active Items

            var activeItems =
                deliveryChallan.Items
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();

            if (activeItems.Count == 0)
            {
                throw new BusinessException(
                    "At least one dispatch item is required before Finalization.");
            }

            #endregion


            #region Duplicate PDI Validation

            ValidateDuplicatePdis(
                activeItems);

            #endregion


            #region Final Quantity Validation

            int? customerId =
                null;

            string? customerName =
                null;

            foreach (var item
                in activeItems)
            {
                var pdi =
                    await LoadAndValidatePdiAsync(
                        item.PreDispatchInspectionId);

                ValidateAndSetCustomer(
                    pdi,
                    ref customerId,
                    ref customerName);

                var remainingQuantity =
                    await GetRemainingDispatchQuantityAsync(
                        pdi.Id,
                        deliveryChallan.Id);

                ValidateDispatchQuantity(
                    item.DispatchQuantity,
                    remainingQuantity,
                    pdi);

                /*
                 * Refresh only trusted PDI line snapshot.
                 *
                 * ProductReference and HsnNumber remain manual.
                 *
                 * Customer / Company master snapshots are NOT
                 * refreshed here.
                 */
                ApplyTrustedItemSnapshot(
                    item,
                    pdi);
            }

            #endregion


            #region Customer Consistency

            if (
                customerId.HasValue &&
                customerId.Value !=
                deliveryChallan.CustomerId
            )
            {
                throw new BusinessException(
                    "Dispatch items do not belong to the Delivery Challan Customer.");
            }

            #endregion


            #region Finalize

            deliveryChallan.Status =
                DeliveryChallanStatus.Finalized;

            deliveryChallan.FinalizedOn =
                DateTime.UtcNow;

            deliveryChallan.FinalizedBy =
                "System";

            deliveryChallan.ModifiedOn =
                DateTime.UtcNow;

            deliveryChallan.ModifiedBy =
                "System";

            #endregion


            #region Save

            await _repository
                .UpdateAsync(
                    deliveryChallan);

            #endregion

            return deliveryChallan;
        }

        #endregion


        #region PDF

        public async Task<byte[]>
            GeneratePdfAsync(
                int id)
        {
            var deliveryChallan =
                await _repository
                    .GetByIdAsync(
                        id);

            if (deliveryChallan == null)
            {
                throw new BusinessException(
                    "Delivery Challan not found.");
            }

            if (
                deliveryChallan.Status !=
                DeliveryChallanStatus.Finalized
            )
            {
                throw new BusinessException(
                    "Only Finalized Delivery Challan can generate PDF.");
            }

            if (
                deliveryChallan.Items == null ||
                !deliveryChallan.Items.Any(x =>
                    !x.IsDeleted &&
                    x.IsActive)
            )
            {
                throw new BusinessException(
                    "Delivery Challan has no dispatch items.");
            }

            return _pdfGenerator
                .Generate(
                    deliveryChallan);
        }

        #endregion


        #region Delete

        public async Task DeleteAsync(
            int id)
        {
            var deliveryChallan =
                await _repository
                    .GetForUpdateAsync(
                        id);

            if (deliveryChallan == null)
            {
                throw new BusinessException(
                    "Delivery Challan not found.");
            }

            if (
                deliveryChallan.Status !=
                DeliveryChallanStatus.Draft
            )
            {
                throw new BusinessException(
                    "Finalized Delivery Challan cannot be deleted.");
            }

            deliveryChallan.IsDeleted =
                true;

            deliveryChallan.IsActive =
                false;

            deliveryChallan.ModifiedOn =
                DateTime.UtcNow;

            deliveryChallan.ModifiedBy =
                "System";

            await _repository
                .UpdateAsync(
                    deliveryChallan);
        }

        #endregion


        #region Deleted Challans

        public async Task<List<DeliveryChallan>>
            GetDeletedAsync()
        {
            return await _repository
                .GetDeletedAsync();
        }


        public async Task RestoreAsync(
            int id)
        {
            var deliveryChallan =
                await _repository
                    .GetDeletedForUpdateAsync(
                        id);

            if (deliveryChallan == null)
            {
                throw new BusinessException(
                    "Deleted Delivery Challan not found.");
            }

            if (
                deliveryChallan.Status !=
                DeliveryChallanStatus.Draft
            )
            {
                throw new BusinessException(
                    "Only Draft Delivery Challan can be restored.");
            }

            var activeItems =
                deliveryChallan.Items
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .OrderBy(x =>
                        x.SequenceNumber)
                    .ToList();

            if (activeItems.Count == 0)
            {
                throw new BusinessException(
                    "Delivery Challan has no dispatch items to restore.");
            }

            foreach (var item
                in activeItems)
            {
                var pdi =
                    await LoadAndValidatePdiAsync(
                        item.PreDispatchInspectionId);

                var remainingQuantity =
                    await GetRemainingDispatchQuantityAsync(
                        pdi.Id);

                if (
                    item.DispatchQuantity >
                    remainingQuantity
                )
                {
                    throw new BusinessException(
                        $"Delivery Challan cannot be restored. " +
                        $"PDI {pdi.Code} currently has only " +
                        $"{remainingQuantity:0.###} {pdi.UnitName} " +
                        $"available for dispatch.");
                }
            }

            deliveryChallan.IsDeleted =
                false;

            deliveryChallan.IsActive =
                true;

            deliveryChallan.ModifiedOn =
                DateTime.UtcNow;

            deliveryChallan.ModifiedBy =
                "System";

            await _repository
                .UpdateAsync(
                    deliveryChallan);
        }

        #endregion


        #region Master Snapshot Loading

        private async Task ApplyInitialMasterSnapshotsAsync(
            DeliveryChallan deliveryChallan,
            int customerId,
            bool preserveSubmittedAddress = false)
        {
            #region Customer Master

            var customer =
                await _repository
                    .GetCustomerForDispatchAsync(
                        customerId);

            if (customer == null)
            {
                throw new BusinessException(
                    "Active Customer Master record was not found.");
            }

            deliveryChallan.CustomerId =
                customer.Id;

            deliveryChallan.CustomerName =
                customer.CustomerName;

            deliveryChallan.CustomerSnapshotJson =
                SerializeScalarSnapshot(
                    customer);

            #endregion


            #region Customer Address

            if (!preserveSubmittedAddress)
            {
                ApplyCustomerMasterAddress(
                    deliveryChallan,
                    customer);
            }
            else
            {
                /*
                 * Create form normally sends the auto-loaded
                 * editable address.
                 *
                 * If all address fields are empty because the
                 * form has not yet been upgraded / populated,
                 * fall back to Customer Master so a usable
                 * address snapshot is still created.
                 */

                if (!HasAnyCustomerAddress(
                    deliveryChallan))
                {
                    ApplyCustomerMasterAddress(
                        deliveryChallan,
                        customer);
                }
            }

            #endregion


            #region Company Workshop Master

            var company =
                await _repository
                    .GetCompanyForDispatchAsync();

            if (company == null)
            {
                throw new BusinessException(
                    "Active Company / Workshop Master record was not found.");
            }

            deliveryChallan.CompanyId =
                company.CompanyId;

            deliveryChallan.CompanyName =
                company.CompanyName;

            deliveryChallan.CompanySnapshotJson =
                SerializeScalarSnapshot(
                    company);

            #endregion
        }

        #endregion


        #region Customer Address Mapping

        private static void ApplyCustomerMasterAddress(
            DeliveryChallan deliveryChallan,
            Customer customer)
        {
            deliveryChallan.CustomerAddressLine1 =
                NormalizeOptional(
                    customer.AddressLine1);

            deliveryChallan.CustomerAddressLine2 =
                NormalizeOptional(
                    customer.AddressLine2);

            deliveryChallan.CustomerCity =
                NormalizeOptional(
                    customer.City);

            deliveryChallan.CustomerDistrict =
                NormalizeOptional(
                    customer.District);

            deliveryChallan.CustomerState =
                NormalizeOptional(
                    customer.State);

            deliveryChallan.CustomerPincode =
                NormalizeOptional(
                    customer.Pincode);

            deliveryChallan.CustomerCountry =
                NormalizeOptional(
                    customer.Country);
        }


        private static bool HasAnyCustomerAddress(
            DeliveryChallan deliveryChallan)
        {
            return
                !string.IsNullOrWhiteSpace(
                    deliveryChallan.CustomerAddressLine1) ||

                !string.IsNullOrWhiteSpace(
                    deliveryChallan.CustomerAddressLine2) ||

                !string.IsNullOrWhiteSpace(
                    deliveryChallan.CustomerCity) ||

                !string.IsNullOrWhiteSpace(
                    deliveryChallan.CustomerDistrict) ||

                !string.IsNullOrWhiteSpace(
                    deliveryChallan.CustomerState) ||

                !string.IsNullOrWhiteSpace(
                    deliveryChallan.CustomerPincode) ||

                !string.IsNullOrWhiteSpace(
                    deliveryChallan.CustomerCountry);
        }

        #endregion


        #region Generic Master Snapshot Serialization

        private static string SerializeScalarSnapshot(
            object master)
        {
            /*
             * Only scalar/simple public properties are captured.
             *
             * Benefits:
             * - New scalar fields added later are automatically
             *   included in new snapshots.
             * - Navigation properties / collections are excluded.
             * - Avoids circular-reference problems.
             */

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

            return JsonSerializer.Serialize(
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
                actualType.IsEnum ||
                actualType == typeof(string) ||
                actualType == typeof(bool) ||
                actualType == typeof(byte) ||
                actualType == typeof(short) ||
                actualType == typeof(int) ||
                actualType == typeof(long) ||
                actualType == typeof(float) ||
                actualType == typeof(double) ||
                actualType == typeof(decimal) ||
                actualType == typeof(DateTime) ||
                actualType == typeof(DateTimeOffset) ||
                actualType == typeof(TimeSpan) ||
                actualType == typeof(Guid);
        }

        #endregion


        #region PDI Validation

        private async Task<PreDispatchInspection>
            LoadAndValidatePdiAsync(
                int preDispatchInspectionId)
        {
            var pdi =
                await _repository
                    .GetFinalizedPdiForDispatchAsync(
                        preDispatchInspectionId);

            if (pdi == null)
            {
                throw new BusinessException(
                    "Selected Finalized PDI Report is not available for dispatch.");
            }

            if (
                pdi.Status !=
                PreDispatchInspectionStatus.Finalized
            )
            {
                throw new BusinessException(
                    $"PDI Report {pdi.Code} is not Finalized.");
            }

            if (pdi.AcceptedQuantity <= 0)
            {
                throw new BusinessException(
                    $"PDI Report {pdi.Code} has no Accepted Quantity available for dispatch.");
            }

            return pdi;
        }

        #endregion


        #region Customer Validation

        private static void ValidateAndSetCustomer(
            PreDispatchInspection pdi,
            ref int? customerId,
            ref string? customerName)
        {
            if (!customerId.HasValue)
            {
                customerId =
                    pdi.CustomerId;

                customerName =
                    pdi.CustomerName;

                return;
            }

            if (
                customerId.Value !=
                pdi.CustomerId
            )
            {
                throw new BusinessException(
                    "All items in one Delivery Challan must belong to the same Customer.");
            }
        }

        #endregion


        #region Dispatch Quantity Validation

        private static void ValidateDispatchQuantity(
            decimal dispatchQuantity,
            decimal remainingQuantity,
            PreDispatchInspection pdi)
        {
            if (dispatchQuantity <= 0)
            {
                throw new BusinessException(
                    $"Dispatch Quantity for PDI {pdi.Code} must be greater than zero.");
            }

            if (
                dispatchQuantity >
                remainingQuantity
            )
            {
                throw new BusinessException(
                    $"Dispatch Quantity for PDI {pdi.Code} " +
                    $"cannot exceed available quantity " +
                    $"{remainingQuantity:0.###} {pdi.UnitName}.");
            }
        }

        #endregion


        #region Duplicate PDI Validation

        private static void ValidateDuplicatePdis(
            IEnumerable<DeliveryChallanItem> items)
        {
            var duplicatePdi =
                items
                    .Where(x =>
                        !x.IsDeleted)
                    .GroupBy(x =>
                        x.PreDispatchInspectionId)
                    .FirstOrDefault(x =>
                        x.Key > 0 &&
                        x.Count() > 1);

            if (duplicatePdi != null)
            {
                throw new BusinessException(
                    "The same PDI Report cannot be added more than once in one Delivery Challan.");
            }
        }

        #endregion


        #region Trusted Item Mapping

        private static DeliveryChallanItem
            CreateTrustedItem(
                PreDispatchInspection pdi,
                int sequenceNumber,
                decimal dispatchQuantity,
                string? productReference,
                string? hsnNumber)
        {
            var item =
                new DeliveryChallanItem
                {
                    SequenceNumber =
                        sequenceNumber,

                    ProductReference =
                        NormalizeProductReference(
                            productReference),

                    HsnNumber =
                        NormalizeHsnNumber(
                            hsnNumber),

                    DispatchQuantity =
                        dispatchQuantity,

                    IsActive =
                        true,

                    IsDeleted =
                        false,

                    CreatedOn =
                        DateTime.UtcNow,

                    CreatedBy =
                        "System"
                };

            ApplyTrustedItemSnapshot(
                item,
                pdi);

            return item;
        }


        private static void ApplyTrustedItemSnapshot(
            DeliveryChallanItem item,
            PreDispatchInspection pdi)
        {
            #region PDI

            item.PreDispatchInspectionId =
                pdi.Id;

            item.PreDispatchInspectionCode =
                pdi.Code;

            item.PdiAcceptedQuantity =
                pdi.AcceptedQuantity;

            #endregion


            #region Production Job

            item.ProductionJobId =
                pdi.ProductionJobId;

            item.ProductionJobCode =
                pdi.ProductionJobCode;

            #endregion


            #region Customer PO

            item.CustomerPurchaseOrderItemId =
                pdi.CustomerPurchaseOrderItemId;

            item.CustomerPurchaseOrderCode =
                pdi.CustomerPurchaseOrderCode;

            item.CustomerPurchaseOrderNumber =
                pdi.CustomerPurchaseOrderNumber;

            item.CustomerItemCode =
                NormalizeOptional(
                    pdi.CustomerItemCode);

            #endregion


            #region Item

            /*
             * ProductReference and HsnNumber are not
             * overwritten here because both are currently
             * manual Challan snapshot values.
             */

            item.ItemId =
                pdi.ItemId;

            item.ItemCode =
                pdi.ItemCode;

            item.ItemName =
                pdi.ItemName;

            item.PartNumber =
                NormalizeOptional(
                    pdi.PartNumber);

            item.UnitName =
                NormalizeOptional(
                    pdi.UnitName);

            #endregion


            #region Customer Drawing

            /*
             * Drawing snapshot remains stored in backend
             * for traceability even though current customer
             * PDF will not display Drawing No. / Revision.
             */

            item.CustomerDrawingId =
                pdi.CustomerDrawingId;

            item.CustomerDrawingNumber =
                NormalizeOptional(
                    pdi.CustomerDrawingNumber);

            item.CustomerDrawingRevision =
                NormalizeOptional(
                    pdi.CustomerDrawingRevision);

            #endregion
        }

        #endregion


        #region Synchronize Items

        private static void SynchronizeItems(
            DeliveryChallan existing,
            List<PreparedChallanItem> preparedItems)
        {
            var existingActiveItems =
                existing.Items
                    .Where(x =>
                        !x.IsDeleted &&
                        x.IsActive)
                    .ToList();

            var submittedExistingIds =
                preparedItems
                    .Where(x =>
                        x.SubmittedItem.Id > 0)
                    .Select(x =>
                        x.SubmittedItem.Id)
                    .ToHashSet();

            var archiveSequence =
                existing.Items
                    .Select(x =>
                        x.SequenceNumber)
                    .DefaultIfEmpty(
                        0)
                    .Max()
                + 1000;

            foreach (var existingItem
                in existingActiveItems)
            {
                if (
                    submittedExistingIds.Contains(
                        existingItem.Id)
                )
                {
                    continue;
                }

                existingItem.SequenceNumber =
                    archiveSequence++;

                existingItem.IsDeleted =
                    true;

                existingItem.IsActive =
                    false;

                existingItem.ModifiedOn =
                    DateTime.UtcNow;

                existingItem.ModifiedBy =
                    "System";
            }

            foreach (var preparedItem
                in preparedItems)
            {
                var submittedItem =
                    preparedItem.SubmittedItem;

                if (submittedItem.Id > 0)
                {
                    var existingItem =
                        existingActiveItems
                            .FirstOrDefault(x =>
                                x.Id ==
                                submittedItem.Id);

                    if (existingItem == null)
                    {
                        throw new BusinessException(
                            "Invalid Delivery Challan Item.");
                    }

                    existingItem.SequenceNumber =
                        preparedItem.SequenceNumber;

                    existingItem.ProductReference =
                        NormalizeProductReference(
                            submittedItem.ProductReference);

                    existingItem.HsnNumber =
                        NormalizeHsnNumber(
                            submittedItem.HsnNumber);

                    existingItem.DispatchQuantity =
                        submittedItem.DispatchQuantity;

                    ApplyTrustedItemSnapshot(
                        existingItem,
                        preparedItem.Pdi);

                    existingItem.ModifiedOn =
                        DateTime.UtcNow;

                    existingItem.ModifiedBy =
                        "System";
                }
                else
                {
                    existing.Items.Add(
                        CreateTrustedItem(
                            preparedItem.Pdi,
                            preparedItem.SequenceNumber,
                            submittedItem.DispatchQuantity,
                            submittedItem.ProductReference,
                            submittedItem.HsnNumber));
                }
            }
        }

        #endregion


        #region Header Validation

        private static void ValidateHeader(
            DeliveryChallan deliveryChallan)
        {
            #region Challan Date

            if (
                deliveryChallan.ChallanDate ==
                default
            )
            {
                throw new BusinessException(
                    "Challan Date is required.");
            }

            #endregion


            #region LPG Number

            ValidateMaximumLength(
                deliveryChallan.LpgNumber,
                100,
                "L.P.G. No.");

            #endregion


            #region Customer Address

            ValidateMaximumLength(
                deliveryChallan.CustomerAddressLine1,
                500,
                "Customer Address Line 1");

            ValidateMaximumLength(
                deliveryChallan.CustomerAddressLine2,
                500,
                "Customer Address Line 2");

            ValidateMaximumLength(
                deliveryChallan.CustomerCity,
                150,
                "Customer City");

            ValidateMaximumLength(
                deliveryChallan.CustomerDistrict,
                150,
                "Customer District");

            ValidateMaximumLength(
                deliveryChallan.CustomerState,
                150,
                "Customer State");

            ValidateMaximumLength(
                deliveryChallan.CustomerPincode,
                20,
                "Customer Pincode");

            ValidateMaximumLength(
                deliveryChallan.CustomerCountry,
                100,
                "Customer Country");

            #endregion


            #region Transport

            ValidateMaximumLength(
                deliveryChallan.TransporterName,
                250,
                "Transporter Name");

            ValidateMaximumLength(
                deliveryChallan.VehicleNumber,
                100,
                "Vehicle Number");

            ValidateMaximumLength(
                deliveryChallan.TransportReference,
                150,
                "Transport Reference");

            ValidateMaximumLength(
                deliveryChallan.DispatchFrom,
                250,
                "Dispatch From");

            ValidateMaximumLength(
                deliveryChallan.Destination,
                250,
                "Destination");

            #endregion


            #region Remarks

            ValidateMaximumLength(
                deliveryChallan.Remarks,
                2000,
                "Remarks");

            #endregion
        }


        private static void ValidateMaximumLength(
            string? value,
            int maximumLength,
            string fieldName)
        {
            if (
                value?.Length >
                maximumLength
            )
            {
                throw new BusinessException(
                    $"{fieldName} cannot exceed {maximumLength} characters.");
            }
        }

        #endregion


        #region Product Reference

        private static string?
            NormalizeProductReference(
                string? value)
        {
            var normalized =
                NormalizeOptional(
                    value);

            if (normalized == null)
            {
                return null;
            }

            if (normalized.Length > 100)
            {
                throw new BusinessException(
                    "Product ID cannot exceed 100 characters.");
            }

            return normalized;
        }

        #endregion


        #region HSN Number

        private static string?
            NormalizeHsnNumber(
                string? value)
        {
            var normalized =
                NormalizeOptional(
                    value);

            if (normalized == null)
            {
                return null;
            }

            if (normalized.Length > 50)
            {
                throw new BusinessException(
                    "HSN No. cannot exceed 50 characters.");
            }

            return normalized;
        }

        #endregion


        #region LPG Number

        private static string?
            NormalizeLpgNumber(
                string? value)
        {
            var normalized =
                NormalizeOptional(
                    value);

            if (normalized == null)
            {
                return null;
            }

            if (normalized.Length > 100)
            {
                throw new BusinessException(
                    "L.P.G. No. cannot exceed 100 characters.");
            }

            return normalized;
        }

        #endregion


        #region Challan Code

        private async Task<string>
            GenerateCodeAsync()
        {
            var today =
                DateTime.Today;

            var fiscalYear =
                GetFiscalYear(
                    today);

            var prefix =
                $"AI/DC/{fiscalYear}/";

            var lastCode =
                await _repository
                    .GetLastCodeAsync(
                        prefix);

            if (string.IsNullOrWhiteSpace(
                lastCode))
            {
                return
                    $"{prefix}00001";
            }

            var numberPart =
                lastCode.Substring(
                    prefix.Length);

            if (
                !int.TryParse(
                    numberPart,
                    out var lastNumber)
            )
            {
                throw new BusinessException(
                    "Unable to generate Delivery Challan Code.");
            }

            return
                $"{prefix}{lastNumber + 1:00000}";
        }


        private static string GetFiscalYear(
            DateTime date)
        {
            var startYear =
                date.Month >= 4
                    ? date.Year
                    : date.Year - 1;

            var endYear =
                startYear + 1;

            return
                $"{startYear % 100:00}-{endYear % 100:00}";
        }

        #endregion


        #region Normalization

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
                pageSize != 10 &&
                pageSize != 25 &&
                pageSize != 50
            )
            {
                pageSize =
                    10;
            }
        }

        #endregion


        #region Internal Prepared Item

        private sealed class PreparedChallanItem
        {
            public DeliveryChallanItem SubmittedItem
            {
                get;
                set;
            } = null!;


            public PreDispatchInspection Pdi
            {
                get;
                set;
            } = null!;


            public int SequenceNumber
            {
                get;
                set;
            }
        }

        #endregion
    }
}