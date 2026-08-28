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

Purchase Order does not increase stock.

Material receipt and stock impact begin from GRN.

---

## Production Flow

Current production flow begins from the Customer Purchase Order.

Customer Purchase Order

↓

Customer PO Items

↓

Production Job

↓

Production Steps / Operations

↓

Production Execution

↓

Step-wise Good / Rejected Quantity

↓

Production Completed

↓

PDI / Quality Inspection, where applicable

↓

Delivery / Billing readiness

Important rules:

- Production Job is linked to a Customer Purchase Order Item.
- Production must be completed before it becomes eligible for Invoice.
- Production completion is the authoritative billing eligibility condition.
- PDI and Delivery Challan are operational / traceability documents and are not mandatory prerequisites for Invoice creation.
- Production Job remains the primary production traceability reference for Invoice Items.

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

Production Consumption

↓

Finished Goods / Completed Production

↓

Dispatch / Delivery

Inventory must be transaction-driven.

Important rules:

- Purchase Order itself does not affect stock.
- Stock changes must originate from an actual stock transaction.
- Production material consumption and finished output should eventually be reflected through stock transactions.
- Inventory availability, shortage and historical usage should be traceable from Item-level transactions.

---

## Sales / Customer Order Flow

Customer Purchase Order

↓

Customer PO Items

↓

Production Job Creation

↓

Production Execution

↓

Production Completed

↓

PDI / Quality Inspection
(Optional for Invoice)

↓

Delivery Challan
(Optional for Invoice)

↓

Invoice

↓

Customer Receipt / Payment

Important rules:

- Customer Purchase Order is the commercial source for production and billing.
- Invoice is not created from Delivery Challan as the mandatory source.
- Invoice creation starts by selecting the Customer Purchase Order.
- After selecting a Customer Purchase Order, eligible Completed Production Jobs are loaded automatically.
- Only Completed Production Jobs are eligible for Invoice.
- PDI is not mandatory for Invoice creation.
- Delivery Challan is not mandatory for Invoice creation.
- If PDI or Delivery Challan is missing, the system displays a warning.
- The user may explicitly confirm the warning and continue with Invoice creation / finalization.
- InvoiceService performs authoritative validation of source Production Jobs and invoiceable quantities.
- Delivery Challan references, when available, remain optional historical / traceability information.

---

## Invoice Flow

Customer Purchase Order

↓

Select Customer PO on Invoice

↓

Load Completed Production Jobs

↓

Check Remaining Invoiceable Quantity

↓

Check PDI / Delivery Challan Status

↓

If PDI / Delivery Challan Missing

↓

Show Warning and Require User Confirmation

↓

Create Draft Invoice

↓

Edit / Review Invoice

↓

Finalize Invoice

↓

Generate Invoice PDF

Important rules:

- Production Completed is the mandatory eligibility condition.
- PDI and Delivery Challan are warning-only conditions.
- Invoice Item is primarily linked to Production Job.
- Customer PO information is retained as Invoice traceability.
- Delivery Challan information is optional and may remain available for historical records.
- Invoice quantity cannot exceed the available invoiceable Production quantity.
- Finalized Invoice cannot be edited or deleted through the normal Draft workflow.
- Browser-side financial calculations are previews only.
- Final financial values are calculated and validated by the service layer.

---

## Invoice Document Flow

Invoice

↓

Draft

↓

Review Commercial Details

↓

PDI / Delivery Challan Warning Confirmation, if required

↓

Finalized

↓

PDF Generated

The Invoice PDF contains:

- Invoice Number
- Invoice Date
- Due Date
- Customer / Billing Details
- Customer PO Number in Bill To section
- Customer PO Number at Invoice Item level
- Item / Product Details
- HSN Number
- Quantity and UOM
- Rate
- Discount
- GST
- Line Amount
- Financial Summary
- Amount In Words
- Bank Details
- Terms & Conditions
- Authorized Signature section

---

## Customer Receipt / Payment Flow

Planned next stage:

Finalized Invoice

↓

Customer Payment / Receipt

↓

Full / Partial Payment Allocation

↓

Invoice Outstanding Update

↓

Customer Outstanding Tracking

↓

Payment Receipt

This module is the next logical stage after the completed Invoice workflow.

---

## Overall Order-to-Cash Flow

Customer Purchase Order

↓

Production Job

↓

Production Execution

↓

Production Completed

↓

PDI / Quality Check
(Optional for Invoice)

↓

Delivery Challan
(Optional for Invoice)

↓

Invoice

↓

Customer Receipt / Payment

Core rule:

Production completion controls Invoice eligibility.

PDI and Delivery Challan improve operational control and traceability but do not block billing when an authorized user explicitly confirms the warning.