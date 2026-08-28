# 10 - Architecture

## Project

Ajay Industries ERP

## Status

**ERP Architecture Freeze v1.0 - LOCKED**

---

# 1. Architecture Style

Clean Architecture with:

- Domain
- Application
- Infrastructure
- Web

Current presentation:

ASP.NET Core MVC .NET 8

Future:

ASP.NET Core Web API may reuse Application Services.

---

# 2. Technology Stack

## Web

- ASP.NET Core MVC .NET 8
- Razor Views
- Bootstrap
- JavaScript / jQuery
- Select2
- Toastr

## Application

- Service interfaces
- Service implementations
- Business rules
- BusinessException
- PagedResult
- Helpers
- PDF service contract/implementation currently follows the project Service convention

## Infrastructure

- Entity Framework Core
- Repository implementations
- SQL Server
- Entity configurations
- Dependency Injection registration
- QuestPDF PDF generation

## Domain

- Entities
- Enums
- Common business/audit base types

---

# 3. Runtime Request Flow

MVC View

↓

Controller

↓

Application Service

↓

Repository Interface / Repository Implementation

↓

ApplicationDbContext

↓

SQL Server

↓

Response

↓

MVC View

This is the runtime/business execution flow.

---

# 4. Project Dependency Intent

Domain is the core business-data layer.

Application depends on Domain.

Infrastructure implements persistence/repository concerns used by Application contracts and depends on the required Application/Domain contracts.

Web is the composition/presentation layer and wires Application + Infrastructure through Dependency Injection.

Do not confuse runtime call direction with project-reference direction.

---

# 5. Business Rule Boundaries

## Controller

Responsible for:

- request/response coordination
- ModelState
- TempData
- redirects
- ViewModel mapping
- lightweight AJAX endpoints
- workflow request coordination

Controller must not become the authoritative source for transaction business rules.

---

## Application Service

Responsible for:

- business validation
- calculations
- transaction workflow
- source validation
- snapshot logic
- code generation
- financial validation
- finalization validation
- warning/confirmation business rules

Application Service is the authoritative business-rule layer.

Browser-posted calculated values or source snapshots must not be blindly trusted.

---

## Repository

Responsible for:

- EF Core database access
- Includes
- CRUD persistence
- search/pagination
- existence checks
- source-data queries
- allocation calculations
- workflow-support queries

Repository does not own presentation behavior.

---

## Domain

Responsible for:

- entities
- enums
- common business data
- persisted transaction state

---

# 6. EF Core Configuration

Entity mapping belongs in:

`AjayIndustriesERP.Infrastructure/Configurations`

Pattern:

`IEntityTypeConfiguration<T>`

DbContext applies configurations from the Infrastructure assembly.

Inline module mapping inside Controllers/Services is not allowed.

---

# 7. Dependency Injection

Main registration location:

`Infrastructure/DependencyInjection/DependencyInjection.cs`

Program.cs calls the Infrastructure registration method and acts as the application composition root.

Module registrations should follow the existing DI pattern instead of being scattered through Controllers.

---

# 8. Shared Architecture Patterns

- Repository Pattern
- Service Pattern
- Dependency Injection
- BusinessException
- Soft Delete
- PagedResult
- Shared Search
- Shared Pagination
- Delete Confirmation Modal
- Toast Notification
- Quick Master pattern
- Historical transaction snapshot pattern
- Header-line transaction pattern
- Explicit transaction workflow actions
- Server-authoritative financial calculation
- Server-authoritative transaction source validation

---

# 9. Controller Standard

Typical Master:

- Index
- Details
- Create GET
- Create POST
- Edit GET
- Edit POST
- Delete POST

Transaction modules may additionally contain:

- workflow POST actions
- PDF/print GET actions
- lightweight AJAX helper actions
- source-data lookup actions

Business logic still remains in Services.

AJAX actions may provide data required by the UI, but must not replace Service-level validation.

---

# 10. Repository Standard

Typical operations:

- GetAllAsync
- GetByIdAsync
- SearchAsync
- GetPagedAsync
- Add/Create
- Update
- Delete
- Exists checks
- SaveChangesAsync

Additional repository methods are allowed when required by a business module, for example:

