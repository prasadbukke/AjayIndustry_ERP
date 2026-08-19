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

---

# 23. Company Purchase Order Additions

Company Master now supports Purchase-related reusable presentation data.

Relevant fields include:

- State
- optional GstNumber
- Website
- PurchaseOrderTermsAndConditions

Rules:

- GST Number is optional.
- Company State is required for Purchase Order GST type.
- Standard Purchase Order Terms & Conditions are maintained once in Company Master.

---

# 24. Purchase Order Header Table

## PurchaseOrders

Important fields:

- Id
- Code
- PODate
- ExpectedDeliveryDate
- Status
- CompanyId
- CompanyName
- CompanyAddress
- CompanyState
- CompanyGSTIN
- CompanyPhone
- CompanyEmail
- CompanyWebsite
- SupplierId
- SupplierName
- SupplierAddress
- SupplierGSTIN
- SupplierContactPerson
- SupplierPhone
- SupplierEmail
- DeliveryAddress
- PaymentTerms
- DeliveryTerms
- Remarks
- TermsAndConditions
- SubTotal
- DiscountAmount
- TaxableAmount
- CGSTAmount
- SGSTAmount
- IGSTAmount
- TransportCharges
- OtherCharges
- RoundOffAmount
- GrandTotal
- ConfirmedOn
- SentToSupplierOn
- ClosedOn
- CancelledOn
- CancellationReason
- BaseEntity audit/status fields

Business compatibility note:

- `DiscountAmount` remains in the schema but is forced to `0`.
- `RoundOffAmount` remains in the schema but is forced to `0`.
- Current UI/business flow does not use Purchase Order Discount or Round Off.

---

# 25. Purchase Order Number

Format:

`AI/PO/26-27/00001`

Rules:

- Generated in PurchaseOrderService.
- Financial Year is April to March.
- Five-digit sequence.
- Unique.
- Deleted Purchase Order numbers are not reused.
- Repository last-code lookup is prefix-based by Financial Year.

---

# 26. Purchase Order Item Table

## PurchaseOrderItems

Important fields:

- Id
- Code
- PurchaseOrderId
- ItemId
- ItemCode
- ItemName
- Description
- Specification
- UnitName
- HSNCode
- DrawingId (nullable)
- DrawingNumber
- DrawingRevision
- Quantity
- UnitPrice
- DiscountPercent
- DiscountAmount
- TaxableAmount
- GSTPercent
- CGSTAmount
- SGSTAmount
- IGSTAmount
- LineTotal
- RequiredDate
- Remarks
- BaseEntity audit/status fields

Compatibility note:

- `DiscountPercent` and line `DiscountAmount` remain in the schema but are forced to `0`.

---

# 27. Purchase Order Relationships

PurchaseOrders:

- CompanyId → Companies
- SupplierId → Suppliers

PurchaseOrderItems:

- PurchaseOrderId → PurchaseOrders
- ItemId → Items
- DrawingId → Drawings (optional)

Master foreign keys use Restrict where historical transaction integrity requires it.

PurchaseOrder → PurchaseOrderItems is a parent-child relationship.

Application behavior uses Soft Delete.

---

# 28. Purchase Order Snapshot Strategy

Purchase Order stores historical snapshots.

Company snapshot:

- Name
- Address
- State
- optional GSTIN
- Phone
- Email
- Website

Supplier snapshot:

- Name
- Address
- optional GSTIN
- Contact Person
- Phone
- Email

Item line snapshot:

- Item Code
- Item Name
- Description
- Specification
- UOM name
- Drawing Number / Revision where selected

Terms snapshot:

Company.PurchaseOrderTermsAndConditions
→ PurchaseOrder.TermsAndConditions

Reason:

Historical PO and PDF output must remain stable even when Master data changes later.

---

# 29. Purchase Order GST Design

GST rate is stored per Purchase Order line.

Default UI value:

18%

Tax type is determined from:

Company.State
vs
Supplier.State

Same State:

CGST + SGST

Different State:

IGST

GSTIN is optional and is not used to determine GST type.

Final values are calculated in PurchaseOrderService.

---

# 30. Purchase Order Calculation

Line:

Quantity × UnitPrice
= TaxableAmount

Tax is calculated from TaxableAmount.

LineTotal:

TaxableAmount
+ CGST
+ SGST
+ IGST

Header GrandTotal:

TaxableAmount
+ CGSTAmount
+ SGSTAmount
+ IGSTAmount
+ TransportCharges
+ OtherCharges

Current rule:

- no Purchase Order Discount
- no Round Off
- no separate GST on Transport / Other Charges

---

# 31. Purchase Order Workflow Fields

Implemented workflow:

Draft
→ Confirmed
→ Sent

Receipt-related enum values exist for future GRN integration:

- PartiallyReceived
- Received

Additional enum/workflow fields may exist for future lifecycle support, but no separate Cancel action is currently implemented in the UI/business flow.

Only Draft Purchase Orders can currently be Soft Deleted.

---

# 32. Purchase Order PDF

PDF generation is implemented using QuestPDF.

PDF reads the saved Purchase Order transaction/snapshot data.

Supplier-facing PDF includes:

- Company logo/details
- PO number/date
- Supplier/delivery details
- Item/specification/drawing
- HSN
- quantity/UOM/rate
- GST
- taxable/line total
- tax summary
- transport/other charges
- grand total
- remarks
- Terms & Conditions
- authorized signatory

Status is intentionally not printed on the supplier PDF.

---

# 33. Current Next Transaction Module

Next selected module:

**GRN - Goods Receipt Note**

GRN will be responsible for future:

- material receipt against Purchase Order
- partial/full receipt
- received/pending quantity
- Warehouse receipt
- PO receipt status update
- Inventory Stock Transaction
- Stock Ledger integration

Detailed GRN database design is not yet finalized.

---

# 34. Deferred Database Areas

Deferred:

- Purchase Requisition
- Purchase Invoice
- Purchase Return
- Full Warehouse Stock
- Stock Ledger
- Opening Stock
- Minimum / Maximum Stock
- BOM
- Production
- Quality
- Sales
- Accounting
- GST reporting
- Supplier balances
- Full Drawing approval workflow

→ GoodsReceiptNotes
→ GoodsReceiptNoteItems
→ Supplier + Challan uniqueness rule
→ PO/POItem relationships