# 15 - Master Dependency

## Ajay Industries ERP

This document describes Master dependencies and their future transaction usage.

---

# 1. Dependency Principle

Master data should contain stable identity/configuration information.

Transaction-derived values should remain in transaction modules.

---

# 2. Current Master Overview

Company

Employee

UOM

Warehouse

Item Category
↓
Item

Brand
↓
Item

Shape
↓
Item

Specification
↓
ItemSpecifications
↓
Item

Item
↓
Drawing

Supplier

---

# 3. Item Dependencies

Item depends on:

- Item Category
- Brand
- UOM
- Optional Shape
- Specifications
- Optional Specification UOM

Structure:

ItemCategory
→ Item

Brand
→ Item

UOM
→ Item

Shape
→ Item

Item
→ ItemSpecifications

Specification
→ ItemSpecifications

UOM
→ ItemSpecifications

---

# 4. Item Configuration Identity

Duplicate identity:

ItemName
+ Shape
+ Specifications

Specifications include:

- SpecificationId
- SpecificationValue
- Optional UomId

PartNumber is not part of duplicate identity.

---

# 5. Drawing Dependency

Drawing depends on Item.

Final business relationship:

Item
→ Drawing Number
→ Revision History

Rules:

- One Item has one Drawing Number.
- Drawing Number cannot be reassigned.
- Item cannot be changed after Drawing creation.
- Engineering changes use Revision History.

---

# 6. Drawing Revision Dependency

All revisions share:

- ItemId
- DrawingNumber
- DrawingName
- DrawingType

Revision-specific fields:

- RevisionNumber
- FileName
- FilePath
- Description
- Current/Inactive state

---

# 7. Item to Drawing UI Dependency

Item Details/Edit may display the Current Drawing.

However:

Item does not own Drawing revision business logic.

Drawing Master remains responsible for:

- Revision creation
- Revision activation
- Revision deletion
- Drawing deletion
- Drawing restore

---

# 8. Supplier Dependency

Supplier is currently independent Master data.

Future use:

Supplier
→ Purchase Order

Supplier will provide Purchase Order data such as:

- Supplier Name
- GSTIN
- Address
- Contact
- Payment Terms

Exact snapshot behavior will be finalized during Purchase Order design.

---

# 9. Company Dependency

Company will be required by Purchase Order.

Future use:

Company
→ Purchase Order

Company information may be used on Purchase Order PDF.

---

# 10. Item Dependency in Purchase Order

Purchase Order will use Item.

Future relationship:

Purchase Order
→ Purchase Order Lines
→ Item

Likely line-level usage:

- Item
- UOM
- Quantity
- Rate
- Tax
- Amount

Exact design is pending.

---

# 11. UOM Dependency

UOM is currently used by:

- Item
- ItemSpecifications

Future use:

- Purchase Order
- Inventory
- BOM
- Production
- Sales

---

# 12. Warehouse Dependency

Warehouse is currently standalone Master data.

Future:

Warehouse
→ Inventory / Stock Transactions

Warehouse is not currently part of Item Master.

---

# 13. Next Transaction Dependency

Next module:

Purchase Order

Expected high-level dependency:

Company
   ↓
Purchase Order
   ↑
Supplier

Purchase Order
   ↓
Purchase Order Lines
   ↓
Item
   ↓
UOM

---

# 14. Future Dependency Direction

Expected future flow:

Supplier
→ Purchase Order
→ GRN / Purchase Receipt
→ Warehouse Stock
→ Inventory

Item
→ BOM
→ Production
→ Quality

Drawing
→ Production / Quality engineering reference