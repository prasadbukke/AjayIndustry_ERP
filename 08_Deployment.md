# 08 - Deployment

## Current Status

Production deployment architecture is not yet finalized.

The project is currently developed and tested as an ASP.NET Core MVC .NET 8 application with SQL Server.

Do not document a hosting provider, server topology or production connection strategy until it is explicitly selected.

---

## Current Application Requirements

- .NET 8 runtime / hosting support
- SQL Server
- ASP.NET Core MVC
- Static file support for `wwwroot`

---

## Runtime File Areas

Current file-based features include:

Drawing files:

`wwwroot/uploads/drawings`

Purchase Order company logo:

`wwwroot/images/company/`

These folders must be available in a deployed environment.

Drawing history files must not be accidentally deleted during publish/deployment.

---

## Database

EF Core migrations are used for schema changes.

Deployment procedure must eventually include:

- connection string configuration
- migration strategy
- database backup
- environment-specific secrets
- rollback plan

These are not yet frozen.

---

## PDF

Purchase Order PDF generation uses QuestPDF.

Deployment must preserve:

- QuestPDF package/runtime dependencies
- company logo static file
- any production license configuration required for the chosen QuestPDF license

---

## Future Deployment Checklist

To be finalized before production release:

- Hosting environment
- IIS / reverse proxy configuration if applicable
- HTTPS certificate
- Production connection string
- Secret storage
- Database backup policy
- File backup policy
- Logging
- Error handling
- Health monitoring
- Publish process
- Migration/rollback process
