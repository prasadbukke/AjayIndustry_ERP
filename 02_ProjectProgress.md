# 02 - Project Progress

## Project

Ajay Industries ERP

## Current Milestone

Purchase Order module completed.

Next selected module:

**GRN - Goods Receipt Note**

---

# Foundation

Status: Completed / Stable

- ASP.NET Core MVC .NET 8
- SQL Server
- Entity Framework Core
- Clean Architecture
- Repository + Service Pattern
- Dependency Injection
- BaseEntity audit fields
- BusinessException
- PagedResult
- Soft Delete pattern
- Search
- Pagination
- Toast Notifications
- Delete Confirmation
- Select2
- Quick Add Master pattern
- Name Similarity pattern

---

# Completed Master Modules

- Company ✅
- Employee ✅
- UOM ✅
- Warehouse ✅
- Item Category ✅
- Brand ✅
- Shape ✅
- Specification ✅
- Supplier ✅
- Item ✅
- Drawing ✅

---

# Item / Drawing Engineering Foundation

Status: Completed / Locked

Implemented:

- ItemCode generation
- PartNumber
- Dynamic Specifications
- Item configuration duplicate protection
- One Item → One Drawing Number → Many Revisions
- Drawing revision history
- Current revision
- Previous revision reactivation
- Inactive revision soft delete
- Drawing soft delete / restore
- Drawing file history
- Item Details / Edit drawing integration

---

# Purchase Order Module

Status: Completed

Implemented:

- Purchase Order header + line architecture
- Financial-year PO number
- Company snapshot
- Supplier snapshot
- Item snapshot
- Specification snapshot
- Optional Drawing + Revision snapshot
- HSN as purchase-specific data
- Quantity / UOM / Rate
- Default GST 18% with manual GST-rate change
- Same-state CGST + SGST
- Different-state IGST
- GST type based on Company.State vs Supplier.State
- GSTIN optional and not used for GST type
- Transport Charges
- Other Charges
- Discount disabled for Purchase Order
- Round Off disabled for Purchase Order
- Company-level standard Purchase Order Terms & Conditions
- Terms & Conditions snapshot on Purchase Order
- Draft → Confirmed → Sent workflow
- Draft-only Edit
- Draft-only Soft Delete
- Professional Purchase Order PDF using QuestPDF
- Company logo in PDF
- Supplier / Delivery details in PDF
- Item / Specification / Drawing details in PDF
- GST summary and Grand Total in PDF
- Terms & Conditions in PDF
- Authorized Signatory area
- PDF download from Purchase Order UI

---

# Purchase Order Business Rule State

Locked:

- Purchase Requisition is deferred.
- Purchase Order does not increase stock.
- GRN will be responsible for material receipt and future stock increase.
- `PartiallyReceived` and `Received` status transitions are reserved for GRN integration.
- No separate Purchase Order Cancel action is currently implemented.
- Only Draft Purchase Orders can be deleted.

---

# Next Module

## GRN - Goods Receipt Note

Primary goals to design next:

- Receive material against Purchase Order
- Support partial receipt
- Support multiple receipt events
- Received Quantity vs Pending Quantity
- Warehouse selection
- Receipt date / challan / invoice references as required
- PO status update to PartiallyReceived / Received
- Inventory stock transaction creation
- Stock ledger integration

GRN business rules and database design must be finalized before coding.

GRN Phase 1 = Completed

ACTION: Customer Master → Completed
ADD:
- Customer CRUD
- Search + Pagination
- GSTIN / Mobile / Email validation
- Shared Toast integration
- Soft Delete

ACTION: Customer Purchase Order → Completed

ADD:
- Customer PO Create / Edit / Details / Index
- Multiple Item lines using existing Item Master
- FY code AI/CPO/26-27/00001
- Draft → Confirmed workflow
- Same Customer + Same Customer PO Number duplicate block
- Search + Pagination
- Soft Delete + Deleted Orders + Restore
- Item blocks collapse for compact multi-item entry

- Machine Master → Completed
- Create / Edit / Details / Search / Pagination
- Manual Machine Status
- Soft Delete + Deleted Machines + Restore