# 13 - Project State

## Project

Ajay Industries ERP

---

# Current Status

ERP foundation and major Purchase / Sales operational flow are now established.

Completed major areas include:

- Core Master Data
- Item / Drawing Engineering
- Supplier Master
- Purchase Order
- GRN Phase 1
- Customer Master
- Customer Purchase Order
- Machine Master
- Production-related operational flow
- PDI / Delivery Challan integration
- Customer Invoice

Latest completed milestone:

**Customer Invoice Module**

Current next planned module:

**Customer Payment / Receipt / Accounts Receivable**

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

Database changes are managed using EF Core migrations.

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
- Dynamic transaction line rows
- State-based GST calculation/preview
- QuestPDF document generation
- Soft Delete / Restore patterns
- Draft workflow actions
- Shared Create/Edit form partials where applicable
- Separate page-specific JavaScript files

---

# Completed Masters

Completed / established:

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
- Customer
- Machine

---

# Company Master Current State

Company includes standard:

- Business information
- Contact information
- Address information
- GST information
- State
- Bank details
- Terms & Conditions fields
- Website / presentation information where configured

Company information is also used for historical transaction snapshots and PDFs.

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

Supplier transaction values are not stored permanently in Supplier Master.

Transaction documents use snapshots where historical accuracy is required.

---

# Purchase Order Module Final State

Purchase Order uses Header + Lines.

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

Current rule:

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

Company Master stores standard Purchase Order Terms & Conditions.

During Draft Create/Update:

Company.PurchaseOrderTermsAndConditions
→ PurchaseOrder.TermsAndConditions

Purchase Order stores the historical snapshot.

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

No separate Cancel action is currently implemented.

GRN integration handles receipt progression separately.

---

# Purchase Order PDF

Implemented using QuestPDF.

Supplier-facing PDF includes:

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
- Footer / Page number

Supplier PDF intentionally does not show Purchase Order Status.

---

# Purchase Order Stock Rule

Purchase Order itself does not increase physical stock.

Material receipt / GRN is responsible for inventory impact.

---

# GRN Module State

## GRN Phase 1

Completed.

GRN is connected to the Purchase Order receipt flow.

Core receipt architecture was established before moving further into later ERP modules.

Stock impact starts from material receipt rather than Purchase Order creation.

Further inventory depth such as complete stock-ledger reporting and advanced stock control can continue independently.

---

# Customer Master State

Customer Master is completed.

Customer is the base commercial master for:

- Customer Purchase Order
- Production traceability
- Delivery
- Invoice
- Future Payment / Outstanding tracking

Customer information required for historical financial documents is snapshotted into transactions where applicable.

---

# Customer Purchase Order Module State

Customer Purchase Order is completed and forms the commercial source for downstream Sales / Production flow.

Current downstream relationship:

Customer
→ Customer Purchase Order
→ Customer Purchase Order Items
→ Production Jobs
→ Invoice / Delivery related transactions

Customer Purchase Order information is retained as traceability throughout downstream transactions.

---

# Machine Master State

Machine Master is completed.

It forms part of the Production / Process foundation.

Machine information can be associated with manufacturing operations as required by the Production workflow.

---

# Production Flow Current State

Production Jobs are connected to Customer Purchase Order Items.

Important current Production Job data includes:

- Production Job Id
- Production Job Code
- Customer Purchase Order Item
- Item
- Job Quantity
- Production Steps
- Status
- Completion Date
- Active / Deleted state

Production Job completion rule is based on completion of the required active production steps.

Completed Production Jobs are used as the authoritative production source for Customer Invoice eligibility.

---

# PDI / Delivery Challan Position

PDI and Delivery Challan exist as downstream operational documents.

For the current Invoice business flow:

- PDI is NOT a mandatory prerequisite for Invoice.
- Delivery Challan is NOT a mandatory prerequisite for Invoice.
- Missing PDI or Delivery Challan generates a warning.
- User may explicitly confirm the warning and continue.
- Production must still be Completed.

Therefore:

Production Completion
= Invoice eligibility gate

PDI / Delivery Challan
= warning / operational traceability

not hard Invoice blockers.

---

# Customer Invoice Module Final State

Customer Invoice module is completed to the current business scope.

## Invoice Source Flow

Final flow:

Customer Purchase Order
→ Completed Production Jobs
→ Invoice

Invoice creation no longer depends on selecting Delivery Challans.

User selects a Customer Purchase Order.

System loads eligible Production Jobs where:

- Production Job belongs to the selected Customer PO.
- Production Job status is Completed.
- Invoiceable quantity remains available.

PDI or Delivery Challan does not need to exist for the Production Job.

---

# Invoice PDI / Delivery Challan Warning Rule

For selected Completed Production Jobs:

If PDI or Delivery Challan is missing:

System shows:

PDI / Delivery Challan Warning

User must explicitly confirm that Invoice processing may continue despite the pending PDI / Delivery Challan.

Without confirmation:

Invoice Create / Finalize operation is blocked.

With confirmation:

Invoice may continue.

The service layer performs authoritative validation.

---

# Invoice Item Source Design

Primary operational source:

`ProductionJobId`

InvoiceItem keeps Production Job traceability.

Customer Purchase Order traceability is also stored through:

- CustomerPurchaseOrderItemId
- CustomerPurchaseOrderCode
- CustomerPurchaseOrderNumber

Delivery Challan references remain available as optional / historical fields.

They are not mandatory for new Invoice creation.

This preserves compatibility with previously created Invoice records while supporting the new Production-based process.

---

# Invoice Quantity Rule

Current Invoice eligibility uses Completed Production Job quantity.

System checks:

Production Quantity
- Already Invoiced Quantity
= Available Invoice Quantity

