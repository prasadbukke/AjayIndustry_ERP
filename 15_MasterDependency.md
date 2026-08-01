# Master Dependency

Status

✅ LOCKED

---

## Foundation

Company

↓

Employee

---

## Configuration Masters

Unit Of Measure (UOM)

↓

Currency

↓

Tax Master

↓

Payment Terms

↓

Bank

↓

Financial Year

---

## Inventory Masters

Warehouse

↓

Item Category

↓

Item Group

↓

Brand

↓

Item Master

---

## Business Masters

Supplier

↓

Customer

---

## Dependency

Company

Used By

- Employee
- Users
- Branch
- Financial Year

---

Employee

Used By

- Purchase Approval
- Sales Approval
- Production
- Attendance
- Audit Log

---

UOM

Used By

- Item
- Purchase
- Sales
- Production

---

Tax

Used By

- Purchase
- Sales

---

Currency

Used By

- Purchase
- Sales

---

Warehouse

Used By

- Item Stock
- Material Issue
- Material Receipt
- Finished Goods

---

Category

Used By

- Item

---

Item Group

Used By

- Item

---

Brand

Used By

- Item

---

Item

Used By

- Purchase
- Inventory
- Production
- Sales

---

Supplier

Used By

- Purchase Requisition
- Purchase Order
- Purchase Invoice
- Payment

---

Customer

Used By

- Quotation
- Sales Order
- Delivery
- Sales Invoice
- Receipt

---

Future Masters

Machine

Work Center

Shift

Operator

BOM

Routing

Reason Master

Quality Parameter

Status

Pending

---

Development Order

Company ✅

Employee ✅

UOM

Currency

Tax

Payment Terms

Bank

Financial Year

Warehouse

Category

Item Group

Brand

Item

Supplier

Customer

Production Masters

Status

✅ APPROVED