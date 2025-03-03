using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StockManagemant.Entities.Models
{
    public class ReceiptDetail
    {
        public int Id { get; set; }

        [Required]
        public int ReceiptId { get; set; }
        public Receipt Receipt { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; } // SubTotal: ProductPriceAtSale * Quantity

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ProductPriceAtSale { get; set; }

        public bool IsDeleted { get; set; } = false;
    }


}
