# 09 - Sprint Log

## Ajay Industries ERP

---

# Foundation Completed

Technology foundation:

- ASP.NET Core MVC
- .NET 8
- Entity Framework Core
- SQL Server
- Clean Architecture
- Repository + Service Pattern

Shared infrastructure established:

- BaseEntity
- Soft Delete
- BusinessException
- PagedResult
- Search
- Pagination
- Toast Notifications
- Delete Confirmation
- Name Similarity
- Select2
- Quick Add Masters

---

# Completed Master Modules

Completed and working:

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

# Item Master Development

Implemented:

- Auto ItemCode
- ItemName
- Category
- Brand
- Main UOM
- Optional Shape
- Description
- Dynamic Specifications
- Specification Value
- Optional Specification UOM
- Quick Add Master support
- Similar Item Name warning
- Exact configuration duplicate protection
- Specification-aware search
- Compact Specification display

Final Item duplicate identity:

ItemName
+ Shape
+ Complete Specifications

---

# Item Part Number Enhancement

PartNumber was added and finalized.

Rules:

- Optional
- Maximum 100 characters
- Not unique
- Searchable
- Create/Edit supported
- Index display supported
- Details display supported

PartNumber is not part of Item duplicate configuration.

---

# Item Image Cleanup

ImagePath had previously been introduced as a possible Item field.

After reviewing actual ERP usage, Item Image was considered unnecessary.

ImagePath was removed from:

- Item Entity
- Configuration
- Database
- Snapshot/current model

Reason:

Engineering identification is already provided through Drawing and technical Item data.

---

# Item UI Finalization

Item Details was improved to a clean three-column layout.

Final Item Information arrangement:

- Item Code
- Item Name
- Part Number
- Category
- Brand
- UOM
- Shape
- Status
- Description

Item Index now includes Part Number.

Specification rendering in Item Index was corrected to display compact technical badges correctly.

---

# Supplier Master Sprint

Completed:

- Supplier Code generation
- Supplier Name
- Contact information
- GSTIN
- PAN
- Address
- Payment Terms
- Similar spelling detection
- Duplicate validation
- Search
- Pagination
- Soft Delete

SupplierCode format:

SUP00001

---

# Drawing Master Sprint

Final relationship:

One Item
→ One Drawing Number
→ Many Revisions

Completed:

- Drawing Number
- Drawing Name
- Drawing Type
- Item relationship
- File upload
- Search
- Details
- Edit
- Soft Delete

---

# Drawing Revision Workflow

Implemented:

- Auto RV-01
- Auto next Revision
- Revision History
- Current / Inactive state
- Activate Previous Revision
- Delete Inactive Revision
- Current Revision protection

Deleted Revision Numbers remain reserved.

---

# Drawing Soft Delete / Restore

Complete Drawing Soft Delete was implemented.

Deleted Drawing Number remains reserved.

Dedicated Deleted Drawings UI implemented.

Restore functionality implemented.

Restore returns revision history and Current Revision.

---

# Drawing IsPrimary Cleanup

Earlier Drawing design contained IsPrimary.

Final One-Item-One-Drawing architecture made IsPrimary unnecessary.

IsPrimary was removed from:

- Entity
- Configuration
- Repository
- Service
- ViewModel
- Controller
- UI
- Database

---

# Item and Drawing Integration Sprint

Completed integration:

Item Details:

- Drawing Number
- Drawing Name
- Current Revision
- Drawing Type
- Drawing File
- Open Drawing Details

Item Edit:

- Read-only Drawing summary
- Add Drawing action when no Drawing exists

Item Create:

- Displays Save-first Drawing message

After Item Save:

- User is redirected to Item Details

Add Drawing:

- Opens Drawing Create
- Item is automatically selected

Second Drawing for the same Item remains blocked.

---

# Testing Completed

Successfully tested:

- Item Create
- Item Edit
- PartNumber
- PartNumber Search
- Duplicate PartNumber allowed
- Dynamic Specifications
- Item Details
- Drawing Create
- Drawing Revision Add
- Drawing Revision Activate
- Drawing Revision Delete
- Auto Revision Number
- Drawing Soft Delete
- Drawing Restore
- Item to Drawing navigation
- Add Drawing from Item
- Item auto-selection in Drawing Create
- Same Item second Drawing blocked

---

# Git Milestones

Drawing Master milestone committed.

Item Master + Drawing integration milestone committed.

Latest milestone commit:

Finalize item master with part number and drawing integration

---

# Next Sprint

Next selected module:

Purchase Order

Primary requirements:

- Supplier-based Purchase Order
- Multiple Item lines
- Quantity
- Rate
- Tax structure
- Delivery details
- Terms
- PO lifecycle
- Professional Purchase Order PDF
- PDF suitable for sending to Supplier

Purchase Requisition is deferred for now.

Detailed Purchase Order design will be finalized before coding.