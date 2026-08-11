/*
==============================================================

File : DrawingService.cs

Purpose :
Manages Drawing Master and Drawing Revision workflow.

Final Business Rules :
- One Item = One Drawing Number.
- Drawing Number is permanent.
- Every row represents one revision.
- Revision Numbers are system generated.
- Revision Numbers are never reused.
- Only one revision can be Current.
- Previous revisions can be reactivated.
- Inactive revisions can be soft deleted.
- Complete Drawing can be soft deleted/restored.
- Drawing files are preserved for history.

==============================================================
*/

using AjayIndustriesERP.Application.Common;
using AjayIndustriesERP.Application.Exceptions;
using AjayIndustriesERP.Application.Interfaces;
using AjayIndustriesERP.Domain.Entities;
using System.Text.RegularExpressions;

namespace AjayIndustriesERP.Application.Services
{
    public class DrawingService :
        IDrawingService
    {
        private readonly IDrawingRepository
            _drawingRepository;

        private readonly IItemRepository
            _itemRepository;

        public DrawingService(
            IDrawingRepository drawingRepository,
            IItemRepository itemRepository)
        {
            _drawingRepository =
                drawingRepository;

            _itemRepository =
                itemRepository;
        }

        #region Read

        public async Task<List<Drawing>>
            GetAllAsync()
        {
            return await _drawingRepository
                .GetAllAsync();
        }

        public async Task<Drawing?>
            GetByIdAsync(
                int drawingId)
        {
            return await _drawingRepository
                .GetByIdAsync(
                    drawingId);
        }

        public async Task<List<Drawing>>
            GetByItemIdAsync(
                int itemId)
        {
            if (itemId <= 0)
            {
                return new List<Drawing>();
            }

            return await _drawingRepository
                .GetByItemIdAsync(
                    itemId);
        }

        public async Task<List<Drawing>>
            GetRevisionHistoryAsync(
                int drawingId)
        {
            var drawing =
                await _drawingRepository
                    .GetByIdAsync(
                        drawingId);

            if (drawing == null)
            {
                return new List<Drawing>();
            }

            return await _drawingRepository
                .GetRevisionHistoryAsync(
                    drawing.DrawingNumber);
        }

        public async Task<List<Drawing>>
            SearchAsync(
                string searchText)
        {
            return await _drawingRepository
                .SearchAsync(
                    searchText);
        }

        public async Task<PagedResult<Drawing>>
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

            return await _drawingRepository
                .GetPagedAsync(
                    pageNumber,
                    pageSize);
        }

        public async Task<List<Drawing>>
            GetDeletedDrawingsAsync()
        {
            return await _drawingRepository
                .GetDeletedDrawingsAsync();
        }

        #endregion

        #region Create Drawing

        public async Task CreateAsync(
            Drawing drawing)
        {
            NormalizeCommonFields(
                drawing);

            ValidateCommonFields(
                drawing);

            await ValidateItemAsync(
                drawing.ItemId);

            /*
             * Drawing Number is permanently reserved.
             * Deleted Drawing Numbers cannot be reused.
             */
            if (await _drawingRepository
                .ExistsByDrawingNumberAsync(
                    drawing.DrawingNumber))
            {
                throw new BusinessException(
                    $"Drawing Number " +
                    $"{drawing.DrawingNumber} " +
                    $"already exists. Open the existing " +
                    $"Drawing or restore it if deleted.");
            }

            /*
             * One active Drawing Number per Item.
             *
             * Drawing changes must be managed through
             * Revision History.
             */
            var existingItemDrawings =
                await _drawingRepository
                    .GetByItemIdAsync(
                        drawing.ItemId);

            if (existingItemDrawings.Any())
            {
                var existingDrawing =
                    existingItemDrawings.First();

                throw new BusinessException(
                    $"Selected Item already has Drawing " +
                    $"{existingDrawing.DrawingNumber}. " +
                    $"Open the existing Drawing and add " +
                    $"a new Revision.");
            }

            /*
             * First revision is always RV-01.
             */
            drawing.RevisionNumber =
                FormatRevisionNumber(1);

            NormalizeRevisionFields(
                drawing);

            ValidateRevisionFields(
                drawing);

            drawing.IsActive =
                true;

            drawing.IsDeleted =
                false;

            drawing.CreatedOn =
                DateTime.UtcNow;

            drawing.CreatedBy =
                "System";

            await _drawingRepository
                .AddAsync(
                    drawing);

            await _drawingRepository
                .SaveChangesAsync();
        }

