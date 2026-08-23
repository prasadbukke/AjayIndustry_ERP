/*
==============================================================

File : CustomerDrawingService.cs

Purpose :
Manages Customer Drawing and Customer Drawing
Revision workflow.

Final Business Rules :
- One Customer + One Item = One Drawing Number.
- Customer is permanent after Drawing creation.
- Item is permanent after Drawing creation.
- Drawing Number is permanent after Drawing creation.
- Every row represents one revision.
- First Revision is automatically RV-01.
- Revision Numbers are system generated.
- Revision Numbers are never reused.
- Only one revision can be Current.
- Previous revisions can be reactivated.
- Inactive revisions can be soft deleted.
- Complete Customer Drawing can be soft deleted/restored.
- Drawing files are preserved for revision history.
- Same Item may have different Drawings for
  different Customers.
- Same Drawing Number may exist for different Customers.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using System.Text.RegularExpressions;

namespace AjayIndustriesERP.Application.Services
{
    public class CustomerDrawingService :
        ICustomerDrawingService
    {
        #region Fields

        private readonly ICustomerDrawingRepository
            _customerDrawingRepository;

        private readonly ICustomerRepository
            _customerRepository;

        private readonly IItemRepository
            _itemRepository;

        #endregion


        #region Constructor

        public CustomerDrawingService(
            ICustomerDrawingRepository customerDrawingRepository,
            ICustomerRepository customerRepository,
            IItemRepository itemRepository)
        {
            _customerDrawingRepository =
                customerDrawingRepository;

            _customerRepository =
                customerRepository;

            _itemRepository =
                itemRepository;
        }

        #endregion


        #region Read

        public async Task<List<CustomerDrawing>>
            GetAllAsync()
        {
            return await _customerDrawingRepository
                .GetAllAsync();
        }


        public async Task<CustomerDrawing?>
            GetByIdAsync(
                int customerDrawingId)
        {
            return await _customerDrawingRepository
                .GetByIdAsync(
                    customerDrawingId);
        }


        public async Task<CustomerDrawing?>
            GetByCustomerAndItemAsync(
                int customerId,
                int itemId)
        {
            if (customerId <= 0 ||
                itemId <= 0)
            {
                return null;
            }


            return await _customerDrawingRepository
                .GetByCustomerAndItemAsync(
                    customerId,
                    itemId);
        }


        public async Task<List<CustomerDrawing>>
            GetRevisionHistoryAsync(
                int customerDrawingId)
        {
            var drawing =
                await _customerDrawingRepository
                    .GetByIdAsync(
                        customerDrawingId);


            if (drawing == null)
            {
                return new List<CustomerDrawing>();
            }


            return await _customerDrawingRepository
                .GetRevisionHistoryAsync(
                    drawing.CustomerId,
                    drawing.DrawingNumber);
        }


        public async Task<List<CustomerDrawing>>
            SearchAsync(
                string searchText)
        {
            return await _customerDrawingRepository
                .SearchAsync(
                    searchText);
        }


        public async Task<PagedResult<CustomerDrawing>>
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


            return await _customerDrawingRepository
                .GetPagedAsync(
                    pageNumber,
                    pageSize);
        }


        public async Task<List<CustomerDrawing>>
            GetDeletedDrawingsAsync()
        {
            return await _customerDrawingRepository
                .GetDeletedDrawingsAsync();
        }

        #endregion


        #region Create Customer Drawing

        public async Task CreateAsync(
            CustomerDrawing customerDrawing)
        {
            if (customerDrawing == null)
            {
                throw new BusinessException(
                    "Customer Drawing is required.");
            }


            NormalizeCommonFields(
                customerDrawing);


            ValidateCommonFields(
                customerDrawing);


            await ValidateCustomerAsync(
                customerDrawing.CustomerId);


            await ValidateItemAsync(
                customerDrawing.ItemId);


            /*
             * Drawing Number is permanently reserved
             * within one Customer.
             *
             * Deleted Customer Drawing Numbers cannot
             * be reused for the same Customer.
             *
             * Another Customer may independently use
             * the same Drawing Number.
             */
            if (await _customerDrawingRepository
                .ExistsByDrawingNumberAsync(
                    customerDrawing.CustomerId,
                    customerDrawing.DrawingNumber))
            {
                throw new BusinessException(
                    $"Customer Drawing Number " +
                    $"{customerDrawing.DrawingNumber} " +
                    $"already exists for the selected Customer. " +
                    $"Open the existing Customer Drawing or " +
                    $"restore it if deleted.");
            }


            /*
             * One current Customer Drawing per
             * Customer + Item.
             */
            var existingCustomerItemDrawing =
                await _customerDrawingRepository
                    .GetByCustomerAndItemAsync(
                        customerDrawing.CustomerId,
                        customerDrawing.ItemId);


            if (existingCustomerItemDrawing != null)
            {
                throw new BusinessException(
                    $"Selected Customer and Item already have " +
                    $"Customer Drawing " +
                    $"{existingCustomerItemDrawing.DrawingNumber}. " +
                    $"Open the existing Customer Drawing and " +
                    $"add a new Revision.");
            }


            /*
             * First revision is always RV-01.
             *
             * Any RevisionNumber received from UI
             * is ignored.
             */
            customerDrawing.RevisionNumber =
                FormatRevisionNumber(1);


            NormalizeRevisionFields(
                customerDrawing);


            ValidateRevisionFields(
                customerDrawing);


            customerDrawing.IsActive =
                true;

            customerDrawing.IsDeleted =
                false;

            customerDrawing.CreatedOn =
                DateTime.UtcNow;

            customerDrawing.CreatedBy =
                "System";


            await _customerDrawingRepository
                .AddAsync(
                    customerDrawing);


            await _customerDrawingRepository
                .SaveChangesAsync();
        }

        #endregion


        #region Update Customer Drawing / Add Revision

        public async Task UpdateAsync(
            CustomerDrawing customerDrawing,
            IEnumerable<CustomerDrawing>
                newRevisions)
        {
            if (customerDrawing == null)
            {
                throw new BusinessException(
                    "Customer Drawing is required.");
            }


            var existingDrawing =
                await _customerDrawingRepository
                    .GetByIdAsync(
                        customerDrawing.CustomerDrawingId);


            if (existingDrawing == null)
            {
                throw new BusinessException(
                    "Customer Drawing not found.");
            }


            /*
             * Customer, Item and Drawing Number
             * are permanent after creation.
             *
             * Ignore posted/tampered values.
             */
            var permanentCustomerId =
                existingDrawing.CustomerId;

            var permanentItemId =
                existingDrawing.ItemId;

            var permanentDrawingNumber =
                existingDrawing.DrawingNumber;


            customerDrawing.CustomerId =
                permanentCustomerId;

            customerDrawing.ItemId =
                permanentItemId;

            customerDrawing.DrawingNumber =
                permanentDrawingNumber;


            NormalizeCommonFields(
                customerDrawing);


            ValidateCommonFields(
                customerDrawing);


            await ValidateCustomerAsync(
                permanentCustomerId);


            await ValidateItemAsync(
                permanentItemId);


            var history =
                await _customerDrawingRepository
                    .GetByDrawingNumberForUpdateAsync(
                        permanentCustomerId,
                        permanentDrawingNumber);


            if (history.Count == 0)
            {
                throw new BusinessException(
                    "Customer Drawing revision history not found.");
            }


            var currentRevision =
                history.FirstOrDefault(x =>
                    x.IsActive);


            if (currentRevision == null)
            {
                throw new BusinessException(
                    "Current Customer Drawing revision not found.");
            }


            /*
             * Drawing-level information remains
             * consistent across every revision.
             *
             * Revision-specific fields:
             * - RevisionNumber
             * - FileName
             * - FilePath
             * - Description
             *
             * are NOT overwritten here.
             */
            foreach (var revision
                in history)
            {
                revision.CustomerId =
                    permanentCustomerId;

                revision.ItemId =
                    permanentItemId;

                revision.DrawingNumber =
                    permanentDrawingNumber;

                revision.DrawingName =
                    customerDrawing.DrawingName;

                revision.DrawingType =
                    customerDrawing.DrawingType;

                revision.ModifiedOn =
                    DateTime.UtcNow;

                revision.ModifiedBy =
                    "System";
            }


            var requestedRevisions =
                newRevisions?
                    .ToList()
                ??
                new List<CustomerDrawing>();


            var preparedRevisions =
                await PrepareNewRevisionsAsync(
                    permanentCustomerId,
                    permanentDrawingNumber,
                    customerDrawing,
                    requestedRevisions);


            if (preparedRevisions.Count > 0)
            {
                /*
                 * Previous Current revision becomes
                 * historical revision.
                 */
                currentRevision.IsActive =
                    false;

                currentRevision.ModifiedOn =
                    DateTime.UtcNow;

                currentRevision.ModifiedBy =
                    "System";


                /*
                 * More than one revision can be entered
                 * during one Save.
                 *
                 * Only the final newly-created revision
                 * becomes Current.
                 */
                for (var index = 0;
                     index < preparedRevisions.Count;
                     index++)
                {
                    var revision =
                        preparedRevisions[index];


                    revision.IsActive =
                        index ==
                        preparedRevisions.Count - 1;
                }


                await _customerDrawingRepository
                    .AddRangeAsync(
                        preparedRevisions);
            }


            await _customerDrawingRepository
                .SaveChangesAsync();
        }

        #endregion


        #region Activate Revision

        public async Task ActivateRevisionAsync(
            int customerDrawingId)
        {
            var selectedRevision =
                await _customerDrawingRepository
                    .GetByIdAsync(
                        customerDrawingId);


            if (selectedRevision == null)
            {
                throw new BusinessException(
                    "Customer Drawing revision not found.");
            }


            var history =
                await _customerDrawingRepository
                    .GetByDrawingNumberForUpdateAsync(
                        selectedRevision.CustomerId,
                        selectedRevision.DrawingNumber);


            var targetRevision =
                history.FirstOrDefault(x =>
                    x.CustomerDrawingId ==
                        customerDrawingId);


            if (targetRevision == null)
            {
                throw new BusinessException(
                    "Customer Drawing revision not found.");
            }


            if (targetRevision.IsDeleted)
            {
                throw new BusinessException(
                    "Deleted Customer Drawing revision " +
                    "cannot be activated.");
            }


            if (targetRevision.IsActive)
            {
                throw new BusinessException(
                    $"Revision " +
                    $"{targetRevision.RevisionNumber} " +
                    $"is already Current.");
            }


            var currentRevision =
                history.FirstOrDefault(x =>
                    x.IsActive &&
                    !x.IsDeleted);


            if (currentRevision == null)
            {
                throw new BusinessException(
                    "Current Customer Drawing revision not found.");
            }


            /*
             * Two SaveChanges calls are intentional.
             *
             * Filtered unique indexes allow only one
             * Current revision for:
             *
             * Customer + Drawing Number
             * Customer + Item
             *
             * Therefore:
             *
             * 1. Deactivate current revision.
             * 2. Save.
             * 3. Activate selected historical revision.
             * 4. Save.
             *
             * Both operations stay inside one transaction.
             */
            await _customerDrawingRepository
                .ExecuteInTransactionAsync(
                    async () =>
                    {
                        foreach (var revision
                            in history)
                        {
                            revision.IsActive =
                                false;

                            revision.ModifiedOn =
                                DateTime.UtcNow;

                            revision.ModifiedBy =
                                "System";
                        }


                        await _customerDrawingRepository
                            .SaveChangesAsync();


                        targetRevision.IsActive =
                            true;

                        targetRevision.ModifiedOn =
                            DateTime.UtcNow;

                        targetRevision.ModifiedBy =
                            "System";


                        await _customerDrawingRepository
                            .SaveChangesAsync();
                    });
        }

        #endregion


        #region Delete Revision

        public async Task DeleteRevisionAsync(
            int customerDrawingId)
        {
            var selectedRevision =
                await _customerDrawingRepository
                    .GetByIdAsync(
                        customerDrawingId);


            if (selectedRevision == null)
            {
                throw new BusinessException(
                    "Customer Drawing revision not found.");
            }


            var history =
                await _customerDrawingRepository
                    .GetByDrawingNumberForUpdateAsync(
                        selectedRevision.CustomerId,
                        selectedRevision.DrawingNumber);


            var revision =
                history.FirstOrDefault(x =>
                    x.CustomerDrawingId ==
                        customerDrawingId);


            if (revision == null)
            {
                throw new BusinessException(
                    "Customer Drawing revision not found.");
            }


            if (revision.IsActive)
            {
                throw new BusinessException(
                    "Current revision cannot be deleted. " +
                    "Activate another revision first.");
            }


            revision.IsDeleted =
                true;

            revision.IsActive =
                false;

            revision.ModifiedOn =
                DateTime.UtcNow;

            revision.ModifiedBy =
                "System";


            await _customerDrawingRepository
                .SaveChangesAsync();
        }

        #endregion


        #region Delete Complete Customer Drawing

        public async Task DeleteAsync(
            int customerDrawingId)
        {
            var drawing =
                await _customerDrawingRepository
                    .GetByIdAsync(
                        customerDrawingId);


            if (drawing == null)
            {
                throw new BusinessException(
                    "Customer Drawing not found.");
            }


            var history =
                await _customerDrawingRepository
                    .GetByDrawingNumberForUpdateAsync(
                        drawing.CustomerId,
                        drawing.DrawingNumber);


            if (history.Count == 0)
            {
                throw new BusinessException(
                    "Customer Drawing not found.");
            }


            foreach (var revision
                in history)
            {
                /*
                 * Preserve IsActive.
                 *
                 * This remembers which revision was
                 * Current before complete Drawing deletion.
                 */
                revision.IsDeleted =
                    true;

                revision.ModifiedOn =
                    DateTime.UtcNow;

                revision.ModifiedBy =
                    "System";
            }


            await _customerDrawingRepository
                .SaveChangesAsync();
        }

        #endregion


        #region Restore Customer Drawing

        public async Task RestoreAsync(
            int customerDrawingId)
        {
            var deletedDrawings =
                await _customerDrawingRepository
                    .GetDeletedDrawingsAsync();


            var selectedDrawing =
                deletedDrawings
                    .FirstOrDefault(x =>
                        x.CustomerDrawingId ==
                            customerDrawingId);


            if (selectedDrawing == null)
            {
                throw new BusinessException(
                    "Deleted Customer Drawing not found.");
            }


            var history =
                await _customerDrawingRepository
                    .GetDeletedHistoryForUpdateAsync(
                        selectedDrawing.CustomerId,
                        selectedDrawing.DrawingNumber);


            if (history.Count == 0)
            {
                throw new BusinessException(
                    "Deleted Customer Drawing history not found.");
            }


            await ValidateCustomerAsync(
                selectedDrawing.CustomerId);


            await ValidateItemAsync(
                selectedDrawing.ItemId);


            /*
             * Cannot restore this Drawing if the same
             * Customer + Item currently belongs to
             * another active Customer Drawing.
             */
            var existingCustomerItemDrawing =
                await _customerDrawingRepository
                    .GetByCustomerAndItemAsync(
                        selectedDrawing.CustomerId,
                        selectedDrawing.ItemId);


            if (existingCustomerItemDrawing != null)
            {
                throw new BusinessException(
                    $"Selected Customer and Item already have " +
                    $"active Customer Drawing " +
                    $"{existingCustomerItemDrawing.DrawingNumber}. " +
                    $"Delete that Customer Drawing before restoring " +
                    $"{selectedDrawing.DrawingNumber}.");
            }


            /*
             * Older deleted records may have lost
             * their Current flag.
             *
             * If a Current revision cannot be identified,
             * latest revision becomes Current.
             */
            var currentRevision =
                history.FirstOrDefault(x =>
                    x.IsActive);


            if (currentRevision == null)
            {
                currentRevision =
                    history
                        .OrderByDescending(x =>
                            x.CustomerDrawingId)
                        .First();
            }


            /*
             * Normalize complete history before Restore
             * so exactly one Current revision exists.
             */
            foreach (var revision
                in history)
            {
                revision.IsDeleted =
                    false;

                revision.IsActive =
                    revision.CustomerDrawingId ==
                    currentRevision.CustomerDrawingId;

                revision.ModifiedOn =
                    DateTime.UtcNow;

                revision.ModifiedBy =
                    "System";
            }


            await _customerDrawingRepository
                .SaveChangesAsync();
        }

        #endregion


        #region Prepare New Revisions

        private async Task<List<CustomerDrawing>>
            PrepareNewRevisionsAsync(
                int customerId,
                string drawingNumber,
                CustomerDrawing commonDrawing,
                IReadOnlyCollection<CustomerDrawing>
                    requestedRevisions)
        {
            var prepared =
                new List<CustomerDrawing>();


            if (requestedRevisions == null ||
                requestedRevisions.Count == 0)
            {
                return prepared;
            }


            var nextSequence =
                await GetNextRevisionSequenceAsync(
                    customerId,
                    drawingNumber);


            foreach (var requested
                in requestedRevisions)
            {
                /*
                 * Completely empty dynamic row should
                 * not create a new revision.
                 */
                if (string.IsNullOrWhiteSpace(
                        requested.FileName) &&
                    string.IsNullOrWhiteSpace(
                        requested.FilePath) &&
                    string.IsNullOrWhiteSpace(
                        requested.Description))
                {
                    continue;
                }


                /*
                 * Ignore posted Revision Number.
                 *
                 * Service is the only authority that
                 * generates Revision Numbers.
                 */
                requested.RevisionNumber =
                    FormatRevisionNumber(
                        nextSequence);


                nextSequence++;


                NormalizeRevisionFields(
                    requested);


                ValidateRevisionFields(
                    requested);


                if (await _customerDrawingRepository
                    .ExistsByRevisionAsync(
                        customerId,
                        drawingNumber,
                        requested.RevisionNumber!))
                {
                    throw new BusinessException(
                        $"Revision " +
                        $"{requested.RevisionNumber} " +
                        $"already exists for Customer Drawing " +
                        $"{drawingNumber}.");
                }


                prepared.Add(
                    new CustomerDrawing
                    {
                        CustomerId =
                            customerId,

                        ItemId =
                            commonDrawing.ItemId,

                        DrawingNumber =
                            drawingNumber,

                        DrawingName =
                            commonDrawing.DrawingName,

                        DrawingType =
                            commonDrawing.DrawingType,

                        RevisionNumber =
                            requested.RevisionNumber,

                        FileName =
                            requested.FileName,

                        FilePath =
                            requested.FilePath,

                        Description =
                            requested.Description,

                        IsActive =
                            false,

                        IsDeleted =
                            false,

                        CreatedOn =
                            DateTime.UtcNow,

                        CreatedBy =
                            "System"
                    });
            }


            return prepared;
        }

        #endregion


        #region Revision Number Generation

        private async Task<int>
            GetNextRevisionSequenceAsync(
                int customerId,
                string drawingNumber)
        {
            var revisionNumbers =
                await _customerDrawingRepository
                    .GetRevisionNumbersIncludingDeletedAsync(
                        customerId,
                        drawingNumber);


            var maximumSequence =
                0;


            foreach (var revisionNumber
                in revisionNumbers)
            {
                if (!TryGetRevisionSequence(
                    revisionNumber,
                    out var sequence))
                {
                    continue;
                }


                if (sequence >
                    maximumSequence)
                {
                    maximumSequence =
                        sequence;
                }
            }


            return maximumSequence + 1;
        }


        private static bool TryGetRevisionSequence(
            string? revisionNumber,
            out int sequence)
        {
            sequence =
                0;


            if (string.IsNullOrWhiteSpace(
                revisionNumber))
            {
                return false;
            }


            var normalized =
                revisionNumber
                    .Trim()
                    .ToUpperInvariant();


            /*
             * Supports legacy:
             *
             * R01
             *
             * And current:
             *
             * RV-01
             */
            var match =
                Regex.Match(
                    normalized,
                    @"^(?:R|RV-?)(\d+)$");


            if (!match.Success)
            {
                return false;
            }


            return int.TryParse(
                match.Groups[1].Value,
                out sequence);
        }


        private static string
            FormatRevisionNumber(
                int sequence)
        {
            return $"RV-{sequence:00}";
        }

        #endregion


        #region Customer Validation

        private async Task ValidateCustomerAsync(
            int customerId)
        {
            if (customerId <= 0)
            {
                throw new BusinessException(
                    "Please select a Customer.");
            }


            var customer =
                await _customerRepository
                    .GetByIdAsync(
                        customerId);


            if (customer == null)
            {
                throw new BusinessException(
                    "Selected Customer does not exist.");
            }
        }

        #endregion


        #region Item Validation

        private async Task ValidateItemAsync(
            int itemId)
        {
            if (itemId <= 0)
            {
                throw new BusinessException(
                    "Please select an Item.");
            }


            var item =
                await _itemRepository
                    .GetByIdAsync(
                        itemId);


            if (item == null)
            {
                throw new BusinessException(
                    "Selected Item does not exist.");
            }
        }

        #endregion


        #region Normalization

        private static void NormalizeCommonFields(
            CustomerDrawing customerDrawing)
        {
            customerDrawing.DrawingNumber =
                NormalizeUpperText(
                    customerDrawing.DrawingNumber)
                ??
                string.Empty;


            customerDrawing.DrawingName =
                NormalizeText(
                    customerDrawing.DrawingName);


            customerDrawing.DrawingType =
                NormalizeText(
                    customerDrawing.DrawingType);
        }


        private static void NormalizeRevisionFields(
            CustomerDrawing customerDrawing)
        {
            customerDrawing.RevisionNumber =
                NormalizeUpperText(
                    customerDrawing.RevisionNumber);


            customerDrawing.FileName =
                NormalizeText(
                    customerDrawing.FileName);


            customerDrawing.FilePath =
                NormalizeText(
                    customerDrawing.FilePath);


            customerDrawing.Description =
                NormalizeText(
                    customerDrawing.Description);
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

        #endregion


        #region Validation

        private static void ValidateCommonFields(
            CustomerDrawing customerDrawing)
        {
            if (customerDrawing.CustomerId <= 0)
            {
                throw new BusinessException(
                    "Please select a Customer.");
            }


            if (customerDrawing.ItemId <= 0)
            {
                throw new BusinessException(
                    "Please select an Item.");
            }


            if (string.IsNullOrWhiteSpace(
                customerDrawing.DrawingNumber))
            {
                throw new BusinessException(
                    "Drawing Number is required.");
            }


            if (customerDrawing.DrawingNumber.Length >
                100)
            {
                throw new BusinessException(
                    "Drawing Number cannot exceed 100 characters.");
            }


            if (customerDrawing.DrawingName?.Length >
                200)
            {
                throw new BusinessException(
                    "Drawing Name cannot exceed 200 characters.");
            }


            if (customerDrawing.DrawingType?.Length >
                100)
            {
                throw new BusinessException(
                    "Drawing Type cannot exceed 100 characters.");
            }
        }


        private static void ValidateRevisionFields(
            CustomerDrawing customerDrawing)
        {
            if (string.IsNullOrWhiteSpace(
                customerDrawing.RevisionNumber))
            {
                throw new BusinessException(
                    "Revision Number is required.");
            }


            if (customerDrawing.RevisionNumber.Length >
                50)
            {
                throw new BusinessException(
                    "Revision Number cannot exceed 50 characters.");
            }


            if (customerDrawing.FileName?.Length >
                255)
            {
                throw new BusinessException(
                    "File Name cannot exceed 255 characters.");
            }


            if (customerDrawing.FilePath?.Length >
                500)
            {
                throw new BusinessException(
                    "File Path cannot exceed 500 characters.");
            }


            if (customerDrawing.Description?.Length >
                500)
            {
                throw new BusinessException(
                    "Revision Remarks cannot exceed 500 characters.");
            }
        }

        #endregion
    }
}