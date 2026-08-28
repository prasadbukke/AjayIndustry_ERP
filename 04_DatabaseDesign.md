# 04 - Database Design

## Project

Ajay Industries ERP

## Technology

- ASP.NET Core MVC
- .NET 8
- Entity Framework Core
- SQL Server
- Clean Architecture
- Repository + Service Pattern

---

# 1. General Database Rules

Business entities use the common BaseEntity audit structure where applicable.

Common fields:

- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

General rules:

- Soft Delete is the default.
- Physical deletion is avoided.
- Business codes are never reused.
- Deleted records are included when checking generated codes where the code is permanent.
- Foreign-key delete behavior should normally use Restrict for master/transaction references.
- Application Services enforce business rules.
- Database indexes/constraints provide additional protection where practical.
- Browser-posted snapshot/calculated values are not trusted when the authoritative source can be reloaded from the database.
- Historical transaction snapshots are retained so old documents remain stable even if Master data changes later.

---

# 2. Current Database Scope

## Completed / Active Master Areas

- Companies
- Employees
- UOMs
- Warehouses
- ItemCategories
- Brands
- Shapes
- Specifications
- Items
- ItemSpecifications
- Suppliers
- Drawings
- Customers
- Machines
- ProductionOperations

## Manufacturing Configuration

- ItemProcessRoutings
- Item Process Routing Steps

## Implemented Transaction Areas

- PurchaseOrders
- PurchaseOrderItems
- GoodsReceiptNotes
- GoodsReceiptNoteItems
- CustomerPurchaseOrders
- CustomerPurchaseOrderItems
- ProductionJobs
- ProductionJobSteps
- ProductionJobStepHistory
- DeliveryChallans
- DeliveryChallanItems
- Invoices
- InvoiceItems

## PDI Status

PDI is part of the business flow and document design.

However, the exact final PDI database entity/table schema is not locked in this document because the complete implemented PDI persistence model has not yet been verified against the current source.

Do not create or rename PDI tables based only on this document.

---

# 3. Automatic Codes

## Master Codes

| Master | Code Format |
|---|---|
| Company | CMP00001 |
| Employee | EMP00001 |
| Warehouse | WH00001 |
| Item Category | CAT00001 |
| Brand | BRD00001 |
| Shape | SHP00001 |
| Specification | SPC00001 |
| Item | ITM00001 |
| Supplier | SUP00001 |

UOM Code is manually maintained.

Customer has a permanent Customer Code, but its exact format is intentionally not restated here until verified from the current Customer module source.

## Financial-Year Transaction Codes

| Transaction | Code Format |
|---|---|
| Purchase Order | AI/PO/26-27/00001 |
| Goods Receipt Note | AI/GRN/26-27/00001 |
| Customer Purchase Order | AI/CPO/26-27/00001 |
| Production Job | AI/PJOB/26-27/00001 |
| Invoice | AI/INV/26-27/00001 |

Financial Year:

- April to March
- Example: 01-Apr-2026 to 31-Mar-2027 = `26-27`
- Sequence is five digits.
- Deleted transaction codes remain reserved.
- Generated document codes are never reused.

---

# 4. Items Table

## Items

Important fields:

- ItemId
- ItemCode
- ItemName
- PartNumber
- Description
- ItemCategoryId
- BrandId
- UomId
- ShapeId
- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

## ItemCode

- System generated
- Format: ITM00001
- Permanent ERP identity
- Unique
- Never reused

ItemCode is not intended to describe the Item.

---

# 5. Item Name and Configuration Identity

ItemName is not globally unique.

The same Item Name may represent different technical configurations.

Example:

- MS Round Bar / Dia 25
- MS Round Bar / Dia 30

Exact Item duplicate identity is:

- ItemName
- Shape
- Complete Specification configuration

Specification configuration includes:

- SpecificationId
- Normalized SpecificationValue
- Optional UomId

Specification row order does not affect duplicate identity.

The following are intentionally excluded from Item duplicate identity:

- Category
- Brand
- Main UOM
- PartNumber

---

# 6. Part Number

PartNumber is stored directly on Item.

Rules:

- Optional
- Maximum 100 characters
- Not unique
- Searchable
- Editable
- Displayed in Index and Details

PartNumber may represent:

- Internal part reference
- Customer part reference
- Manufacturer reference
- Engineering reference

Two different Items may use the same PartNumber.

---

# 7. Item Image Decision

Item Image storage is not part of the current Item Master.

The previously introduced ImagePath field was removed.

Reason:

Engineering identification is already available through:

- Item Name
- Part Number
- Shape
- Specifications
- Drawing Number
- Drawing Revision
- Drawing File

Maintaining a separate Item image would add unnecessary storage and UI complexity.

---

# 8. ItemSpecifications Table

