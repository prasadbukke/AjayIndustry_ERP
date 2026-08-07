# Coding Standards

## Architecture

- Clean Architecture
- Repository Pattern
- Service Pattern
- Dependency Injection

---

## Project Structure

Domain

↓

Application

↓

Infrastructure

↓

Web (MVC)

↓

Web API (Future)

---

## File Standard

Every C# file must contain

- File Header Comment
- XML Comments
- Regions
- Proper Naming

---

## Controller Rules

Controller contains

- HTTP Request Handling
- Model Validation
- Service Calls
- Redirects

Controller must NOT contain

- Business Logic
- Entity Framework Code
- SQL Queries

---

## Service Rules

Service contains

- Business Rules
- Validation
- Mapping
- Workflow

Service must NOT access DbContext directly.

---

## Repository Rules

Repository contains

- Entity Framework Queries
- CRUD Operations
- Search
- Pagination

Repository must NOT contain business logic.

---

## Entity Rules

Every Entity inherits

BaseEntity

Contains

- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

---

## Delete Rule

Soft Delete Only

Never physically delete records.

---

## Async Rule

Use async / await for

- Database
- Repository
- Service

---

## Validation Rule

Use

- DataAnnotations
- Service Validation
- Duplicate Validation

---

## Naming Convention

Entity

Company

Repository

CompanyRepository

Interface

ICompanyRepository

Service

CompanyService

Interface

ICompanyService

Controller

CompanyController

ViewModel

CompanyViewModel

---

## Module Development Flow

Entity

↓

Configuration

↓

Migration

↓

Repository

↓

Service

↓

Dependency Injection

↓

Controller

↓

Views

↓

Create

↓

List

↓

Details

↓

Edit

↓

Delete

↓

Search

↓

Pagination

↓

Documentation

↓

Git Commit

----

## Reference Module

Company Master

All future ERP modules must follow the Company Master coding pattern.

Small lookup masters should never redirect
the user away from the transaction screen.

Use the reusable Quick Master Modal.

Required Features

- Live Search
- Similar Name Detection
- Exact Duplicate Blocking
- AJAX Save
- Auto Select