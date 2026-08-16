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

Controller:

- request/response coordination
- ModelState
- TempData
- redirects

Application Service:

- business validation
- calculations
- transaction workflow
- snapshot logic
- code generation

Repository:

- EF Core database access
- Includes
- CRUD persistence
- search/pagination
- existence checks

Domain:

- entities
- enums
- common business data

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

Business logic still remains in Services.

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
- prefix-based Purchase Order sequence lookup

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
- Breaking database changes require explicit approval.

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

Reason:

Single Responsibility and historical transaction accuracy.

---

# 15. Purchase Order Transaction Architecture

Purchase Order is the first completed header-line transaction reference.

Flow:

PurchaseOrderController
→ IPurchaseOrderService
→ PurchaseOrderService
→ IPurchaseOrderRepository
→ PurchaseOrderRepository
→ ApplicationDbContext

PDF flow:

PurchaseOrderController
→ IPurchaseOrderPdfService
→ PurchaseOrderPdfService
→ QuestPDF
→ PDF file response

Purchase Order snapshots Master information so historical PDFs do not change when Master records are later edited.

---

# 16. Development Rule

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

# 17. Change Policy

Architecture breaking change:

Not allowed without explicit review.

Database breaking change:

Not allowed without explicit review.

Folder/layer convention change:

Not allowed casually.

Business rules may grow while preserving the frozen architecture.

---

# 18. Reference Modules

Baseline CRUD:

Company Master

Dynamic master/engineering:

Item + Drawing

Transaction header-lines / PDF:

Purchase Order
