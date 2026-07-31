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