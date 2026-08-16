# 19 - Production Workflow

## Status

Future design - approved direction, not yet implemented.

Production should remain deferred until the Purchase/Inventory foundation required by production material movement is available.

---

# Production Flow

Production Order

↓

Planning

↓

Material Availability

↓

Material Reservation

↓

Material Issue

↓

Cutting

↓

Turning

↓

Drilling

↓

Grinding

↓

Heat Treatment

↓

Quality Inspection

↓

Packing

↓

Finished Goods Receipt

↓

Inventory Update

↓

Production Complete

---

# Each Stage May Store

- Operation
- Machine
- Operator
- Start Time
- End Time
- Duration
- Good Qty
- Reject Qty
- Rework Qty
- Remarks

Exact schema will be finalized during Production module design.

---

# Planned Statuses

- Pending
- Running
- Completed
- Rejected
- On Hold
- Cancelled

---

# Dependency Direction

Production will depend on future:

- Item
- Drawing
- BOM
- Warehouse / Inventory
- Material availability
- Stock Ledger
- Machine
- Employee / Operator
- Quality

Current next module is GRN, not Production.
