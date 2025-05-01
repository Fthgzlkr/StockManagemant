using System.ComponentModel.DataAnnotations;
using System.Globalization;
using StockManagemant.Entities.Models;


namespace StockManagemant.Entities.DTO
{
    public class WarehouseProductParsedDto
    {
        public int ProductId { get; set; }
        public int QuantityChange { get; set; }
        public int? WarehouseLocationId { get; set; } // Excel'den gelen veya var olan
        public string? StockCode { get; set; }
    }
}
