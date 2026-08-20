# 12 - Decision Log

## Ajay Industries ERP

This document records architecture and business decisions that should not be changed casually.

---

# Decision 001 - Clean Architecture

Layers:

- Domain
- Application
- Infrastructure
- Web

Current presentation:

ASP.NET Core MVC

React is not currently used.

Web API may be added later when required.

---

# Decision 002 - Repository + Service Pattern

Repository:

Database access.

Application Service:

Business rules and validation.

Controller:

UI request coordination.

Business validation uses BusinessException.

---

# Decision 003 - Soft Delete

Business data uses Soft Delete.

Physical delete is avoided.

Common fields:

- IsDeleted
- IsActive

---

# Decision 004 - Business Codes Are Never Reused

Examples:

- ITM00001
- SUP00001
- SPC00001

Deleted records are included when generating/checking codes.

---

# Decision 005 - ItemCode Is System Identity

ItemCode remains auto generated.

Format:

ITM00001

ItemCode is not intended to describe technical Item properties.

Technical meaning belongs to:

- ItemName
- PartNumber
- Shape
- Specifications
- Drawing

---

# Decision 006 - Item Name Is Not Globally Unique

Same ItemName may exist for technically different Items.

Duplicate identity:

ItemName
+ Shape
+ Complete Specifications

Category, Brand, Main UOM and PartNumber do not participate in this signature.

---

# Decision 007 - Shape Is a Separate Master

Shape is reusable Master data.

Shape is optional on Item.

Shape participates in Item duplicate configuration.

---

# Decision 008 - Specification Is Dynamic

Specifications are not static Item columns.

Examples:

- Diameter
- Length
- Width
- Thickness
- Grade
- Hardness

Grade remains a Specification rather than a dedicated Item field.

---

# Decision 009 - PartNumber Is Optional and Non-Unique

PartNumber belongs directly to Item.

Rules:

- Optional
- Maximum 100 characters
- Searchable
- Not unique

PartNumber is not part of exact Item duplicate identity.

---

# Decision 010 - Item Image Removed

Item Image support is not required at this stage.

ImagePath was removed.

Reason:

Technical/engineering identification is already covered by:

- PartNumber
- Shape
- Specifications
- Drawing
- Drawing Revision
- Drawing File

This avoids unnecessary file-storage complexity.

---

# Decision 011 - Drawing Is a Separate Module

Drawing Number is not stored directly on Item.

Drawing has its own lifecycle and revision history.

---

# Decision 012 - One Drawing Table

Current architecture uses one Drawings table.

Each row represents one Revision.

A separate DrawingRevisions table is intentionally not used at this stage.

---

# Decision 013 - One Item Has One Drawing Number

Final relationship:

One Item
→ One Drawing Number
→ Many Revisions

Same Item cannot receive a second active Drawing Number.

Engineering changes must use Revision History.

---

# Decision 014 - Drawing Number Is Permanent

Drawing Number:

- Manual
- Required
- Immutable after Create
- Never reused
- Reserved after Soft Delete

---

# Decision 015 - Drawing Similarity Checking

Exact Drawing Number:

Block Create.

Similar Drawing Number:

Warning.

Drawing Name:

Similar/exact warning only.

---

# Decision 016 - Revision Number Is Auto Generated

Revision Number is system generated.

Format:

RV-01
RV-02
RV-03

Deleted Revision Numbers remain reserved.

Legacy values such as R01/R02 are recognized for sequence calculation.

---

# Decision 017 - One Current Revision

For each Drawing Number:

Maximum one Current non-deleted revision.

IsActive = true means Current.

Historical revisions are inactive.

---

# Decision 018 - Previous Revision Can Be Reactivated

Inactive revisions can be activated.

Activation:

- deactivates Current revision
- activates selected revision
- executes transactionally

---

# Decision 019 - Revision Soft Delete

Only inactive revisions can be soft deleted.

Current Revision must first be replaced by activating another revision.

Deleted revision:

- remains in database
- remains reserved
- retains physical file

---

# Decision 020 - Drawing Soft Delete Uses Restore

Complete Drawing deletion does not free its Drawing Number.

Deleted Drawings have a dedicated Restore UI.

