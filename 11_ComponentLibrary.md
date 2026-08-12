# 11 - Component Library

## Ajay Industries ERP

This document records reusable application and UI patterns.

---

# 1. Shared Search Bar

File:

Views/Shared/Components/_SearchBar.cshtml

Used for Master list searching.

---

# 2. Shared Pagination

File:

Views/Shared/Components/_Pagination.cshtml

Application model:

Application.Common.PagedResult<T>

Supports:

- PageNumber
- PageSize
- TotalRecords
- TotalPages
- HasPrevious
- HasNext

---

# 3. Delete Confirmation

File:

Views/Shared/Components/_ConfirmDeleteModal.cshtml

JavaScript:

confirmDelete()

Business modules normally use Soft Delete.

---

# 4. Toast Notifications

File:

Views/Shared/Components/_ToastNotification.cshtml

Toastr is used for:

- Success
- Error
- Warning

Controllers commonly use TempData.

---

# 5. Name Similarity Pattern

Helper:

Application/Common/NameSimilarityHelper.cs

Supports:

- Normalize
- Exact Match
- Similar Match
- Levenshtein
- Live Search Match
- Ranked Suggestions

Used for Master duplicate/spelling assistance.

---

# 6. Quick Add Master Modal

Reusable Item form Quick Add supports:

- Category
- Brand
- UOM
- Shape
- Specification

Features:

- Bootstrap Modal
- AJAX Save
- Select2
- Similar Name Warning
- Exact Duplicate Handling
- New Record Auto Select

---

# 7. Select2 Pattern

Select2 is used for searchable dropdowns.

Current examples:

- Item Master dropdowns
- Drawing Item dropdown

Drawing Item display may include:

- ItemCode
- ItemName
- PartNumber
- Shape
- Specifications

---

# 8. Dynamic Item Specification Row

Pattern:

Specification
| Value
| Optional UOM
| Remove

Supports:

- Add row
- Remove row
- Explicit collection indexes
- SortOrder
- Select2
- Quick Add Specification
- Quick Add UOM

---

# 9. Item Three-Column Details Pattern

Item Details uses equal-width bordered information cards.

Desktop layout:

3 columns per row.

Example:

Item Code
| Item Name
| Part Number

Category
| Brand
| UOM

Shape
| Status
| Description

This layout may be reused for future Master Details pages.

---

# 10. Item Specification Badge Pattern

Item Index displays compact Specification badges.

Example:

Diameter: 25 MM
Length: 500 MM
Grade: EN8

Maximum first three Specifications shown inline.

Remaining count shown as:

+N more

---

# 11. Dynamic Drawing Revision Row

Drawing Edit supports dynamic Revision input.

Fields:

- Revision = Auto
- Drawing File
- Remarks
- Remove

Collection binding uses:

NewRevisions.Index

---

# 12. Drawing Revision History Table

Columns:

- Revision
- File
- Remarks
- Status
- Created On
- Created By
- Actions

Status:

- Current
- Inactive

Historical actions:

- Activate
- Delete

---

# 13. Drawing Revision Activation

Server-side POST action.

Behavior:

- Existing Current revision is deactivated.
- Selected historical revision becomes Current.
- Operation uses a database transaction.
- Only one Current revision remains.

---

# 14. Drawing Revision Delete

Rules:

- Inactive revision only
- Soft Delete
- Current revision protected
- Revision Number remains reserved
- Physical file retained

---

# 15. Deleted Drawing Restore Pattern

Dedicated Deleted Drawings screen.

Displays one row per deleted Drawing Number.

Supports Restore.

This pattern may later be reused for important recoverable ERP records.

---

# 16. Drawing File Upload Pattern

Supported:

- PDF
- JPG
- JPEG
- PNG
- DWG
- DXF

Maximum:

25 MB

Physical storage:

wwwroot/uploads/drawings

Database stores:

- FileName
- FilePath

---

# 17. Permanent Identity Pattern

Drawing Edit demonstrates read-only permanent identity fields.

Permanent after Create:

- Item
- Drawing Number

Protection exists in:

- UI
- Application Service

Posted tampered values are not trusted.

---

# 18. Item to Drawing Summary Component Pattern

Item Details/Edit display read-only Drawing information.

Fields:

- Drawing Number
- Drawing Name
- Current Revision
- Drawing Type
- Drawing File

Available actions:

- View Drawing File
- Open Drawing Details
- Add Drawing when none exists

Drawing editing remains inside the Drawing module.

---

# 19. Save-Then-Child Pattern

Item Create demonstrates a useful parent-child workflow.

Flow:

Create Parent
→ Save Parent
→ Redirect to Details
→ Create dependent record

Current use:

Create Item
→ Save
→ Item Details
→ Add Drawing

This ensures the ItemId exists before Drawing creation.

---

# 20. Auto-Selected Related Master Pattern

When Add Drawing is started from Item Details/Edit:

ItemId is passed in the URL.

Drawing Create:

- receives ItemId
- automatically selects that Item

This reduces user selection errors.

---

# 21. Audit Display Pattern

Typical audit fields:

- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

For multi-line Razor expressions use explicit wrapping.

Example:

@(
    Model.CreatedOn
        .ToLocalTime()
        .ToString("dd-MMM-yyyy hh:mm tt")
)

This avoids Razor property-chain rendering issues.