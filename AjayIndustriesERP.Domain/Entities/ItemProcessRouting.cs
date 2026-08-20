/*
============================================================
File: ItemProcessRouting.cs

Purpose:
Represents the manufacturing Process Routing defined for
an Item.

Responsibilities:
- Link a Routing to Item Master.
- Maintain Routing revision history.
- Maintain Draft / Released / Obsolete lifecycle.
- Store optional effective date and remarks.
- Contain the ordered manufacturing Process Steps.
- Act as the source template for future Production Job Steps.

Important:
- Routing is an Item-level manufacturing definition.
- Customer PO information is NOT stored here.
- Production Job execution information is NOT stored here.
- Actual Machine assignment and actual execution times belong
  to future Production Job Steps.
- Released Routing will later be copied into Production Job
  Steps. Existing Jobs must not change when Routing changes.
============================================================
*/

using AjayIndustriesERP.Domain.Common;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Domain.Entities
{
    public class ItemProcessRouting : BaseEntity
    {
        #region Primary Identification

        public int Id { get; set; }


        /// <summary>
        /// Internal ERP generated Routing Code.
        /// Example: AI/RTE/00001
        /// </summary>
        public string Code { get; set; } =
            string.Empty;

        #endregion


        #region Item Relationship

        public int ItemId { get; set; }


        public Item Item { get; set; } =
            null!;

        #endregion


        #region Revision Information

        /// <summary>
        /// Sequential Routing revision for the Item.
        /// Example:
        /// 1, 2, 3...
        /// </summary>
        public int RevisionNumber { get; set; } = 1;


        public ItemProcessRoutingStatus Status { get; set; } =
            ItemProcessRoutingStatus.Draft;


        /// <summary>
        /// Optional date from which this Routing revision
        /// becomes effective.
        /// </summary>
        public DateTime? EffectiveFrom { get; set; }

        #endregion


        #region Remarks

        public string? Remarks { get; set; }

        #endregion


        #region Routing Steps

        public ICollection<ItemProcessRoutingStep> Steps
        {
            get;
            set;
        } = new List<ItemProcessRoutingStep>();

        #endregion
    }
}