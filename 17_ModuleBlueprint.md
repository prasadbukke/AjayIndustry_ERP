# ERP Module Blueprint

Status

✅ LOCKED

---

Every ERP Module must follow this template.

---

# 1. Module Information

Module Name

Purpose

Business Owner

Department

Priority

Status

---

# 2. Business Requirement

Why this module is required?

Who will use it?

When will it be used?

Inputs

Outputs

Dependencies

---

# 3. Business Flow

Step 1

↓

Step 2

↓

Step 3

↓

Completed

---

# 4. Actors

Admin

Manager

Purchase User

Production User

Store User

Sales User

Accounts User

Quality User

---

# 5. Database

Header Table

Detail Table

Relationships

Foreign Keys

Indexes

Unique Constraints

---

# 6. Business Rules

Mandatory Fields

Duplicate Rules

Approval Rules

Validation Rules

Edit Rules

Delete Rules

Cancel Rules

Reopen Rules

---

# 7. Workflow

Pending

↓

Approved

↓

In Progress

↓

Completed

↓

Closed

---

# 8. Screens

List

Create

Edit

Details

Print

Reports

Dashboard

---

# 9. Reports

Summary

Details

Pending

Completed

Cancelled

Analysis

---

# 10. Audit

Created By

Created On

Modified By

Modified On

Status History

Remarks

---

# 11. Testing

Positive Cases

Negative Cases

Edge Cases

Performance

---

# 12. Documentation

Database

API

Business Rules

Screenshots

Sprint Log

Git Commit

---

Status

✅ APPROVED

UOM Decision

Code Entry

Manual

Examples

NOS
PCS
KG
GM
LTR
MTR
BOX
SET

Auto Code

❌ Not Allowed

Status

✅ LOCKED

Master Modules

✔ Company
✔ Employee
✔ UOM
✔ Warehouse
✔ Item Category
✔ Brand

Next

Item Master
Purchase
Inventory
Production
Sales

Item Master (LOCKED)

Fields

ItemCode
ItemName
Description
CategoryId
BrandId
UomId
WarehouseId
OpeningStock
MinimumStock
MaximumStock
ReorderLevel
IsActive

# Item Master Module Blueprint

Last Updated: 08-Aug-2026

---

## Purpose

Item Master provides a single reusable Item definition that can later be
used across:

- Purchase
- Inventory
- BOM
- Production
- Sales
- Planning
- Reporting

---

## Architecture Flow

Domain
    Item
    Shape
    Specification
    ItemSpecification

Application
    Repository Interfaces
    Services
    Business Validation
    Duplicate Configuration Logic
    Name Similarity Helper

Infrastructure
    EF Core Configurations
    Repository Implementations
    SQL Server Persistence

Web
    ViewModels
    Controllers
    Razor Views
    Select2
    Quick Master Modal
    Dynamic Specification Rows

---

## Create Flow

User opens Item Create

    ↓

Load:

Category
Brand
UOM
Shape
Specification Options

    ↓

User enters Item information

    ↓

Optional Dynamic Specification Rows

    ↓

Controller maps ItemViewModel to Item aggregate

    ↓

ItemService normalizes input

    ↓

Validate:

Main Item fields
Specification rows
Specification duplicates
Specification UOM
Exact Item configuration

    ↓

Generate Item Code

    ↓

Save Item + ItemSpecifications

    ↓

Single DbContext SaveChanges

---

## Edit Flow

Load Item

    ↓

Load active ItemSpecifications

    ↓

Map to ItemViewModel

    ↓

User:

Updates existing row
Adds new row
Removes row

    ↓

ItemService synchronization

Existing row:
UPDATE

New row:
INSERT

Removed row:
SOFT DELETE

    ↓

SaveChanges

---

## Item Delete Flow

Item Delete

    ↓

Load active ItemSpecifications

    ↓

Soft Delete ItemSpecifications

    ↓

Soft Delete Item

    ↓

SaveChanges

---

## Duplicate Validation

Name-only duplicates are allowed.

Exact configuration duplicate is blocked.

Comparison:

ItemName
ShapeId
SpecificationId
SpecificationValue
Specification UomId

Specification order is ignored.

---

## Search

Item Search currently supports:

- Item Code
- Item Name
- Description
- Category Code
- Category Name
- Brand Code
- Brand Name
- Main UOM
- Shape
- Specification Code
- Specification Name
- Specification Value
- Specification UOM

Example searches:

25
EN8
Diameter
Round
MM

return the matching Items.

---

## Quick Master

Item Form supports AJAX Quick Add for:

- Category
- Brand
- UOM
- Shape
- Specification

Quick Add Flow:

Search dropdown

    ↓

No Result

    ↓

Add Master

    ↓

Live similar-name detection

    ↓

Create

    ↓

New record automatically selected

No page redirect is required.