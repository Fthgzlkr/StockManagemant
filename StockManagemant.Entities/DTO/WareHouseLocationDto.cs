using System.ComponentModel.DataAnnotations;
using System.Globalization;
using StockManagemant.Entities.Models;


namespace StockManagemant.Entities.DTO
{
     public class WarehouseLocationDto
{
    public int Id { get; set; }
    public int WarehouseId { get; set; }
    public string Corridor { get; set; } = null!;
    public string? Shelf { get; set; }
    public string? Bin { get; set; }
}
}
