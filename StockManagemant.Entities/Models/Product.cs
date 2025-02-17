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
        public Category Category { get; set; }
            
        public int Stock { get; set; }

        public CurrencyType Currency { get; set; }

        public bool IsDeleted { get; set; } = false; // Soft Delete için
    }

    public enum CurrencyType
    {
        TL = 0,
        USD = 1
    }
}
    