        #endregion

        #region Update Drawing / Add Revision

        public async Task UpdateAsync(
            Drawing drawing,
            IReadOnlyCollection<Drawing>
                newRevisions)
        {
            var existingDrawing =
                await _drawingRepository
                    .GetByIdAsync(
                        drawing.DrawingId);

            if (existingDrawing == null)
            {
                throw new BusinessException(
                    "Drawing not found.");
            }

            /*
             * Item and Drawing Number are permanent.
             */
            var permanentDrawingNumber =
                existingDrawing.DrawingNumber;

            var permanentItemId =
                existingDrawing.ItemId;

            drawing.DrawingNumber =
                permanentDrawingNumber;

            drawing.ItemId =
                permanentItemId;

            NormalizeCommonFields(
                drawing);

            ValidateCommonFields(
                drawing);

            await ValidateItemAsync(
                permanentItemId);

            var history =
                await _drawingRepository
                    .GetByDrawingNumberForUpdateAsync(
                        permanentDrawingNumber);

            if (history.Count == 0)
            {
                throw new BusinessException(
                    "Drawing revision history not found.");
            }

            var currentRevision =
                history.FirstOrDefault(x =>
                    x.IsActive);

            if (currentRevision == null)
            {
                throw new BusinessException(
                    "Current Drawing revision not found.");
            }

            /*
             * DrawingName and DrawingType are
             * Drawing-level information.
             *
             * Keep them consistent across all revisions.
             */
            foreach (var revision in history)
            {
                revision.ItemId =
                    permanentItemId;

                revision.DrawingName =
                    drawing.DrawingName;

                revision.DrawingType =
                    drawing.DrawingType;

                revision.ModifiedOn =
                    DateTime.UtcNow;

                revision.ModifiedBy =
                    "System";
            }

            var preparedRevisions =
                await PrepareNewRevisionsAsync(
                    permanentDrawingNumber,
                    drawing,
                    newRevisions);

            if (preparedRevisions.Count > 0)
            {
                /*
                 * Previous Current revision becomes history.
                 */
                currentRevision.IsActive =
                    false;

                /*
                 * If multiple revisions are entered
                 * in one Save, only the final revision
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

                await _drawingRepository
                    .AddRangeAsync(
                        preparedRevisions);
            }

            await _drawingRepository
                .SaveChangesAsync();
        }

        #endregion

        #region Activate Revision

        public async Task ActivateRevisionAsync(
            int drawingId)
        {
            var selectedRevision =
                await _drawingRepository
                    .GetByIdAsync(
                        drawingId);

            if (selectedRevision == null)
            {
                throw new BusinessException(
                    "Drawing revision not found.");
            }

            var history =
                await _drawingRepository
                    .GetByDrawingNumberForUpdateAsync(
                        selectedRevision.DrawingNumber);

            var targetRevision =
                history.FirstOrDefault(x =>
                    x.DrawingId ==
                    drawingId);

            if (targetRevision == null)
            {
                throw new BusinessException(
                    "Drawing revision not found.");
            }

            if (targetRevision.IsDeleted)
            {
                throw new BusinessException(
                    "Deleted Drawing revision cannot be activated.");
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
                    "Current Drawing revision not found.");
            }

            /*
             * Two SaveChanges calls are intentional.
             *
             * Unique filtered index allows only one
             * Current revision.
             *
             * First deactivate current revision,
             * then activate selected historical revision,
             * all inside one transaction.
             */
            await _drawingRepository
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

                        await _drawingRepository
                            .SaveChangesAsync();

                        targetRevision.IsActive =
                            true;

                        targetRevision.ModifiedOn =
                            DateTime.UtcNow;

                        targetRevision.ModifiedBy =
                            "System";

                        await _drawingRepository
                            .SaveChangesAsync();
                    });
        }

        #endregion

        #region Delete Revision

        public async Task DeleteRevisionAsync(
            int drawingId)
        {
            var selectedRevision =
                await _drawingRepository
                    .GetByIdAsync(
                        drawingId);

            if (selectedRevision == null)
            {
                throw new BusinessException(
                    "Drawing revision not found.");
            }

            var history =
                await _drawingRepository
                    .GetByDrawingNumberForUpdateAsync(
                        selectedRevision.DrawingNumber);

            var revision =
                history.FirstOrDefault(x =>
                    x.DrawingId ==
                    drawingId);

            if (revision == null)
            {
                throw new BusinessException(
                    "Drawing revision not found.");
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

            await _drawingRepository
                .SaveChangesAsync();
        }

        #endregion

        #region Delete Complete Drawing

        public async Task DeleteAsync(
            int drawingId)
        {
            var drawing =
                await _drawingRepository
                    .GetByIdAsync(
                        drawingId);

            if (drawing == null)
            {
                throw new BusinessException(
                    "Drawing not found.");
            }

            var history =
                await _drawingRepository
                    .GetByDrawingNumberForUpdateAsync(
                        drawing.DrawingNumber);

            if (history.Count == 0)
            {
                throw new BusinessException(
                    "Drawing not found.");
            }

            foreach (var revision
                in history)
            {
                /*
                 * Preserve IsActive.
                 *
                 * This remembers which revision was
                 * Current before Drawing deletion.
                 */
                revision.IsDeleted =
                    true;

                revision.ModifiedOn =
                    DateTime.UtcNow;

                revision.ModifiedBy =
                    "System";
            }

            await _drawingRepository
                .SaveChangesAsync();
        }

        #endregion

        #region Restore Drawing

        public async Task RestoreAsync(
            int drawingId)
        {
            var deletedDrawings =
                await _drawingRepository
                    .GetDeletedDrawingsAsync();

            var selectedDrawing =
                deletedDrawings
                    .FirstOrDefault(x =>
                        x.DrawingId ==
                        drawingId);

            if (selectedDrawing == null)
            {
                throw new BusinessException(
                    "Deleted Drawing not found.");
            }

            var history =
                await _drawingRepository
                    .GetDeletedHistoryForUpdateAsync(
                        selectedDrawing.DrawingNumber);

            if (history.Count == 0)
            {
                throw new BusinessException(
                    "Deleted Drawing history not found.");
            }

            /*
             * Cannot restore if the Item currently
             * belongs to another active Drawing.
             */
            var existingItemDrawings =
                await _drawingRepository
                    .GetByItemIdAsync(
                        selectedDrawing.ItemId);

            if (existingItemDrawings.Any())
            {
                var existingDrawing =
                    existingItemDrawings.First();

                throw new BusinessException(
                    $"Item already has active Drawing " +
                    $"{existingDrawing.DrawingNumber}. " +
                    $"Delete that Drawing before restoring " +
                    $"{selectedDrawing.DrawingNumber}.");
            }

            /*
             * Older deleted records may have lost
             * their Current flag.
             *
             * If Current revision cannot be identified,
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
                            x.DrawingId)
                        .First();
            }

            /*
             * Normalize history before restoring so
             * exactly one Current revision exists.
             */
            foreach (var revision
                in history)
            {
                revision.IsDeleted =
                    false;

                revision.IsActive =
                    revision.DrawingId ==
                    currentRevision.DrawingId;

                revision.ModifiedOn =
                    DateTime.UtcNow;

                revision.ModifiedBy =
                    "System";
            }

            await _drawingRepository
                .SaveChangesAsync();
        }

        #endregion

        #region Prepare New Revisions

        private async Task<List<Drawing>>
            PrepareNewRevisionsAsync(
                string drawingNumber,
                Drawing commonDrawing,
                IReadOnlyCollection<Drawing>
                    requestedRevisions)
        {
            var prepared =
                new List<Drawing>();

            if (requestedRevisions == null ||
                requestedRevisions.Count == 0)
            {
                return prepared;
            }

            var nextSequence =
                await GetNextRevisionSequenceAsync(
                    drawingNumber);

            foreach (var requested
                in requestedRevisions)
            {
                /*
                 * Completely empty dynamic row
                 * should not create a revision.
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

                requested.RevisionNumber =
                    FormatRevisionNumber(
                        nextSequence);

                nextSequence++;

                NormalizeRevisionFields(
                    requested);

                ValidateRevisionFields(
                    requested);

                if (await _drawingRepository
                    .ExistsByRevisionAsync(
                        drawingNumber,
                        requested.RevisionNumber!))
                {
                    throw new BusinessException(
                        $"Revision " +
                        $"{requested.RevisionNumber} " +
                        $"already exists for Drawing " +
                        $"{drawingNumber}.");
                }

                prepared.Add(
                    new Drawing
                    {
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
                string drawingNumber)
        {
            var revisionNumbers =
                await _drawingRepository
                    .GetRevisionNumbersIncludingDeletedAsync(
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
            sequence = 0;

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
             * R01
             *
             * And current:
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

        private static string FormatRevisionNumber(
            int sequence)
        {
            return $"RV-{sequence:00}";
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
            Drawing drawing)
        {
            drawing.DrawingNumber =
                NormalizeUpperText(
                    drawing.DrawingNumber)
                ?? string.Empty;

            drawing.DrawingName =
                NormalizeText(
                    drawing.DrawingName);

            drawing.DrawingType =
                NormalizeText(
                    drawing.DrawingType);
        }

        private static void NormalizeRevisionFields(
            Drawing drawing)
        {
            drawing.RevisionNumber =
                NormalizeUpperText(
                    drawing.RevisionNumber);

            drawing.FileName =
                NormalizeText(
                    drawing.FileName);

            drawing.FilePath =
                NormalizeText(
                    drawing.FilePath);

            drawing.Description =
                NormalizeText(
                    drawing.Description);
        }

        private static string? NormalizeText(
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

        private static string? NormalizeUpperText(
            string? value)
        {
            return NormalizeText(
                value)?
                .ToUpperInvariant();
        }

        #endregion

        #region Validation

        private static void ValidateCommonFields(
            Drawing drawing)
        {
            if (drawing.ItemId <= 0)
            {
                throw new BusinessException(
                    "Please select an Item.");
            }

            if (string.IsNullOrWhiteSpace(
                drawing.DrawingNumber))
            {
                throw new BusinessException(
                    "Drawing Number is required.");
            }

            if (drawing.DrawingNumber.Length >
                100)
            {
                throw new BusinessException(
                    "Drawing Number cannot exceed 100 characters.");
            }

            if (drawing.DrawingName?.Length >
                200)
            {
                throw new BusinessException(
                    "Drawing Name cannot exceed 200 characters.");
            }

            if (drawing.DrawingType?.Length >
                100)
            {
                throw new BusinessException(
                    "Drawing Type cannot exceed 100 characters.");
            }
        }

        private static void ValidateRevisionFields(
            Drawing drawing)
        {
            if (string.IsNullOrWhiteSpace(
                drawing.RevisionNumber))
            {
                throw new BusinessException(
                    "Revision Number is required.");
            }

            if (drawing.RevisionNumber.Length >
                50)
            {
                throw new BusinessException(
                    "Revision Number cannot exceed 50 characters.");
            }

            if (drawing.FileName?.Length >
                255)
            {
                throw new BusinessException(
                    "File Name cannot exceed 255 characters.");
            }

            if (drawing.FilePath?.Length >
                500)
            {
                throw new BusinessException(
                    "File Path cannot exceed 500 characters.");
            }

            if (drawing.Description?.Length >
                500)
            {
                throw new BusinessException(
                    "Revision Remarks cannot exceed 500 characters.");
            }
        }

        #endregion
    }
}