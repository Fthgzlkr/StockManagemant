using System.ComponentModel.DataAnnotations;
using System.Globalization;
using StockManagemant.Entities.Models;


namespace StockManagemant.Entities.DTO
{

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
        
        [Required]
        [MaxLength(50)]
        public string ReceiptNumber { get; set; }
        public decimal TotalAmount { get; set; }

        public bool? IsDeleted { get; set; } // Create için null olabilir, Update işlemi için kullanılabilir

        public int WareHouseId { get; set; }

        public string? WareHouseName { get; set; } // Get için gerekli, Create işleminde opsiyonel

        public ReceiptType ReceiptType { get; set; } // Giriş veya Çıkış fişi
        public string FormattedReceiptType
        {
            get
            {
                return ReceiptType switch
                {
                    ReceiptType.Entry => "Depo Giriş",
                    ReceiptType.Exit => "Depo Çıkış",
                    _ => "Bilinmeyen"
                };
            }
        }

        public string? Description { get; set; } // Açıklama alanı (isteğe bağlı)

        public ICollection<ReceiptDetailDto>? ReceiptDetails { get; set; } // Create ve Update işlemlerinde opsiyonel olabilir
    }
}
   