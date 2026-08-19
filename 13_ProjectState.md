# 13 - Project State

## Project

Ajay Industries ERP

## Current Status

Master-data foundation is stable.

Item Master, Drawing Master and Purchase Order are finalized to the current business scope.

Current next module:

**GRN - Goods Receipt Note**

---

# Technology

- ASP.NET Core MVC
- .NET 8
- Entity Framework Core
- SQL Server
- Clean Architecture
- Repository + Service Pattern
- Bootstrap
- JavaScript / jQuery
- Select2
- Toastr
- QuestPDF

---

# Architecture Layers

1. Domain
2. Application
3. Infrastructure
4. Web

Runtime flow:

MVC View
→ Controller
→ Application Service
→ Repository
→ DbContext
→ SQL Server

---

# EF Core Configuration

Entity configurations use:

`IEntityTypeConfiguration<T>`

Location:

`Infrastructure/Configurations`

DbContext applies configurations from the Infrastructure assembly.

---

# Dependency Injection

Infrastructure registrations are maintained in:

`Infrastructure/DependencyInjection/DependencyInjection.cs`

Program.cs calls the Infrastructure registration method.

---

# Common Application Components

Available:

- BaseEntity
- BusinessException
- PagedResult<T>
- NameSimilarityHelper
- ItemConfigurationSimilarityHelper

---

# Shared UI Components / Patterns

Available:

- Search Bar
- Pagination
- Confirm Delete Modal
- Toast Notification
- common.js
- Select2
- Quick Add Master Modal
- Dynamic Item Specification rows
- Drawing revision history/actions
- Dynamic Purchase Order line rows
- State-based GST preview
- Purchase Order PDF download

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

# Company Master Current State

Company includes standard business/contact/address fields.

Current Purchase-related additions:

- GST Number is optional.
- State is required and is used for Purchase Order GST type.
- PurchaseOrderTermsAndConditions stores standard PO terms.
- Company Website is available for company/PDF presentation where configured.

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

`ITM00001`

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

# Item / Drawing Engineering State

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

Supports:

- Add Revision
- Revision History
- One Current Revision
- Activate Previous Revision
- Soft Delete Inactive Revision
- Current Revision Delete Protection
- Complete Drawing Soft Delete
- Deleted Drawing Restore
- Drawing file history

Item Details/Edit shows read-only Current Drawing information.

---

# Supplier Master Final State

Supplier supports:

- SupplierCode
- SupplierName
- ContactPerson
- Mobile
- Alternate Mobile
- Email
- Optional GSTIN
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

SupplierCode:

`SUP00001`

Supplier transaction values are not stored in Supplier Master.

---

# Purchase Order Module Final State

Purchase Order uses header + lines.

## Header

Important fields/business data:

- Id
- Code
- PODate
- ExpectedDeliveryDate
- Status
- CompanyId
- Company snapshot
- SupplierId
- Supplier snapshot
- DeliveryAddress
- PaymentTerms
- DeliveryTerms
- Remarks
- TermsAndConditions snapshot
- SubTotal
- TaxableAmount
- CGSTAmount
- SGSTAmount
- IGSTAmount
- TransportCharges
- OtherCharges
- GrandTotal
- workflow timestamps
- audit fields

Legacy compatibility fields:

- DiscountAmount = forced 0
- RoundOffAmount = forced 0

## Lines

PurchaseOrderItem stores:

- ItemId
- Item snapshot
- Specification snapshot
- UnitName snapshot
- HSNCode
- optional DrawingId
- DrawingNumber snapshot
- DrawingRevision snapshot
- Quantity
- UnitPrice
- GSTPercent
- TaxableAmount
- CGSTAmount
- SGSTAmount
- IGSTAmount
- LineTotal
- RequiredDate
- Remarks
- audit fields

Legacy compatibility fields:

- DiscountPercent = forced 0
- DiscountAmount = forced 0

---

# Purchase Order Number

Format:

`AI/PO/26-27/00001`

