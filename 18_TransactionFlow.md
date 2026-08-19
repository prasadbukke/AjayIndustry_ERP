# 18 - ERP Transaction Flow

## Status

Current purchase flow is implemented through Purchase Order.

Downstream receipt/inventory flow is planned and will be finalized during GRN design.

---

# Purchase Transaction Lifecycle

Purchase Order Draft

↓

Validate Company / Supplier / Items

↓

Calculate GST / Totals

↓

Save Purchase Order Snapshot

↓

Confirm

↓

Mark as Sent

↓

GRN - Next Module

↓

Partial or Full Material Receipt

↓

PO receipt status update

↓

Inventory Stock Transaction

↓

Warehouse Stock / Stock Ledger

↓

Purchase Invoice

↓

Supplier Payment

Important:

Purchase Order itself has **no stock impact**.

---

# Purchase Order Status Handoff

Implemented:

Draft
→ Confirmed
→ Sent

Planned through GRN:

Sent
→ PartiallyReceived
→ Received

No separate Cancel action is currently implemented.

---

# Inventory Transaction Direction

GRN / Opening Stock

↓

Stock Transaction

↓

Warehouse Stock

↓

Stock Ledger

↓

Available Stock

Future outward/movement transactions:

- Production Material Issue
- Stock Transfer
- Stock Adjustment
- Sales Dispatch

---

# Production Transaction Direction

Sales Order

↓

Production Planning

↓

Production Order

↓

Material Reservation

↓

Material Issue

↓

Production / Operations

↓

Quality Inspection

↓

Finished Goods Receipt

↓

Inventory Update

Production remains deferred.

---

# Sales Transaction Direction

Quotation

↓

Sales Order

↓

Dispatch Planning

↓

Delivery Challan

↓

Sales Invoice

↓

Receipt

---

# Finance Transaction Direction

Purchase Invoice / Sales Invoice

↓

Payment / Receipt

↓

Journal / Ledger

↓

Reports

Exact accounting design is deferred.
→ PO Sent → GRN Receipt
→ Stock update marked as next integration