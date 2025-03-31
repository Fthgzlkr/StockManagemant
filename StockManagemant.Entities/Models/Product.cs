using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace StockManagemant.Entities.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public decimal? Price { get; set; }

        [Required]
        public int CategoryId { get; set; }  // Foreign Key
        
        public virtual Category Category { get; set; } 

        public CurrencyType Currency { get; set; }


        public bool IsDeleted { get; set; } = false; // Soft Delete için
        public ICollection<ReceiptDetail> ReceiptDetails { get; set; }
        public ICollection<WarehouseProduct> WarehouseProducts { get; set; } = new List<WarehouseProduct>();
    }

    public enum CurrencyType
    {
        TL = 0,
        USD = 1
    }
}
    