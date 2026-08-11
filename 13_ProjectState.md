# Project State

## Project

Ajay Industries ERP

---

## Current Version

Version 1.0 (Foundation)

---

## Technology

- ASP.NET Core MVC (.NET 8)
- ASP.NET Core Web API (Planned)
- SQL Server
- Entity Framework Core
- Clean Architecture
- Bootstrap 5
- AutoMapper

---

## Completed

### Foundation

- Solution Structure
- Clean Architecture
- Dependency Injection
- BaseEntity

### Authentication

- Login UI

### Dashboard

- Dashboard UI
- Sidebar
- Navigation
- Theme

### Company Master

- Create
- List
- Details
- Edit
- Soft Delete
- Search
- Pagination
- Auto Company Code
- Duplicate Company Code Validation
- Duplicate GST Validation
- Audit Fields

---

## Pending Modules

### Masters

- Employees
- Customers
- Suppliers
- Warehouses
- Units
- Categories
- Items
- Machines
- Bill Of Materials

### Purchase

- Purchase Requisition
- Purchase Order
- Goods Receipt
- Purchase Invoice
- Purchase Return

### Inventory

- Stock
- Stock Adjustment
- Stock Transfer
- Warehouse Stock
- Stock Ledger

### Production

- Production Order
- Material Issue
- Material Return
- Production Entry
- Finished Goods

### Sales

- Quotation
- Sales Order
- Delivery Challan
- Sales Invoice
- Sales Return

### Finance

- Payment Entry
- Receipt Entry
- Expenses
- Outstanding Payments

### Reports

- Purchase Report
- Sales Report
- Inventory Report
- Production Report
- GST Report
- Profit & Loss

### Settings

- Users
- Roles
- Company Settings
- Financial Year
- Backup

---

## Reference Module

Company Master

All future modules will follow the Company Master implementation pattern.

---

## Next Module

Employee Master

Completed

Shared Components

Completed

Completed

✅ Dashboard

✅ Login UI

✅ Company Master

✅ Employee Master

✅ Shared Search

✅ Shared Pagination

✅ Shared Delete Modal

✅ Toast Notification

✅ Business Exception

Current Sprint

Sprint 05

Next Module

Planning Phase

# Project State

Last Updated: 08-Aug-2026

---

## Completed Masters

| Module | Status |
|---|---|
| Company Master | Completed |
| Employee Master | Completed |
| UOM Master | Completed |
| Warehouse Master | Completed |
| Item Category Master | Completed |
| Brand Master | Completed |
| Shape Master | Completed |
| Specification Master | Completed |
| Item Master | Completed |

---

## Item Master Status

Item Master Phase is complete.

Implemented features:

- Automatic Item Code generation
- Category selection
- Brand selection
- Main UOM selection
- Optional Shape
- Dynamic Item Specifications
- Optional Specification UOM
- Specification row Add/Remove
- Specification Edit synchronization
- Removed child rows use Soft Delete
- Quick Add Category
- Quick Add Brand
- Quick Add UOM
- Quick Add Shape
- Quick Add Specification
- Select2 searchable Master dropdowns
- Live similar Item Name detection
- Live similar Master Name detection
- Exact Master duplicate prevention
- Item configuration duplicate prevention
- Same Item Name variants allowed
- Specification-aware Item search
- Shape-aware Item search
- Item List Specification summary
- Item Details Specification table
- Soft Delete

---

## Item Duplicate Rule

Current duplicate identity:

ItemName
+ Shape
+ Specifications

Specification comparison contains:

SpecificationId
+ SpecificationValue
+ UomId

Specification order is ignored.

---

## Current Next Module

Supplier Master

Planned Supplier Code:

SUP00001

Supplier Master will become the base dependency for future Purchase and
Supplier-related transactions.

---

## Deferred Item Features

The following are intentionally deferred:

- Opening Stock
- Warehouse Stock
- Min Stock
- Max Stock
- Reorder Level
- GST
- Pricing
- Drawing Number

These will be implemented in their appropriate future modules.