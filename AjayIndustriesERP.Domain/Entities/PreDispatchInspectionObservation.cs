/*
============================================================
File: PreDispatchInspectionObservation.cs

Purpose:
Represents one actual inspection reading captured against
a PDI Inspection Line.

Responsibilities:
- Store actual observed value.
- Maintain observation sequence.
- Distinguish normal sample observations from
  interval readings.
- Support numeric as well as text inspection values.

Important:
- Observation Value is stored as string intentionally.
- Inspection readings may be numeric:
    14.05
    31.10
- Or textual:
    OK
    FOUND OK
    NOT OK
- Number of observations is not hard-coded.
============================================================
*/

using AjayIndustriesERP.Domain.Common;

namespace AjayIndustriesERP.Domain.Entities
{
    public class PreDispatchInspectionObservation
        : BaseEntity
    {
        #region Identification

        public int Id { get; set; }

        public int PreDispatchInspectionLineId
        {
            get;
            set;
        }

        public PreDispatchInspectionLine
            PreDispatchInspectionLine
        {
            get;
            set;
        } = null!;

        #endregion


        #region Observation

        /*
         * Sequence within its own group.
         *
         * Example:
         *
         * Observation:
         * 1, 2, 3, 4, 5, 6, 7
         *
         * Interval Reading:
         * 1, 2, 3
         */

        public int SequenceNumber { get; set; }


        /*
         * false =
         * Normal Observation / Sample Reading
         *
         * true =
         * Reading At Interval
         */

        public bool IsIntervalReading { get; set; }


        /*
         * String is intentional because actual inspection
         * values may contain:
         *
         * 14.05
         * 31.10
         * OK
         * FOUND OK
         * NOT OK
         */

        public string? Value { get; set; }

        #endregion
    }
}