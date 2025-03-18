using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockManagemant.Entities.Models;

namespace StockManagemant.Entities.DTO
{

    public class ProductDto
    {
        public int? Id { get; set; } // Create işlemi için nullable

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ürün adı 2 ile 100 karakter arasında olmalıdır.")]
        public string Name { get; set; }

        [Range(0.01, 100000, ErrorMessage = "Fiyat 0.01 ile 100000 arasında olmalıdır.")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Kategori seçilmelidir.")]
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; } // Get için gerekli ama Create/Update işlemlerinde opsiyonel

        [Required(ErrorMessage = "Para birimi seçilmelidir.")]
        public CurrencyType Currency { get; set; }

        public bool? IsDeleted { get; set; } // Create işlemi için null olabilir, Update işlemi için kullanılabilir
    }


    public class CategoryDto
    {
        public int? Id { get; set; } // Create işlemi için nullable

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public bool? IsDeleted { get; set; } // Create işlemi için null olabilir, Update işlemi için kullanılabilir

        public List<ProductDto>? Products { get; set; } // Get işlemlerinde kullanılır, Create ve Update için opsiyonel
    }


    public class ReceiptDto
    {
        public int? Id { get; set; } // Create işlemi için nullable

        public DateTime Date { get; set; } = DateTime.UtcNow;

        public string FormattedDate
        {
            get
            {
                return Date.ToString("dd MMMM yyyy", new CultureInfo("tr-TR"));
            }
        }

        public decimal TotalAmount { get; set; }

        public bool? IsDeleted { get; set; } // Create için null olabilir, Update işlemi için kullanılabilir

        public int WareHouseId { get; set; }

        public string? WareHouseName { get; set; } // Get için gerekli, Create işleminde opsiyonel

        public ICollection<ReceiptDetailDto>? ReceiptDetails { get; set; } // Create ve Update işlemlerinde opsiyonel olabilir
    }


    public class ReceiptDetailDto
    {
        public int? Id { get; set; } // Create işlemi için nullable

        [Required]
        public int ReceiptId { get; set; } // Güncelleme işlemi için gerekli değilse nullable olabilir

        [Required]
        public int ProductId { get; set; }

        public string? ProductName { get; set; } // Get için gerekli ama Create/Update işlemlerinde opsiyonel

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal ProductPriceAtSale { get; set; }

        public decimal SubTotal => ProductPriceAtSale * Quantity;

        public bool? IsDeleted { get; set; } // Create işlemi için null olabilir, Update işlemi için kullanılabilir
    }


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



    public class WareHouseDto
    {
        public int? Id { get; set; } // Create işlemi için nullable, Update için zorunlu

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Location { get; set; }

        public bool? IsDeleted { get; set; } // Create için null olabilir, Update işlemi için kullanılabilir

        public int? TotalProducts { get; set; } // Create ve Update işlemlerinde gerekli değil, Get işlemleri için kullanılır
    }

}
