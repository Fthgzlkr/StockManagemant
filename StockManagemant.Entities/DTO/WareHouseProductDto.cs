using System.ComponentModel.DataAnnotations;
using System.Globalization;
using StockManagemant.Entities.Models;


namespace StockManagemant.Entities.DTO
{
     public class WarehouseProductDto
    {
        [Required]
        public int ProductId { get; set; } // Ürün ID

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
    }
}