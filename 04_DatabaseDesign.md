# 04 - Database Design

## Project

Ajay Industries ERP

## Technology

- ASP.NET Core MVC
- .NET 8
- Entity Framework Core
- SQL Server
- Clean Architecture
- Repository + Service Pattern

---

# 1. General Database Rules

Business entities use the common BaseEntity audit structure where applicable.

Common fields:

- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

General rules:

- Soft Delete is the default.
- Physical deletion is avoided.
- Business codes are never reused.
- Deleted records are included when checking generated codes.
- Foreign-key delete behavior should normally use Restrict.
- Application Services enforce business rules.
- Database indexes/constraints provide additional protection where practical.

---

# 2. Completed Master Tables

Completed Masters:

- Companies
- Employees
- UOMs
- Warehouses
- ItemCategories
- Brands
- Shapes
- Specifications
- Items
- ItemSpecifications
- Suppliers
- Drawings

---

# 3. Automatic Master Codes

| Master | Code Format |
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

UOM Code is manually maintained.

Rules:

- Generated codes are permanent.
- Deleted records are included while finding the last code.
- Deleted codes are never reused.

---

# 4. Items Table

## Items

Important fields:

- ItemId
- ItemCode
- ItemName
- PartNumber
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

- System generated
- Format: ITM00001
- Permanent ERP identity
- Unique
- Never reused

ItemCode is not intended to describe the Item.

---

# 5. Item Name and Configuration Identity

ItemName is not globally unique.

The same Item Name may represent different technical configurations.

Example:

- MS Round Bar / Dia 25
- MS Round Bar / Dia 30

Exact Item duplicate identity is:

- ItemName
- Shape
- Complete Specification configuration

Specification configuration includes:

- SpecificationId
- Normalized SpecificationValue
- Optional UomId

Specification row order does not affect duplicate identity.

The following are intentionally excluded from Item duplicate identity:

- Category
- Brand
- Main UOM
- PartNumber

---

# 6. Part Number

PartNumber is stored directly on Item.

Rules:

- Optional
- Maximum 100 characters
- Not unique
- Searchable
- Editable
- Displayed in Index and Details

PartNumber may represent:

- Internal part reference
- Customer part reference
- Manufacturer reference
- Engineering reference

Two different Items may use the same PartNumber.

---

# 7. Item Image Decision

Item Image storage is not part of the current Item Master.

The previously introduced ImagePath field was removed.

Reason:

Engineering identification is already available through:

- Item Name
- Part Number
- Shape
- Specifications
- Drawing Number
- Drawing Revision
- Drawing File

Maintaining a separate Item image would add unnecessary storage and UI complexity.

---

# 8. ItemSpecifications Table

Each Item may have multiple dynamic Specifications.

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

Active uniqueness:

ItemId + SpecificationId

for non-deleted rows.

The same Specification cannot appear twice in one active Item configuration.

---

# 9. Supplier Table

## Suppliers

Important fields:

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

SupplierCode:

SUP00001

Rules:

- SupplierCode never reused.
- Exact active SupplierName duplicate is blocked.
- Similar SupplierName generates warning.
- GSTIN is optional and unique among active non-deleted Suppliers when provided.
- PAN is optional and not unique.

Supplier financial transaction values are not stored in Supplier Master.

---

# 10. Drawing Architecture

Drawing Master uses one Drawings table.

Each database row represents one Drawing Revision.

Final business relationship:

One Item
→ One Drawing Number
→ Many Revisions

A second active Drawing Number cannot be created for the same Item.

---

# 11. Drawings Table

Important fields:

- DrawingId
- ItemId
- DrawingNumber
- DrawingName
- RevisionNumber
- DrawingType
- FileName
- FilePath
- Description
- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

IsPrimary has been removed.

Reason:

One Item can have only one Drawing Number.

---

# 12. Drawing Number

DrawingNumber is:

- Manually entered
- Required
- Permanent
- Immutable after Create
- Never reused
- Reserved even after Drawing Soft Delete

Exact existing Drawing Number blocks Create.

Similar Drawing Number produces a warning.

---

# 13. Drawing Revision Number

Revision Number is system generated.

Current format:

- RV-01
- RV-02
- RV-03

Rules:

- User does not manually enter Revision Number.
- First revision is RV-01.
- Next Revision Number is generated automatically.
- Deleted revisions remain part of the numbering sequence.
- Revision Numbers are never reused.

Legacy revision formats such as R01 and R02 remain supported while calculating the next sequence.

---

# 14. Drawing Revision Uniqueness

Permanent unique index:

DrawingNumber + RevisionNumber

Deleted revisions remain included in this uniqueness rule.

Therefore a deleted Revision Number cannot be reused.

---

# 15. Current Revision

IsActive represents the Current Revision.

For one Drawing Number:

- Many historical revisions may exist.
- Maximum one non-deleted revision may be Current.

Filtered unique index:

DrawingNumber
WHERE IsActive = 1
AND IsDeleted = 0

---

# 16. One Drawing Per Item

One Item may have only one active Drawing identity.

Database protection:

ItemId
WHERE IsActive = 1
AND IsDeleted = 0

Service validation also blocks creating a second Drawing for the same Item.

Historical revision rows are inactive and therefore allowed to reuse the same ItemId.

---

# 17. Revision Activation

Inactive previous revisions may be reactivated.

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

Revision switching is executed transactionally.

---

# 18. Revision Soft Delete

Only inactive revisions can be deleted.

Current Revision cannot be deleted directly.

Deleted Revision:

- remains in database
- disappears from normal UI
- keeps its Revision Number reserved
- retains its physical file

---

# 19. Drawing Soft Delete and Restore

Complete Drawing Delete performs Soft Delete.

Deleted Drawing:

- disappears from normal Drawing Index
- remains in database
- keeps Drawing Number reserved
- keeps revision files

A dedicated Deleted Drawings screen supports Restore.

Restore returns:

- Drawing identity
- revision history
- Current Revision

Restore is blocked if the Item already has another active Drawing.

---

# 20. Drawing File Storage

Supported formats:

- PDF
- JPG
- JPEG
- PNG
- DWG
- DXF

Maximum file size:

25 MB

Physical storage:

wwwroot/uploads/drawings

Database stores:

- Original FileName
- Relative FilePath

File binary is not stored in SQL Server.

---

# 21. Item and Drawing Integration

Item Details displays the Current Drawing linked to the Item.

Displayed information:

- Drawing Number
- Drawing Name
- Current Revision
- Drawing Type
- Current Drawing File
- Open Drawing Details

If no Drawing exists:

- Add Drawing action is displayed.

Item Edit displays a read-only Drawing summary.

Drawing data is not editable from Item Edit.

Drawing lifecycle remains controlled by the Drawing module.

---

# 22. Item Create to Drawing Flow

New Item flow:

Create Item
→ Save Item
→ Redirect to Item Details
→ Add Drawing
→ Drawing Create opens
→ Item automatically selected

This allows Drawing creation only after a valid ItemId exists.

---

# 23. Current Next Transaction Module

The next module selected for development is:

Purchase Order

Purchase Order will depend primarily on:

- Company
- Supplier
- Item
- UOM

A professional Purchase Order PDF must be generated for sharing with the Supplier.

Detailed Purchase Order database design will be finalized before coding begins.

---

# 24. Deferred Database Areas

Deferred:

- Purchase Requisition
- Goods Receipt / GRN
- Stock Ledger
- Warehouse Stock
- Opening Stock
- Minimum / Maximum Stock
- BOM
- Production
- Quality
- Sales
- Accounting
- GST transaction calculations
- Supplier balances
- Full Drawing approval workflow