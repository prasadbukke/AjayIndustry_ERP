/*
============================================================

File:
CustomerDrawing.cs

Purpose:
Represents a Customer-specific Drawing for an Item.

Responsibilities:
- Link a Customer with an Item.
- Store the current Customer Drawing information.
- Store Customer Drawing revision and uploaded file details.
- Allow the same Item to have different Drawings for
  different Customers.

Important:
- CustomerId + ItemId must be unique for non-deleted records.
- Same Customer + Same Item can have only one current
  Customer Drawing record.
- Different Customers can have separate Drawings for
  the same Item.
- Existing Drawing entity represents Owner / Workshop
  Drawing and is not affected by this entity.

============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities;

public class CustomerDrawing : BaseEntity
{
    // =========================================================
    // PRIMARY KEY
    // =========================================================

    public int CustomerDrawingId { get; set; }


    // =========================================================
    // CUSTOMER
    // =========================================================

    public int CustomerId { get; set; }

    public Customer Customer { get; set; } = null!;


    // =========================================================
    // ITEM
    // =========================================================

    public int ItemId { get; set; }

    public Item Item { get; set; } = null!;


    // =========================================================
    // DRAWING INFORMATION
    // =========================================================

    public string DrawingNumber { get; set; } =
        string.Empty;

    public string? DrawingName { get; set; }

    public string? DrawingType { get; set; }

    public string? RevisionNumber { get; set; }


    // =========================================================
    // DRAWING FILE
    // =========================================================

    public string? FileName { get; set; }

    public string? FilePath { get; set; }


    // =========================================================
    // DESCRIPTION
    // =========================================================

    public string? Description { get; set; }
}