- last business-code lookup
- prefix-based sequence lookup
- transaction source lookup
- remaining quantity calculation
- allocated quantity calculation
- workflow existence checks
- finalized-source checks

---

# 11. Service Standard

Typical operations:

- GetAllAsync
- GetByIdAsync
- SearchAsync
- GetPagedAsync
- CreateAsync
- UpdateAsync
- DeleteAsync

Transactions may add explicit workflow methods.

Purchase Order examples:

- ConfirmAsync
- MarkAsSentAsync
- IsIntraStateAsync

Invoice examples:

- PrepareDraftAsync
- GetRemainingInvoiceQuantityAsync
- GetProductionJobIdsRequiringWarningAsync
- FinalizeAsync
- GeneratePdfAsync

Workflow-specific Service methods are allowed when the business transaction requires them.

---

# 12. Validation Standard

UI / DataAnnotations

↓

Application Service business rules

↓

Repository / Database constraints

↓

Controller catches BusinessException

↓

Toast / validation feedback

Important transaction rules must be validated again in the Application Service even when the browser already performed equivalent validation.

---

# 13. Database Standard

BaseEntity provides common audit/status fields:

- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

Identity and business-code property names are entity-specific.

Rules:

- Business codes are permanent.
- Deleted codes are not reused.
- Soft Delete is preferred.
- Restrict is preferred for important Master foreign keys unless a specific transaction relationship requires another behavior.
- Historical transaction references may remain nullable when the related source is optional in the business process.
- Breaking database changes require explicit approval.
- Entity schema changes must be applied through reviewed EF Core migrations.

---

# 14. Item Master Domain Separation

Status: LOCKED

Item Master stores stable master/configuration data.

Purchase Module owns transaction-specific:

- Purchase Rate
- GST
- HSN
- Supplier purchase transaction values

Inventory owns:

- Current Stock
- Stock Transactions
- Stock Ledger

Transaction documents may store historical snapshots of Item information required for document accuracy.

Reason:

Single Responsibility and historical transaction accuracy.

---

# 15. Historical Transaction Snapshot Pattern

Transaction documents must preserve important historical information where later Master changes must not alter old business documents.

Examples include:

- Company information
- Customer information
- Supplier information
- Item information
- Customer PO reference
- transaction-specific rates
- GST values
- billing information
- payment terms

A finalized historical document should not depend on current Master values for information that was already captured when the transaction was created.

Live source data may still be used for validation before finalization where required.

---

# 16. Purchase Order Transaction Architecture

Purchase Order is the first completed header-line transaction reference.

Flow:

PurchaseOrderController

↓

IPurchaseOrderService

↓

PurchaseOrderService

↓

IPurchaseOrderRepository

↓

PurchaseOrderRepository

↓

ApplicationDbContext

PDF flow:

PurchaseOrderController

↓

IPurchaseOrderPdfService

↓

PurchaseOrderPdfService

↓

QuestPDF

↓

PDF file response

Purchase Order snapshots Master information so historical PDFs do not change when Master records are later edited.

---

# 17. Invoice Transaction Architecture

Invoice follows the same Controller → Service → Repository transaction architecture.

Primary business source flow:

Customer Purchase Order

↓

Production Job

↓

Production Completed

↓

Invoice

Invoice does not require Delivery Challan as its mandatory transaction source.

A Production Job must belong to the selected Customer Purchase Order and must be completed before it can be invoiced.

Runtime flow:

InvoiceController

↓

IInvoiceService

↓

InvoiceService

↓

IInvoiceRepository

↓

InvoiceRepository

↓

ApplicationDbContext

Invoice creation UI may use lightweight AJAX lookup to load eligible completed Production Jobs for the selected Customer Purchase Order.

The Service must revalidate the selected Production Jobs before Create, Update or Finalize.

---

# 18. Invoice Source Validation Rule

Status: LOCKED

The authoritative Invoice eligibility condition is:

**Production Job must be Completed.**

PDI and Delivery Challan are not mandatory gates for Invoice creation/finalization.

If either required operational document is missing:

- Finalized PDI missing, or
- Delivery Challan missing

the system shows a warning.

The user may explicitly confirm the warning and continue with the Invoice.

Therefore:

Production Completed

→ mandatory

PDI / Delivery Challan

→ warning-based operational controls

The confirmation must not bypass the Production Completed requirement.

