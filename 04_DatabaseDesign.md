# 04 - Database Design

## Project

Ajay Industries ERP

## Technology

- ASP.NET Core MVC
- .NET 8
- Entity Framework Core
- SQL Server
- Clean Architecture
- Repository + Service pattern

---

# 1. General Database Rules

All business entities should follow the ERP base-entity pattern where applicable.

Common audit fields:

- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

General rules:

- Physical delete is avoided.
- Soft Delete is the default.
- Business codes that must never be reused continue checking deleted records.
- Foreign-key delete behavior should normally use `Restrict`.
- Business validation is enforced in Application Services.
- Database constraints/indexes should provide a second level of protection where practical.

---

# 2. Existing Master Tables

The following masters are completed:

- Companies
- Employees
- UOMs
- Warehouses
- ItemCategories
- Brands
- Shapes
- Specifications
- Items
- Suppliers
- Drawings

---

# 3. Automatic Master Codes

The following code formats are currently used:

| Master | Format |
|---|---|
| Company | CMP00001 |
| Employee | EMP00001 |
| Warehouse | WH00001 |
| Item Category | CAT00001 |
| Brand | BRD00001 |
| Shape | SHP00001 |
| Specification | SPC00001 |
| Item | ITM00001 |
| Supplier | SUP00001 |

UOM code is manually maintained.

Code-generation rules:

- Generated codes are never reused.
- Deleted records are included when finding the last generated code.
- Duplicate code checks include deleted records.

---

# 4. Item Table

## Items

Main Item Master table.

Important fields:

- ItemId
- ItemCode
- ItemName
- PartNumber
- ImagePath
- Description
- ItemCategoryId
- BrandId
- UomId
- ShapeId
- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

## ItemCode

- System generated.
- Format: `ITM00001`
- Unique.
- Never reused.
- Not intended to describe the Item.
- Used as permanent ERP identity.

## ItemName

Item Name is not globally unique.

Same Item Name may be used for multiple Item configurations.

Example:

- MS Round Bar - Dia 25 MM
- MS Round Bar - Dia 30 MM

Item duplicate identity is determined using:

- ItemName
- Shape
- Complete specification configuration

Category, Brand and main UOM are intentionally not part of the duplicate signature.

## PartNumber

- Optional.
- Stored directly on Item.
- Not currently unique.
- Used as manufacturer/internal/business part reference.

## ImagePath

- Optional.
- Stores a file path/reference only.
- Image binary is not stored in SQL Server.
- Actual Item image upload integration is the next Item enhancement.

---

# 5. Item Specifications

## ItemSpecifications

Child table of Items.

Fields:

- ItemSpecificationId
- ItemId
- SpecificationId
- SpecificationValue
- UomId
- SortOrder
- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

Relationships:

- ItemId -> Items
- SpecificationId -> Specifications
- UomId -> UOMs

Delete behavior:

- Restrict

Active uniqueness:

- ItemId + SpecificationId
- Filter: `IsDeleted = 0`

Therefore the same Specification cannot appear twice in one active Item configuration.

Specification row order does not affect Item duplicate identity.

---

# 6. Shape Master

## Shapes

Standalone master.

Code format:

`SHP00001`

Used by:

- Item Master
- Item duplicate/configuration identity
- Future engineering/production workflows

Shape is optional on Item.

---

# 7. Specification Master

## Specifications

Standalone master.

Code format:

`SPC00001`

Examples:

- Diameter
- Length
- Width
- Thickness
- Grade
- Material Grade

Grade is handled as a Specification rather than a separate Item column.

---

# 8. Supplier Table

## Suppliers

Fields:

- SupplierId
- SupplierCode
- SupplierName
- ContactPerson
- MobileNumber
- AlternateMobileNumber
- Email
- Gstin
- Pan
- AddressLine1
- AddressLine2
- City
- State
- Pincode
- PaymentTermsDays
- Description
- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

## SupplierCode

- Auto-generated
- Format: `SUP00001`
- Globally unique
- Deleted codes are never reused

## SupplierName

- Required
- Exact duplicate active Supplier Name is blocked
- Similar spelling is shown as warning
- Live similarity search is supported

## GSTIN

- Optional
- Unique among active non-deleted Suppliers when provided

Filtered uniqueness:

`Gstin IS NOT NULL AND IsDeleted = 0`

## PAN

- Optional
- Not unique

Reason:

One legal entity may have multiple GST registrations/locations.

## Supplier Financial Data

The following are intentionally not stored in Supplier Master:

- Opening Balance
- Total Purchase Amount
- Pending Payment
- Last Purchase
- GST totals

These belong to future transaction/accounting modules.

---

# 9. Drawing Architecture

Drawing Master uses a single `Drawings` table.

This is intentionally a single-table revision-history design.

## Final Business Relationship

One Item:

`1 Item -> 1 Drawing Number -> Many Revisions`

