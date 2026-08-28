# 17 - Module Blueprint

## Ajay Industries ERP

This document defines standard module implementation patterns and
approved implementation references for completed ERP modules.

---

# 1. Standard Master Module Implementation Order

1. Domain Entity
2. EF Core Configuration
3. ApplicationDbContext
4. Repository Interface
5. Repository Implementation
6. Service Interface
7. Service Implementation
8. Dependency Injection
9. ViewModel
10. Controller
11. Views
12. JavaScript
13. Migration
14. Runtime Testing
15. Documentation
16. Git Commit

---

# 2. Standard Master Features

Where applicable:

- Index
- Search
- Pagination
- Create
- Edit
- Details
- Soft Delete
- Restore
- Active/Inactive
- Duplicate Validation
- Similar Name Warning
- Audit Fields
- Auto Business Code
- Toast Notifications
- Confirmation Modal

---

# 3. Name Similarity Pattern

Helper:

`NameSimilarityHelper`

Behavior:

- Exact duplicate -> block when required
- Similar spelling -> warning
- Live suggestions
- Normalized matching
- Fuzzy matching

---

# 4. Item Module Blueprint

Flow:

Create Item
→ Item Name
→ Part Number
→ Category
→ Brand
→ UOM
→ Optional Shape
→ Dynamic Specifications
→ Duplicate Configuration Validation
→ Save

Duplicate configuration:

ItemName
+ Shape
+ Specifications

---

# 5. Item Quick Add Blueprint

Supported Masters:

- Category
- Brand
- UOM
- Shape
- Specification

Flow:

Select2
→ No Result
→ Add Master
→ Quick Add Modal
→ Validation
→ AJAX Save
→ Auto Select

---

# 6. Item Specification Blueprint

Dynamic row:

Specification
| Value
| Optional UOM
| Remove

Features:

- Dynamic indexes
- SortOrder
- Duplicate Specification protection
- Quick Add

---

# 7. Item Details Blueprint

Item Information:

Three-column desktop layout.

Fields:

- Item Code
- Item Name
- Part Number
- Category
- Brand
- UOM
- Shape
- Status
- Description

Then:

- Item Specifications
- Drawing Information
- Audit Information

---

# 8. Supplier Module Blueprint

Supplier flow:

Create Supplier
→ Identity
→ Contact
→ GSTIN / PAN
→ Address
→ Payment Terms
→ Similar Name Validation
→ Save

Standard features:

- Index
- Search
- Pagination
- Create
- Edit
- Details
- Soft Delete
- Restore where applicable
- Active / Inactive
- Validation
- Toast notifications
- Delete confirmation
- Audit information

---

# 9. Customer Master Blueprint

Customer Master follows the standard Master module architecture.

Flow:

Create Customer
→ Customer Identity
→ Contact Information
→ GSTIN / PAN
→ Billing / Address Information
→ Commercial Information
→ Validation
→ Save

Standard features:

- Index
- Search
- Pagination
- Create
- Edit
- Details
- Soft Delete
- Restore where applicable
- Active / Inactive
- Duplicate validation
- Similar name validation where applicable
- Toast notifications
- Delete confirmation
- Audit fields

Customer Master is used as a reference source by downstream
Sales transactions.

Current downstream usage includes:

Customer
→ Customer Purchase Order
→ Production
→ Invoice

Historical transactional documents must use saved customer
snapshots where required instead of depending only on live
Customer Master values.

---

# 10. Drawing Module Blueprint

Final Drawing flow:

Select Item
→ Enter Drawing Number
→ Drawing Name / Type
→ Upload First File
→ System Generates RV-01
→ Save

Business rule:

One Item
→ One Drawing Number
→ Many Revisions

---

# 11. Drawing Edit Blueprint

Read-only:

- Item
- Drawing Number

Editable:

- Drawing Name
- Drawing Type

Revision area:

- Revision History
- Add Revision
- Activate Previous Revision
- Delete Inactive Revision

---

# 12. Drawing Revision Add

New Revision input:

- Revision Number = Auto
- Drawing File
- Remarks

Save:

- Previous Current becomes Inactive
- New Revision created
- Last added Revision becomes Current

---

