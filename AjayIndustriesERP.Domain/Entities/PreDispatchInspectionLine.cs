/*
============================================================
File: PreDispatchInspectionLine.cs

Purpose:
Represents one inspection parameter row of a
Pre-Dispatch / Final Inspection Report.

Responsibilities:
- Store inspection parameter.
- Store required specification.
- Store inspection / measuring method.
- Maintain display sequence.
- Store line-level result and remarks.
- Maintain multiple sample observations.

Important:
- One PDI Report can contain multiple Inspection Lines.
- Observations are stored separately so the number of
  samples is not hard-coded.
- Specification is snapshotted at PDI creation time.
============================================================
*/

using AjayIndustriesERP.Domain.Common;
using AjayIndustriesERP.Domain.Enums;

namespace AjayIndustriesERP.Domain.Entities
{
    public class PreDispatchInspectionLine
        : BaseEntity
    {
        #region Identification

        public int Id { get; set; }

        public int PreDispatchInspectionId
        {
            get;
            set;
        }

        public PreDispatchInspection
            PreDispatchInspection
        {
            get;
            set;
        } = null!;

        public int SequenceNumber { get; set; }

        #endregion


        #region Inspection Parameter

        /*
         * Example:
         *
         * Length (P)
         * O.D. (D)
         * A/F (S)
         * Thread (M)
         * Surface Finish
         */

        public string Parameter { get; set; } =
            string.Empty;


        /*
         * Snapshot of the applicable specification.
         *
         * Example:
         *
         * 14 ± 0.2
         * 31 ± 1
         * M26 X 1.5
         * 3.2 µm
         */

        public string Specification { get; set; } =
            string.Empty;

        #endregion


        #region Inspection Method

        /*
         * Example:
         *
         * Vernier Caliper
         * Micrometer
         * Thread Ring Gauge
         * Comparator
         * Visual
         */

        public string? InspectionMethod
        {
            get;
            set;
        }

        #endregion


        #region Result

        public PreDispatchInspectionLineResult Result
        {
            get;
            set;
        }

        public string? Remarks { get; set; }

        #endregion


        #region Observations

        /*
         * Stores:
         *
         * Observation 1
         * Observation 2
         * ...
         *
         * Reading At Interval 1
         * Reading At Interval 2
         * ...
         *
         * Number of observations is flexible.
         */

        public ICollection<PreDispatchInspectionObservation>
            Observations
        { get; set; } =
            new List<PreDispatchInspectionObservation>();

        #endregion
    }
}