/*
==============================================================

File : ItemConfigurationSimilarityHelper.cs

Purpose :
Compares complete Item configurations for duplicate detection.

Duplicate Identity :
- Item Name
- Shape
- Specification
- Specification Value
- Specification UOM

Notes :
- Specification row order is ignored.
- Specification values are compared case-insensitively.
- Extra spaces are ignored.
- UOM is optional.
- Category, Brand and Main Item UOM are intentionally
  not part of the duplicate signature.

==============================================================
*/

using AjayIndustriesERP.Domain.Entities;
using System.Text.RegularExpressions;

namespace AjayIndustriesERP.Application.Common
{
    /// <summary>
    /// Provides deterministic Item configuration comparison.
    /// </summary>
    public static class ItemConfigurationSimilarityHelper
    {
        #region Public Comparison

        /// <summary>
        /// Determines whether two Items represent the same
        /// Item configuration.
        /// </summary>
        public static bool IsSameConfiguration(
            Item first,
            Item second)
        {
            if (!IsSameItemName(
                first.ItemName,
                second.ItemName))
            {
                return false;
            }

            if (NormalizeNullableId(first.ShapeId) !=
                NormalizeNullableId(second.ShapeId))
            {
                return false;
            }

            return AreSpecificationsEqual(
                first.ItemSpecifications,
                second.ItemSpecifications);
        }

        /// <summary>
        /// Compares two Item Specification collections.
        ///
        /// Order does not matter.
        /// </summary>
        public static bool AreSpecificationsEqual(
            IEnumerable<ItemSpecification> first,
            IEnumerable<ItemSpecification> second)
        {
            var firstSignatures =
                BuildSpecificationSignatures(first);

            var secondSignatures =
                BuildSpecificationSignatures(second);

            if (firstSignatures.Count !=
                secondSignatures.Count)
            {
                return false;
            }

            for (var index = 0;
                 index < firstSignatures.Count;
                 index++)
            {
                if (!string.Equals(
                    firstSignatures[index],
                    secondSignatures[index],
                    StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Item Name

        public static bool IsSameItemName(
            string? first,
            string? second)
        {
            return string.Equals(
                NormalizeText(first),
                NormalizeText(second),
                StringComparison.Ordinal);
        }

        #endregion

        #region Specification Signature

        /// <summary>
        /// Builds sorted normalized signatures such as:
        ///
        /// 1|25|2
        /// 4|EN8|0
        ///
        /// Format:
        /// SpecificationId | Value | UomId
        /// </summary>
        public static List<string>
            BuildSpecificationSignatures(
                IEnumerable<ItemSpecification> specifications)
        {
            if (specifications == null)
            {
                return new List<string>();
            }

            return specifications
                .Where(x => !x.IsDeleted)
                .Select(x =>
                {
                    var specificationId =
                        x.SpecificationId;

                    var normalizedValue =
                        NormalizeText(
                            x.SpecificationValue);

                    var uomId =
                        NormalizeNullableId(
                            x.UomId) ?? 0;

                    return
                        $"{specificationId}|{normalizedValue}|{uomId}";
                })
                .OrderBy(x => x)
                .ToList();
        }

        #endregion

        #region Normalization

        private static string NormalizeText(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalized =
                Regex.Replace(
                    value.Trim(),
                    @"\s+",
                    " ");

            return normalized
                .ToUpperInvariant();
        }

        private static int? NormalizeNullableId(
            int? value)
        {
            return value.HasValue &&
                   value.Value > 0
                ? value.Value
                : null;
        }

        #endregion
    }
}