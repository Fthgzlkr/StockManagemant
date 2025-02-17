using System.ComponentModel.DataAnnotations;

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
        public int Quantity { get; set; }

        [Required]
        public decimal SubTotal { get; set; } // SubTotal: ProductPriceAtSale * Quantity

        [Required]
        public decimal ProductPriceAtSale { get; set; }

        public bool IsDeleted { get; set; } = false;
    }

}
