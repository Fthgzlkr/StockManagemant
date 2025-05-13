using System.ComponentModel.DataAnnotations;
using System.Globalization;
using StockManagemant.Entities.Models;
using StockManagemant.Entities.Enums;


namespace StockManagemant.Entities.DTO
{
     public class WarehouseProductDto
{
    [Required]
    public int ProductId { get; set; } // Ürün ID

    public int? WarehouseProductId { get; set; }

    public string? ProductName { get; set; } // Get işlemleri için gerekli, Create/Update için opsiyonel

    public decimal? Price { get; set; } // Get işlemlerinde kullanılabilir

    public string? CategoryName { get; set; } // Get işlemlerinde kullanılabilir

    public CurrencyType? Currency { get; set; } // Get işlemlerinde kullanılabilir

    [Required]
    public int WarehouseId { get; set; }  // Ürünün bulunduğu depo ID'si

    public string? WarehouseName { get; set; }  // Get işlemleri için gerekli, Create/Update için opsiyonel

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı negatif olamaz.")]
    public int StockQuantity { get; set; } // Depodaki stok miktarı güncellenebilir

    [Required(ErrorMessage = "Lokasyon seçilmelidir.")]
    public int? WarehouseLocationId { get; set; } // Yeni: Ürünün bulunduğu lokasyon ID'si

    public string? LocationDisplay { get; set; } // Get işlemleri için (örn. C1 > R2 > G1 gibi)
    public string? StockCode { get; set; }
    public string? Barcode { get; set; }
    public string? ImageUrl { get; set; }
     public string? Description { get; set; }
}
    public class UpdateStockDto
{
        [Required]
        public int WarehouseProductId { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı negatif olamaz.")]
        public int StockQuantity { get; set; }
}

}