The Application Service owns this validation.

---

# 19. Invoice Quantity Architecture

Invoice quantity validation is server authoritative.

The system must validate:

- selected Production Job
- completed production source
- already invoiced quantity
- remaining invoiceable quantity
- requested Invoice Quantity

The UI may display:

- Production Quantity
- Already Invoiced Quantity
- Available Quantity

but these values are informational.

Create/Update/Finalize must revalidate quantities through the Service/Repository before persistence or workflow completion.

---

# 20. Invoice Historical Reference Architecture

InvoiceItem stores transaction snapshots and traceability information.

Primary current source:

- ProductionJobId
- ProductionJobCode
- CustomerPurchaseOrderItemId
- CustomerPurchaseOrderCode
- CustomerPurchaseOrderNumber

Product snapshots include values such as:

- Item
- Item Code
- Item Name
- Part Number
- Customer Item Code
- Unit
- HSN
- Product Reference

Legacy/historical Delivery Challan references may remain available as nullable fields.

Delivery Challan information is not required for new Invoice creation.

This allows historical compatibility without making Delivery Challan mandatory in the current Invoice process.

---

# 21. Invoice Financial Calculation Architecture

Invoice financial calculations are controlled by the Application Service.

Typical calculated values include:

- Gross Amount
- Discount Amount
- Taxable Amount
- CGST
- SGST
- IGST
- Total Tax
- Other Charges
- Round Off
- Grand Total

The browser may calculate values for immediate UI feedback.

However, posted calculated totals must not be treated as authoritative.

The Service recalculates and validates financial values before persistence/finalization.

---

# 22. Invoice Finalization Architecture

Draft Invoice:

- may be edited
- may be deleted
- may be finalized

Finalization performs business validation again.

When PDI or Delivery Challan warning exists:

- user confirmation is required
- confirmation is explicitly submitted
- Service validates the warning confirmation
- Invoice can then be finalized

After finalization, the document becomes the historical transaction record according to the module workflow rules.

---

# 23. Invoice PDF Architecture

PDF flow:

InvoiceController

↓

IInvoicePdfGenerator / Invoice PDF contract

↓

InvoicePdfGenerator

↓

QuestPDF

↓

PDF file response

Invoice PDF uses persisted transaction information and saved snapshots.

Current PDF includes:

- Company information
- Invoice number/date/due date
- Customer billing information
- Customer PO reference in BILL TO
- Customer PO reference at item level
- Item/Product information
- HSN Number
- Quantity
- Rate
- Discount
- GST
- financial summary
- Amount In Words
- Bank Details
- Terms & Conditions
- Authorized Signature

GST summary should display applicable rates, for example:

- CGST (9%)
- SGST (9%)

or

- IGST (18%)

depending on the transaction.

---

# 24. Shared Transaction UI Architecture

Create/Edit transaction pages may share a common partial View when they represent the same business form.

Current Invoice pattern:

- `Create.cshtml`
- `Edit.cshtml`
- shared `_Form.cshtml`
- separate `invoice-form.js`

JavaScript-specific transaction behavior remains in the dedicated JavaScript file instead of being duplicated inside Create/Edit pages.

Server-side validation remains authoritative regardless of client-side JavaScript behavior.

---

# 25. Development Rule

Requirement

↓

Business Flow

↓

Database Design

↓

Business Rules

↓

UI

↓

Coding

↓

Testing

↓

Documentation

↓

Git Commit

---

# 26. Change Policy

Architecture breaking change:

Not allowed without explicit review.

Database breaking change:

Not allowed without explicit review.

Folder/layer convention change:

Not allowed casually.

Business rules may grow while preserving the frozen architecture.

Changes inside an existing module are not considered architecture changes when they continue to follow the established:

Controller

↓

Service

↓

Repository

↓

DbContext

pattern.

---

# 27. Reference Modules

Baseline CRUD:

Company Master

Dynamic master/engineering:

Item + Drawing

Transaction header-lines / PDF:

- Purchase Order
- Invoice

Workflow/source-validation transaction:

- Invoice

Invoice is the current reference implementation for:

- Customer PO based source selection
- Completed Production validation
- transaction allocation validation
- warning-with-confirmation workflow
- historical transaction snapshots
- server-authoritative financial calculations
- finalization
- PDF generation