Each Item may have multiple dynamic Specifications.

Fields:

- ItemSpecificationId
- ItemId
- SpecificationId
- SpecificationValue
- UomId
- SortOrder
- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

Relationships:

- ItemId -> Items
- SpecificationId -> Specifications
- UomId -> UOMs

Active uniqueness:

`ItemId + SpecificationId`

for non-deleted rows.

The same Specification cannot appear twice in one active Item configuration.

---

# 9. Supplier Table

## Suppliers

Important fields:

- SupplierId
- SupplierCode
- SupplierName
- ContactPerson
- MobileNumber
- AlternateMobileNumber
- Email
- Gstin
- Pan
- AddressLine1
- AddressLine2
- City
- State
- Pincode
- PaymentTermsDays
- Description
- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

SupplierCode:

`SUP00001`

Rules:

- SupplierCode never reused.
- Exact active SupplierName duplicate is blocked.
- Similar SupplierName generates warning.
- GSTIN is optional and unique among active non-deleted Suppliers when provided.
- PAN is optional and not unique.

Supplier financial transaction values are not stored in Supplier Master.

---

# 10. Drawing Architecture

Drawing Master uses one Drawings table.

Each database row represents one Drawing Revision.

Final business relationship:

One Item  
→ One Drawing Number  
→ Many Revisions

A second active Drawing Number cannot be created for the same Item.

---

# 11. Drawings Table

Important fields:

- DrawingId
- ItemId
- DrawingNumber
- DrawingName
- RevisionNumber
- DrawingType
- FileName
- FilePath
- Description
- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

IsPrimary has been removed.

Reason:

One Item can have only one Drawing Number.

---

# 12. Drawing Number

DrawingNumber is:

- Manually entered
- Required
- Permanent
- Immutable after Create
- Never reused
- Reserved even after Drawing Soft Delete

Exact existing Drawing Number blocks Create.

Similar Drawing Number produces a warning.

---

# 13. Drawing Revision Number

Revision Number is system generated.

Current format:

- RV-01
- RV-02
- RV-03

Rules:

- User does not manually enter Revision Number.
- First revision is RV-01.
- Next Revision Number is generated automatically.
- Deleted revisions remain part of the numbering sequence.
- Revision Numbers are never reused.

Legacy revision formats such as R01 and R02 remain supported while calculating the next sequence.

---

# 14. Drawing Revision Uniqueness

Permanent unique index:

`DrawingNumber + RevisionNumber`

Deleted revisions remain included in this uniqueness rule.

Therefore a deleted Revision Number cannot be reused.

---

# 15. Current Revision

IsActive represents the Current Revision.

For one Drawing Number:

- Many historical revisions may exist.
- Maximum one non-deleted revision may be Current.

Filtered unique index:

`DrawingNumber`
WHERE `IsActive = 1`
AND `IsDeleted = 0`

---

# 16. One Drawing Per Item

One Item may have only one active Drawing identity.

Database protection:

`ItemId`
WHERE `IsActive = 1`
AND `IsDeleted = 0`

Service validation also blocks creating a second Drawing for the same Item.

Historical revision rows are inactive and therefore allowed to reuse the same ItemId.

---

# 17. Revision Activation

Inactive previous revisions may be reactivated.

Example:

Before:

- RV-03 Current
- RV-02 Inactive
- RV-01 Inactive

Activate RV-01:

After:

- RV-03 Inactive
- RV-02 Inactive
- RV-01 Current

Revision switching is executed transactionally.

---

# 18. Revision Soft Delete

Only inactive revisions can be deleted.

Current Revision cannot be deleted directly.

Deleted Revision:

- remains in database
- disappears from normal UI
- keeps its Revision Number reserved
- retains its physical file

---

# 19. Drawing Soft Delete and Restore

Complete Drawing Delete performs Soft Delete.

Deleted Drawing:

- disappears from normal Drawing Index
- remains in database
- keeps Drawing Number reserved
- keeps revision files

A dedicated Deleted Drawings screen supports Restore.

Restore returns:

- Drawing identity
- revision history
- Current Revision

Restore is blocked if the Item already has another active Drawing.

---

# 20. Drawing File Storage

Supported formats:

- PDF
- JPG
- JPEG
- PNG
- DWG
- DXF

Maximum file size:

25 MB

Physical storage:

`wwwroot/uploads/drawings`

Database stores:

- Original FileName
- Relative FilePath

File binary is not stored in SQL Server.

---

# 21. Item and Drawing Integration

Item Details displays the Current Drawing linked to the Item.

Displayed information:

- Drawing Number
- Drawing Name
- Current Revision
- Drawing Type
- Current Drawing File
- Open Drawing Details

If no Drawing exists:

- Add Drawing action is displayed.

