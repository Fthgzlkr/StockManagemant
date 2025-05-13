using System.ComponentModel.DataAnnotations;
using System.Globalization;
 using StockManagemant.Entities.Enums;


namespace StockManagemant.Entities.DTO
{public class StockChangePayloadDto
{
    public int WarehouseId { get; set; }
    public string? Description { get; set; }
    public ReceiptType ReceiptType { get; set; } // Entry or Exit

    public ReceiptSourceType? SourceType { get; set; } // New: Customer, Warehouse etc.
    public int? SourceId { get; set; }

    public List<StockChangeItemDto> Changes { get; set; } = new();
}

public class StockChangeItemDto
{
    public int Id { get; set; } // WarehouseProductId
    public int Quantity { get; set; }

    public decimal? UnitPrice { get; set; } // Optional: Alış/Satış fiyatı
}
}
