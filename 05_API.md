# API Design

## Purpose

The ERP follows a reusable architecture.

Business logic is written once in the Application Layer.

Initially the application is delivered using ASP.NET Core MVC.

In future the same business logic will be exposed through ASP.NET Core Web API without changing the Service or Repository layer.

---

# API Version

v1

---

# Base URL

/api

---

# Company API

## Get All Companies

GET

/api/company

---

## Get Company By Id

GET

/ api/company/{id}

---

## Create Company

POST

/ api/company

Validation

- Company Name Required
- GST Number Required
- GST Number Unique
- Company Code Auto Generated

---

## Update Company

PUT

/ api/company/{id}

---

## Delete Company

DELETE

/ api/company/{id}

Delete Type

Soft Delete

---

## Search Company

GET

/api/company?searchText=abc

Search Fields

- Company Code
- Company Name
- GST Number

---

## Pagination

GET

/api/company?pageNumber=1&pageSize=10

---

# Future APIs

## Masters

- Employee API
- Customer API
- Supplier API
- Warehouse API
- Unit API
- Category API
- Item API
- Machine API
- Bill Of Material API

---

## Purchase

- Purchase Requisition API
- Purchase Order API
- Goods Receipt API
- Purchase Invoice API
- Purchase Return API

---

## Inventory

- Stock API
- Stock Adjustment API
- Stock Transfer API
- Warehouse Stock API
- Stock Ledger API

---

## Production

- Production Order API
- Material Issue API
- Material Return API
- Production Entry API
- Finished Goods API

---

## Sales

- Quotation API
- Sales Order API
- Delivery Challan API
- Sales Invoice API
- Sales Return API

---

## Finance

- Payment API
- Receipt API
- Expense API
- Outstanding Payment API

---

Status

MVC Completed

Web API Planned

Employee CRUD API
Employee Search
Employee Pagination

## Employee APIs

GET

/api/Employee

GET

/api/Employee/{id}

POST

/api/Employee

PUT

/api/Employee

POST

/api/Employee/Delete/{id}

Features

- Search
- Pagination
- Auto Employee Code