using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManagemant.Entities.Models
{
    public class WarehouseProduct
    {
        public int Id { get; set; }

        [Required]
        public int WarehouseId { get; set; } // Hangi Depo
        public Warehouse Warehouse { get; set; }

        [Required]
        public int ProductId { get; set; } // Hangi Ürün
        public Product Product { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; } // O depodaki stok miktarı
        public int? WarehouseLocationId { get; set; }
        public string? StockCode { get; set; }

        public WarehouseLocation? WarehouseLocation { get; set; }

        public bool IsDeleted { get; set; } = false; // Soft Delete
    }


}