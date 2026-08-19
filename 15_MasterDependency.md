# 15 - Master Dependency

## Ajay Industries ERP

This document describes Master dependencies and their transaction usage.

---

# 1. Dependency Principle

Master data contains stable identity/configuration information.

Transaction-derived values belong to transaction modules.

When historical transactions must remain unchanged after Master edits, the transaction stores a snapshot.

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

Final relationship:

Item
→ Drawing Number
→ Revision History

Rules:

- One Item has one Drawing Number.
- Drawing Number cannot be reassigned.
- Item cannot be changed after Drawing creation.
- Engineering changes use Revision History.

---

# 6. Supplier Dependency

Supplier is used by Purchase Order.

Supplier
1
→ many
Purchase Orders

Purchase Order stores Supplier snapshot fields so historical POs do not change when Supplier Master changes.

Current snapshot usage includes:

- Supplier Name
- Address
- optional GSTIN
- Contact Person
- Phone
- Email

Supplier State is used with Company State for Purchase Order GST type.

PaymentTermsDays may provide the default Purchase Order Payment Terms.

---

# 7. Company Dependency

Company is used by Purchase Order.

Company
1
→ many
Purchase Orders

Current PO usage:

- Company Name
- Address
- State
- optional GSTIN
- Phone
- Email
- Website where configured
- Standard Purchase Order Terms & Conditions

Purchase Order stores a Company snapshot.

Company State is authoritative for Purchase Order intra/inter-state comparison.

---

# 8. Item Dependency in Purchase Order

Purchase Order line depends on Item.

PurchaseOrder
→ PurchaseOrderItem
→ Item

At save time Purchase Order stores:

- ItemCode
- ItemName
- Description
- Specification snapshot
- UnitName snapshot

Purchase-specific values stay on PurchaseOrderItem:

- HSN
- Quantity
- Rate
- GST
- Taxable
- tax amounts
- Line Total

---

# 9. Drawing Dependency in Purchase Order

Drawing is optional on a Purchase Order line.

If selected:

- Drawing must belong to the selected Item.
- Drawing must be the Current active revision.
- Purchase Order stores Drawing Number and Revision snapshot.

Historical PO does not depend on later Drawing revision changes for its printed value.

---

# 10. UOM Dependency

UOM is used by:

- Item
- ItemSpecifications

Purchase Order derives/stores the Item UOM as a line snapshot (`UnitName`).

Future modules:

- GRN
- Inventory
- BOM
- Production
- Sales

---

# 11. Warehouse Dependency

Warehouse is currently a completed standalone Master.

Next major transaction use:

GRN / Inventory receipt.

Purchase Order itself does not create Warehouse stock.

---

# 12. Purchase Order Current Dependency

Company
   ↓
Purchase Order
   ↑
Supplier

Purchase Order
   ↓
Purchase Order Items
   ↓
Item
   ↓
Specification / UOM snapshot

Optional:

Item
↓
Current Drawing
↓
Drawing Number / Revision snapshot

---

# 13. Next Transaction Dependency

Next module:

GRN

Planned dependency direction:

Purchase Order
↓
GRN
↓
GRN Lines
↓
Item
↓
Warehouse / Stock Transaction

GRN will also drive future Purchase Order receipt status:

- PartiallyReceived
- Received

---

# 14. Future Dependency Direction

Supplier
→ Purchase Order
→ GRN
→ Warehouse Stock
→ Inventory / Stock Ledger

Item
→ BOM
→ Production
→ Quality

Drawing
→ Production / Quality engineering reference

ACTION: ADD Customer Master dependency
ADD:
Customer
   ↓
Customer PO / Sales Order
   ↓
Production Job / Pipeline

ACTION: UPDATE
ADD:
- Customer Master completed
- Customer PO / Sales Order next