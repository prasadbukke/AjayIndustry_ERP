# Architecture

## Architecture Style

Clean Architecture

---

## Technology Stack

Presentation

- ASP.NET Core MVC (.NET 8)

API

- ASP.NET Core Web API (.NET 8) (Future)

Application

- Services
- Interfaces
- Contracts

Infrastructure

- Entity Framework Core
- Repositories
- SQL Server

Domain

- Entities
- Common

---

## Layer Flow

MVC View

↓

Controller

↓

Application Service

↓

Repository

↓

DbContext

↓

SQL Server

---

## Future Flow

MVC

↓

Web API

↓

Application

↓

Infrastructure

↓

Domain

↓

SQL Server

Business logic will be reused by both MVC and Web API.

---

## Dependency Rule

Web

↓

Application

↓

Infrastructure

↓

Domain

Dependencies are always inward.

---

## Business Rules

- Business logic only in Service Layer.
- Database access only through Repository.
- Controllers handle only requests and responses.
- Domain contains only business entities.

---

## Data Flow

User

↓

MVC View

↓

Controller

↓

Service

↓

Repository

↓

Entity Framework Core

↓

SQL Server

↓

Response

↓

MVC View

---

## Reference Module

Company Master

The Company module is the reference implementation for all future ERP modules.



# ERP Architecture Freeze v1.0

Project

Ajay Industries ERP

Status

✅ LOCKED

---

Architecture

Clean Architecture

Web

↓

Application

↓

Domain

↓

Infrastructure

---

Patterns

- Repository Pattern
- Service Pattern
- Dependency Injection
- Business Exception
- Shared Components

---

Shared Components

- Search
- Pagination
- Delete Confirmation Modal
- Toast Notification

---

Controller Standard

Index

Details

Create (GET)

Create (POST)

Edit (GET)

Edit (POST)

Delete (POST)

---

Repository Standard

GetAllAsync

GetByIdAsync

SearchAsync

GetPagedAsync

CreateAsync

UpdateAsync

DeleteAsync

SaveChangesAsync

---

Service Standard

GetAllAsync

GetByIdAsync

SearchAsync

GetPagedAsync

CreateAsync

UpdateAsync

DeleteAsync

---

Validation Standard

UI

DataAnnotations

↓

Service

Business Rules

↓

Controller

BusinessException

↓

Toast Notification

---

Database Standard

Every table contains

Id

Code

IsActive

IsDeleted

CreatedBy

CreatedOn

ModifiedBy

ModifiedOn

---

Development Rule

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

Change Policy

Architecture Changes

❌ Not Allowed

Database Breaking Changes

❌ Not Allowed

Folder Structure Changes

❌ Not Allowed

Only Business Rules can grow.

Status

✅ APPROVED

Decision : Item Master Domain Separation

Status : LOCKED

Item Master stores only master information.

Purchase Module owns:
- Purchase Price
- GST
- HSN
- Supplier Pricing

Inventory Module owns:
- Current Stock
- Stock Transactions
- Stock Ledger

Reason:
Single Responsibility Principle.
Avoid duplicate data.
Production ERP design.

Decision

Brand and Item Category are independent Master Modules.

Reason

Both modules are referenced by Item Master through Foreign Keys.

Status

LOCKED

Reusable Quick Master Framework

Purpose

Provides a common framework for creating
small master records without leaving the
current transaction screen.

Current Masters

- Item Category
- Brand
- UOM

Future

- Supplier Type
- Customer Type
- Machine Type
- Department
- Designation
- Payment Terms
- Tax Group
- Warehouse Type