A second Drawing Number cannot be created for the same active Item.

When engineering data changes, a new Revision must be added to the existing Drawing.

---

# 10. Drawings Table

## Drawings

Each database row represents one Drawing Revision.

Fields:

- DrawingId
- ItemId
- DrawingNumber
- DrawingName
- DrawingType
- RevisionNumber
- FileName
- FilePath
- Description
- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

`IsPrimary` has been removed.

Reason:

One Item can have only one Drawing Number, therefore Primary/Non-Primary Drawing is unnecessary.

---

# 11. Drawing Number

Drawing Number:

- Manual
- Required
- Permanent
- Cannot change after Drawing creation
- Never reused
- Deleted Drawing Number remains reserved

Example:

`DRG-SS-340`

Drawing Number live behavior:

- Exact existing number -> block Create
- Similar number -> warning/suggestion
- Existing Drawing should be opened and revised instead

---

# 12. Drawing Name

Drawing Name:

- Optional
- Not unique
- Exact/similar names generate a warning
- Similarity warning does not block Save

Reason:

Different engineering drawings may legitimately have similar names.

---

# 13. Drawing Revision Number

Revision Number is system generated.

Current format:

- RV-01
- RV-02
- RV-03
- ...

Rules:

- User does not manually enter Revision Number.
- First Drawing revision = RV-01.
- Each next revision increments automatically.
- Deleted Revision Numbers are included when calculating the next number.
- Revision numbers are never reused.

Example:

RV-01 -> deleted  
RV-02 -> current

Next generated revision:

RV-03

Legacy revision values such as `R01` / `R02` are supported when calculating the next revision sequence.

---

# 14. Drawing Revision Uniqueness

Unique index:

- DrawingNumber + RevisionNumber

Deleted records are included.

Therefore a historical/deleted revision number cannot be reused.

---

# 15. Current Drawing Revision

`IsActive` represents the Current Revision.

For one Drawing Number:

- Many historical revisions may exist.
- Maximum one revision can have `IsActive = true`.

Filtered unique index:

`DrawingNumber UNIQUE WHERE IsActive = 1 AND IsDeleted = 0`

---

# 16. One Drawing Per Item

Final business rule:

One Item can have only one active Drawing Number.

Service validation blocks another Drawing Create for the same Item.

Database protection:

`ItemId UNIQUE WHERE IsActive = 1 AND IsDeleted = 0`

Historical revisions use the same ItemId but are inactive.

---

# 17. Revision Activation

A previous inactive revision may be reactivated.

Example:

Before:

- RV-03 Current
- RV-02 Inactive
- RV-01 Inactive

Activate RV-01:

After:

- RV-03 Inactive
- RV-02 Inactive
- RV-01 Current

Maximum one Current revision is enforced.

Revision switching is executed transactionally.

---

# 18. Revision Soft Delete

Inactive revisions may be soft deleted.

Current revision cannot be deleted.

To delete the Current revision:

1. Activate another revision.
2. Delete the previously Current revision.

Deleted revision:

- remains in database
- is hidden from normal revision history
- revision number remains reserved
- physical Drawing file is retained

---

# 19. Drawing Soft Delete and Restore

Deleting a Drawing soft-deletes its Drawing identity/revision history.

Deleted Drawings are hidden from the normal Drawing Index.

A dedicated `Deleted Drawings` screen supports Restore.

Restore behavior:

- Same Drawing Number is restored.
- Revision history is restored.
- Previous Current revision is restored when known.
- For legacy deleted records where Current state was lost, latest revision becomes Current.
- Restore is blocked if the Item already has another active Drawing.

Drawing Number is never recreated as a new Drawing after soft delete.

The correct action is Restore.

---

# 20. Drawing Files

Supported Drawing file types:

- PDF
- JPG
- JPEG
- PNG
- DWG
- DXF

Current maximum upload size:

25 MB

Storage:

`wwwroot/uploads/drawings`

Database stores:

- Original FileName
- Relative FilePath

Database does not store file binary.

Historical Drawing files are retained.

---

# 21. Drawing Search

Drawing search includes:

- Drawing Number
- Drawing Name
- Revision Number
- Drawing Type
- File Name
- Description
- Item Code
- Item Name
- Item Part Number

Normal search/index returns Current revisions only.

---

# 22. Item and Drawing Future Integration

Next Item enhancement will include:

- PartNumber UI integration
- Item image upload/display
- Item Details Drawing section
- Current Drawing Number
- Current Drawing Revision
- Drawing file access

---

# 23. Deferred Database Areas

The following remain intentionally deferred:

- Stock ledger
- Warehouse stock
- Opening stock
- Minimum stock
- Maximum stock
- GST/HSN Item taxation
- Pricing
- Purchase transactions
- BOM
- Production transactions
- Quality inspection
- Accounting balances
- Full Drawing approval workflow
- Separate Drawing revision table

These should be implemented only when their corresponding module is designed.