Invoice quantity must:

- be greater than zero
- not exceed remaining available quantity

Service layer performs authoritative validation.

---

# Invoice Financial Calculation

Invoice supports:

- Quantity
- Rate
- Discount %
- Taxable Amount
- GST %
- CGST
- SGST
- IGST
- Other Charges
- Round Off
- Grand Total

Same-state transaction:

CGST + SGST

Inter-state transaction:

IGST

Browser calculation provides live preview.

Application Service performs authoritative financial calculation and validation.

---

# Invoice GST Display

Invoice Details / PDF display GST rates together with tax labels.

Examples:

Same State with GST 18%:

- CGST (9%)
- SGST (9%)

Inter State with GST 18%:

- IGST (18%)

Mixed GST rates are handled as mixed rates where applicable.

---

# Invoice Number

Format:

`AI/INV/26-27/00001`

Financial Year:

April to March

Sequence:

Five digits.

Invoice number generation belongs to the Invoice Service.

Deleted document numbers are not reused.

---

# Invoice Workflow

Current core workflow:

Draft
→ Finalized

Draft:

- Edit allowed
- Delete allowed
- Finalize allowed

Finalized:

- Edit blocked
- Delete blocked

When PDI / Delivery Challan warning exists, user confirmation is required before Finalize.

Finalized Invoice is treated as the commercial document of record.

---

# Invoice UI Architecture

Create and Edit use shared:

`_Form.cshtml`

Invoice-specific JavaScript is maintained separately:

`wwwroot/js/invoice-form.js`

The JavaScript handles:

- Customer PO selection
- Completed Production Job loading
- Production quantity display
- Already invoiced quantity
- Available quantity
- Invoice quantity validation
- Warning display
- GST calculations
- totals
- ASP.NET collection re-indexing

Business rules remain authoritative in Application Service.

---

# Invoice Details Screen

Invoice Details shows:

- Invoice information
- Customer / billing information
- Customer PO
- Production Job
- Item / Product
- HSN
- Invoice quantity
- Rate
- Discount
- GST
- Taxable amount
- Line total
- Financial summary
- Terms
- Finalization information

For Draft invoices requiring source warning:

- PDI / Delivery Challan warning is displayed.
- Explicit checkbox confirmation is required.
- Validation message is displayed when user attempts Finalize without confirmation.

---

# Invoice PDF

Implemented using QuestPDF.

PDF includes:

- Company header
- Invoice Number
- Invoice Date
- Due Date
- Customer details
- Billing Address
- Customer PO in BILL TO section
- Customer PO on Invoice Item rows
- Product information
- HSN Number
- Quantity / UOM
- Rate
- Discount
- GST
- CGST / SGST or IGST with percentage
- Financial summary
- Amount in Words
- Bank Details
- Terms & Conditions
- Remarks
- Authorized Signatory
- Footer / Page number

Delivery Challan is not shown as the primary Invoice source.

---

# Transaction Snapshot Principle

For commercial and historical documents, transaction-time values should be stored as snapshots where required.

This prevents later changes to Masters from incorrectly changing historical documents.

Examples include:

- Company snapshot
- Customer snapshot
- Supplier snapshot
- Item information snapshot
- Customer PO references
- Drawing references
- Terms & Conditions

---

# Soft Delete Principle

Where implemented:

- Records use IsDeleted.
- Active state is separately represented using IsActive where required.
- Deleted records are excluded from normal operational queries.
- Restore is supported where business rules permit.
- Transaction integrity takes priority over physical database deletion.

---

# Database Migration State

EF Core migrations are maintained as the database schema history.

Migrations have been added throughout development for completed modules and schema changes.

Latest Invoice source-flow work includes schema changes required for optional Delivery Challan references / Production-based Invoice processing where applicable.

Exact migration class names remain in source control and EF migration history as the authoritative record.

---

# Git State

Latest completed milestone:

**Invoice Production Source Flow**

Implemented, tested and committed.

The current Invoice flow has been manually tested after the latest changes.

---

# Current End-to-End Sales / Manufacturing Flow

Current implemented business flow can be represented as:

Customer
→ Customer Purchase Order
→ Production Job
→ Production Completion
→ PDI / Delivery Challan where applicable
→ Customer Invoice

For Invoice:

Production Completion is mandatory.

PDI / Delivery Challan are not mandatory Invoice blockers.

---

# Current Next Module

## Customer Payment / Receipt / Accounts Receivable

Before coding, finalize:

- Receipt Number format
- Customer selection rule
- Finalized Invoice selection
- Invoice Outstanding Amount
- Full Payment
- Partial Payment
- Multiple payments against one Invoice
- One payment against multiple Invoices, if required
- Payment Date
- Payment Mode
- Cash
- Bank Transfer
- NEFT / RTGS
- UPI
- Cheque
- Transaction / UTR / Cheque Number
- Bank reference
- Remarks
- Already Received Amount
- Remaining Balance
- Invoice payment status
- Unpaid
- Partially Paid
- Paid
- Customer-wise outstanding
- Payment Receipt PDF
- Delete / reversal rules
- Accounting integration boundary

Entity/table design must be finalized before implementation.

---

# Pending / Future Areas

Major future areas include:

- Customer Payment / Receipt
- Accounts Receivable
- Customer Outstanding
- Purchase Invoice
- Supplier Payment / Payables
- Purchase Return
- Sales Return
- Advanced Inventory / Stock Ledger reporting
- Opening Stock
- Minimum / Maximum Stock
- BOM
- Advanced Production planning
- Advanced Quality workflow
- Accounting
- GST reporting
- Supplier balances
- Customer balances
- Full Drawing approval workflow
- Management reports / dashboards

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

No major module should begin coding before its core business design is frozen.