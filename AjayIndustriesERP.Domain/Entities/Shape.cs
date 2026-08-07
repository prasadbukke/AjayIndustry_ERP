/*
==============================================================

File : Shape.cs

Purpose :
Represents Shape Master information used for manufacturing
and raw material Items.

==============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    /// <summary>
    /// Represents a material or Item shape.
    /// </summary>
    public class Shape : BaseEntity
    {
        #region Primary Key

        public int ShapeId { get; set; }

        #endregion

        #region Shape Information

        public string ShapeCode { get; set; } = string.Empty;

        public string ShapeName { get; set; } = string.Empty;

        public string? Description { get; set; }

        #endregion
    }
}