# 12 - Decision Log

## Project

Ajay Industries ERP

This document records architecture and business decisions that should not be changed casually.

---

# Decision 001 - Clean Architecture

The ERP uses:

- Domain
- Application
- Infrastructure
- Web

ASP.NET Core MVC is the current presentation technology.

React is not part of the current architecture.

Web API may be added later when required.

---

# Decision 002 - Repository + Service Pattern

Database access belongs to Repository classes.

Business rules belong to Application Services.

MVC Controllers should remain thin.

Business validation errors use `BusinessException`.

---

# Decision 003 - Soft Delete

Business records use Soft Delete.

Physical deletion is avoided.

Standard fields:

- IsDeleted
- IsActive

Deleted records are normally excluded from UI lists.

---

# Decision 004 - Business Code Reuse

Auto-generated business codes must never be reused.

Examples:

- ITM00001
- SUP00001
- SPC00001

Deleted records are included when generating/checking codes.

---

# Decision 005 - Item Code

ItemCode remains system generated.

Format:

`ITM00001`

ItemCode is ERP identity, not a human-readable Item description.

Meaningful Item information belongs to:

- ItemName
- Shape
- Specifications
- PartNumber
- Drawing
- Image

---

# Decision 006 - Item Name Is Not Globally Unique

Same Item Name may exist for different Item configurations.

Example:

MS Round Bar with different Diameter/Grade combinations.

Exact Item duplicate identity is:

- ItemName
- Shape
- Complete specification configuration

Specification row order is ignored.

Category, Brand and main UOM are excluded from duplicate signature.

---

# Decision 007 - Shape Is a Standalone Master

Shape is maintained independently.

Item has optional ShapeId.

Shape participates in Item duplicate/configuration identity.

---

# Decision 008 - Specification Is a Standalone Master

Specification is maintained independently.

Examples:

- Diameter
- Length
- Width
- Thickness
- Grade

Grade is handled as a Specification.

A separate Grade master/column is not currently required.

---

# Decision 009 - Drawing Number Was Deferred Initially

Drawing Number was initially deferred while Item configuration architecture was still evolving.

After Item/Specification architecture stabilized, Drawing was implemented as a dedicated module.

---

# Decision 010 - Drawing Is Not an Item Column

Drawing Number is not stored directly on Item.

Drawing has its own engineering lifecycle.

Therefore Drawing is represented in a dedicated `Drawings` table.

---

# Decision 011 - One Drawing Table

Drawing and revision history currently use one table.

A separate `DrawingRevisions` table is intentionally not used at this stage.

Every Drawings row represents one revision.

This avoids unnecessary multi-table complexity while still supporting revision history.

---

# Decision 012 - One Item Has One Drawing Number

Final Drawing relationship:

`One Item -> One Drawing Number -> Many Revisions`

Same Item cannot have another Drawing Number.

If a Drawing changes, a new Revision must be added.

This decision replaced the earlier idea of allowing multiple Drawings per Item.

---

# Decision 013 - Drawing Number Is Permanent

Drawing Number:

- is manually entered
- is required
- is permanent
- cannot be changed after creation
- cannot be reused after deletion

Drawing Number is an engineering identity.

---

# Decision 014 - Drawing Number Similarity

During Drawing Create:

Exact Drawing Number:

- blocks Create
- user is instructed to open existing Drawing and add Revision

Similar Drawing Number:

- warning only

Drawing Name:

- exact/similar spelling warning
- does not block Save

---

# Decision 015 - Revision Number Is Auto Generated

Revision Number is not manually entered.

Format:

- RV-01
- RV-02
- RV-03

Deleted revisions remain part of numbering history.

Revision Numbers are never reused.

Legacy values such as R01/R02 are recognized while calculating the next number.

---

# Decision 016 - Only One Current Revision

For one Drawing Number, only one Revision can be Current.

`IsActive = true` means Current Revision.

Historical revisions use:

`IsActive = false`

A filtered unique database index protects this rule.

---

# Decision 017 - Previous Revision Can Be Reactivated

Historical inactive revisions may be activated again.

When an old revision becomes Current:

- existing Current revision becomes Inactive
- selected revision becomes Current

Only one Current revision can exist.

The switch is executed inside a database transaction.

---

# Decision 018 - Revision Soft Delete

Inactive revisions may be soft deleted.

Current revision cannot be deleted directly.

The user must first activate another revision.

Deleted revision:

- remains in database
- disappears from normal UI
- retains its revision number
- retains its physical Drawing file

---

# Decision 019 - Drawing Soft Delete and Restore

Deleting the complete Drawing does not free the Drawing Number.

Deleted Drawings appear in a dedicated Deleted Drawings screen.

User can Restore the Drawing.

This prevents the confusing situation where:

- Drawing disappears from normal UI
- same Drawing Number remains reserved
- user cannot understand why Create is blocked

Restore is the correct recovery path.

---

# Decision 020 - Drawing Files Are Not Stored as SQL Binary

Drawing files are stored in the web file system.

Current location:

`wwwroot/uploads/drawings`

Database stores:

- FileName
- FilePath

File binary is not stored in SQL Server.

---

# Decision 021 - Historical Drawing Files Are Preserved

Old revision files are not overwritten.

A new revision creates a new file reference.

Soft deletion does not physically remove the file.

This preserves engineering history.

---

# Decision 022 - IsPrimary Removed

The original Drawing design contained `IsPrimary`.

After finalizing:

`One Item -> One Drawing Number`

the Primary concept became unnecessary.

`IsPrimary` was removed from:

- Domain entity
- ViewModel
- Service
- Repository
- UI
- Database

---

# Decision 023 - Supplier Master Financial Separation

Supplier Master stores identity/contact/tax/address/payment-term data only.

It does not store transaction-derived values such as:

- Opening Balance
- Purchase Total
- Pending Payment
- Last Purchase
- GST totals

These belong to future accounting/purchase modules.

---

# Decision 024 - Supplier Duplicate Rules

Supplier Name:

- exact active duplicate blocked
- similar spelling warning

GSTIN:

- optional
- active unique when provided

PAN:

- optional
- not unique

---

# Decision 025 - Item Part Number

PartNumber is an optional Item field.

It is not currently unique.

Reason:

Manufacturer/internal part numbers may overlap across brands/sources.

---

# Decision 026 - Item Image

Item should support a primary image.

Current design:

`Items.ImagePath`

Image binary will not be stored in SQL Server.

Actual Item image upload/display integration is the next Item Master enhancement.

---

# Decision 027 - Production Workflow Is Database Driven

Future Production workflow will be database driven.

Planned statuses:

- Pending
- Running
- Completed
- Rejected
- On Hold
- Cancelled

Future production history should capture:

- Operator
- Machine
- Start Time
- End Time
- Duration
- Good Quantity
- Reject Quantity
- Rework Quantity
- Remarks
- Current Stage
- Overall Progress

Production coding is deferred until its module phase.