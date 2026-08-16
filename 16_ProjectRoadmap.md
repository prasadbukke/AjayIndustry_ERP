# 16 - Project Roadmap

## Current Position

Foundation and core Masters are completed.

Engineering Item/Drawing foundation is completed.

First Purchase transaction module is completed:

**Purchase Order**

---

# Completed Phase 1 - Foundation

- Clean Architecture
- Repository + Service Pattern
- EF Core / SQL Server
- Dependency Injection
- Shared UI components
- Soft Delete
- Search / Pagination
- BusinessException
- Quick Master framework

---

# Completed Phase 2 - Master Data

- Company
- Employee
- UOM
- Warehouse
- Item Category
- Brand
- Shape
- Specification
- Supplier
- Item
- Drawing

---

# Completed Phase 3 - Purchase Order

- Header + lines
- Company/Supplier/Item/Drawing snapshots
- GST
- Terms & Conditions
- Workflow
- PDF

---

# Next Phase - GRN

Next module:

**Goods Receipt Note**

Required design topics:

- PO-based receipt
- partial receipts
- multiple GRNs against one PO
- received/pending quantity
- warehouse
- accepted/rejected quantity if required
- PO status integration
- stock transaction
- stock ledger

Do not start GRN coding until business rules and database design are finalized.

---

# Planned Purchase Sequence

Purchase Order ✅

↓

GRN - Next

↓

Purchase Invoice

↓

Purchase Return if required

↓

Supplier accounting/payment integration

Purchase Requisition remains deferred.

---

# Planned Inventory Sequence

GRN / Opening Stock

↓

Stock Transaction

↓

Warehouse Stock

↓

Stock Ledger

↓

Transfer / Adjustment / Production Issue

---

# Future Phases

- Customers
- Sales
- BOM
- Production
- Quality
- Finance
- Reports
- Users / Roles
- Production deployment hardening

Module order after GRN should be reviewed based on the dependencies created by GRN and Inventory.
