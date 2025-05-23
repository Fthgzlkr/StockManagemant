namespace StockManagemant.Business.Managers
{
    public static class LocationParser
{
   
    public static List<string>? ParseDynamic(string locationText)
    {
        if (string.IsNullOrWhiteSpace(locationText))
            return null;

        var separators = new[] { '>', '|' };

        var parts = locationText
            .Split(separators, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToList();

        return parts.Count > 0 ? parts : null;
    }
}
}