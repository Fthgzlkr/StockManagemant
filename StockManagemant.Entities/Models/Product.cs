using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using StockManagemant.Entities.Enums;

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

        public string? Barcode { get; set; }
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public StorageType StorageType { get; set; } = StorageType.Undefined;
        
        public bool IsDeleted { get; set; } = false; // Soft Delete için
        public ICollection<ReceiptDetail> ReceiptDetails { get; set; }
        public ICollection<WarehouseProduct> WarehouseProducts { get; set; } = new List<WarehouseProduct>();
    }
 
}
    