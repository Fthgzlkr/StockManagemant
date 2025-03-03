using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using StockManagemant.Entities.Models;

namespace StockManagemant.Entities.DTO
{
    public class WarehouseProductDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal? Price { get; set; }
        public string CategoryName { get; set; }
        public CurrencyType Currency { get; set; }

        public int StockQuantity { get; set; } // Sadece bu depodaki stok miktarı

        public int WarehouseId { get; set; }  // Ürünün bulunduğu depo ID'si
        public string WarehouseName { get; set; }  // Depo Adı
    }

    public class UpdateWarehouseProductStockDto
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public int WarehouseId { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stok miktarı negatif olamaz.")]
        public int StockQuantity { get; set; } // Sadece bu depodaki stok miktarı güncellenebilir
    }

    public class AddExistingProductToWarehouseDto
    {
        [Required]
        public int ProductId { get; set; } // Var olan ürün ID

        [Required]
        public int WarehouseId { get; set; } // Hangi depo?

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Stok miktarı en az 1 olmalıdır.")]
        public int StockQuantity { get; set; } // Depoya eklenen stok miktarı
    }
}
