using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using StockManagemant.Entities.Enums;

namespace StockManagemant.Entities.Models
{
    public class Receipt
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public int WarehouseId { get; set; } // İşlem yapılan depo
        public Warehouse Warehouse { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string ReceiptNumber { get; set; }

        [Required]
       public ReceiptType ReceiptType { get; set; } // "Giriş" veya "Çıkış"

        public string? Description { get; set; }

        public ICollection<ReceiptDetail> ReceiptDetails { get; set; } = new List<ReceiptDetail>();

        public ReceiptSourceType? SourceType { get; set; } // Enum'a karşılık gelir
        public int? SourceId { get; set; }   // Depo, Müşteri ya da Tedarikçi ID'si

        public bool IsDeleted { get; set; } = false; // Soft Delete
    }

 
}
