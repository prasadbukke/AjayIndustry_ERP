# 13 - Project State

## Project

Ajay Industries ERP

## Current Status

Master-data foundation is stable.

Item Master and Drawing Master have been finalized and committed.

Next selected module:

Purchase Order

---

# Technology

- ASP.NET Core MVC
- .NET 8
- Entity Framework Core
- SQL Server
- Clean Architecture
- Repository + Service Pattern
- Bootstrap
- jQuery
- Select2
- Toastr

---

# Architecture Layers

1. Domain
2. Application
3. Infrastructure
4. Web

---

# EF Core Configuration

Entity configurations use:

IEntityTypeConfiguration<T>

Location:

Infrastructure/Configurations

DbContext applies configurations using:

ApplyConfigurationsFromAssembly

---

# Dependency Injection

Infrastructure registrations are maintained in:

Infrastructure/DependencyInjection/DependencyInjection.cs

Program.cs is not used as the main location for module registrations.

---

# Common Application Components

Available:

- BaseEntity
- BusinessException
- PagedResult<T>
- NameSimilarityHelper
- ItemConfigurationSimilarityHelper

---

# Shared UI Components

Available:

- Search Bar
- Pagination
- Confirm Delete Modal
- Toast Notification
- common.js
- Select2
- Quick Add Master Modal

---

# Completed Masters

Finalized:

- Company
- Employee
- UOM
- Warehouse
- Item Category
- Brand
- Shape
- Specification
- Supplier
- Item
- Drawing

---

# Item Master Final State

Item supports:

- ItemCode
- ItemName
- PartNumber
- Description
- Category
- Brand
- Main UOM
- Shape
- Dynamic Specifications
- Specification Value
- Specification UOM
- Active/Inactive
- Soft Delete
- Search
- Pagination

ItemCode:

ITM00001

System generated and never reused.

---

# Item Duplicate Rule

ItemName is not globally unique.

Exact configuration duplicate:

ItemName
+ Shape
+ Complete Specifications

Specification order is ignored.

PartNumber does not participate in duplicate identity.

---

# PartNumber

Final rules:

- Optional
- Maximum 100 characters
- Not unique
- Searchable
- Create/Edit supported
- Index visible
- Details visible

---

# Item Image

Item Image support is not used.

ImagePath was removed from Entity and Database.

Technical Item identification uses Drawing and structured Item information instead.

---

# Item Specifications

Dynamic child collection.

Supports:

- Specification
- Value
- Optional UOM
- SortOrder
- Add
- Remove
- Quick Add Specification
- Quick Add UOM

Same Specification cannot appear twice on one active Item.

---

# Item UI

Item Index:

- Item Code
- Item Name
- Part Number
- Category
- Brand
- UOM
- Shape
- Specification summary
- Status
- Actions

Item Details:

Three-column information layout.

Item Edit:

Supports complete Item editing and read-only Drawing summary.

---

# Supplier Master Final State

Supplier supports:

- SupplierCode
- SupplierName
- ContactPerson
- Mobile
- Alternate Mobile
- Email
- GSTIN
- PAN
- Address
- City
- State
- Pincode
- PaymentTermsDays
- Description
- Active/Inactive
- Soft Delete
- Search
- Similar Name Detection

SupplierCode format:

SUP00001

---

# Drawing Master Final State

Final relationship:

One Item
→ One Drawing Number
→ Many Revisions

Drawing Number:

- Manual
- Permanent
- Immutable
- Never reused

Revision:

- Auto generated
- RV-01, RV-02, ...
- Never reused

---

# Drawing Revision Workflow

Supports:

- Add Revision
- Revision History
- One Current Revision
- Activate Previous Revision
- Soft Delete Inactive Revision
- Current Revision Delete Protection

---

# Drawing Delete and Restore

Complete Drawing supports:

- Soft Delete
- Deleted Drawings screen
- Restore

Deleted Drawing Number remains reserved.

Historical files remain preserved.

---

# Drawing Files

Supported:

- PDF
- JPG
- JPEG
- PNG
- DWG
- DXF

Maximum:

25 MB

Storage:

wwwroot/uploads/drawings

---

# Item and Drawing Integration

Item Details displays Current Drawing information.

Displayed:

- Drawing Number
- Drawing Name
- Current Revision
- Drawing Type
- Current Drawing File
- Open Drawing Details

If no Drawing exists:

Add Drawing button is displayed.

---

# Item Edit Drawing Integration

Item Edit shows Drawing summary.

Drawing data is read-only.

Drawing revision lifecycle remains managed by Drawing Master.

---

# New Item to Drawing Flow

Create Item
→ Save
→ Redirect to Item Details
→ Add Drawing
→ Drawing Create
→ Item automatically selected

---

# Git State

Drawing Master milestone committed.

Item Master + Drawing integration milestone committed.

Latest functional milestone commit:

Finalize item master with part number and drawing integration

Documentation update should be committed separately.

---

# Current Next Module

## Purchase Order

Purchase Order is the next development module.

Main goals:

- Select Supplier
- Create PO Header
- Add multiple Item lines
- Quantity
- UOM
- Rate
- Tax calculations
- Delivery information
- Payment Terms
- Remarks
- PO status/lifecycle
- Professional Purchase Order PDF
- PDF suitable for sending to Supplier

---

# Purchase Order Design Status

Coding has not started.

Before coding, finalize:

- PO Number format
- Header fields
- Line fields
- GST/tax structure
- Supplier address snapshot strategy
- Company details strategy
- Delivery terms
- Payment terms
- Status
- Revision/amendment requirements
- PDF layout
- Delete/cancel rules

---

# Deferred Modules

Deferred:

- Purchase Requisition
- GRN
- Purchase Receipt
- Warehouse Stock
- Stock Ledger
- Opening Stock
- Minimum / Maximum Stock
- BOM
- Production
- Quality
- Sales
- Accounting
- GST reporting

---

# Development Rule

Before starting coding for a new major module:

1. Finalize business rules.
2. Finalize Entity/table design.
3. Finalize workflow.
4. Update architecture decisions if required.
5. Then implement using the standard module pattern.