# 13 - Project State

## Project

Ajay Industries ERP

## Current Status

Core Master-data foundation is under active development.

The project is using:

- ASP.NET Core MVC
- .NET 8
- Entity Framework Core
- SQL Server
- Clean Architecture
- Repository + Service pattern
- Soft Delete
- Bootstrap
- jQuery
- Select2
- Toastr

---

# Architecture

Layers:

1. Domain
2. Application
3. Infrastructure
4. Web

Entity configurations use:

`IEntityTypeConfiguration<T>`

DbContext applies configurations using:

`ApplyConfigurationsFromAssembly`

Dependency Injection registrations are maintained in:

`Infrastructure/DependencyInjection/DependencyInjection.cs`

---

# Shared Application Components

Available/reused patterns include:

- `PagedResult<T>`
- `BusinessException`
- `NameSimilarityHelper`
- `ItemConfigurationSimilarityHelper`
- Shared Search Bar
- Shared Pagination
- Shared Confirm Delete Modal
- Shared Toast Notification
- `common.js`
- Select2
- Quick Add modal infrastructure

---

# Completed Masters

The following modules are completed:

- Company
- Employee
- UOM
- Warehouse
- Item Category
- Brand
- Shape
- Specification
- Item
- Supplier
- Drawing

---

# Item Master State

Item Master currently supports:

- Auto ItemCode
- ItemName
- Category
- Brand
- Main UOM
- Optional Shape
- Dynamic Specifications
- Specification Value
- Optional Specification UOM
- Description
- PartNumber database field
- ImagePath database field

ItemCode format:

`ITM00001`

ItemCode is permanent system identity.

Item Name is not unique.

Item duplicate signature:

- ItemName
- Shape
- Complete Specification set

Category, Brand and main UOM are excluded from duplicate signature.

Dynamic Item Specification UI is complete.

Item search supports:

- Item Code
- Item Name
- Description
- Category
- Brand
- Main UOM
- Shape
- Specification Code
- Specification Name
- Specification Value
- Specification UOM

Current Item list shows compact Specification summary.

---

# Supplier Master State

Supplier Master is completed and tested.

Fields include:

- Supplier Code
- Supplier Name
- Contact Person
- Mobile
- Alternate Mobile
- Email
- GSTIN
- PAN
- Address
- City
- State
- Pincode
- Payment Terms Days
- Description
- Status

SupplierCode:

`SUP00001`

Rules:

- exact Supplier Name blocked
- similar Supplier Name warning
- live similarity checking
- GSTIN optional but unique among active records
- PAN not unique
- soft delete
- deleted Supplier Code not reused
- search complete

---

# Drawing Master State

Drawing Master is completed, tested and considered stable.

Final relationship:

`One Item -> One Drawing Number -> Many Revisions`

Drawing Number:

- manual
- permanent
- cannot change after Create
- never reused
- live duplicate/similarity checking

Drawing Name:

- similar spelling warning
- not unique

Revision Number:

- auto generated
- format `RV-01`
- deleted revision numbers never reused

Revision capabilities:

- Add Revision
- View complete history
- Activate previous revision
- Soft delete inactive revision
- Only one Current revision
- Current revision cannot be deleted directly

Drawing capabilities:

- soft delete complete Drawing
- Deleted Drawings screen
- Restore deleted Drawing
- revision history restored
- same Drawing Number cannot be recreated after deletion

Drawing file support:

- PDF
- JPG
- JPEG
- PNG
- DWG
- DXF
- maximum 25 MB

Storage:

`wwwroot/uploads/drawings`

Database stores FileName and FilePath.

`IsPrimary` has been removed from Drawing architecture.

---

# Drawing UI

Drawing Index:

- Current revision only
- Drawing Number clickable
- search
- edit
- delete

Drawing Details:

- 2-column Current Drawing information
- full Revision History
- Activate action
- Delete Revision action
- file links

Drawing Edit:

- Item read-only
- Drawing Number read-only
- Drawing Name editable
- Drawing Type editable
- revision history
- Add Revision
- Activate previous revision
- Delete inactive revision

Deleted Drawings:

- one entry per Drawing Number
- Restore action

---

# Current Database Rules

Drawing:

- DrawingNumber + RevisionNumber unique
- one Current row per DrawingNumber
- one Current Drawing row per Item
- deleted Drawing Number reserved
- deleted Revision Number reserved

Item Specification:

- active ItemId + SpecificationId unique

Supplier:

- SupplierCode unique
- active SupplierName unique
- active GSTIN unique when GSTIN is provided

---

# Current Next Work

Next major task:

## Item Master Enhancement

Implement:

1. PartNumber in Item UI
2. Item image upload
3. Item image display
4. Item Details Drawing section
5. Current Drawing Number display
6. Current Drawing Revision display
7. Drawing file access from Item Details

After that:

- Item + Drawing integration testing
- documentation update
- Git commit

---

# Deferred Modules

Not yet started:

- Purchase
- Purchase Order
- Goods Receipt
- Stock
- Warehouse Stock
- Stock Ledger
- Supplier Transactions
- BOM
- Production
- Quality
- Sales
- Accounting
- GST transaction logic

---

# Important Development Rule

Before starting a new major transaction module:

- current master architecture should be locked
- docs should be updated
- migration should be applied
- runtime testing should pass
- Git commit should be created