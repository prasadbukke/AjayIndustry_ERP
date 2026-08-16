# 06 - UI Standards

## Theme

- Professional ERP
- Clean Interface
- Enterprise Layout
- Responsive Design
- Consistent action placement

---

## Technology

- Bootstrap 5
- Font Awesome / Bootstrap Icons
- Custom CSS
- Select2 where searchable dropdowns are useful

---

## Layout

- Left Sidebar
- Top Navbar
- Content Area
- Footer

---

## Every Module Page

Where applicable:

- Page Header
- Muted subtitle/context
- Search Box
- Add Button
- Card Layout
- Responsive Bootstrap Table
- Pagination
- Empty Record Message
- Toast feedback

---

## Create / Edit Page

- `<form>` tag belongs in Create/Edit wrapper view.
- Shared `_Form.cshtml` contains common form content only.
- `_ValidationScriptsPartial` belongs in Create/Edit wrapper.
- Use Bootstrap Grid.
- Validation Summary at top.
- Save / Update button at bottom.
- Cancel / Back action available.
- Business validation remains authoritative in Service Layer.

---

## Details Page

- Read-only presentation
- Card layout
- 3-column desktop information layout when suitable
- Status badge where status is useful
- Back button
- Transaction workflow buttons only when the current status permits the action

---

## Table Standard

Typical columns:

- Sr. No.
- Primary Code / Transaction Number
- Main Name / Supplier / Reference
- Status
- Amounts where applicable
- Actions

Tables must use responsive containers when horizontal width can grow.

---

## Action Standard

Master actions:

- Details
- Edit
- Delete

Transaction actions may also include:

- PDF / Print
- Workflow action

For transaction lists, keep action layout visually stable.

If Edit/Delete is not permitted for a transaction status, prefer a visible disabled button instead of removing the action slot when consistent alignment is important.

---

## Purchase Order UI Standard

Purchase Order Create/Edit:

- Company
- Supplier
- PO Date
- Expected Delivery
- Payment Terms
- Delivery Terms
- Delivery Address
- Remarks
- Dynamic Item rows
- Optional Drawing
- HSN
- Quantity
- UOM
- Rate
- GST %
- Line Total
- Transport Charges
- Other Charges
- Amount Summary
- Terms & Conditions information block

Purchase Order Details:

- 3-column Purchase Order information
- 3-column Delivery & Terms information
- Terms & Conditions
- Item table
- Amount Summary
- Workflow actions
- PDF download

Purchase Order Index:

- Details
- PDF
- Edit
- Delete

Edit/Delete are Draft-only.

---

## Search

- Search at top
- Search by meaningful business fields
- Case insensitive where practical
- Shared Search component preferred

---

## Pagination

Supported sizes:

- 10
- 25
- 50

Shared Pagination component preferred.

---

## Buttons

Primary:

- Save
- Confirm / main workflow action

Secondary:

- Cancel
- Back

Warning:

- Edit

Danger:

- Delete
- PDF may use danger-outline icon style

Info:

- Details

---

## Status Badge

Master:

- Active
- Inactive

Purchase Order:

- Draft
- Confirmed
- Sent
- future receipt statuses when GRN is implemented

---

## Icons

Use Font Awesome or Bootstrap Icons consistently within the same screen.

Common areas:

- Dashboard
- Company
- Employee
- Customer
- Supplier
- Warehouse
- Category
- Item
- Drawing
- Purchase
- Inventory
- Production
- Sales
- Finance
- Reports
- Settings

---

## Reference

Company Master remains the baseline CRUD reference.

Item / Drawing provide the reference for dynamic engineering data.

Purchase Order provides the reference for transaction header-lines, live totals, status actions and supplier PDF output.
