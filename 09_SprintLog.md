# 09 - Sprint Log

## Ajay Industries ERP

---

# Foundation Completed

Technology foundation:

- ASP.NET Core MVC
- .NET 8
- Entity Framework Core
- SQL Server
- Clean Architecture
- Repository + Service Pattern

Shared infrastructure established:

- BaseEntity
- Soft Delete
- BusinessException
- PagedResult
- Search
- Pagination
- Toast Notifications
- Delete Confirmation
- Name Similarity
- Select2
- Quick Add Masters

---

# Completed Master Modules

Completed and working:

- Company
- Employee
- UOM
- Warehouse
- Item Category
- Brand
- Shape
- Specification
- Supplier
- Item
- Drawing

---

# Item Master Development

Implemented:

- Auto ItemCode
- ItemName
- Category
- Brand
- Main UOM
- Optional Shape
- Description
- Dynamic Specifications
- Specification Value
- Optional Specification UOM
- Quick Add Master support
- Similar Item Name warning
- Exact configuration duplicate protection
- Specification-aware search
- Compact Specification display

Final Item duplicate identity:

ItemName
+ Shape
+ Complete Specifications

---

# Item Part Number Enhancement

PartNumber was added and finalized.

Rules:

- Optional
- Maximum 100 characters
- Not unique
- Searchable
- Create/Edit supported
- Index display supported
- Details display supported

PartNumber is not part of Item duplicate configuration.

---

# Item Image Cleanup

ImagePath had previously been introduced as a possible Item field.

After reviewing actual ERP usage, Item Image was considered unnecessary.

ImagePath was removed from:

- Item Entity
- Configuration
- Database
- Snapshot/current model

Reason:

Engineering identification is already provided through Drawing and technical Item data.

---

# Item UI Finalization

Item Details was improved to a clean three-column layout.

Final Item Information arrangement:

- Item Code
- Item Name
- Part Number
- Category
- Brand
- UOM
- Shape
- Status
- Description

Item Index now includes Part Number.

Specification rendering in Item Index was corrected to display compact technical badges correctly.

---

# Supplier Master Sprint

Completed:

- Supplier Code generation
- Supplier Name
- Contact information
- GSTIN
- PAN
- Address
- Payment Terms
- Similar spelling detection
- Duplicate validation
- Search
- Pagination
- Soft Delete

SupplierCode format:

SUP00001

---

# Drawing Master Sprint

Final relationship:

One Item
→ One Drawing Number
→ Many Revisions

Completed:

- Drawing Number
- Drawing Name
- Drawing Type
- Item relationship
- File upload
- Search
- Details
- Edit
- Soft Delete

---

# Drawing Revision Workflow

Implemented:

- Auto RV-01
- Auto next Revision
- Revision History
- Current / Inactive state
- Activate Previous Revision
- Delete Inactive Revision
- Current Revision protection

Deleted Revision Numbers remain reserved.

---

# Drawing Soft Delete / Restore

Complete Drawing Soft Delete was implemented.

Deleted Drawing Number remains reserved.

Dedicated Deleted Drawings UI implemented.

Restore functionality implemented.

Restore returns revision history and Current Revision.

---

# Drawing IsPrimary Cleanup

Earlier Drawing design contained IsPrimary.

Final One-Item-One-Drawing architecture made IsPrimary unnecessary.

IsPrimary was removed from:

- Entity
- Configuration
- Repository
- Service
- ViewModel
- Controller
- UI
- Database

---

# Item and Drawing Integration Sprint

Completed integration:

Item Details:

- Drawing Number
- Drawing Name
- Current Revision
- Drawing Type
- Drawing File
- Open Drawing Details

Item Edit:

- Read-only Drawing summary
- Add Drawing action when no Drawing exists

Item Create:

- Displays Save-first Drawing message

After Item Save:

- User is redirected to Item Details

Add Drawing:

- Opens Drawing Create
- Item is automatically selected

Second Drawing for the same Item remains blocked.

---

# Testing Completed

Successfully tested:

- Item Create
- Item Edit
- PartNumber
- PartNumber Search
- Duplicate PartNumber allowed
- Dynamic Specifications
- Item Details
- Drawing Create
- Drawing Revision Add
- Drawing Revision Activate
- Drawing Revision Delete
- Auto Revision Number
- Drawing Soft Delete
- Drawing Restore
- Item to Drawing navigation
- Add Drawing from Item
- Item auto-selection in Drawing Create
- Same Item second Drawing blocked

