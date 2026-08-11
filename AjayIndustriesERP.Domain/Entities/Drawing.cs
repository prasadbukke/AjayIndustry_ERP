/*
==============================================================

File : Drawing.cs

Purpose :
Represents an engineering Drawing revision
linked to an Item.

Final Design :
- One Item has one Drawing Number.
- One Drawing Number can have many revisions.
- Every database row represents one revision.
- Drawing Number is permanent.
- Revision Number is system generated.
- Only one revision can be Current (IsActive = true).
- Old revisions may be activated again.
- Old revisions may be soft deleted.
- Complete Drawing may be soft deleted and restored.

==============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    /// <summary>
    /// Represents one revision of an engineering Drawing.
    /// </summary>
    public class Drawing : BaseEntity
    {
        #region Primary Key

        public int DrawingId { get; set; }

        #endregion

        #region Item

        public int ItemId { get; set; }

        #endregion

        #region Drawing Identity

        /// <summary>
        /// Permanent Drawing Number.
        ///
        /// Example:
        /// DRG-10025
        /// </summary>
        public string DrawingNumber { get; set; } =
            string.Empty;

        /// <summary>
        /// Human-readable Drawing Name.
        /// </summary>
        public string? DrawingName { get; set; }

        /// <summary>
        /// Optional Drawing classification.
        ///
        /// Examples:
        /// Manufacturing
        /// Inspection
        /// Customer
        /// Assembly
        /// </summary>
        public string? DrawingType { get; set; }

        #endregion

        #region Revision

        /// <summary>
        /// System-generated Revision Number.
        ///
        /// Examples:
        /// RV-01
        /// RV-02
        /// RV-03
        /// </summary>
        public string? RevisionNumber { get; set; }

        #endregion

        #region File

        /// <summary>
        /// Original uploaded Drawing file name.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Relative Drawing file path.
        ///
        /// Actual file binary is not stored
        /// inside the database.
        /// </summary>
        public string? FilePath { get; set; }

        #endregion

        #region Revision Information

        /// <summary>
        /// Revision-specific remarks.
        /// </summary>
        public string? Description { get; set; }

        #endregion

        #region Navigation

        public Item Item { get; set; } = null!;

        #endregion
    }
}