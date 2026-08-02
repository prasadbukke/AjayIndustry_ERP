# Database Design

## Database

AjayIndustriesERPDB

---

# Common Audit Fields

Every Master Table Contains

- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

---

# Company

| Column | Type |
|---------|------|
| CompanyId | int (PK) |
| CompanyCode | nvarchar(20) |
| CompanyName | nvarchar(100) |
| GstNumber | nvarchar(20) |
| PanNumber | nvarchar(20) |
| PhoneNumber | nvarchar(20) |
| Email | nvarchar(100) |
| Website | nvarchar(100) |
| ContactPerson | nvarchar(100) |
| Address | nvarchar(500) |
| City | nvarchar(100) |
| State | nvarchar(100) |
| Country | nvarchar(100) |
| PostalCode | nvarchar(20) |
| IsActive | bit |
| IsDeleted | bit |
| CreatedOn | datetime |
| CreatedBy | nvarchar(100) |
| ModifiedOn | datetime |
| ModifiedBy | nvarchar(100) |

Status

✅ Completed

---

# Upcoming Tables

## Masters

- Employee
- Customer
- Supplier
- Warehouse
- Unit
- Category
- Item
- Machine
- BillOfMaterial

## Purchase

- PurchaseRequisition
- PurchaseOrder
- GoodsReceipt
- PurchaseInvoice
- PurchaseReturn

## Inventory

- Stock
- StockAdjustment
- StockTransfer
- WarehouseStock
- StockLedger

## Production

- ProductionOrder
- MaterialIssue
- MaterialReturn
- ProductionEntry
- FinishedGoods

## Sales

- Quotation
- SalesOrder
- DeliveryChallan
- SalesInvoice
- SalesReturn
- 
Employee

Status

✅ Completed
## Finance

- Payment
- Receipt
- Expense
- OutstandingPayment

## Security

- User
- Role
- UserRole

Employee → Status Completed

## Employee

Status

✅ Completed

Features

- CRUD
- Search
- Pagination
- Auto Employee Code
- Soft Delete
- Duplicate Validation
- Toast Notification