# 03 - Business Flow

## Purchase Flow

Current operational entry point:

Purchase Order

↓

Draft

↓

Confirmed

↓

Sent to Supplier

↓

Goods Receipt Note (GRN)

↓

Partial / Full Material Receipt

↓

Purchase Invoice

↓

Payment

Purchase Requisition is currently deferred and may be introduced later if an internal request/approval process becomes necessary.

Important rule:

Purchase Order does not increase stock. Material receipt and stock impact begin from GRN.

---

## Production Flow

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

Production Entry / Operations

↓

Quality Inspection

↓

Finished Goods Receipt

↓

Inventory Update

Production remains a future module.

---

## Inventory Flow

Opening / Existing Stock

↓

Purchase GRN / Other Stock-In Transaction

↓

Warehouse Stock

↓

Stock Ledger

↓

Production Material Issue / Transfer / Adjustment

↓

Finished Goods Receipt

↓

Sales Dispatch

Inventory must be transaction-driven.

---

## Sales Flow

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

Receipt / Payment

Commercial discount rules are expected to belong to Sales/Finished Goods billing rather than Purchase Order.
