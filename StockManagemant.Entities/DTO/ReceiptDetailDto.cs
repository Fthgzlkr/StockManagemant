using System.ComponentModel.DataAnnotations;
using System.Globalization;
using StockManagemant.Entities.Models;


namespace StockManagemant.Entities.DTO
{
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


}