# 09 - Sprint Log

## Ajay Industries ERP

Last Updated: 28-Aug-2026

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
- Customer
- Item
- Drawing
- Machine
- Production Operation

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

# Customer Master Sprint

Customer Master Phase 1 completed and runtime tested.

Implemented:

- Customer master registration
- Customer identity and contact information
- GST / tax related information
- Billing address information
- Payment Terms
- Credit Days
- Search
- Pagination
- Soft Delete
- Restore
- Customer reuse in sales transactions

Customer Master is now reused by:

- Customer Purchase Order
- Production traceability
- Delivery Challan
- Invoice

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

# Item / Drawing Testing

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

# Purchase Order Design Sprint

Final Purchase Order architecture:

PurchaseOrder
→ Header

PurchaseOrderItem
→ Multiple lines

Implemented business design:

- Company reference + snapshot
- Supplier reference + snapshot
- Item reference + snapshot
- Specification snapshot
- Optional Drawing reference
- Drawing Number / Revision snapshot
- Purchase-specific HSN
- Quantity
- UOM snapshot
- Unit Price
- GST
- Transport Charges
- Other Charges
- Grand Total
- Terms & Conditions snapshot

PO Number format:

`AI/PO/26-27/00001`

Financial Year:

April to March

Sequence uses five digits.

PO numbers are never reused.

---

# Purchase Order GST Sprint

Implemented:

- Default GST 18%
- Manual GST percentage change per line
- Company.State vs Supplier.State tax-type detection
- Same State → CGST + SGST
- Different State → IGST
- Live Create/Edit GST preview
- Service-layer authoritative GST calculation

GSTIN is optional and is not used to determine GST type.

---

# Purchase Order Commercial Rule Cleanup

Final Purchase Order rules:

- Discount is not used.
- Round Off is not used.
- Existing database fields are retained for compatibility and forced to zero.
- Line Total remains.
- Transport Charges remain.
- Other Charges remain.
- No separate GST is currently calculated on Transport / Other Charges.

Final amount logic:

Quantity × Rate
→ Taxable Amount
→ GST
→ Transport Charges
→ Other Charges
→ Grand Total

---

# Purchase Order Terms & Conditions Sprint

Company Master was extended with standard Purchase Order Terms & Conditions.

Flow:

Company.PurchaseOrderTermsAndConditions
→ PurchaseOrder.TermsAndConditions snapshot

Historical Purchase Orders keep their saved Terms & Conditions even if Company Master changes later.

Company GST Number was made optional.

---

# Purchase Order Workflow Sprint

Implemented workflow:

Draft
→ Confirmed
→ Sent

Rules:

- Draft can be edited.
- Draft can be soft deleted.
- Confirmed/Sent cannot be edited.
- Confirmed/Sent cannot be deleted through normal workflow.
- No separate Cancel action is currently implemented.
- PartiallyReceived / Received are reserved for receipt integration.

Purchase Order itself does not directly update inventory stock.

---

# Purchase Order PDF Sprint

Professional supplier PDF implemented using QuestPDF.

PDF includes:

- Company logo
- Company information
- PO Number / PO Date
- Supplier & Delivery grid
- Item / Specification / Drawing
- HSN
- Qty / UOM / Rate
- GST %
- Taxable / Line Total
- CGST / SGST / IGST
- Transport Charges
- Other Charges
- Grand Total
- Remarks
- Terms & Conditions
- Prepared / Checked By
- Authorized Signatory
- Footer / page number

PDF status is intentionally not displayed.

PDF layout was compared against the approved Purchase Order format and refined.

---

# Purchase Order Testing

Successfully tested:

- Create
- Edit Draft
- Details
- Draft delete
- Confirm
- Mark as Sent
- Item add/remove
- Drawing / no Drawing
- Specification display
- Default GST
- Manual GST rate change
- Same-state GST split
- Different-state GST split
- Supplier change live tax-type refresh
- Transport Charges
- Other Charges
- Terms & Conditions snapshot
- PDF generation
- PDF download

---

# GRN - Goods Receipt Note Sprint

GRN Phase 1 implemented.

Core flow:

Purchase Order
→ Goods Receipt Note
→ Purchase Order Item receipt quantities

GRN Number format:

`AI/GRN/26-27/00001`

Financial Year:

April to March

Implemented:

