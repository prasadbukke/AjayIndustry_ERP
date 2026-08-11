# ERP Database Relationship

Status

✅ LOCKED

---

# Foundation

Company

│

├── Employee

├── Financial Year

├── User

└── Branch (Future)

---

# Configuration

UOM

Tax

Currency

Payment Terms

Bank

---

# Inventory

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

# Purchase

Supplier

↓

Purchase Requisition

↓

Purchase Requisition Details

↓

Purchase Order

↓

Purchase Order Details

↓

Goods Receipt Note (GRN)

↓

GRN Details

↓

Purchase Invoice

↓

Purchase Invoice Details

↓

Purchase Return

---

# Production

BOM

↓

BOM Details

↓

Routing

↓

Routing Operations

↓

Production Order

↓

Production Order Operations

↓

Material Issue

↓

Material Return

↓

Production Entry

↓

Quality Inspection

↓

Finished Goods Receipt

---

# Sales

Customer

↓

Quotation

↓

Quotation Details

↓

Sales Order

↓

Sales Order Details

↓

Delivery Challan

↓

Delivery Challan Details

↓

Sales Invoice

↓

Sales Invoice Details

↓

Sales Return

---

# Inventory Transactions

Item

↓

Stock Ledger

↓

Warehouse Stock

↓

Batch Stock

↓

Serial Number

↓

Stock Adjustment

↓

Stock Transfer

---

# Finance

Receipt

↓

Payment

↓

Journal

↓

Contra

↓

Expense

↓

Ledger

---

# Common Tables

Users

Roles

Permissions

Approval Workflow

Audit Log

Notifications

Attachments

Remarks

Status History

---

# Future

Machine

Work Center

Shift

Operator

Maintenance

Calibration

Tool Master

Quality Parameters

Reject Reasons

Rework Reasons

Customer Complaint

Vendor Rating

---

Status

✅ APPROVED

# Database Relationship Update

Last Updated: 08-Aug-2026

---

## Item Related Relationships

ItemCategories
    1
    |
    *
Items

Brands
    1
    |
    *
Items

Uoms
    1
    |
    *
Items

Shapes
    1
    |
    *
Items

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

Uoms
    1
    |
    *
ItemSpecifications

---

## Detailed Model

ItemCategory
    |
    | 1 : Many
    |
    +------ Items

Brand
    |
    | 1 : Many
    |
    +------ Items

UOM
    |
    | 1 : Many
    |
    +------ Items
    |
    | 1 : Many
    |
    +------ ItemSpecifications

Shape
    |
    | 1 : Many
    |
    +------ Items

Item
    |
    | 1 : Many
    |
    +------ ItemSpecifications
                 |
                 +------ Specification
                 |
                 +------ Optional UOM

---

## Item Specification Example

Items
------------------------------------------------
ItemId: 10
ItemCode: ITM00010
ItemName: MS Round Bar
ShapeId: Round

ItemSpecifications
------------------------------------------------
ItemId  Specification    Value   UOM
10      Diameter         25      MM
10      Length           6000    MM
10      Grade            EN8     NULL

---

## Delete Behavior

Master relationships use Restrict where appropriate.

Business records use Soft Delete.

Deleting an Item through the application also soft-deletes its active
ItemSpecification child rows.

Deleting a Specification definition that is already referenced by
historical/business data must not physically remove dependent data.