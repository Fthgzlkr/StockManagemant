using System.ComponentModel.DataAnnotations;
using System.Globalization;
using StockManagemant.Entities.Models;


namespace StockManagemant.Entities.DTO
{
 public class RawProductModel
{
    public string Name { get; set; }
    public string? Price { get; set; }
    public string CategoryName { get; set; }
    public string CurrencyText { get; set; }
    public string? Barcode { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
}
}
