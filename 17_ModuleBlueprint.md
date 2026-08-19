# 17 - Module Blueprint

## Ajay Industries ERP

This document defines standard module implementation patterns.

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

NameSimilarityHelper

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

---

# 9. Drawing Module Blueprint

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

# 10. Drawing Edit Blueprint

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

# 11. Drawing Revision Add

New Revision input:

- Revision Number = Auto
- Drawing File
- Remarks

Save:

- Previous Current becomes Inactive
- New Revision created
- Last added Revision becomes Current

---

# 12. Drawing Revision Activation

Flow:

Select Historical Revision
→ Deactivate Current
→ Save
→ Activate Selected
→ Save
→ Commit Transaction

---

# 13. Drawing Revision Delete

Rules:

- Inactive only
- Soft Delete
- Current Revision protected
- File retained
- Revision Number retained

---

# 14. Drawing Delete / Restore

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

# 15. Item and Drawing Integration Blueprint

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

# 16. Item Edit Drawing Blueprint

Item Edit displays read-only Drawing summary.

Drawing information is not editable from Item Master.

Engineering lifecycle remains in Drawing Master.

---

# 17. New Item to Drawing Blueprint

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

# 18. File Upload Blueprint

Drawing File:

- Validate extension
- Validate size
- Generate physical stored filename
- Preserve original FileName
- Store relative FilePath
- Do not store binary in SQL

---

# 19. Error Handling Blueprint

Application Service:

throw BusinessException

Controller:

- Catch BusinessException
- TempData Error
- Catch unexpected Exception
- Generic error message

Global middleware remains deferred.

---

# 20. Transaction Module General Pattern

Future transaction modules should generally use:

Header
→ Lines
→ Validation
→ Totals
→ Status
→ Audit
→ Output / Print / PDF

---

---

# 21. Purchase Order Module Blueprint

Purchase Order is the completed reference transaction module.

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

# 22. Purchase Order Header Blueprint

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

# 23. Purchase Order Line Blueprint

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

# 24. Purchase Order Calculation Blueprint

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

# 25. Purchase Order Snapshot Blueprint

Before persistence, Service validates related Masters and copies required historical values.

Snapshot sources:

Company
Supplier
Item
Drawing
Company Terms & Conditions

The PDF reads Purchase Order snapshots, not live Master values.

---

# 26. Purchase Order Workflow Blueprint

Draft
→ Confirmed
→ Sent

Service methods own transitions.

Controller exposes explicit POST actions.

Edit/Delete are blocked after Draft.

Future GRN owns receipt transitions.

---

# 27. Purchase Order PDF Blueprint

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
- signatory
- footer/page number

Status is not printed on supplier PDF.

---

# 28. Next Transaction Blueprint - GRN

Before GRN coding finalize:

Header candidates:

- GRN Id / Code
- GRN Date
- PurchaseOrderId
- Supplier
- Warehouse
- Supplier Challan reference
- Supplier Invoice reference if required
- Remarks
- Status

Line candidates:

- PurchaseOrderItemId
- ItemId
- Ordered Quantity
- Previously Received Quantity
- Current Received Quantity
- Accepted Quantity
- Rejected Quantity if required
- Pending Quantity
- UOM
- Batch/Heat/Lot if required later

Business rules to finalize:

- multiple GRNs per PO
- no over-receipt unless explicitly allowed
- partial receipt
- full receipt
- PO status update
- stock transaction
- reversal/delete behavior

These fields are planning candidates only until GRN design is approved.
→ GRN finalized implementation blueprint

ACTION: ADD Customer Master completion/reference
ADD:
- Standard Master CRUD pattern
- Search + Pagination
- Toast
- Validation
- Delete confirmation