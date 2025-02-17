using System.ComponentModel.DataAnnotations;

namespace StockManagemant.Entities.Models
{
    public class Receipt
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }

        public ICollection<ReceiptDetail> ReceiptDetails { get; set; }

        public bool IsDeleted { get; set; } = false; // Soft Delete için

    }

}