Item Edit displays a read-only Drawing summary.

Drawing data is not editable from Item Edit.

Drawing lifecycle remains controlled by the Drawing module.

---

# 22. Item Create to Drawing Flow

New Item flow:

Create Item  
→ Save Item  
→ Redirect to Item Details  
→ Add Drawing  
→ Drawing Create opens  
→ Item automatically selected

This allows Drawing creation only after a valid ItemId exists.

---

# 23. Company Purchase / Invoice Additions

Company Master stores reusable presentation and statutory data used by transaction documents.

Relevant fields used by downstream modules include:

- State
- optional GstNumber
- Website
- PurchaseOrderTermsAndConditions
- InvoiceTermsAndConditions
- PAN / statutory details where configured
- ISO certification information where configured
- Bank details where configured

Rules:

- GST Number is optional.
- Company State is required when GST tax split must be determined.
- Standard Purchase Order Terms & Conditions are maintained once in Company Master.
- Standard Invoice Terms & Conditions are maintained once in Company Master.
- Transaction documents store snapshots where historical stability is required.

---

# 24. Purchase Order Header Table

## PurchaseOrders

Important fields:

- Id
- Code
- PODate
- ExpectedDeliveryDate
- Status
- CompanyId
- CompanyName
- CompanyAddress
- CompanyState
- CompanyGSTIN
- CompanyPhone
- CompanyEmail
- CompanyWebsite
- SupplierId
- SupplierName
- SupplierAddress
- SupplierGSTIN
- SupplierContactPerson
- SupplierPhone
- SupplierEmail
- DeliveryAddress
- PaymentTerms
- DeliveryTerms
- Remarks
- TermsAndConditions
- SubTotal
- DiscountAmount
- TaxableAmount
- CGSTAmount
- SGSTAmount
- IGSTAmount
- TransportCharges
- OtherCharges
- RoundOffAmount
- GrandTotal
- ConfirmedOn
- SentToSupplierOn
- ClosedOn
- CancelledOn
- CancellationReason
- BaseEntity audit/status fields

Compatibility note:

- `DiscountAmount` remains in the schema but is forced to `0`.
- `RoundOffAmount` remains in the schema but is currently not used by the Purchase Order UI/business calculation.

---

# 25. Purchase Order Number

Format:

`AI/PO/26-27/00001`

Rules:

- Generated in PurchaseOrderService.
- Financial Year is April to March.
- Five-digit sequence.
- Unique.
- Deleted Purchase Order numbers are not reused.
- Repository last-code lookup is prefix-based by Financial Year.

---

# 26. Purchase Order Item Table

## PurchaseOrderItems

Important fields:

- Id
- Code
- PurchaseOrderId
- ItemId
- ItemCode
- ItemName
- Description
- Specification
- UnitName
- HSNCode
- DrawingId (nullable)
- DrawingNumber
- DrawingRevision
- Quantity
- UnitPrice
- DiscountPercent
- DiscountAmount
- TaxableAmount
- GSTPercent
- CGSTAmount
- SGSTAmount
- IGSTAmount
- LineTotal
- RequiredDate
- Remarks
- BaseEntity audit/status fields

Compatibility note:

- `DiscountPercent` and line `DiscountAmount` remain in the schema but are forced to `0`.

---

# 27. Purchase Order Relationships

PurchaseOrders:

- CompanyId → Companies
- SupplierId → Suppliers

PurchaseOrderItems:

- PurchaseOrderId → PurchaseOrders
- ItemId → Items
- DrawingId → Drawings (optional)

Master foreign keys use Restrict where historical transaction integrity requires it.

PurchaseOrder → PurchaseOrderItems is a parent-child relationship.

Application behavior uses Soft Delete.

---

# 28. Purchase Order Snapshot Strategy

Purchase Order stores historical snapshots.

Company snapshot:

- Name
- Address
- State
- optional GSTIN
- Phone
- Email
- Website

Supplier snapshot:

- Name
- Address
- optional GSTIN
- Contact Person
- Phone
- Email

Item line snapshot:

- Item Code
- Item Name
- Description
- Specification
- UOM name
- Drawing Number / Revision where selected

Terms snapshot:

`Company.PurchaseOrderTermsAndConditions`
→ `PurchaseOrder.TermsAndConditions`

Reason:

Historical PO and PDF output must remain stable even when Master data changes later.

---

# 29. Purchase Order GST Design

GST rate is stored per Purchase Order line.

Default UI value:

18%

Tax type is determined from:

Company.State  
vs  
Supplier.State

Same State:

CGST + SGST

Different State:

IGST

GSTIN is optional and is not used to determine GST type.

Final values are calculated in PurchaseOrderService.

---

# 30. Purchase Order Calculation

Line:

`Quantity × UnitPrice = TaxableAmount`

Tax is calculated from TaxableAmount.

LineTotal:

`TaxableAmount + CGST + SGST + IGST`

Header GrandTotal:

`TaxableAmount + CGSTAmount + SGSTAmount + IGSTAmount + TransportCharges + OtherCharges`

Current rule:

- no Purchase Order Discount
- no Purchase Order Round Off in the active UI/business flow
- no separate GST on Transport / Other Charges

---

# 31. Purchase Order Workflow

Implemented workflow:

Draft  
→ Confirmed  
→ Sent

Enum values also exist for future/receipt lifecycle:

- PartiallyReceived
- Received
- Closed
- Cancelled

Current important behavior:

- Only Draft Purchase Orders can be edited.
- Only Draft Purchase Orders can be soft deleted in the current Purchase Order service.
- GRN Phase 1 does not automatically update Purchase Order Status.
- GRN Phase 1 also does not update stock.
- Therefore `PartiallyReceived` / `Received` status integration remains a later inventory/purchase integration step.

---

# 32. Purchase Order PDF

PDF generation is implemented using QuestPDF.

PDF reads the saved Purchase Order transaction/snapshot data.

Supplier-facing PDF includes:

- Company logo/details
- PO number/date
- Supplier/delivery details
- Item/specification/drawing
- HSN
- quantity/UOM/rate
- GST
- taxable/line total
- tax summary
- transport/other charges
- grand total
- remarks
- Terms & Conditions
- authorized signatory

Status is intentionally not printed on the supplier PDF.

---

# 33. Goods Receipt Note Header

## GoodsReceiptNotes

Confirmed important fields:

- Id
- Code
- GRNDate
- PurchaseOrderId
- SupplierId
- SupplierName
- SupplierChallanNumber
- SupplierChallanDate
- Remarks
- BaseEntity audit/status fields

GRN source is a Purchase Order.

GRN does not trust browser-posted PO quantities or Item snapshots.

The Service reloads the Purchase Order and recalculates receipt quantities before saving.

---

# 34. Goods Receipt Note Items

## GoodsReceiptNoteItems

Confirmed important fields:

- Id
- Code
- GoodsReceiptNoteId
- PurchaseOrderItemId
- ItemId
- ItemCode
- ItemName
- Specification
- UnitName
- OrderedQuantity
- PreviouslyReceivedQuantity
- BalanceQuantity
- ReceiptStatus
- ReceivedQuantity
- PendingQuantity
- MaterialStatus
- Remarks
- BaseEntity audit/status fields

Receipt statuses:

- NotReceived
- PartialReceived
- FullReceived

Material status is required only when material is actually received.

Current material status values are stored for receipt traceability; their stock/accounting effect is deferred.

---

# 35. GRN Quantity Logic

For every Purchase Order line:

`BalanceQuantity = OrderedQuantity - PreviouslyReceivedQuantity`

Not Received:

- ReceivedQuantity = 0
- PendingQuantity = current BalanceQuantity
- MaterialStatus = null

Partial Received:

- ReceivedQuantity must be greater than 0
- ReceivedQuantity cannot exceed current BalanceQuantity
- PendingQuantity is recalculated
- MaterialStatus is required

Full Received:

- ReceivedQuantity = current BalanceQuantity
- PendingQuantity = 0
- MaterialStatus is required

At least one Purchase Order item must actually be Partial Received or Full Received before a GRN can be saved.

---

# 36. GRN Number and Duplicate Challan Rule

GRN Code:

`AI/GRN/26-27/00001`

Rules:

- Financial Year is April to March.
- Five-digit sequence.
- Deleted document codes remain reserved.
- Code is never reused.

Supplier Challan rule:

`SupplierId + SupplierChallanNumber`

must not repeat when SupplierChallanNumber is provided.

Supplier Challan Number is currently optional.

Blank Supplier Challan Number skips duplicate validation.

---

# 37. GRN Purchase Order Eligibility

GRN may be created only against an eligible active Purchase Order.

Current service eligibility accepts:

- Sent
- PartiallyReceived

Important implementation status:

- Stock is not updated by GRN Phase 1.
- Purchase Order status is not automatically updated by GRN Phase 1.
- Approved / Rejected / Failure / Return are currently stored receipt decisions only.
- Inventory Stock Transaction and Stock Ledger integration remain deferred.

---

# 38. Customer Master

## Customers

Customer Master is now part of the active Sales / Production flow.

Confirmed fields used by downstream modules include:

- Id
- Code
- CustomerName
- GSTIN / Gstin
- PAN / Pan
- AddressLine1
- AddressLine2
- City
- District
- State
- Pincode
- Country
- PaymentTerms
- CreditDays
- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

Additional Customer contact fields exist in the Customer module but are not repeated here until checked against the final current entity.

