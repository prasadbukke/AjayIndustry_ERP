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

# Master Dependency Update

Last Updated: 08-Aug-2026

---

## Item Master Dependency

Item
|
|-- ItemCategory      Required
|
|-- Brand             Required
|
|-- UOM               Required
|
|-- Shape             Optional
|
`-- ItemSpecifications[]       Optional
      |
      |-- Specification        Required per row
      |
      `-- UOM                  Optional per row

---

## Relationship Meaning

### Item Category

Defines the logical category of an Item.

Examples:

- Raw Material
- Consumable
- Finished Goods
- Spare

---

### Brand

Defines the Item Brand.

Brand is currently required by Item Master.

---

### Main UOM

Defines the primary unit used for the Item.

Example:

Item:
MS Round Bar

Main UOM:
KG

Technical Specifications may independently use MM or another UOM.

---

### Shape

Optional physical representation of the Item.

Example:

Shape:
Round

---

### Specifications

An Item may contain zero or more technical Specifications.

Example:

Item:
MS Round Bar

Shape:
Round

Specifications:

Diameter = 25 MM
Length = 6000 MM
Grade = EN8

---

## Quick Master Dependencies

Item Create/Edit allows Quick Add for:

Item Category
Brand
UOM
Shape
Specification

Newly created records are automatically selected in the originating
dropdown.

---

## Future Dependency

Supplier Master will be independent of Item Master.

Future Purchase transactions will connect:

Supplier
+
Item
+
Warehouse
+
Quantity
+
Rate
+
Tax