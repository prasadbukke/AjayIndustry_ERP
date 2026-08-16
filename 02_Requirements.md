# Ajay Industries ERP

## Client

Manufacturing Workshop

## Business

Manufacturing of Machinery Parts

---

# Functional Scope

## Dashboard

- Dashboard

---

## Masters

- Company ✅
- Employee ✅
- Customer ⏳
- Supplier ✅
- Warehouse ✅
- UOM ✅
- Item Category ✅
- Brand ✅
- Shape ✅
- Specification ✅
- Item ✅
- Drawing ✅
- Machine ⏳
- Bill Of Materials ⏳

---

## Purchase

- Purchase Requisition - Deferred / Optional
- Purchase Order ✅
- Goods Receipt Note (GRN) - Next
- Purchase Invoice ⏳
- Purchase Return ⏳

Purchase Order current requirements include:

- Company
- Supplier
- PO Date
- Expected Delivery Date
- Delivery Address
- Payment Terms
- Delivery Terms
- Remarks
- Multiple Item lines
- Item snapshot
- Specification snapshot
- Optional Drawing / Revision snapshot
- HSN
- Quantity
- UOM
- Rate
- GST
- Transport Charges
- Other Charges
- Grand Total
- Status lifecycle
- Terms & Conditions snapshot
- Supplier-ready PDF

---

## Inventory

- Current Stock
- Stock Transactions
- Warehouse Stock
- Stock Adjustment
- Stock Transfer
- Stock Ledger
- Opening Stock
- Minimum / Maximum Stock

Inventory values are not stored in Item Master.

---

## Production

- Production Planning
- Production Order
- Material Reservation
- Material Issue
- Material Return
- Production Entry
- Quality Inspection
- Finished Goods Receipt

---

## Sales

- Quotation
- Sales Order
- Delivery Challan
- Sales Invoice
- Sales Return

Sales/Finished Goods billing is the future area where commercial discount rules may be introduced.

---

## Finance

- Payment Entry
- Receipt Entry
- Expenses
- Outstanding Payments
- Supplier / Customer accounting integration

---

## Reports

- Purchase Report
- Sales Report
- Inventory Report
- Production Report
- GST Report
- Profit & Loss

---

## Settings

- Users
- Roles
- Company Settings
- Financial Year
- Backup

---

# Cross-Module Requirements

- Soft Delete for business data where appropriate
- Permanent business codes
- Audit fields
- Search and pagination for list modules
- Service-layer business validation
- Repository-only database access
- Historical transaction snapshots where Master changes must not alter old transactions
- State-based GST split for Purchase Order
- GSTIN may be optional
- Professional transaction PDF/print output where required

---

# Current Development Position

Completed through:

Purchase Order

Next:

GRN - Goods Receipt Note