- Select eligible Purchase Order
- Load all Purchase Order items
- Supplier reference
- Supplier Challan Number
- Supplier Challan Date
- Ordered Quantity
- Previously Received Quantity
- Balance Quantity
- Receipt Status
- Received Quantity
- Pending Quantity
- Material Status
- Item-level Remarks
- GRN Remarks
- Trusted PO snapshot preparation
- Financial-year based GRN numbering

Receipt statuses:

- Not Received
- Partial Received
- Full Received

Partial receipt logic:

Ordered Quantity
- Previously Received Quantity
= Balance Quantity

Balance Quantity
- Received Now
= Pending Quantity

Full Received automatically receives the complete remaining balance.

At least one PO Item must actually be Partial Received or Full Received.

---

# GRN Material Status

Material Status is required only when material is actually received.

Supported Phase 1 statuses:

- Approved
- Rejected
- Failure
- Return

Not Received:

- Material Status remains empty

Partial / Full Received:

- Material Status is required

---

# GRN Phase 1 Important Limitation

GRN Phase 1 currently records receipt and material status information.

It does NOT yet perform final Inventory Stock effects.

Current Phase 1 rules:

- Stock is not updated by GRN.
- Purchase Order status is not automatically changed by GRN.
- Approved / Rejected / Failure / Return are stored.
- Their final Inventory / PO effects will be implemented during Inventory integration.

This separation is intentional.

---

# Customer Purchase Order Sprint

Customer Purchase Order Phase 1 completed and runtime tested.

Architecture:

CustomerPurchaseOrder
→ Header

CustomerPurchaseOrderItem
→ Multiple Customer Order lines

Internal ERP Customer PO Code:

`AI/CPO/26-27/00001`

External Customer PO Number is stored separately.

---

# Customer Purchase Order Business Rules

Implemented:

- Customer Master selection
- Customer PO Number
- Customer PO Date
- Received Date
- Required Delivery Date
- Priority
- Customer Reference
- Remarks
- Multiple Item lines
- Customer-specific Item Code
- Customer Drawing Number
- Customer Drawing Revision
- Item Delivery Date
- Item Priority
- Item Remarks
- Trusted Item Master snapshots
- Item specification snapshot
- UOM snapshot
- Search
- Pagination

Duplicate protection:

Same Customer
+ Same Customer PO Number
→ Not allowed

Similar Customer PO Number:

- Warning is shown.
- User confirmation can allow continuation when it is not an exact duplicate.

---

# Customer Purchase Order Workflow

Workflow:

Draft
→ Confirmed

Rules:

- Draft can be edited.
- Confirmed PO cannot be edited.
- Confirmed PO becomes available for Production planning.
- Soft Delete supported.
- Deleted Customer PO can be restored.
- Restore preserves original transaction status.

Customer / Item browser-posted snapshot values are not trusted.

Application Service reloads authoritative Master data before saving.

---

# Customer Purchase Order Item Design

Customer PO line stores order-specific information independently from Item Master.

Examples:

- Customer Item Code
- Customer Drawing Number
- Customer Drawing Revision
- Ordered Quantity
- Required Delivery Date
- Priority
- Remarks

Same Item can exist on multiple Customer PO lines when order-specific references differ.

Production Machine or Production Pipeline information is intentionally not stored in Customer PO.

---

# Machine Master Sprint

Machine Master Phase 1 completed and runtime tested.

Purpose:

- Maintain manufacturing machine records.
- Make machines available for Routing defaults.
- Make machines available during Production execution.

Machine selection during actual Production execution may differ from the default Routing machine.

---

# Production Operation Sprint

Production Operation Master implemented for manufacturing process definition.

Purpose:

Reusable manufacturing operations used by:

Item Process Routing
→ Production Job Steps
→ Production Execution

Operations are selected inside Item Process Routing and copied into Production Job execution snapshots.

---

# Item Process Routing Sprint

Item Process Routing implemented.

Purpose:

Define reusable manufacturing process templates for Items.

Architecture:

Item
→ Item Process Routing
→ Routing Steps

Each Routing Step can contain:

- Sequence Number
- Production Operation
- Optional Default Machine
- Setup Time
- Cycle Time
- Operation Instruction
- Remarks

Default Machine is optional.

Actual Machine can be selected during Production execution.

---

# Routing Revision / Release Concept

Production uses the current Released Routing for the Item.

Important architecture rule:

Routing
= reusable manufacturing template

Production Job
= actual manufacturing transaction

When Production Job is created:

Released Routing Header
+ Routing Steps
→ copied into Production Job snapshot

Future changes to Routing do not modify existing Production Jobs.

This protects manufacturing history.

