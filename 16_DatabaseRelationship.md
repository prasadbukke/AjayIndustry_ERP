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

This is one-to-many physically because every Drawing Revision is stored as a separate row.

---

# 9. Item to Drawing Business Relationship

Business relationship:

ONE Item
→ ONE Drawing Number
→ MANY Revision Rows

Example:

ItemId 1006
→ DRG-1001
   → RV-01
   → RV-02
   → RV-03

Every revision row uses the same ItemId and DrawingNumber.

---

# 10. Current Drawing Revision

Example:

RV-01  IsActive = false
RV-02  IsActive = false
RV-03  IsActive = true

Filtered uniqueness:

DrawingNumber
WHERE IsActive = 1
AND IsDeleted = 0

Maximum one Current revision.

---

# 11. One Current Drawing Per Item

Filtered uniqueness:

ItemId
WHERE IsActive = 1
AND IsDeleted = 0

This prevents two active Drawing identities for the same Item.

Historical revisions remain valid because they are inactive.

---

# 12. Drawing Revision Identity

Permanent unique pair:

DrawingNumber + RevisionNumber

This index includes deleted records.

Deleted Revision Numbers cannot be reused.

---

# 13. Revision Soft Delete

Inactive revision:

IsDeleted = true
IsActive = false

The row remains in the database.

Physical Drawing file remains stored.

---

# 14. Complete Drawing Soft Delete

Complete Drawing:

IsDeleted = true

for its revision history.

Normal queries exclude deleted rows.

Drawing Number remains reserved.

Restore is supported.

---

# 15. Supplier Current Relationship

Supplier is currently a standalone Master.

Future physical relationship:

Suppliers
1
|
*
PurchaseOrders

This table does not yet exist.

---

# 16. Purchase Order Planned Relationship

Next planned transaction structure:

Companies
1
|
*
PurchaseOrders

Suppliers
1
|
*
PurchaseOrders

PurchaseOrders
1
|
*
PurchaseOrderLines

Items
1
|
*
PurchaseOrderLines

UOMs
1
|
*
PurchaseOrderLines

This is a planning relationship only.

Exact schema will be finalized before Purchase Order coding.

---

# 17. Future Purchase Flow

Supplier
→ Purchase Order
→ Purchase Order Lines
→ Item

Later:

Purchase Order
→ GRN / Purchase Receipt
→ Warehouse / Stock

---

# 18. Delete Behavior

Master foreign keys generally use:

DeleteBehavior.Restrict

Business history should not be physically cascade deleted.

Soft Delete is preferred.

---

# 19. Future Engineering Relationship

Drawing may later be referenced by:

- Production
- Quality
- BOM
- Inspection

Exact relationships will be designed when those modules are started.