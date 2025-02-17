using System.ComponentModel.DataAnnotations;

namespace StockManagemant.Entities.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public ICollection<Product> Products { get; set; }

        public bool IsDeleted { get; set; } = false; // Soft Delete için

    }
}