---

# Production Job Sprint

Production Job module implemented.

Production Job Code:

`AI/PJOB/26-27/00001`

Source:

Confirmed Customer PO Item
→ Production Job

Implemented:

- Customer PO Item source
- Remaining production quantity calculation
- Production Job Quantity
- Released Routing auto-selection
- Routing snapshot
- Routing Step snapshot
- Planned Start
- Planned Completion
- Production Job Remarks
- Search
- Pagination
- Details / Audit screen
- Production Pipeline screen
- Machine assignment
- Good Quantity
- Rejected Quantity
- Execution Remarks
- Step History

---

# Production Quantity Allocation

Production Job quantity cannot exceed remaining Customer PO Item quantity.

Logic:

Customer PO Ordered Quantity
- Quantity already allocated to Production Jobs
= Remaining Production Quantity

Production Job cannot exceed this remaining quantity.

Cancelled Production Jobs do not permanently consume usable production allocation.

---

# Production Job Workflow

Implemented statuses:

Draft
→ Ready
→ In Progress
→ Completed

Additional status:

Cancelled

Rules:

- Draft Job can be prepared and marked Ready.
- Production execution occurs step-by-step.
- Machine can be assigned during execution.
- Step execution records actual production information.
- Production Job becomes Completed after active production steps are completed.
- Cancellation reason is stored when Job is cancelled.

Production Details acts as read-only manufacturing audit history.

Pipeline screen handles execution actions.

---

# Production Job Drawing Traceability

Production Job Details includes current Item Drawing information.

Displayed information includes:

- Drawing Number
- Drawing Revision
- Drawing Name
- Drawing Type
- Drawing File

Production Job also preserves Routing revision information used for the actual manufacturing transaction.

---

# PDI - Pre-Dispatch Inspection Sprint

PDI / Pre-Dispatch Inspection flow was introduced after Production.

Purpose:

Completed Production Job
→ Inspection
→ Dispatch release reference

PDI design includes traceability for:

- Customer
- Customer PO
- ERP Customer PO Code
- Production Job
- Item
- Job Quantity
- Inspection Quantity
- Drawing references
- Technical / dimensional inspection
- Accepted Quantity
- Rejected Quantity
- Overall Result
- Inspection Remarks
- Approval / Release

PDI document numbering follows Financial Year format.

Example:

`AI/PDI/26-27/00001`

Finalized PDI represents the locked inspection record.

---

# Delivery Challan Sprint

Delivery Challan module was introduced for dispatch processing.

Purpose:

Production / Dispatch source
→ Customer Delivery Challan

Delivery Challan keeps transaction traceability to:

- Customer
- Customer PO
- Customer PO Item
- Production Job
- Item / Product
- Dispatch Quantity

Delivery Challan can be finalized as a dispatch document.

Delivery Challan information remains available for historical traceability even though Invoice no longer requires Delivery Challan as its mandatory source.

---

# Invoice Initial Design

Invoice module was initially designed as:

Finalized Delivery Challan
→ Invoice

This initial design required finalized Delivery Challan lines as the trusted source.

During final business review this requirement was changed.

The old Delivery Challan-only Invoice flow is no longer the active business process.

---

# Invoice Final Source Flow Sprint

Final Invoice business flow:

Customer Purchase Order
→ Completed Production Jobs
→ Invoice

Production completion is the authoritative eligibility gate.

Invoice creation no longer requires:

- Finalized PDI
- Delivery Challan

Customer selects Customer PO on Invoice.

The system loads Production Jobs from that Customer PO where:

`ProductionJob.Status = Completed`

Completed Production Jobs become invoiceable even when PDI or Delivery Challan is not available.

---

# Invoice PDI / Delivery Challan Warning Rule

PDI and Delivery Challan are no longer hard prerequisites for Invoice.

Rule:

Completed Production
+ Missing PDI OR Missing Delivery Challan
→ Show Warning

User may explicitly confirm:

Continue even though PDI / Delivery Challan is pending.

After confirmation:

Invoice Create / Finalize can continue.

Without confirmation:

Invoice finalization is blocked and a validation message is displayed.

This warning is intentionally informational/business-control based.

Production Completed remains mandatory.

---

# InvoiceItem Source Design

InvoiceItem now uses Production Job as the primary transaction source.

Primary source:

- ProductionJobId
- ProductionJobCode

Traceability retained:

- CustomerPurchaseOrderItemId
- CustomerPurchaseOrderCode
- CustomerPurchaseOrderNumber

