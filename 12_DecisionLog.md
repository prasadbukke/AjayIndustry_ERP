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

## Decision 011

Title

Shared UI Components

Decision

The ERP will use reusable components for:

- Search
- Pagination
- Delete Confirmation
- Toast Notification

Status

Approved

## Decision 012

Title

Business Exception Handling

Decision

Business validations will throw BusinessException.

Controllers will catch BusinessException and display Toast Notification.

Unexpected exceptions will display a generic message.

Global Exception Middleware will be implemented during Production Hardening phase.

Status

Approved

# Decision Log Update

Date: 08-Aug-2026

---

## Item Shape

Decision:

Shape will be maintained as a standalone reusable Shape Master.

Item contains:

ShapeId - Nullable

Reason:

Shape is reusable across many Items and should not be entered as random
free text.

Examples:

- Round
- Flat
- Square
- Sheet
- Pipe

---

## Item Specifications

Decision:

Specification names will NOT be stored as free-text fields directly
inside Item Master.

A reusable Specification Master will be maintained.

Item Specification values will be stored in ItemSpecifications.

Example:

Specification Master:
Diameter
Length
Grade

Item:
MS Round Bar

Values:
Diameter = 25 MM
Length = 6000 MM
Grade = EN8

Reason:

Prevents inconsistent names such as:

Diameter
Dia
OD
Diamter

and enables future reporting, filtering and searching.

---

## Grade

Decision:

Grade will currently be treated as an Item Specification.

Example:

Grade = EN8

A dedicated Grade Master is not required at this stage.

This decision may be revisited if Grade later requires its own business
attributes or transactional behavior.

---

## Drawing Number

Decision:

Drawing Number is not included in Item Master at this stage.

It can be introduced later when drawing-controlled manufactured Items
require it.

---

## Same Item Name

Decision:

ItemName is NOT unique.

Example:

MS Round Bar - Diameter 25 MM

MS Round Bar - Diameter 30 MM

Both are valid separate Items.

---

## Item Duplicate Rule

Decision:

An Item is considered an exact duplicate only when all of the following
match:

- Item Name
- Shape
- Complete Specification Set
- Specification Values
- Specification UOMs

Specification row order does not affect duplicate detection.

---

## Item Stock

Decision:

Stock quantities will NOT be stored directly in Item Master.

Stock information belongs to Inventory transactions and warehouse-wise
stock tables.

Future Item availability information will be derived from Inventory.

---

## Warehouse

Decision:

Warehouse is not permanently attached to Item Master.

One Item can exist across multiple Warehouses.

Warehouse-wise stock will be handled by the Inventory module.

---

## GST and Tax

Decision:

GST and tax configuration will not be stored directly in Item Master.

Tax configuration will be maintained separately and linked as required.

---

## Pricing

Decision:

Purchase and Sales prices are not stored directly in Item Master.

Pricing will be transaction/supplier/customer dependent and handled in
dedicated modules.

---

## Quick Master Creation

Decision:

Reusable Master dropdowns may provide Quick Add functionality without
leaving the current transaction/form.

Current supported Quick Masters:

- Category
- Brand
- UOM
- Shape
- Specification

Quick Add supports:

- Live similar-name suggestions
- Exact duplicate prevention
- Similar-name confirmation
- AJAX creation
- Automatic selection of the newly created record

---

## Name Similarity

Decision:

Name-based Masters will use reusable NameSimilarityHelper logic.

Behavior:

Exact match:
Block duplicate creation.

Similar spelling:
Show warning and allow user confirmation.

Live suggestions:
Start while the user is typing.

This pattern should be reused by future name-based Masters.