Financial Year:

April to March

Sequence:

Five digits.

PO Number generation belongs to PurchaseOrderService.

Deleted numbers are not reused.

---

# Purchase Order GST

Default new line GST:

18%

GST rate may be changed manually.

Tax type:

Company.State
vs
Supplier.State

Same State:

CGST + SGST

Different State:

IGST

GSTIN is optional and is not used for tax-type determination.

UI provides live preview.

Service performs authoritative calculation.

---

# Purchase Order Calculation

Final current rule:

Quantity × UnitPrice
= Taxable Amount

Taxable Amount
+ CGST / SGST or IGST
+ Transport Charges
+ Other Charges
= Grand Total

No Purchase Order Discount.

No Purchase Order Round Off.

No separate GST on Transport/Other Charges at the current business-rule version.

---

# Purchase Order Terms & Conditions

Company Master stores standard PO Terms & Conditions.

During Draft Create/Update:

Company.PurchaseOrderTermsAndConditions
→ PurchaseOrder.TermsAndConditions

The Purchase Order stores a historical snapshot.

---

# Purchase Order Workflow

Implemented:

Draft
→ Confirmed
→ Sent

Rules:

Draft:
- Edit allowed
- Delete allowed
- Confirm allowed

Confirmed:
- Edit blocked
- Delete blocked
- Mark as Sent allowed

Sent:
- Edit blocked
- Delete blocked

No separate Cancel action is implemented.

Future GRN integration will use:

- PartiallyReceived
- Received

---

# Purchase Order PDF

Implemented using QuestPDF.

PDF is supplier-facing and includes:

- Company logo
- Company details
- PO Number / Date
- Supplier + Delivery details
- Item / Specification / Drawing
- HSN
- Quantity / UOM / Rate
- GST
- Taxable / Line Total
- GST totals
- Transport / Other Charges
- Grand Total
- Remarks
- Terms & Conditions
- Prepared / Checked By
- Authorized Signatory
- footer/page number

Supplier PDF intentionally does not show Purchase Order Status.

---

# Purchase Order Stock Rule

Purchase Order does not increase stock.

Inventory impact will start from GRN/material receipt.

---

# Database Migrations Added During Purchase Order Work

Confirmed Purchase Order work includes migrations for:

- Purchase Order module
- Company reference/snapshot in Purchase Order
- Purchase Order Terms & Conditions
- Company/Purchase Order website snapshot support where applied

Exact migration class names should remain in source control as the database history of record.

---

# Git State

Core Purchase Order workflow/GST/T&C milestone has been committed.

Current milestone being finalized:

- Purchase Order PDF
- final Purchase Order documentation
- documentation cleanup

After this documentation update, commit the completed Purchase Order milestone.

---

# Current Next Module

## GRN - Goods Receipt Note

Before coding finalize:

- GRN Number format
- PO reference rule
- Supplier reference/snapshot needs
- multiple GRNs against one PO
- partial receipt
- received/pending quantity logic
- warehouse
- receipt date
- challan/invoice reference requirements
- accepted/rejected quantity if required
- PO status update
- stock transaction design
- stock ledger design
- delete/reversal rules

---

# Deferred Modules

- Purchase Requisition
- Purchase Invoice
- Purchase Return
- Full Inventory / Stock Ledger
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

---

# Development Rule

Before starting coding for a new major module:

1. Finalize requirement.
2. Finalize business flow.
3. Finalize entity/table design.
4. Finalize business rules.
5. Finalize workflow/status transitions.
6. Finalize UI.
7. Implement using the frozen architecture.
8. Test.
9. Update canonical documentation.
10. Git commit.
→ GRN Phase 1 completed
→ Next = Inventory / Stock Ledger

ACTION: UPDATE Current State
ADD:
- Customer Master = Completed
- Next Module = Customer PO / Sales Order

ACTION: UPDATE

Customer Master          = Completed
Customer Purchase Order  = Completed
Next Module              = Machine Master