Product snapshot retained:

- ItemId
- ItemCode
- ItemName
- PartNumber
- CustomerItemCode
- ProductReference
- UnitName
- HSN Number

Historical Delivery Challan fields remain in InvoiceItem but are optional / nullable.

They are retained for backward compatibility and historical traceability.

No new Invoice entity was introduced for this process change.

---

# Invoice Quantity Allocation

Invoiceable quantity is based on Completed Production Job quantity.

Logic:

Production Job Quantity
- Quantity already allocated to active Invoices
= Available Invoice Quantity

Invoice UI displays:

- Production Quantity
- Already Invoiced Quantity
- Available Quantity
- Invoice Quantity

Invoice Quantity cannot exceed available quantity.

Service layer revalidates quantity during save/finalization.

Browser-posted source quantities are not trusted.

---

# Invoice Commercial Sprint

Implemented per-line commercial fields:

- Invoice Quantity
- Rate
- Discount %
- GST %

Calculated:

Quantity × Rate
→ Gross Amount
→ Discount
→ Taxable Amount
→ GST
→ Line Total

Invoice header includes:

- Gross Amount
- Discount Amount
- Taxable Amount
- CGST
- SGST
- IGST
- Other Charges
- Round Off
- Grand Total

Financial calculations are authoritative in InvoiceService.

JavaScript calculations are preview only.

---

# Invoice GST Sprint

GST type is determined from:

Company State
vs
Billing State

Same State:

GST
→ CGST + SGST

Different State:

GST
→ IGST

Example for GST 18%:

Same State:

- CGST (9%)
- SGST (9%)

Inter-State:

- IGST (18%)

GST percentage remains configurable per Invoice line.

---

# Invoice Customer / Company Snapshot Sprint

Invoice stores historical Customer and Company information.

Customer snapshot supports:

- Customer information
- GSTIN
- PAN
- Billing information
- Payment Terms
- Credit Days

Company snapshot supports:

- Company information
- GST information
- Bank information
- ISO information
- Invoice Terms & Conditions

Snapshots are used so historical finalized Invoices do not change when Master data changes later.

---

# Invoice Workflow

Invoice workflow:

Draft
→ Finalized

Draft Invoice:

- Can be edited
- Can be soft deleted
- Can be restored

Finalized Invoice:

- Cannot be edited through normal workflow
- PDF can be generated

Finalization revalidates:

- Production Job source
- Production completed status
- Invoice quantity availability
- Commercial inputs
- GST
- Financial totals
- PDI / Delivery Challan warning confirmation where required

---

# Invoice Numbering

Invoice Code format:

`AI/INV/{YY-YY}/{00001}`

Example:

`AI/INV/26-27/00001`

Financial Year:

April to March

Document sequence uses five digits.

Invoice numbers are not reused.

---

# Invoice UI Sprint

Create/Edit use shared:

`_Form.cshtml`

Client-side Invoice logic is maintained separately in:

`invoice-form.js`

Final UI flow:

Select Customer PO
→ Load Completed Production Jobs
→ Display Production / Invoice quantity
→ Enter Rate / Discount / GST
→ Display PDI/DC warning if required
→ Save Invoice

Invoice Details includes:

- Invoice information
- Customer information
- Customer PO
- Production Job
- Product / Item
- HSN
- Quantity
- Rate
- Discount
- GST
- Taxable
- Line Total
- Financial Summary
- PDI / Delivery Challan warning
- Finalize action

---

# Invoice PDF Sprint

Invoice PDF implemented using QuestPDF.

PDF includes:

- Company Header
- Tax Invoice title
- Invoice Number
- Invoice Date
- Due Date
- Customer / BILL TO
- Customer PO Number in BILL TO
- Billing Address
- Customer GSTIN
- Customer PAN
- Place of Supply
- Payment Terms
- Credit Days
- Invoice Items
- Customer PO column in Items
- HSN Number column
- Quantity / UOM
- Rate
- Discount %
- GST %
- Line Amount
- Gross Amount
- Discount
- Taxable Amount
- CGST / SGST or IGST
- Other Charges
- Round Off
- Grand Total
- Amount In Words
- Company Bank Details
- Invoice Terms & Conditions
- Remarks
- Authorized Signatory
- Page numbering

Customer PO remains visible in both:

BILL TO section
and
Invoice Items table.

Delivery Challan number is not used as the primary Invoice PDF item source.

---

# Invoice Testing

Final Customer PO / Production based Invoice flow runtime tested.