Design rules retained from the Customer module:

- Customer Code is permanent and unique.
- Customer is selected by Customer Purchase Order.
- Customer master data is reloaded by Service before transaction creation.
- Customer snapshots are used by downstream transaction documents where historical stability is required.

---

# 39. Customer Purchase Order Header

## CustomerPurchaseOrders

Important confirmed fields:

- Id
- Code
- CustomerId
- CustomerName
- CustomerPurchaseOrderNumber
- CustomerPurchaseOrderDate
- ReceivedDate
- RequiredDeliveryDate
- Priority
- Status
- CustomerReference
- Remarks
- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

Internal ERP Customer PO Code:

`AI/CPO/26-27/00001`

The customer's own PO number is stored separately in:

`CustomerPurchaseOrderNumber`

---

# 40. Customer Purchase Order Items

## CustomerPurchaseOrderItems

Important confirmed fields:

- Id
- Code
- CustomerPurchaseOrderId
- ItemId
- ItemCode
- ItemName
- Specification
- UnitName
- CustomerItemCode
- CustomerDrawingNumber
- Revision
- OrderedQuantity
- RequiredDeliveryDate
- Priority
- Remarks
- BaseEntity audit/status fields

The Item is selected from Item Master.

Trusted Item snapshot values are rebuilt by the Application Service.

Specification snapshot is built from the Item's active ItemSpecifications.

---

# 41. Customer Purchase Order Rules

Permanent Customer PO identity rule:

`CustomerId + CustomerPurchaseOrderNumber`

The same exact Customer PO Number cannot be used twice for the same Customer.

The same Item cannot be selected more than once in the same Customer Purchase Order in the current implemented validation.

Similar Customer PO Numbers:

- are displayed as a warning
- exact duplicate is blocked
- similar-but-not-exact values may continue after confirmation

Customer PO line Codes are internal stable identifiers and are not based on visible row sequence.

---

# 42. Customer Purchase Order Workflow

Current workflow:

Draft  
→ Confirmed

Rules:

- Create starts as Draft.
- Only Draft Customer POs can be edited.
- Confirm changes Draft to Confirmed.
- Confirmed Customer PO Items become eligible sources for Production Jobs.
- Soft Delete is used.
- Restore preserves the original transaction Status.

Production Machine and execution pipeline information is not stored in CustomerPurchaseOrders.

---

# 43. Production Masters and Routing Foundation

Production uses:

- Machines
- ProductionOperations
- ItemProcessRoutings
- Routing Step rows
- ProductionJobs
- ProductionJobSteps
- ProductionJobStepHistory

Machines are reusable execution resources.

ProductionOperations are reusable process definitions.

ItemProcessRouting is a reusable manufacturing template for an Item.

ProductionJob is the actual manufacturing transaction.

Routing changes after Production Job creation do not retroactively change the copied Production Job Steps.

---

# 44. Item Process Routing

## ItemProcessRoutings

Confirmed routing header concepts/fields:

- Id
- Code
- ItemId
- RevisionNumber
- Status
- EffectiveFrom
- Remarks
- IsActive
- IsDeleted
- audit fields

Rules:

- Item is selected when Routing is created.
- Item cannot be changed after Routing creation.
- Routing has a Revision Number.
- Production uses only a current active Released Routing.
- EffectiveFrom is optional.

Exact routing code format is intentionally not stated here until verified against the final routing service.

---

# 45. Item Process Routing Steps

Routing Step data includes:

- SequenceNumber
- ProductionOperationId
- optional DefaultMachineId
- SetupTimeMinutes
- CycleTimeMinutes
- OperationInstruction
- Remarks
- IsActive
- IsDeleted
- audit fields

Rules:

- Routing must contain active steps before it can be used for Production.
- Default Machine is optional.
- Same Production Operation may appear more than once when the manufacturing process requires it.
- Actual Assigned Machine and actual execution quantities do not belong to the reusable Routing.

---

# 46. Production Jobs

## ProductionJobs

Important confirmed fields:

- Id
- Code
- CustomerPurchaseOrderItemId
- ItemId
- ItemCode
- ItemName
- UnitName
- JobQuantity
- ItemProcessRoutingId
- RoutingCode
- RoutingRevisionNumber
- Status
- PlannedStartOn
- PlannedCompletionOn
- StartedOn
- CompletedOn
- CancelledOn
- CancellationReason
- Remarks
- PipelineModificationReason
- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

Production Job Code:

`AI/PJOB/26-27/00001`

Deleted Production Job Codes are included while finding the last code.

Codes are never reused.

---

# 47. Production Job Source and Quantity Allocation

Production Job source:

Confirmed Customer Purchase Order Item.

