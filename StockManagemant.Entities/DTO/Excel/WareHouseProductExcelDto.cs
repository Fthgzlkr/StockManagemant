using System.ComponentModel.DataAnnotations;
using System.Globalization;
using StockManagemant.Entities.Models;


namespace StockManagemant.Entities.DTO
{
    public class WarehouseProductExcelDto
    {
        public string Barcode { get; set; } = null!;           
        public int QuantityChange { get; set; }              
        public string? LocationText { get; set; }        
        public string? StockCode { get; set; }       
    }
}