# 13. Drawing Revision Activation

Flow:

Select Historical Revision
→ Deactivate Current
→ Save
→ Activate Selected
→ Save
→ Commit Transaction

---

# 14. Drawing Revision Delete

Rules:

- Inactive only
- Soft Delete
- Current Revision protected
- File retained
- Revision Number retained

---

# 15. Drawing Delete / Restore

Delete Drawing:

Soft Delete
→ Remove from normal Index
→ Keep Drawing Number reserved

Deleted Drawings:

Dedicated screen
→ Restore

Restore:

- Drawing identity
- Revision history
- Current Revision

---

# 16. Item and Drawing Integration Blueprint

Item Details:

If Drawing exists:

Drawing Number
| Current Revision
| Drawing Type

Drawing Name
| Drawing File
| Open Details

If no Drawing:

No Drawing Available
→ Add Drawing

---

# 17. Item Edit Drawing Blueprint

Item Edit displays read-only Drawing summary.

Drawing information is not editable from Item Master.

Engineering lifecycle remains in Drawing Master.

---

# 18. New Item to Drawing Blueprint

Flow:

Create Item
→ Save Item
→ Redirect Item Details
→ Add Drawing
→ Drawing Create
→ Item Auto Selected
→ Create RV-01

This pattern ensures ItemId exists before Drawing creation.

---

# 19. File Upload Blueprint

Drawing File:

- Validate extension
- Validate size
- Generate physical stored filename
- Preserve original FileName
- Store relative FilePath
- Do not store binary in SQL

---

# 20. Error Handling Blueprint

Application Service:

`throw BusinessException`

Controller:

- Catch BusinessException
- TempData Error
- Catch unexpected Exception where required
- Generic error message

Business-rule validation remains primarily in the Application Service.

Global middleware remains deferred.

---

# 21. Transaction Module General Pattern

Transaction modules should generally use:

Header
→ Lines
→ Source Validation
→ Snapshot Validation
→ Calculations
→ Status
→ Workflow
→ Audit
→ Output / Print / PDF

General principles:

- Service layer owns business rules.
- Controller coordinates HTTP/UI flow.
- Repository owns data access.
- JavaScript improves UX but is not authoritative.
- Browser-calculated financial values must be recalculated or
  validated on the server.
- Transaction snapshots preserve historical document information.
- Workflow actions use explicit service methods.
- Finalized historical documents should not depend on mutable
  Master data.

---

# 22. Purchase Order Module Blueprint

Purchase Order is the completed reference Purchase transaction module.

Architecture:

PurchaseOrder
→ Header

PurchaseOrderItem
→ Lines

Implementation order followed:

1. Domain entities/enums
2. EF configurations
3. DbSets
4. Migration
5. Repository interface
6. Repository implementation
7. Service interface
8. Service implementation
9. Dependency Injection
10. ViewModels
11. Controller
12. Index
13. Create/Edit shared form
14. Details
15. Workflow actions
16. PDF service
17. Runtime testing
18. Documentation
19. Git commit

---

# 23. Purchase Order Header Blueprint

Current header responsibilities:

- PO Number
- PO Date
- Expected Delivery
- Status
- Company reference + snapshot
- Supplier reference + snapshot
- Delivery Address
- Payment Terms
- Delivery Terms
- Remarks
- Terms & Conditions snapshot
- GST totals
- Transport Charges
- Other Charges
- Grand Total
- workflow timestamps
- audit fields

PO Number:

`AI/PO/YY-YY/00001`

Financial Year:

April to March

---

# 24. Purchase Order Line Blueprint

Current line responsibilities:

- ItemId
- ItemCode snapshot
- ItemName snapshot
- Description snapshot
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

Discount fields remain compatibility-only and are forced to zero.

---

# 25. Purchase Order Calculation Blueprint

Line:

Quantity × Unit Price
→ Taxable Amount

Tax:

Same State
→ CGST + SGST

Different State
→ IGST

Header:

Taxable
+ tax
+ Transport Charges
+ Other Charges
→ Grand Total

Rules:

- Default GST 18%
- GST editable
- no Purchase Order Discount
- no Round Off
- no separate GST on Transport/Other Charges currently

---