Only Customer PO Items whose parent Customer Purchase Order is Confirmed and not deleted are eligible.

Quantity rule:

`Remaining Production Qty = Customer PO OrderedQuantity - Allocated Production Job Quantity`

Allocated Production Job Quantity includes non-deleted Production Jobs except Cancelled Jobs.

Therefore:

- Cancelled Jobs do not consume Customer PO quantity.
- Production Job Quantity cannot exceed remaining Customer PO quantity.
- Multiple Production Jobs may consume one Customer PO Item over time until the ordered quantity is fully allocated.

---

# 48. Production Job Routing Snapshot

A Production Job requires an active Released Item Process Routing.

At Job creation, the system snapshots:

- Item identity from Customer PO Item
- Routing Id
- Routing Code
- Routing Revision Number
- Routing Steps

Each Routing Step is copied into a ProductionJobStep.

This isolates actual manufacturing execution from later Routing edits.

---

# 49. Production Job Steps

## ProductionJobSteps

Confirmed important fields:

- Id
- ProductionJobId
- SequenceNumber
- ProductionOperationId
- OperationCode
- OperationName
- OperationType
- DefaultMachineId
- AssignedMachineId
- SetupTimeMinutes
- CycleTimeMinutes
- Status
- StartedOn
- CompletedOn
- GoodQuantity
- RejectedQuantity
- OperationInstruction
- RoutingRemarks
- ExecutionRemarks
- IsActive
- IsDeleted
- audit fields

Rules:

- Initial status is Pending.
- Actual Assigned Machine may differ from Default Machine.
- Only active Machines can be selected for execution.
- Production steps execute in sequence.
- Another step cannot start while another active step is In Progress.
- Previous required steps must be Completed before the next step starts.

---

# 50. Production Job Step History

## ProductionJobStepHistory

Execution history stores snapshots such as:

- PreviousStatus
- NewStatus
- MachineId
- MachineCode
- MachineName
- GoodQuantity
- RejectedQuantity
- Remarks
- ChangedOn
- ChangedBy

Purpose:

- production audit trail
- machine traceability
- quantity traceability
- status transition history

History is appended when important step status changes occur.

---

# 51. Production Completion Rules

Production Job statuses used by the current module:

- Draft
- Ready
- InProgress
- Completed
- Cancelled

Step completion records:

- GoodQuantity
- RejectedQuantity

Validation:

`GoodQuantity + RejectedQuantity <= JobQuantity`

When all active ProductionJobSteps are Completed:

- ProductionJob.Status = Completed
- ProductionJob.CompletedOn is populated

Otherwise the Job remains InProgress.

Current Invoice eligibility is based on ProductionJob Status = Completed.

---

# 52. Production Pipeline Editing

Production Job pipeline may be modified only before production execution starts.

Current design allows controlled pipeline editing while the Job has not started.

Existing executed/non-pending steps cannot be freely replaced.

PipelineModificationReason records the reason for the pre-start pipeline change.

---

# 53. Delivery Challan Database Integration

Delivery Challan is an implemented dispatch transaction area.

Confirmed header/source information used downstream includes:

- Id
- Code
- ChallanDate
- CustomerId
- CustomerName
- Status
- IsActive
- IsDeleted

Confirmed DeliveryChallanItem information includes:

- Id
- DeliveryChallanId
- SequenceNumber
- DispatchQuantity
- ProductReference
- ItemId
- ItemCode
- ItemName
- PartNumber
- CustomerItemCode
- UnitName
- HsnNumber
- CustomerPurchaseOrderItemId
- CustomerPurchaseOrderCode
- CustomerPurchaseOrderNumber
- ProductionJobId
- ProductionJobCode
- IsActive
- IsDeleted

Delivery Challan provides dispatch traceability from Production Job / Customer PO to dispatched quantity.

The exact complete Delivery Challan schema and document-number format should remain owned by the Delivery Challan module documentation rather than being guessed here.

---

# 54. PDI Database Integration Status

PDI is part of the intended Quality / Dispatch flow.

The available document design uses references such as:

- Customer
- Customer PO
- ERP Customer PO Code
- Production Job
- Item
- Workshop Drawing
- Customer Drawing
- Inspected Quantity
- Accepted Quantity
- Rejected Quantity
- Overall Result
- Inspection checklist
- Inspection remarks
- approval / release information

Sample PDI document number:

`AI/PDI/26-27/00001`

Important:

The sample document proves the intended PDI document structure, but the final current PDI database entity/DbSet schema has not been verified while updating this Database Design document.

Therefore:

- do not create new PDI tables from assumptions
- do not hard-code a PDI DbSet name here
- wire Invoice/PDI validation only to the actual PDI persistence model once verified

---

# 55. Invoice Header

## Invoices

Important current fields:

- Id
- Code
- InvoiceDate
- DueDate
- Status
- CustomerId
- CustomerName
- CustomerSnapshotJson
- BillingAddressLine1
- BillingAddressLine2
- BillingCity
- BillingDistrict
- BillingState
- BillingPincode
- BillingCountry
- CompanyId
- CompanyName
- CompanySnapshotJson
- PaymentTerms
- CreditDays
- PlaceOfSupply
- IsInterState
- OtherCharges
- GrossAmount
- DiscountAmount
- TaxableAmount
- CgstAmount
- SgstAmount
- IgstAmount
- RoundOffAmount
- GrandTotal
- InvoiceTermsAndConditions
- Remarks
- FinalizedOn
- FinalizedBy
- BaseEntity audit/status fields

Invoice Code:

`AI/INV/26-27/00001`

---

# 56. Invoice Items

## InvoiceItems

Important current fields:

- Id
- InvoiceId
- SequenceNumber

### Primary Production / Customer PO Traceability

- ProductionJobId (nullable in schema for backward compatibility)
- ProductionJobCode
- CustomerPurchaseOrderItemId
- CustomerPurchaseOrderCode
- CustomerPurchaseOrderNumber

### Product / Item Snapshot

- ProductReference
- ItemId
- ItemCode
- ItemName
- PartNumber
- CustomerItemCode
- UnitName
- HsnNumber

### Historical Delivery Challan References

- DeliveryChallanId (nullable)
- DeliveryChallanCode
- DeliveryChallanItemId (nullable)
- DeliveryChallanQuantity (nullable)

### Commercial / Financial Fields

- InvoiceQuantity
- Rate
- GrossAmount
- DiscountPercent
- DiscountAmount
- TaxableAmount
- GstRate
- CgstRate
- SgstRate
- IgstRate
- CgstAmount
- SgstAmount
- IgstAmount
- TotalTaxAmount
- LineTotal

### Audit

- IsActive
- IsDeleted
- CreatedOn
- CreatedBy
- ModifiedOn
- ModifiedBy

---

# 57. Current Invoice Source Rule

The current Invoice process is no longer Delivery-Challan-driven.

Primary business flow:

Customer Purchase Order  
→ Completed Production Job  
→ Invoice

Create Invoice behavior:

- User selects Customer Purchase Order.
- System loads Production Jobs under that Customer PO.
- Only Production Jobs with Status = Completed are invoice candidates.
- Production Job is the authoritative source for new Invoice lines.
- Invoice can be created even when PDI is not completed.
- Invoice can be created even when Delivery Challan is not created.

PDI and Delivery Challan are no longer mandatory billing gates.

---

# 58. Invoice PDI / Delivery Challan Warning Rule

For each selected Completed Production Job:

- If PDI / Delivery Challan traceability is complete, normal flow continues.
- If PDI is missing OR Delivery Challan is missing, Invoice is not automatically blocked.
- System shows a warning.
- User must explicitly confirm that they want to continue.
- After confirmation, Create / Update / Finalize may proceed.

Therefore:

Production Completed = hard eligibility rule.

PDI / Delivery Challan = warning / traceability rule for Invoice.

The Service remains the authoritative validator.

---

# 59. Invoice Quantity Allocation

Invoice quantity is allocated against Production Job identity in the current process.

For a Production Job:

`Remaining Invoice Qty = Production Job invoiceable quantity - quantity already allocated to active Invoices`

Current schema has no separate ProductionJob header `CompletedQuantity`.

Production execution stores GoodQuantity / RejectedQuantity at step level.

Current Invoice process uses the completed Production Job quantity basis supported by the existing Job model.

Future rule change:

If billing must be limited specifically to final GoodQuantity rather than JobQuantity, that must be implemented explicitly from the final production-step result and documented as a separate business change.

---

# 60. Invoice Snapshot Strategy

Invoice stores historical Customer and Company snapshots.

Customer:

`CustomerSnapshotJson`

Company:

`CompanySnapshotJson`

Purpose:

- finalized Invoice must remain historically stable
- Customer Master changes must not rewrite old Invoice documents
- Company statutory/bank/ISO changes must not rewrite old Invoice documents

InvoiceItem stores transaction-line snapshots for:

- Product / Item
- Customer PO
- Production Job
- optional historical Delivery Challan references

Browser-posted snapshots are not trusted.

Service reloads the authoritative source before save/finalization.

---

# 61. Invoice GST and Financial Calculation

Commercial inputs:

- InvoiceQuantity
- Rate
- DiscountPercent
- GstRate
- OtherCharges

Line calculation:

`GrossAmount = InvoiceQuantity × Rate`

`DiscountAmount = GrossAmount × DiscountPercent / 100`

`TaxableAmount = GrossAmount - DiscountAmount`

