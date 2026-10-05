using System.Globalization;

namespace SimplePoker.Helper
{
    /// <summary>
    /// Helper class containing auxiliary global methods, for example formatting values ​​to monetary value.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Abbreviates a money value in USD format with appropriate suffixes 
        /// (K for thousand, M for million, etc.).
        /// </summary>
        /// <param name="value">The money value to be abbreviated.</param>
        /// <returns>The abbreviated money value with USD currency symbol.</returns>
        public static string AbbreviateMoneyUSD(int value)
        {
            string[] suffixes = { "", "K", "M", "B", "T" };

            int suffixIndex = 0;
            double newValue = value;
            while (newValue >= 1000 && suffixIndex < suffixes.Length - 1)
            {
                newValue /= 1000.0;
                suffixIndex++;
            }

            return $"${newValue.ToString("0.###")}{suffixes[suffixIndex]}";
        }

        /// <summary>
        /// Formats a money value in USD currency format.
        /// </summary>
        /// <param name="value">The money value to be formatted.</param>
        /// <returns>The formatted money value in USD currency format.</returns>
        public static string MoneyUSDFormat(int value)
        {
            string valueMoney = value.ToString("C0", CultureInfo.CreateSpecificCulture("en-us"));
            return valueMoney;
        }
    }
}

