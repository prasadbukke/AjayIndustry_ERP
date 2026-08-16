# Ajay Industries ERP

## Vision

Build a production-ready ERP system for a manufacturing company using ASP.NET Core MVC, SQL Server and Clean Architecture.

The ERP should grow module-by-module without breaking the established architecture, database history or transaction traceability.

---

## Business Goal

Create one connected ERP platform for:

- Master Data
- Purchase
- Inventory
- Production
- Sales
- Finance
- Reports
- Administration / Settings

The system should preserve historical transaction data through snapshots and soft-delete rules instead of depending on mutable Master data.

---

## Engineering Goals

- Real client project
- Enterprise architecture
- Maintainable production code
- Reusable module patterns
- Strong business-rule separation
- Historical transaction traceability
- Professional document/PDF output
- Future Web API reuse
- Portfolio project
- Senior .NET interview preparation

---

## Technology

- ASP.NET Core MVC (.NET 8)
- SQL Server
- Entity Framework Core
- Clean Architecture
- Repository + Service Pattern
- Bootstrap 5
- Font Awesome / Bootstrap Icons
- JavaScript / jQuery where required
- QuestPDF for Purchase Order PDF generation

---

## Guiding Principles

- Business logic belongs in Application Services.
- Database access belongs in Repositories.
- Controllers coordinate requests/responses only.
- Domain entities contain business data only.
- Soft Delete is preferred for business data.
- Business codes are permanent and never reused.
- Master data stores stable identity/configuration.
- Transaction data stores transaction-specific values and historical snapshots.
- Stock changes only through inventory/receipt/issue transactions, not through Master data or Purchase Order creation.
- Major module development follows: Requirement → Business Flow → Database Design → Business Rules → UI → Coding → Testing → Documentation → Git Commit.