GST type:

Company.State  
vs  
BillingState

Same State:

- CGST
- SGST

Different State:

- IGST

LineTotal:

`TaxableAmount + TotalTaxAmount`

Header totals are recalculated by InvoiceService.

Browser/JavaScript calculations are preview only.

Invoice supports `RoundOffAmount` and final `GrandTotal` calculation.

---

# 62. Invoice Workflow

Current workflow:

Draft  
→ Finalized

Rules:

- Create starts as Draft.
- Only Draft Invoice can be edited.
- Finalization locks the Invoice from normal editing.
- PDF can be generated only for Finalized Invoice.
- Draft Invoice supports Soft Delete.
- Deleted Draft Invoice can be restored after authoritative quantity/source revalidation.
- Active Draft and Finalized Invoices participate in quantity allocation.
- Deleted Invoices do not reserve invoice quantity.

The current Customer PO / Production Job source-process change does not require a new Invoice entity or a schema redesign.

Existing nullable Delivery Challan fields remain for historical compatibility.

---

# 63. Invoice PDF

Invoice PDF is generated with QuestPDF from saved transaction data.

It uses:

- saved Invoice header
- Customer historical snapshot
- Company historical snapshot
- saved InvoiceItem snapshots
- GST and amount values calculated by Service

Current PDF presentation includes:

- Company information
- Invoice number/date/due date
- BILL TO
- Customer PO reference
- Billing Address
- GSTIN / PAN where available
- Place of Supply
- Payment Terms / Credit Days
- Item/Product
- HSN
- quantity/UOM/rate
- discount
- GST
- taxable amount
- line total
- amount summary
- Terms & Conditions
- remarks
- signature area

Finalized Invoice PDF is historical output and should not rebuild old transaction values from current Master data.

---

# 64. Current End-to-End Transaction Relationships

## Purchase Side

Company  
→ Supplier  
→ Purchase Order  
→ Purchase Order Item  
→ Goods Receipt Note  
→ Goods Receipt Note Item

Inventory posting is still deferred.

## Customer / Manufacturing Side

Customer  
→ Customer Purchase Order  
→ Customer Purchase Order Item  
→ Released Item Process Routing  
→ Production Job  
→ Production Job Steps  
→ Production Job Step History  
→ Completed Production Job

## Dispatch / Billing Side

Completed Production Job  
→ optional PDI / Quality release  
→ optional Delivery Challan  
→ Invoice

Current Invoice rule:

Completed Production Job is the billing gate.

PDI / Delivery Challan absence creates a warning, not a mandatory block.

---

# 65. Important Snapshot Boundaries

Master/configuration data may change over time.

Transaction documents therefore snapshot the information they actually used.

Examples:

Purchase Order:

- Company
- Supplier
- Item
- Specification
- Drawing
- Terms

Customer Purchase Order:

- Customer Name
- Item Code / Name
- Specification
- UOM
- Customer-specific Item / Drawing references

Production Job:

- Customer PO Item source
- Item snapshot
- Routing Code / Revision
- executable Routing Steps

Delivery Challan:

- Customer PO / Production Job / Item dispatch references

Invoice:

- Customer JSON snapshot
- Company JSON snapshot
- Customer PO
- Production Job
- Product / Item
- optional historical Delivery Challan reference

This prevents later Master/Configuration changes from rewriting transaction history.

---

# 66. Current Deferred Database Areas

Deferred / not yet fully integrated:

- Purchase Requisition
- Purchase Invoice
- Purchase Return
- Full Warehouse Stock
- Inventory Stock Transaction
- Stock Ledger
- Opening Stock
- Minimum Stock
- Maximum Stock
- Material Reservation
- BOM
- Production material consumption
- GRN-to-stock posting
- GRN automatic Purchase Order receipt-status update
- Full PDI persistence schema verification/documentation
- Quality NCR / rejection workflow
- Customer Payment / Receipt
- Supplier Payment
- Accounting Ledger
- GST reporting
- Supplier balances
- Customer balances
- Full Drawing approval workflow

---

# 67. Current Database Design Position

The old document position:

Purchase Order  
→ Next: GRN  
→ Customer / Customer PO as ACTION items

is no longer current.

Current implementation has progressed through:

Purchase Order  
→ GRN  
→ Customer  
→ Customer Purchase Order  
→ Production Routing / Operations / Machines  
→ Production Job / Execution  
→ Delivery / Quality integration  
→ Invoice

The active Invoice process now uses:

Customer PO  
→ Completed Production Job  
→ Invoice

without requiring PDI or Delivery Challan as hard prerequisites.

No new Invoice entity/table is required only for this process change.

Future database changes should be introduced only when a new business requirement cannot be represented safely by the existing schema.

---

# End of 04 - Database Design
