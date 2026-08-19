# 16 - Database Relationship

## Ajay Industries ERP

This document records important physical and business relationships.

---

# 1. Item Category Relationship

ItemCategories
1
|
*
Items

---

# 2. Brand Relationship

Brands
1
|
*
Items

---

# 3. Main UOM Relationship

UOMs
1
|
*
Items

---

# 4. Shape Relationship

Shapes
1
|
*
Items

ShapeId is optional on Item.

---

# 5. Item Specification Relationship

Items
1
|
*
ItemSpecifications

Specifications
1
|
*
ItemSpecifications

UOMs
1
|
*
ItemSpecifications

Specification UOM is optional.

---

# 6. Item Specification Active Uniqueness

For non-deleted rows:

ItemId + SpecificationId

must be unique.

---

# 7. Item Business Identity

Database identity:

ItemId

Business code:

ItemCode

Business configuration duplicate identity:

ItemName
+ Shape
+ Complete Specifications

PartNumber is not unique and is not part of duplicate identity.

---

# 8. Item to Drawing Physical Relationship

Physical database relationship:

Items
1
|
*
Drawings

One-to-many physically because each Drawing Revision is a separate row.

---

# 9. Item to Drawing Business Relationship

ONE Item
→ ONE Drawing Number
→ MANY Revision Rows

Every revision row uses the same ItemId and DrawingNumber.

---

# 10. Current Drawing Revision

For one Drawing Number:

maximum one non-deleted row with `IsActive = true`.

Historical revisions are inactive.

---

# 11. Drawing Revision Identity

Permanent unique pair:

DrawingNumber + RevisionNumber

Deleted Revision Numbers remain reserved.

---

# 12. Supplier to Purchase Order

Suppliers
1
|
*
PurchaseOrders

PurchaseOrder contains `SupplierId`.

Delete behavior:

Restrict for the physical foreign key.

Purchase Order also stores Supplier snapshot fields.

---

# 13. Company to Purchase Order

Companies
1
|
*
PurchaseOrders

PurchaseOrder contains `CompanyId`.

Delete behavior:

Restrict.

Purchase Order also stores Company snapshot fields and Terms & Conditions snapshot.

---

# 14. Purchase Order to Purchase Order Items

PurchaseOrders
1
|
*
PurchaseOrderItems

`PurchaseOrderItem.PurchaseOrderId`
→ `PurchaseOrder.Id`

The current EF configuration uses cascade behavior for physical parent-child delete semantics, while the application normally uses Soft Delete and marks child rows deleted with the Purchase Order.

---

# 15. Item to Purchase Order Item

Items
1
|
*
PurchaseOrderItems

PurchaseOrderItem contains `ItemId`.

Delete behavior:

Restrict.

The line also stores Item snapshot fields.

---

# 16. Drawing to Purchase Order Item

Drawings
1
|
*
PurchaseOrderItems

PurchaseOrderItem.DrawingId is optional.

Delete behavior:

Restrict.

Business rule at Draft Create/Edit:

- Drawing must belong to the selected Item.
- Drawing revision must be Current.
- Drawing Number and Revision are copied to snapshot fields.

---

# 17. UOM in Purchase Order

PurchaseOrderItem does not depend on a separate UOM foreign key for historical display.

The Item's current UOM name is copied to:

`PurchaseOrderItem.UnitName`

This is a transaction snapshot.

---

# 18. Purchase Order Snapshot Principle

Physical foreign keys provide current relational integrity.

Snapshot fields preserve historical transaction presentation.

Current PO snapshots include:

- Company
- Supplier
- Item
- Specification
- UOM name
- Drawing Number / Revision
- Terms & Conditions

---

# 19. Purchase Order Status / GRN Relationship

Current PO workflow:

Draft
→ Confirmed
→ Sent

Future GRN will drive:

Sent / PartiallyReceived
→ PartiallyReceived
→ Received

Exact GRN physical relationships are not yet created.

---

# 20. Future GRN Relationship

Planned:

PurchaseOrders
1
|
*
GRNs

GRNs
1
|
*
GRNItems

PurchaseOrderItems
1
|
*
GRNItems

Items
1
|
*
GRNItems

Warehouse relationship will be finalized during GRN design.

This is planning only until GRN schema is approved.

---

# 21. Delete Behavior

Master foreign keys generally use:

DeleteBehavior.Restrict

Business history should not be physically deleted.

Soft Delete is preferred.

Transaction parent-child configuration may use cascade only for physical relational integrity while application behavior remains Soft Delete.

---

# 22. Future Engineering Relationship

Drawing may later be referenced by:

- Production
- Quality
- BOM
- Inspection

Exact relationships will be designed when those modules are started.
→ PurchaseOrder → GoodsReceiptNotes
→ PurchaseOrderItem → GoodsReceiptNoteItems