# 26. Purchase Order Snapshot Blueprint

Before persistence, Service validates related Masters and copies
required historical values.

Snapshot sources:

- Company
- Supplier
- Item
- Drawing
- Company Terms & Conditions

The PDF reads Purchase Order snapshots, not live Master values.

---

# 27. Purchase Order Workflow Blueprint

Draft
→ Confirmed
→ Sent

Service methods own transitions.

Controller exposes explicit POST actions.

Edit/Delete are blocked after Draft.

Future receipt transactions own downstream receipt transitions.

---

# 28. Purchase Order PDF Blueprint

Interface:

`IPurchaseOrderPdfService`

Implementation:

`PurchaseOrderPdfService`

Library:

QuestPDF

Approved content:

- Company logo/details
- PO number/date
- Supplier & Delivery grid
- Item table
- Remarks/totals
- Terms & Conditions
- Signatory
- Footer/page number

Status is not printed on supplier PDF.

---

# 29. GRN Planning Blueprint

GRN design candidates:

Header:

- GRN Id / Code
- GRN Date
- PurchaseOrderId
- Supplier
- Warehouse
- Supplier Challan reference
- Supplier Invoice reference if required
- Remarks
- Status

Lines:

- PurchaseOrderItemId
- ItemId
- Ordered Quantity
- Previously Received Quantity
- Current Received Quantity
- Accepted Quantity
- Rejected Quantity if required
- Pending Quantity
- UOM
- Batch / Heat / Lot if required later

Business rules to finalize:

- multiple GRNs per PO
- no over-receipt unless explicitly allowed
- partial receipt
- full receipt
- PO status update
- stock transaction
- reversal/delete behavior

Note:

The above remains a planning blueprint unless separately replaced by
the approved finalized GRN implementation documentation.

---

# 30. Invoice Module Blueprint

Invoice is the approved Sales billing transaction module.

Architecture:

Invoice
→ Header

InvoiceItem
→ Lines

Primary business source:

Customer Purchase Order
→ Completed Production Jobs
→ Invoice

Invoice creation is NOT dependent on Delivery Challan selection.

---

# 31. Invoice Implementation Order

Invoice implementation followed the transaction architecture:

1. Domain entities / existing schema review
2. Repository interface
3. Repository implementation
4. Service interface
5. Service implementation
6. Controller
7. Form ViewModel
8. Details ViewModel
9. Shared Create/Edit form
10. Invoice JavaScript
11. Details screen
12. PDF generator
13. Schema / migration update where required
14. Runtime testing
15. Documentation
16. Git commit

---

# 32. Invoice Source Blueprint

Approved source flow:

Select Customer Purchase Order
→ Load Production Jobs belonging to that Customer PO
→ Include only Completed Production Jobs
→ Calculate already invoiced quantity
→ Calculate available invoice quantity
→ Load eligible rows into Invoice
→ User enters commercial values
→ Save Draft Invoice

Production completion is the primary source eligibility gate.

A Production Job that is not Completed cannot be invoiced.

---

# 33. Invoice PDI / Delivery Challan Rule

PDI and Delivery Challan are NOT mandatory prerequisites for
creating or finalizing an Invoice.

Rule:

Completed Production Job
→ PDI / Delivery Challan check

If both required source documents are available:

→ Continue normally

If PDI or Delivery Challan is missing:

→ Show warning
→ Require explicit user confirmation
→ Allow Invoice to continue

The warning does not change Production Completed eligibility.

Business interpretation:

Production Completed
= mandatory

PDI / Delivery Challan
= warning / traceability check

---

# 34. Invoice Source Warning Blueprint

Warning applies when one or more selected Production Jobs do not
have the expected finalized PDI or Delivery Challan source.

UI behavior:

- Show warning alert
- Show confirmation checkbox
- User must explicitly confirm before continuing
- If checkbox is not selected, show validation message
- Do not submit/finalize until confirmation is provided

Server behavior:

The Service also validates the warning confirmation.

Client-side confirmation alone is not authoritative.

---

# 35. Invoice Customer PO Blueprint

Invoice Create/Edit uses Customer Purchase Order as the visible
primary business source.

Flow:

Customer PO Selector
→ AJAX Load
→ Completed Production Jobs
→ Invoice Items

Customer PO information remains visible for traceability.

Customer PO is displayed:

- In Invoice details
- In Invoice PDF BILL TO information
- In Invoice PDF item table

---

# 36. Invoice Production Job Blueprint

Each new Invoice Item is associated with a Production Job.

Primary source fields include:

- ProductionJobId
- ProductionJobCode

Production Job validation confirms:

- Production Job exists
- Production Job is active
- Production Job is not deleted
- Production Job is Completed
- Production Job belongs to the selected Customer PO
- Requested Invoice Quantity does not exceed available quantity

Current production quantity basis:

`ProductionJob.JobQuantity`

Already invoiced quantity is calculated from existing valid
Invoice Items for the same Production Job.

Available quantity:

Production Quantity
- Already Invoiced Quantity
= Available Invoice Quantity

---

# 37. Invoice Item Snapshot Blueprint

InvoiceItem preserves transactional product and source snapshots.

Current relevant fields include:

- ProductionJobId
- ProductionJobCode
- ItemId
- ItemCode
- ItemName
- PartNumber
- ProductReference
- CustomerItemCode
- UnitName
- HsnNumber
- CustomerPurchaseOrderItemId
- CustomerPurchaseOrderCode
- CustomerPurchaseOrderNumber
- InvoiceQuantity
- Rate
- GrossAmount
- DiscountPercent
- DiscountAmount
- TaxableAmount
- GstRate
- CgstRate
- SgstRate
- IgstRate
- CgstAmount
- SgstAmount
- IgstAmount
- TotalTaxAmount
- LineTotal

Historical Delivery Challan fields are retained as optional
compatibility/traceability fields.

They are not the mandatory source for new Invoice creation.

---

# 38. Invoice Delivery Challan Compatibility Blueprint

Existing InvoiceItem Delivery Challan references are retained for
historical compatibility.

Examples:

- DeliveryChallanId
- DeliveryChallanCode
- DeliveryChallanItemId
- DeliveryChallanQuantity

For the new Production-based Invoice flow these values may be null.

Therefore Invoice persistence and UI must not assume a Delivery
Challan exists.

This preserves old Invoice history without making Delivery Challan
mandatory for new Invoices.

---

# 39. Invoice Financial Calculation Blueprint

Invoice line calculations include:

Invoice Quantity
× Rate
→ Gross Amount

Gross Amount
- Discount
→ Taxable Amount

Tax:

Same State
→ CGST + SGST

Different State
→ IGST

Line:

Taxable Amount
+ GST
→ Line Total

Header includes:

- Gross Amount
- Discount Amount
- Taxable Amount
- CGST
- SGST
- IGST
- Other Charges
- Round Off
- Grand Total

Server-side Service owns authoritative financial calculations and
validation.

JavaScript calculations are for user experience only.

---

# 40. Invoice GST Display Blueprint

Invoice details and PDF should show GST percentage clearly.

Examples:

Intra-state:

CGST (9%)
SGST (9%)

for an 18% GST line/rate.

Inter-state:

IGST (18%)

Where multiple GST rates exist, a mixed-rate label may be used in
summary output.

---

# 41. Invoice Number Blueprint

Invoice code format:

`AI/INV/YY-YY/00001`

Example:

`AI/INV/26-27/00001`

Financial Year:

April to March

Code generation remains owned by the Service / repository sequence
logic.

---

# 42. Invoice UI Blueprint

Create and Edit share:

`_Form.cshtml`

JavaScript remains in a separate file:

`invoice-form.js`

Do not inline the main Invoice form JavaScript in the shared partial.

Main form behavior:

Customer PO
→ Completed Production Jobs
→ Auto-load Invoice rows

Displayed source information includes:

- Production Job
- Product / Item
- HSN
- Production Quantity
- Already Invoiced Quantity
- Available Quantity
- Invoice Quantity
- UOM
- Rate
- Discount
- GST
- Taxable Amount
- Line Total

---

# 43. Invoice Edit Blueprint

Edit reuses the same shared form as Create.

Existing entered commercial values must be preserved when the page
is loaded.

Source information may be refreshed for:

- Production quantity
- Already invoiced quantity
- Available quantity
- Source warning

When calculating already invoiced quantity during Edit, the current
Invoice is excluded from allocation calculation.

---

# 44. Invoice Details Blueprint

Invoice Details shows:

- Invoice information
- Customer information
- Billing information
- Customer PO references
- Production Job traceability
- Product/item rows
- HSN
- Invoice quantity
- UOM
- Rate
- Discount
- GST
- Taxable amount
- Line total
- Financial summary
- Terms & Conditions
- Finalization information

Delivery Challan is no longer displayed as the primary source column.

Historical Delivery Challan data remains available in the underlying
transaction where applicable.

---

# 45. Invoice Workflow Blueprint

Current workflow:

Draft
→ Finalized

Draft:

- Editable
- Deletable
- Can be finalized

Finalized:

- Cannot be edited
- Cannot be deleted through Draft workflow
- Used as the final billing document
- PDF can be generated

Finalize action performs source and business validation again.

---

# 46. Invoice Finalize Warning Blueprint

Before finalization:

Service verifies selected Production Jobs.

If PDI / Delivery Challan warning exists:

Details screen shows warning
→ User selects confirmation checkbox
→ Finalize request includes confirmation
→ Service accepts explicit override
→ Invoice Finalized

If confirmation is missing:

→ Finalization is blocked
→ Validation/error message is shown

---

# 47. Invoice PDF Blueprint

Interface:

`IInvoicePdfGenerator`

Implementation:

`InvoicePdfGenerator`

Library:

QuestPDF

Approved PDF content:

- Company header
- Tax Invoice heading
- Invoice number
- Invoice date
- Due date
- Customer / BILL TO information
- Customer PO reference
- GSTIN
- Billing address
- PAN
- Place of Supply
- Payment Terms
- Credit Days
- Invoice Item table
- HSN No.
- Customer PO line reference
- Quantity / UOM
- Rate
- Discount
- GST
- Amount
- Financial summary
- Amount in words
- Company bank details
- Terms & Conditions
- Remarks where applicable
- Authorized Signatory
- Footer and page numbers

Customer PO is intentionally displayed both:

1. In BILL TO information
2. In the Invoice Item table

Delivery Challan is not printed as the primary Invoice source.

---

# 48. Invoice Historical Snapshot Blueprint

Invoice preserves Company and Customer snapshot information.

PDF and historical Invoice presentation should prefer saved
transaction snapshots rather than depending on current mutable
Master data.

Examples:

Company snapshot:

- Company Name
- Address
- GST information
- Contact information
- Bank information

Customer snapshot:

- Customer identity
- GSTIN
- PAN
- Billing/commercial information where applicable

This ensures a Finalized Invoice remains historically accurate even
if Master data changes later.

---

# 49. Invoice Validation Ownership Blueprint

Repository:

- Data access
- Eligible source queries
- Allocation queries
- Existing document checks

Service:

- Business source validation
- Production completion validation
- Quantity validation
- PDI / Delivery Challan warning logic
- Snapshot preparation
- Financial calculation
- Workflow validation
- Code generation coordination

Controller:

- HTTP actions
- ViewModel mapping
- TempData messages
- AJAX endpoints
- Warning presentation coordination

JavaScript:

- Dynamic UI
- Source loading
- Row management
- User-side validation
- Calculation preview

Database / Service remain authoritative.

---

# 50. Invoice Completion Status

Invoice Production-source flow has been:

- Implemented
- UI updated
- Warning workflow implemented
- Details updated
- PDF updated
- Runtime tested
- Git committed

Approved business flow:

Customer
→ Customer Purchase Order
→ Production
→ Completed Production Job
→ Invoice

PDI / Delivery Challan:

Optional for Invoice progression,
with explicit warning confirmation when missing.

---

# 51. Transaction Documentation Rule

Whenever an implemented transaction flow changes:

1. Update Entity / schema documentation if affected
2. Update business flow documentation
3. Update Module Blueprint
4. Update architecture/module status documents
5. Update database relationship documentation where required
6. Record completed workflow behavior
7. Runtime test
8. Git commit

Documentation should be updated before starting the next major module.

---