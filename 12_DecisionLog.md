# Decision Log

## D-001

Decision

Use ASP.NET Core MVC

Reason

Developer experience.
Fast delivery.

---

## D-002

Decision

Bootstrap Grid + Custom CSS

Reason

Better control over enterprise UI.

---

## D-003

Decision

Component Based Sidebar

Reason

Single Responsibility Principle.
Easy maintenance.

## D-005

Sidebar architecture will be component-based using Razor Partial Views.

Reason:
Reusable and easy maintenance.

---

## D-006

React migration postponed.

Current version will use ASP.NET Core MVC.

Reason:
Faster delivery and aligns with current expertise.

---

## D-007

Dashboard UI frozen.

Only live data binding and charts will be added later.

## D-008

Authentication pages use a dedicated layout (_AuthLayout.cshtml).

Reason

Authentication pages should not display application navigation
(Navbar and Sidebar). This provides a cleaner login experience
and separates authentication from the main application shell.

## D-010

Authentication is being developed in two phases.

Phase 1:
Professional UI + Dummy Navigation

Phase 2:
ASP.NET Core Identity + Authorization + Cookies

Reason:
Allows business module development without waiting for authentication implementation.

## D-011

Authentication implementation is split into two phases.

Phase 1
- Login UI
- Dummy Navigation

Phase 2
- Forgot Password
- Reset Password
- ASP.NET Core Identity
- Cookie Authentication

Reason

Complete the user experience first, then integrate production authentication.

## D-012

Authentication pages must use shared partial views for common UI.

Reason:
Reduce duplicate code and improve maintainability.

D-014

Each business module will have its own ViewModel folder.

Reason:
Keeps the solution modular and scalable.

D-015

All ERP Forms must use strongly typed ViewModels with Data Annotation Validation.

Reason

Consistent Validation
Model Binding
Production Standard

D-016

All business entities must inherit from BaseEntity.

Reason

Avoid duplicate audit fields.

Maintain consistent architecture.

Support soft delete across ERP.

D-017

Domain entities must not contain persistence attributes.

All EF Core mapping will be handled using Fluent API configuration classes.

Reason:
- Clean Architecture
- Better maintainability
- Easier testing
- Separation of concerns

Decision No : D-018

Title

ERP Architecture Freeze

Decision

ERP V1 will use ASP.NET Core MVC.

Application layer will contain all business logic.

Repository Pattern will be used.

Infrastructure will contain EF Core.

Future Web API will reuse the same Application Services.

React is intentionally excluded from Version 1.

Status

Approved

Date

Today's Date

Decision No : D-019

Title:
ERP V1 Architecture Freeze

Decision:

Version 1 of Ajay Industries ERP will be developed using ASP.NET Core MVC.

Business Logic will be implemented in the Application layer.

MVC Controllers will communicate with Application Services using ViewModels.

Repository Pattern will be used for data access.

EF Core will remain inside Infrastructure.

Web API will be introduced only in Version 2 for learning and code reuse.

Status:
Approved