A deleted Drawing should be restored rather than recreated.

---

# Decision 021 - Drawing Files Stored Outside SQL

Physical files stored in:

wwwroot/uploads/drawings

SQL stores:

- FileName
- FilePath

File binary is not stored in SQL Server.

---

# Decision 022 - Historical Drawing Files Are Preserved

Revision files are not overwritten or physically deleted during Soft Delete.

Engineering history must remain available.

---

# Decision 023 - IsPrimary Removed

IsPrimary became redundant after finalizing:

One Item
→ One Drawing Number

It was removed from the complete Drawing architecture and database.

---

# Decision 024 - Item Details Shows Drawing Information

Current Drawing information is displayed from Item Details.

Displayed:

- Drawing Number
- Drawing Name
- Current Revision
- Drawing Type
- Drawing File

Drawing modification is not performed inside Item Master.

---

# Decision 025 - Item Edit Shows Drawing Read-Only

Item Edit may show Drawing information.

However Drawing lifecycle remains controlled by Drawing Master.

This prevents engineering revision logic from being duplicated inside Item Master.

---

# Decision 026 - Drawing Requires Existing Item

Drawing cannot be created before Item exists.

New Item flow:

Create Item
→ Save Item
→ Open Details
→ Add Drawing

---

# Decision 027 - Add Drawing Auto Selects Item

When Drawing Create is opened from Item Details/Edit:

ItemId is passed automatically.

Drawing Create selects the correct Item.

This reduces user errors.

---

# Decision 028 - Supplier Financial Values Are Transaction Data

Supplier Master does not store:

- Purchase totals
- Pending balances
- Opening balances
- Last purchase values

These will come from future transaction/accounting modules.

---

# Decision 029 - Purchase Order Is Next Module

Purchase Order is selected as the next module after finalizing Item and Drawing Masters.

Purchase Requisition is deferred.

Reason:

Current business flow can begin directly from Supplier Purchase Order.

Purchase Requisition may be introduced later if required.

---

# Decision 030 - Purchase Order Must Generate PDF

Purchase Order must support professional PDF generation.

The PDF will be shared with the Supplier.

Therefore Purchase Order design must consider:

- Company information
- Supplier information
- PO Number
- PO Date
- Item lines
- Quantity
- UOM
- Rate
- Taxes
- Total
- Delivery Terms
- Payment Terms
- Remarks
- Authorized/Company presentation

Exact PDF layout will be finalized during Purchase Order module design.

---

# Decision 031 - Production Workflow Will Be Database Driven

Future Production flow will be database driven.

Planned statuses:

- Pending
- Running
- Completed
- Rejected
- On Hold
- Cancelled

Production remains deferred until its module phase.

---

# Decision 032 - Purchase Order Uses Header + Line Transaction Architecture

Purchase Order is implemented using:

PurchaseOrder
→ Header

PurchaseOrderItem
→ Lines

One Purchase Order can contain multiple Item lines.

This becomes the reference transaction pattern for future ERP header-line modules.

---

# Decision 033 - Purchase Order Number Is Financial-Year Based

Format:

`AI/PO/26-27/00001`

Financial Year:

April to March

Sequence:

Five digits.

PO Number is generated in the Application Service.

Deleted Purchase Order numbers are not reused.

---

# Decision 034 - Purchase Order Stores Historical Snapshots

Purchase Order stores snapshot fields for:

- Company
- Supplier
- Item
- Specification
- Drawing Number
- Drawing Revision
- Terms & Conditions

Reason:

Historical Purchase Orders and PDFs must not change when Master data is edited later.

---

# Decision 035 - GST Type Uses State, Not GSTIN

GSTIN is optional.

Purchase Order tax type is determined from:

Company.State
vs
Supplier.State

Same State:

CGST + SGST

Different State:

IGST

GSTIN prefix is not used to determine tax type.

---

# Decision 036 - Purchase Order GST Defaults to 18 Percent but Is Editable

New Purchase Order lines default to:

18%

User may change GST percentage when required.

Examples:

18% → CGST 9% + SGST 9% or IGST 18%

12% → CGST 6% + SGST 6% or IGST 12%

Final calculation is always performed in the Application Service.

---

