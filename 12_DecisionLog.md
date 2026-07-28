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

Authentication UI will be completed before starting business modules.

Scope:
- Login
- Forgot Password
- Reset Password

Reason:
The authentication flow should be visually complete before business module development begins.