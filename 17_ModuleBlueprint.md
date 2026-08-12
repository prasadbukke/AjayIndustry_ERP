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

# 21. Next Module - Purchase Order

Purchase Order is the next selected module.

Before coding, finalize:

## Header

Potential fields:

- PurchaseOrderId
- PurchaseOrderNumber
- PurchaseOrderDate
- Company
- Supplier
- Supplier Address Snapshot
- Supplier GSTIN
- Delivery Address
- Payment Terms
- Delivery Terms
- Remarks
- Status

Exact fields are not yet locked.

## Lines

Potential fields:

- PurchaseOrderLineId
- PurchaseOrderId
- ItemId
- UomId
- Quantity
- Rate
- Discount
- Tax
- Amount

Exact fields are not yet locked.

---

# 22. Purchase Order Workflow Goal

Expected basic workflow:

Create PO
→ Select Supplier
→ Add Items
→ Quantity / Rate
→ Tax Calculation
→ Terms
→ Save
→ Generate PDF
→ Share PDF with Supplier

---

# 23. Purchase Order PDF Requirement

Purchase Order must generate a professional PDF.

PDF should be suitable for:

- Printing
- Email
- WhatsApp/file sharing
- Supplier communication

PDF design will be finalized during Purchase Order module design.

---

# 24. Purchase Requisition Decision

Purchase Requisition is not required before the first Purchase Order implementation.

It is deferred.

It may be added later if the business workflow requires internal purchase approval/request processing.

---

# 25. Future Transaction Direction

Expected future sequence:

Purchase Order
→ GRN / Purchase Receipt
→ Warehouse / Stock
→ Supplier Transaction / Accounting

Further modules will be finalized one at a time.