Successfully tested:

- Invoice Create
- Customer PO selection
- Completed Production Job loading
- Production quantity display
- Already Invoiced quantity
- Available quantity
- Invoice quantity validation
- Rate entry
- Discount calculation
- GST calculation
- Same-state CGST / SGST
- Inter-state IGST
- Other Charges
- Round Off
- Draft Invoice
- Edit Invoice
- Invoice Details
- Missing PDI / Delivery Challan warning
- Warning confirmation
- Finalize Invoice
- Checkbox validation before Finalize
- Invoice PDF generation
- Customer PO in PDF BILL TO
- Customer PO in PDF Item table
- HSN Number in PDF
- CGST / SGST percentage labels
- IGST percentage label

Invoice production-source flow milestone committed to Git.

---

# Current End-to-End Sales / Manufacturing Flow

Current implemented flow:

Customer Master
→ Customer Purchase Order
→ Item Process Routing
→ Production Job
→ Production Execution
→ Production Completed
→ PDI / Inspection
→ Delivery Challan
→ Invoice

Important final Invoice rule:

Customer PO
→ Completed Production Job
→ Invoice

PDI and Delivery Challan can remain in the normal operational flow but they are not mandatory blockers for Invoice.

If missing:

Warning + explicit confirmation is used.

---

# Current Purchase Flow

Current purchase-side flow:

Supplier Master
→ Purchase Order
→ Sent Purchase Order
→ GRN Phase 1

GRN currently records receipt information.

Future Inventory integration will determine actual stock effect.

---

# Architecture Rules Confirmed

The project continues to follow:

Domain
→ Entities / Enums

Application
→ Interfaces / Services / Business Rules / Exceptions

Infrastructure
→ EF Core Repositories / Persistence / PDF Generators

Web
→ Controllers / ViewModels / Razor Views / JavaScript

Important principles:

- Controllers do not directly access DbContext.
- Business rules belong in Application Services.
- Database access belongs in Repositories.
- Browser-posted snapshot values are not trusted.
- Browser-calculated financial values are not trusted.
- Services rebuild / validate authoritative transaction data.
- Historical snapshots are retained where required.
- Soft Delete is preferred for business transactions.
- Document numbers are not reused.

---

# Git Milestones

Completed milestones include:

- Foundation architecture
- Item Master
- Drawing Master
- Item + Drawing integration
- Purchase Order
- Purchase Order GST
- Purchase Order Terms & Conditions
- Purchase Order PDF
- GRN Phase 1
- Customer Master
- Customer Purchase Order Phase 1
- Machine Master Phase 1
- Production Operation
- Item Process Routing
- Production Job / Pipeline
- PDI / Dispatch work
- Delivery Challan
- Invoice core module
- Invoice Production Job source-flow refactor
- Invoice PDF refinement

Latest milestone:

Invoice Customer PO
→ Completed Production
→ Warning-based PDI/DC
→ Finalized Invoice flow

Implementation tested and committed.

---

# Current Documentation Sprint

Before starting the next business module, project documentation must be synchronized with the actual implementation.

Documents requiring current-state review include:

- Project Overview
- Architecture
- Database / Entity Design
- Module Status
- Business Flow
- Sprint Log
- Pending / Future Work
- Implementation Roadmap

Old documentation that describes Invoice as:

Finalized Delivery Challan
→ Invoice

must be updated.

Final documented Invoice rule must be:

Customer PO
→ Completed Production Job
→ Invoice

with PDI / Delivery Challan as warning-only controls.

---

# Next Planned Business Sprint

After documentation synchronization:

**Customer Payment / Receipt**

Planned high-level flow:

Finalized Invoice
→ Customer Payment / Receipt
→ Partial / Full Payment
→ Outstanding Balance

Expected future requirements:

- Customer selection / Invoice selection
- Invoice Amount
- Previously Received Amount
- Outstanding Amount
- Receipt Amount
- Partial Payment
- Full Payment
- Payment Date
- Payment Mode
- Bank / UPI / NEFT / RTGS / Cheque reference
- Transaction / UTR / Cheque Number
- Receipt Number
- Customer-wise Outstanding
- Invoice-wise Outstanding
- Payment Receipt PDF
- Payment history

This module will complete the basic Accounts Receivable side of the Sales cycle.

Before coding:

Requirement
→ Business Flow
→ Database Design
→ Business Rules
→ UI Design
→ Coding
→ Runtime Testing
→ Documentation
→ Git Commit

---