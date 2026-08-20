/*
============================================================
File: ProductionOperationType.cs

Purpose:
Defines the category of a Production Operation.

Responsibilities:
- Distinguish manufacturing operations from inspection steps.
- Support future Item Process Routing.
- Support future Production Job Pipeline generation.

Important:
- This enum represents the type of Operation.
- It does NOT represent Production Job Step Status.
============================================================
*/

namespace AjayIndustriesERP.Domain.Enums
{
    public enum ProductionOperationType
    {
        #region Operation Types

        Production = 1,

        Inspection = 2

        #endregion
    }
}