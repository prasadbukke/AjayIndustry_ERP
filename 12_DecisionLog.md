# Decision Log

---

## Decision 001

### Architecture

Approved

- Clean Architecture
- Repository Pattern
- Service Pattern

Status

✅ Approved

---

## Decision 002

### UI Technology

Approved

- ASP.NET Core MVC

Reason

Client delivery will be done using MVC.

Status

✅ Approved

---

## Decision 003

### API Strategy

Approved

ASP.NET Core Web API will be developed after the MVC application.

Business logic will be reused from the Application Layer.

Status

✅ Approved

---

## Decision 004

### Delete Strategy

Approved

Soft Delete

Reason

ERP records should never be physically deleted.

Status

✅ Approved

---

## Decision 005

### Audit Fields

Every master table will contain

- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

Status

✅ Approved

---

## Decision 006

### Company Code

Format

CMP00001

Auto Generated

Status

✅ Approved

---

## Decision 007

### Reference Module

Company Master

Reason

Every future ERP module will follow the same

- Folder Structure
- Coding Style
- Comments
- Validation
- CRUD Flow
- Search
- Pagination
- Documentation

Status

✅ Approved

---

## Decision 008

### ERP Navigation

Navigation is frozen.

Dashboard

Masters

Purchase

Inventory

Production

Sales

Finance

Reports

Settings

No new module will be introduced unless required by the client.

Status

✅ Approved

---

## Decision 009

### Documentation Policy

Every completed module must update

- DatabaseDesign.md
- API.md
- SprintLog.md
- DecisionLog.md
- ProjectState.md

before starting the next module.

Status

✅ Approved

---

## Decision 010

### Git Policy

Every completed module requires

- Git Commit
- Documentation Update

before starting the next module.

Status

✅ Approved

Decision 011

Toast Notification

Approved

Toastr will be used throughout the ERP for

- Success
- Error
- Warning
- Info

Status

✅ Approved

Decision 011

Reusable Components

Approved

- Shared Search
- Shared Pagination
- Shared Delete Modal
- Toast Notification

Status

✅ Approved