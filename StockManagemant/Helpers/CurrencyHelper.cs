using System.Globalization;

namespace StockManagemant.Web.Helpers
{
    public static class CurrencyHelper
    {
        public static string FormatPrice(decimal? price, string currencyType)
        {
            if (price == null) return string.Empty;

            CultureInfo cultureInfo = currencyType == "TL"
                ? new CultureInfo("tr-TR") { NumberFormat = { CurrencySymbol = "₺" } }
                : new CultureInfo("en-US") { NumberFormat = { CurrencySymbol = "$" } };

            return price.Value.ToString("C", cultureInfo);
        }
    }
}
