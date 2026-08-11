# Database Design

## Database

AjayIndustriesERPDB

---

# Common Audit Fields

Every Master Table Contains

- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

---

# Company

| Column | Type |
|---------|------|
| CompanyId | int (PK) |
| CompanyCode | nvarchar(20) |
| CompanyName | nvarchar(100) |
| GstNumber | nvarchar(20) |
| PanNumber | nvarchar(20) |
| PhoneNumber | nvarchar(20) |
| Email | nvarchar(100) |
| Website | nvarchar(100) |
| ContactPerson | nvarchar(100) |
| Address | nvarchar(500) |
| City | nvarchar(100) |
| State | nvarchar(100) |
| Country | nvarchar(100) |
| PostalCode | nvarchar(20) |
| IsActive | bit |
| IsDeleted | bit |
| CreatedOn | datetime |
| CreatedBy | nvarchar(100) |
| ModifiedOn | datetime |
| ModifiedBy | nvarchar(100) |

Status

✅ Completed

---

# Upcoming Tables

## Masters

- Employee
- Customer
- Supplier
- Warehouse
- Unit
- Category
- Item
- Machine
- BillOfMaterial

## Purchase

- PurchaseRequisition
- PurchaseOrder
- GoodsReceipt
- PurchaseInvoice
- PurchaseReturn

## Inventory

- Stock
- StockAdjustment
- StockTransfer
- WarehouseStock
- StockLedger

## Production

- ProductionOrder
- MaterialIssue
- MaterialReturn
- ProductionEntry
- FinishedGoods

## Sales

- Quotation
- SalesOrder
- DeliveryChallan
- SalesInvoice
- SalesReturn
- 
Employee

Status

✅ Completed
## Finance

- Payment
- Receipt
- Expense
- OutstandingPayment

## Security

- User
- Role
- UserRole

Employee → Status Completed

## Employee

Status

✅ Completed

Features

- CRUD
- Search
- Pagination
- Auto Employee Code
- Soft Delete
- Duplicate Validation
- Toast Notification

# Item Master - Final Database Design

Last Updated: 08-Aug-2026

---

## Items

Purpose:

Stores reusable Item Master records used throughout Purchase, Inventory,
Production, BOM and future ERP transactions.

### Fields

| Field | Type | Required | Notes |
|---|---|---:|---|
| ItemId | int | Yes | Primary Key |
| ItemCode | varchar(20) | Yes | Auto generated. Example: ITM00001 |
| ItemName | varchar(150) | Yes | Item display name |
| Description | varchar(500) | No | Optional description |
| ItemCategoryId | int | Yes | FK to ItemCategories |
| BrandId | int | Yes | FK to Brands |
| UomId | int | Yes | Main Item UOM |
| ShapeId | int | No | Optional FK to Shapes |
| IsActive | bit | Yes | BaseEntity |
| IsDeleted | bit | Yes | Soft Delete |
| CreatedOn | datetime | Yes | Audit |
| CreatedBy | varchar(100) | Yes | Audit |
| ModifiedOn | datetime | No | Audit |
| ModifiedBy | varchar(100) | No | Audit |

### Indexes

- ItemCode is unique across all records, including soft-deleted records.
- ItemName is indexed but is NOT unique.
- Active Items can have the same ItemName when their Shape or Specifications differ.

### Item Code

Format:

ITM00001
ITM00002
ITM00003

Deleted Item Codes must never be reused.

---

## Shapes

Purpose:

Stores reusable physical Shape definitions.

Examples:

- Round
- Flat
- Square
- Hexagonal
- Sheet
- Plate
- Pipe

### Fields

| Field | Type | Required | Notes |
|---|---|---:|---|
| ShapeId | int | Yes | Primary Key |
| ShapeCode | varchar(20) | Yes | Auto generated |
| ShapeName | varchar(100) | Yes | Active unique name |
| Description | varchar(500) | No | Optional |
| BaseEntity Fields | - | - | Audit + Soft Delete |

### Shape Code

Format:

SHP00001
SHP00002

Deleted codes are not reused.

---

## Specifications

Purpose:

Stores reusable Specification definitions.

Examples:

- Diameter
- Thickness
- Length
- Width
- Grade
- Hardness
- Finish

### Fields

| Field | Type | Required | Notes |
|---|---|---:|---|
| SpecificationId | int | Yes | Primary Key |
| SpecificationCode | varchar(20) | Yes | Auto generated |
| SpecificationName | varchar(100) | Yes | Active unique name |
| Description | varchar(500) | No | Optional |
| BaseEntity Fields | - | - | Audit + Soft Delete |

### Specification Code

Format:

SPC00001
SPC00002

Deleted codes are never reused.

---

## ItemSpecifications

Purpose:

Stores dynamic technical Specification values assigned to Items.

Example:

Item: MS Round Bar

- Diameter = 25 MM
- Length = 6000 MM
- Grade = EN8

### Fields

| Field | Type | Required | Notes |
|---|---|---:|---|
| ItemSpecificationId | int | Yes | Primary Key |
| ItemId | int | Yes | FK to Items |
| SpecificationId | int | Yes | FK to Specifications |
| SpecificationValue | varchar(200) | Yes | Text/numeric value stored as text |
| UomId | int | No | Optional FK to Uoms |
| SortOrder | int | Yes | Display order |
| BaseEntity Fields | - | - | Audit + Soft Delete |

### Constraints

An active Item cannot contain the same Specification more than once.

Unique active combination:

ItemId + SpecificationId

Soft-deleted ItemSpecification rows are excluded from this uniqueness rule.

---

# Item Duplicate Identity

ItemName alone is NOT considered unique.

Final duplicate validation uses:

ItemName
+ Shape
+ Complete Specification Set

Each Specification comparison includes:

SpecificationId
+ SpecificationValue
+ Specification UOM

Specification row order is ignored.

Example:

MS Round Bar
Shape = Round
Diameter = 25 MM
Grade = EN8

and

MS Round Bar
Shape = Round
Grade = EN8
Diameter = 25 MM

represent the same Item configuration.

However:

MS Round Bar
Shape = Round
Diameter = 30 MM
Grade = EN8

is a different Item.

---

# Data Not Stored In Item Master

The following information is intentionally NOT stored directly in Items:

- Warehouse stock
- Opening stock
- Minimum stock
- Maximum stock
- Reorder stock
- GST rates
- HSN-based tax configuration
- Purchase price
- Sales price
- Supplier-specific pricing

These will be maintained through dedicated Inventory, Tax, Pricing,
Supplier and Transaction modules.