/*
==============================================================

File : Specification.cs

Purpose :
Represents reusable Specification Master information.

Examples :
- Diameter
- Thickness
- Width
- Length
- Grade
- Hardness
- Finish

==============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    /// <summary>
    /// Represents a reusable Item specification definition.
    /// </summary>
    public class Specification : BaseEntity
    {
        #region Primary Key

        public int SpecificationId { get; set; }

        #endregion

        #region Specification Information

        public string SpecificationCode { get; set; } =
            string.Empty;

        public string SpecificationName { get; set; } =
            string.Empty;

        public string? Description { get; set; }

        #endregion
    }
}