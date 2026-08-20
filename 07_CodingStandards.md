# 07 - Coding Standards

## Architecture

- Clean Architecture
- Repository Pattern
- Service Pattern
- Dependency Injection
- BusinessException for business validation

---

## Project Structure

- Domain
- Application
- Infrastructure
- Web

Future:

- ASP.NET Core Web API may reuse Application Services.

---

## File Standard

C# files should use:

- Clear file name
- Appropriate namespace
- Regions for large modules
- XML comments for public methods where useful
- Consistent naming
- No unrelated rewrites during a focused change

---

## Controller Rules

Controller contains:

- HTTP request handling
- ModelState handling
- ViewModel/entity coordination
- Service calls
- TempData messages
- Redirects / responses

Controller must not contain:

- Entity Framework queries
- SQL
- core business rules
- authoritative financial calculation

---

## Service Rules

Service contains:

- Business rules
- Business validation
- Transaction workflow rules
- Snapshot rules
- Authoritative calculations
- Code generation

Service must not access `DbContext` directly.

---

## Repository Rules

Repository contains:

- Entity Framework queries
- Includes
- CRUD persistence
- Search
- Pagination
- existence checks
- last-code lookup
- SaveChanges

Repository must not contain business workflow rules.

---

## EF Core Mapping Rule

EF Core entity mapping belongs in:

`Infrastructure/Configurations`

Use:

`IEntityTypeConfiguration<T>`

Do not move module-specific EF configuration into Controllers or Services.

---

## Entity Rules

Business entities use `BaseEntity` audit fields where applicable:

- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

Entity-specific identity/code fields are defined by the entity; do not assume every table has the same `Id` or `Code` property name.

---

## Delete Rule

Soft Delete is the default for business data.

Physical delete should be avoided unless explicitly approved for a non-business/transient record.

For important transactions, delete permission may depend on status.

Example:

Purchase Order can be deleted only while Draft.

---

## Async Rule

Use async / await for:

- Repository database operations
- Application Service operations that depend on repositories
- Controller actions that call async services

---

## Validation Rule

Validation layers:

DataAnnotations / UI validation

↓

Application Service business validation

↓

Repository/database constraints as additional protection

Controller catches `BusinessException` and shows a user-safe error.

---

## Naming Convention

Examples:

Entity:
`Company`

Repository:
`CompanyRepository`

Repository Interface:
`ICompanyRepository`

Service:
`CompanyService`

Service Interface:
`ICompanyService`

Controller:
`CompanyController`

ViewModel:
`CompanyViewModel`

---

## Create / Edit Partial Rule

Shared form pattern:

`Create.cshtml`
- owns `<form>`
- renders `_Form`
- renders `_ValidationScriptsPartial`

`Edit.cshtml`
- owns `<form>`
- renders `_Form`
- renders `_ValidationScriptsPartial`

`_Form.cshtml`
- contains common fields/UI only
- should not own the outer form tag

---

## Transaction Module Rule

Transaction modules generally use:

Header

↓

Lines

↓

Validation

↓

Snapshot preparation

↓

Calculation

↓

Status

↓

Audit

↓

Output / PDF

Purchase Order is the current reference transaction module.

---

## Business Snapshot Rule

If a historical transaction must not change when Master data changes later, store a transaction snapshot.

Current Purchase Order examples:

- Company details
- Supplier details
- Item details
- Specification
- Drawing Number / Revision
- Terms & Conditions

---

## Module Development Flow

1. Requirement
2. Business Flow
3. Database Design
4. Business Rules
5. UI
6. Coding
7. Build
8. Runtime Testing
9. Documentation
10. Git Commit

---

## Change Delivery Standard

When making a focused code change:

- identify exact file
- identify exact region
- use FIND / ADD / REPLACE instructions when patching
- for large unstable files, prefer one complete ready-to-paste version instead of repeated partial patches
- do not change architecture/folder conventions casually

---

## Documentation Update Standard

At module completion update:

- Project Progress
- Sprint Log
- Project State
- Roadmap

Also update when relevant:

- Database Design
- Database Relationship
- Decision Log
- Business Flow
- Transaction Flow
- UI Standards
- Component Library
- Module Blueprint

See `00_DocumentationIndex.md`.

---

## Reference Modules

CRUD reference:

Company Master

Dynamic engineering reference:

Item + Drawing

Transaction reference:

Purchase Order

Small lookup masters:

Reusable Quick Master Modal with live search, duplicate protection, AJAX save and auto-select.

07_CodingStandards.md

MVC Binding Note:
Avoid naming an action complex-model parameter "model"
when the ViewModel itself contains a property named "Model".
ASP.NET Core model binding is case-insensitive and this can
cause prefix collision.

Prefer:
MachineFormViewModel viewModel