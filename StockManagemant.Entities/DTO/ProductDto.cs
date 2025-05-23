using System.ComponentModel.DataAnnotations;
using System.Globalization;
using StockManagemant.Entities.Enums;


namespace StockManagemant.Entities.DTO
{

        public class ProductDto
    {
        public int? Id { get; set; } // Create işlemi için nullable

        [Required(ErrorMessage = "Ürün adı zorunludur.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Ürün adı 2 ile 100 karakter arasında olmalıdır.")]
        public string Name { get; set; }

        
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Kategori seçilmelidir.")]
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; } // Get için gerekli ama Create/Update işlemlerinde opsiyonel

        [Required(ErrorMessage = "Para birimi seçilmelidir.")]
        public CurrencyType Currency { get; set; }
        public string? Barcode { get; set; }

        public string? ImageUrl { get; set; }

        public string? Description { get; set; }
        public StorageType StorageType { get; set; } = StorageType.Undefined;
    

        public bool? IsDeleted { get; set; } // Create işlemi için null olabilir, Update işlemi için kullanılabilir
    }
}