---

# Git Milestones

Drawing Master milestone committed.

Item Master + Drawing integration milestone committed.

Latest milestone commit:

Finalize item master with part number and drawing integration

---

---

# Purchase Order Design Sprint

Final Purchase Order architecture was designed and implemented as:

PurchaseOrder
→ Header

PurchaseOrderItem
→ Multiple lines

Implemented business design:

- Company reference + snapshot
- Supplier reference + snapshot
- Item reference + snapshot
- Specification snapshot
- Optional Drawing reference + Drawing Number / Revision snapshot
- Purchase-specific HSN
- Quantity
- UOM snapshot
- Unit Price
- GST
- Transport Charges
- Other Charges
- Grand Total
- Terms & Conditions snapshot

PO Number format:

`AI/PO/26-27/00001`

Financial Year:

April to March

Sequence uses five digits and PO numbers are never reused.

---

# Purchase Order GST Sprint

Implemented:

- Default GST 18%
- Manual GST percentage change per line
- Company.State vs Supplier.State tax-type detection
- Same State → CGST + SGST
- Different State → IGST
- Live Create/Edit GST preview
- Service-layer authoritative GST calculation

GSTIN is optional and is not used to determine GST type.

---

# Purchase Order Commercial Rule Cleanup

Final Purchase Order rules:

- Discount is not used.
- Round Off is not used.
- Existing database fields are retained for compatibility and forced to zero.
- Line Total remains.
- Transport Charges remain.
- Other Charges remain.
- No separate GST is currently calculated on Transport / Other Charges.

Final amount logic:

Quantity × Rate
→ Taxable Amount
→ GST
→ Transport Charges
→ Other Charges
→ Grand Total

---

# Purchase Order Terms & Conditions Sprint

Company Master was extended with standard Purchase Order Terms & Conditions.

Flow:

Company.PurchaseOrderTermsAndConditions
→ PurchaseOrder.TermsAndConditions snapshot

Historical Purchase Orders keep their saved Terms & Conditions even if Company Master changes later.

Company GST Number was made optional.

---

# Purchase Order Workflow Sprint

Implemented workflow:

Draft
→ Confirmed
→ Sent

Rules:

- Draft can be edited.
- Draft can be soft deleted.
- Confirmed/Sent cannot be edited.
- Confirmed/Sent cannot be deleted through current workflow.
- No separate Cancel action is currently implemented.
- PartiallyReceived / Received are reserved for GRN integration.

Purchase Order does not update inventory stock.

---

# Purchase Order PDF Sprint

Professional supplier PDF implemented using QuestPDF.

PDF includes:

- Company logo
- Company information
- PO Number / PO Date
- Supplier & Delivery grid
- Item / Specification / Drawing
- HSN
- Qty / UOM / Rate
- GST %
- Taxable / Line Total
- CGST / SGST / IGST
- Transport Charges
- Other Charges
- Grand Total
- Remarks
- Terms & Conditions
- Prepared / Checked By
- Authorized Signatory
- Footer / page number

PDF status is intentionally not displayed.

PDF layout was compared against the approved Purchase Order format and refined.

---

# Purchase Order Testing

Successfully tested during implementation:

- Create
- Edit Draft
- Details
- Draft delete
- Confirm
- Mark as Sent
- Item add/remove
- Drawing / no Drawing
- Specification display
- Default GST
- Manual GST rate change
- Same-state GST split
- Different-state GST split
- Supplier change live tax-type refresh
- Transport Charges
- Other Charges
- Terms & Conditions snapshot
- PDF generation
- PDF download

---

# Git Milestones

Completed earlier:

- Drawing Master milestone
- Item Master + Drawing integration milestone
- Purchase Order core workflow/GST/T&C milestone

Current documentation milestone:

Finalize Purchase Order module documentation and PDF milestone.

---

# Next Sprint

Next selected module:

**GRN - Goods Receipt Note**

GRN design must begin with:

Requirement
→ Business Flow
→ Database Design
→ Business Rules
→ UI
→ Coding

Main future purpose:

Purchase Order
→ Material Receipt
→ Partial / Full Receipt
→ PO receipt status
→ Inventory Stock Transaction

GRN development milestone entry
ACTION: ADD Customer Master completion entry
ADD:
- Customer Master implemented and runtime tested