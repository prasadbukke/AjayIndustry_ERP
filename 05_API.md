# 05 - API Design

## Purpose

The ERP currently uses ASP.NET Core MVC.

Business logic is kept in the Application Layer so the same Services can later be reused by ASP.NET Core Web API.

---

# Current API Status

Web API is **planned**, not the primary current delivery layer.

Current production-facing implementation in this project is MVC.

Do not treat the routes in this document as already implemented unless a dedicated API controller exists in code.

---

# Planned API Version

`v1`

Planned base URL:

`/api`

---

# Planned Company API

## Get All Companies

`GET /api/company`

## Get Company By Id

`GET /api/company/{id}`

## Create Company

`POST /api/company`

Business validation should match the Application Service.

Current Company rules relevant to future API:

- Company Name required
- Company Code auto generated
- GST Number optional
- GST Number validated when provided
- State required
- Purchase Order Terms & Conditions optional

## Update Company

`PUT /api/company/{id}`

## Delete Company

Soft Delete.

## Search / Pagination

Future API should expose the same search/pagination behavior already available through Application Services.

---

# Planned Master APIs

- Employee
- Customer
- Supplier
- Warehouse
- UOM
- Item Category
- Brand
- Shape
- Specification
- Item
- Drawing
- Machine
- Bill Of Material

---

# Planned Purchase APIs

- Purchase Order
- Goods Receipt Note
- Purchase Invoice
- Purchase Return

Purchase Requisition remains deferred.

Future Purchase Order API must reuse the same business rules already enforced by `IPurchaseOrderService` / `PurchaseOrderService`, including:

- Financial-year PO number generation
- Company/Supplier validation
- State-based GST type
- Item/Drawing snapshot validation
- Draft-only edit/delete rules
- Service-authoritative total calculation

PDF generation may later be exposed through an API endpoint using the existing Purchase Order PDF service.

---

# Planned Inventory APIs

- Stock
- Stock Adjustment
- Stock Transfer
- Warehouse Stock
- Stock Ledger

---

# Planned Production APIs

- Production Order
- Material Issue
- Material Return
- Production Entry
- Finished Goods

---

# Planned Sales APIs

- Quotation
- Sales Order
- Delivery Challan
- Sales Invoice
- Sales Return

---

# Planned Finance APIs

- Payment
- Receipt
- Expense
- Outstanding Payment