# Decision 037 - Purchase Order Does Not Use Discount

Purchase Order discount was removed from the business/UI flow.

Reason:

The current purchase workflow records the agreed supplier Rate directly.

Commercial discount decisions are expected to belong to future Sales/Finished Goods billing where applicable.

Existing Purchase Order discount database fields are retained for compatibility and forced to zero by the Service Layer.

---

# Decision 038 - Purchase Order Does Not Use Round Off

Round Off is not part of the current Purchase Order business calculation.

Existing RoundOff database field is retained for compatibility and forced to zero.

---

# Decision 039 - Additional Purchase Order Charges

Purchase Order supports:

- Transport Charges
- Other Charges

Current rule:

These amounts are added after Item GST.

No separate GST is currently calculated on Transport Charges or Other Charges.

This may be extended later only if business/tax requirements demand it.

---

# Decision 040 - Standard Purchase Order Terms and Conditions Belong to Company Master

Company Master stores:

PurchaseOrderTermsAndConditions

When a Purchase Order is saved, the current Company Terms & Conditions are copied to:

PurchaseOrder.TermsAndConditions

Reason:

Users maintain the standard text once while each Purchase Order preserves its historical snapshot.

---

# Decision 041 - Purchase Order Workflow Is Draft to Confirmed to Sent

Current implemented workflow:

Draft
→ Confirmed
→ Sent

Draft:

- Edit allowed
- Soft Delete allowed
- Confirm allowed

Confirmed:

- Edit blocked
- Delete blocked
- Can be marked Sent

Sent:

- Edit blocked
- Delete blocked

`PartiallyReceived` and `Received` are reserved for GRN integration.

No separate Cancel action is currently implemented.

---

# Decision 042 - Purchase Order Does Not Increase Stock

Saving, confirming or sending a Purchase Order does not create Inventory stock.

Stock increase will begin from GRN / material receipt transactions.

Reason:

Purchase Order represents an order/commitment, not physical receipt.

---

# Decision 043 - Purchase Order PDF Uses QuestPDF

Purchase Order supplier PDF is implemented using QuestPDF.

Approved PDF contains:

- Company logo
- Company information
- PO Number
- PO Date
- Supplier & Delivery details
- Item / Specification / Drawing
- HSN
- Quantity / UOM / Rate
- GST
- Taxable / Line Total
- CGST / SGST / IGST
- Transport Charges
- Other Charges
- Grand Total
- Remarks
- Terms & Conditions
- Prepared / Checked By
- Authorized Signatory
- Page footer

Purchase Order Status is intentionally not displayed on the supplier PDF.

---

# Decision 044 - GRN Is the Next Module

After completing Purchase Order, the next selected transaction module is:

GRN - Goods Receipt Note

GRN will be responsible for future:

- material receipt against PO
- partial/full receipt
- PO receipt status update
- warehouse receipt
- stock transaction
- stock ledger integration

→ One PO → Multiple GRNs
→ Index one PO parent row + GRN History
→ Partial / Full / Not Received rules
→ Material Status values
→ Same Supplier + Same Challan duplicate not allowed
→ Only latest GRN editable
→ PO cannot change after GRN creation
→ Stock effect intentionally pending

ACTION: ADD Customer Master decisions
ADD:
- Customer Code = AI/CUS/00001
- One primary contact/address in Phase 1
- GSTIN optional
- Credit Limit deferred
- Duplicate GSTIN / Email / Mobile blocked

ACTION: ADD

- Customer PO is main incoming business transaction
- Existing Item Master is reused
- No separate Product Master
- Same Customer + Same PO Number blocked
- Customer PO Code uses Financial Year
- Same Item may appear on multiple lines
- Draft only editable
- Confirmed PO can still be soft-deleted
- Deleted POs are shown on separate Deleted Orders page
- Restore preserves original Draft / Confirmed status
- Production/Machine fields are NOT stored directly on Customer PO
- Production Pipeline will be linked from Customer PO Item

- Machine operational status is manually maintained
- Machine Status and Production Step Status are separate concepts
- Machine Code format: AI/MCH/00001
- Serial Number optional but unique when provided
- Deleted Machines are listed separately and can be restored
- Restore preserves previous operational status