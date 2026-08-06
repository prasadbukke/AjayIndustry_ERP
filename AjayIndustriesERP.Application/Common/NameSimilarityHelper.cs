/*
==============================================================

File : NameSimilarityHelper.cs

Purpose :
Provides reusable name normalization, exact duplicate checking,
similar spelling detection and live suggestion matching.

Used By :
- Item
- Item Category
- Brand
- UOM
- Warehouse
- Customer
- Supplier
- Machine
- Other name-based masters

==============================================================
*/

using System.Text.RegularExpressions;

namespace AjayIndustriesERP.Application.Common
{
    /// <summary>
    /// Provides reusable methods for comparing names and
    /// detecting possible spelling mistakes.
    /// </summary>
    public static class NameSimilarityHelper
    {
        #region Public Methods

        /// <summary>
        /// Normalizes a name by trimming spaces, converting it
        /// to lowercase and replacing multiple spaces with one.
        /// </summary>
        public static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var normalizedValue =
                value.Trim().ToLowerInvariant();

            return Regex.Replace(
                normalizedValue,
                @"\s+",
                " ");
        }

        /// <summary>
        /// Determines whether two names are exactly equal
        /// after normalization.
        /// </summary>
        public static bool IsExactMatch(
            string? firstName,
            string? secondName)
        {
            var normalizedFirst =
                Normalize(firstName);

            var normalizedSecond =
                Normalize(secondName);

            if (string.IsNullOrWhiteSpace(normalizedFirst) ||
                string.IsNullOrWhiteSpace(normalizedSecond))
            {
                return false;
            }

            return normalizedFirst == normalizedSecond;
        }

        /// <summary>
        /// Determines whether two names are similar enough
        /// to generate a possible spelling mistake warning.
        /// Exact matches are not considered similar because
        /// exact duplicates must be blocked separately.
        /// </summary>
        public static bool IsSimilarMatch(
            string? firstName,
            string? secondName)
        {
            var normalizedFirst =
                Normalize(firstName);

            var normalizedSecond =
                Normalize(secondName);

            if (string.IsNullOrWhiteSpace(normalizedFirst) ||
                string.IsNullOrWhiteSpace(normalizedSecond))
            {
                return false;
            }

            if (normalizedFirst == normalizedSecond)
            {
                return false;
            }

            if (normalizedFirst.Length < 3 ||
                normalizedSecond.Length < 3)
            {
                return false;
            }

            /*
             * Example:
             * Steel and Stainless Steel.
             */
            if (normalizedFirst.Contains(normalizedSecond) ||
                normalizedSecond.Contains(normalizedFirst))
            {
                return true;
            }

            /*
             * Avoid unrelated names beginning with
             * completely different characters.
             */
            if (normalizedFirst[0] != normalizedSecond[0])
            {
                return false;
            }

            var distance =
                CalculateLevenshteinDistance(
                    normalizedFirst,
                    normalizedSecond);

            var maximumLength =
                Math.Max(
                    normalizedFirst.Length,
                    normalizedSecond.Length);

            var similarityPercentage =
                1D -
                ((double)distance / maximumLength);

            /*
             * Short names require special handling.
             *
             * Examples:
             * Steel and Still
             * SKF and SKP
             */
            if (maximumLength <= 6)
            {
                return distance <= 2;
            }

            /*
             * Medium-length names allow a maximum of
             * three spelling changes.
             */
            if (maximumLength <= 12)
            {
                return distance <= 3 &&
                       similarityPercentage >= 0.70D;
            }

            /*
             * Longer names use percentage-based matching.
             */
            return similarityPercentage >= 0.78D;
        }

        /// <summary>
        /// Determines whether an existing name should appear
        /// in live suggestions while the user is typing.
        /// </summary>
        public static bool IsLiveSearchMatch(
            string? enteredName,
            string? existingName)
        {
            var normalizedEnteredName =
                Normalize(enteredName);

            var normalizedExistingName =
                Normalize(existingName);

            if (normalizedEnteredName.Length < 3 ||
                string.IsNullOrWhiteSpace(normalizedExistingName))
            {
                return false;
            }

            if (normalizedExistingName ==
                normalizedEnteredName)
            {
                return true;
            }

            if (normalizedExistingName.StartsWith(
                normalizedEnteredName))
            {
                return true;
            }

            if (normalizedExistingName.Contains(
                normalizedEnteredName))
            {
                return true;
            }

            return IsSimilarMatch(
                normalizedEnteredName,
                normalizedExistingName);
        }

        /// <summary>
        /// Finds and orders matching records for live
        /// similar-name suggestions.
        /// </summary>
        public static List<T> FindMatches<T>(
            IEnumerable<T> records,
            string enteredName,
            Func<T, string?> nameSelector,
            int maximumResults = 5)
        {
            if (records == null)
            {
                return new List<T>();
            }

            var normalizedEnteredName =
                Normalize(enteredName);

            if (normalizedEnteredName.Length < 3)
            {
                return new List<T>();
            }

            if (maximumResults <= 0)
            {
                maximumResults = 5;
            }

            return records
                .Select(record => new
                {
                    Record = record,
                    Name = Normalize(
                        nameSelector(record))
                })
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.Name))
                .Where(x =>
                    IsLiveSearchMatch(
                        normalizedEnteredName,
                        x.Name))
                .GroupBy(x => x.Name)
                .Select(group =>
                    group.First())
                .OrderByDescending(x =>
                    GetMatchScore(
                        normalizedEnteredName,
                        x.Name))
                .ThenBy(x => x.Name)
                .Take(maximumResults)
                .Select(x => x.Record)
                .ToList();
        }

        #endregion

        #region Private Matching Methods

        /// <summary>
        /// Calculates a sorting score so that exact and
        /// prefix matches appear before fuzzy matches.
        /// </summary>
        private static double GetMatchScore(
            string enteredName,
            string existingName)
        {
            if (enteredName == existingName)
            {
                return 100D;
            }

            if (existingName.StartsWith(enteredName))
            {
                return 90D;
            }

            if (existingName.Contains(enteredName))
            {
                return 80D;
            }

            var distance =
                CalculateLevenshteinDistance(
                    enteredName,
                    existingName);

            var maximumLength =
                Math.Max(
                    enteredName.Length,
                    existingName.Length);

            if (maximumLength == 0)
            {
                return 0D;
            }

            return
                (1D -
                 ((double)distance / maximumLength))
                * 100D;
        }

        /// <summary>
        /// Calculates Levenshtein distance between two strings.
        /// </summary>
        private static int CalculateLevenshteinDistance(
            string source,
            string target)
        {
            var sourceLength =
                source.Length;

            var targetLength =
                target.Length;

            var matrix =
                new int[
                    sourceLength + 1,
                    targetLength + 1];

            for (var row = 0;
                 row <= sourceLength;
                 row++)
            {
                matrix[row, 0] = row;
            }

            for (var column = 0;
                 column <= targetLength;
                 column++)
            {
                matrix[0, column] = column;
            }

            for (var row = 1;
                 row <= sourceLength;
                 row++)
            {
                for (var column = 1;
                     column <= targetLength;
                     column++)
                {
                    var replacementCost =
                        source[row - 1] ==
                        target[column - 1]
                            ? 0
                            : 1;

                    matrix[row, column] =
                        Math.Min(
                            Math.Min(
                                matrix[row - 1, column] + 1,
                                matrix[row, column - 1] + 1),
                            matrix[row - 1, column - 1] +
                            replacementCost);
                }
            }

            return matrix[
                sourceLength,
                targetLength];
        }

        #endregion
    }
}