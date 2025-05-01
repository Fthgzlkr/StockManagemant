
namespace StockManagemant.Business.Managers
{
  public static class LocationParser
{
    public static (string corridor, string? shelf, string? bin)? Parse(string locationText)
    {
        if (string.IsNullOrWhiteSpace(locationText))
            return null;

        var parts = locationText.Split('-');

        if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
            return null;

        string corridor = parts[0];
        string? shelf = parts.Length > 1 ? parts[1] : null;
        string? bin = parts.Length > 2 ? parts[2] : null;

        return (corridor, shelf, bin);
    }
}
}
