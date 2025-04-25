using System.ComponentModel.DataAnnotations;
using System.Globalization;
 using StockManagemant.Entities.Models;


namespace StockManagemant.Entities.DTO
{
   public class StockChangePayloadDto
{
    public int WarehouseId { get; set; }
    public string? Description { get; set; }
    public ReceiptType ReceiptType { get; set; } // Giriş veya Çıkış fişi
    public List<StockChangeItemDto> Changes { get; set; } = new();
}

public class StockChangeItemDto
{
    public int Id { get; set; } // WarehouseProductId
    public int Quantity { get; set; }
    public decimal? Price { get; set; }
}
}
