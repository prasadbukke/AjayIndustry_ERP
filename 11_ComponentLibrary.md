# Component Library

## Purpose

Reusable UI components used across the ERP.

---

## Layout Components

- Sidebar
- Top Navbar
- Page Header
- Footer
- Content Container

---

## Form Components

- Textbox
- TextArea
- Dropdown
- Checkbox
- Switch
- Date Picker (Future)
- File Upload (Future)

---

## Validation Components

- Validation Summary
- Field Validation Message
- Required Field Indicator

---

## Table Components

- Bootstrap Table
- Search Box
- Pagination
- Empty Data Message
- Status Badge
- Action Buttons

---

## Action Buttons

- Add
- Save
- Update
- Cancel
- Back
- Details
- Edit
- Delete

---

## Card Components

- Dashboard Card
- Form Card
- Table Card
- Statistics Card (Future)

---

## Status Components

Active

- Green Badge

Inactive

- Red Badge

---

## Notification Components

Current

- TempData Success Message

Future

- Toast Notification
- Error Notification

---

## Modal Components

Future

- Delete Confirmation
- Information Popup

---

## Dashboard Components

- Summary Cards
- Quick Links
- Recent Activities (Future)

---

## Icons

Use only one icon library consistently.

Approved Libraries

- Bootstrap Icons
- Font Awesome

---

## Reference Module

Company Master

All reusable UI components will be designed based on the Company module.

# Component Library Update

Last Updated: 08-Aug-2026

---

## Quick Add Master Modal

Purpose:

Allows reusable Master records to be created without leaving the current
form.

Current supported types:

- Category
- Brand
- UOM
- Shape
- Specification

Features:

- Bootstrap Modal
- AJAX Save
- Anti-forgery token
- Live similar-name suggestions
- Exact duplicate prevention
- Similar-name confirmation
- Existing record selection
- Newly created record auto-selection

---

## Searchable Master Select

Technology:

Select2

Reusable CSS class:

js-master-select

Typical metadata:

data-master-type
data-placeholder
data-add-label

Behavior:

- Search records
- Clear selection where permitted
- Open Quick Add when no record is found
- Auto-select newly created Master

---

## Name Similarity Helper

Shared application utility:

NameSimilarityHelper

Purpose:

Provides common duplicate/similar-name behavior to name-based Masters.

Supports:

- Normalization
- Exact match
- Prefix/contains matching
- Fuzzy matching
- Levenshtein-based similarity
- Ordered live suggestions

---

## Dynamic Child Row Pattern

First implemented in Item Specifications.

Row structure:

Specification | Value | Optional UOM | Remove

Features:

- Add Row
- Remove Row
- Select2 inside dynamic rows
- Explicit MVC collection index
- Dynamic SortOrder
- Duplicate Specification prevention
- Existing row restoration during Edit

MVC posting pattern:

ItemSpecifications.Index

ItemSpecifications[key].ItemSpecificationId
ItemSpecifications[key].SpecificationId
ItemSpecifications[key].SpecificationValue
ItemSpecifications[key].UomId
ItemSpecifications[key].SortOrder

This pattern can be reused for future child-row forms.

---

## Optional UOM Pattern

Some technical values require a UOM.

Example:

Diameter = 25 MM

Some do not.

Example:

Grade = EN8

Therefore child Specification UOM is nullable.

---

## Configuration Duplicate Validation

UI duplicate prevention is not considered sufficient.

Final Item duplicate validation is always executed in Application
Service.

Duplicate identity:

ItemName + Shape + Complete Specification Set

This protects the system even when requests bypass browser-side
JavaScript.