using System.ComponentModel.DataAnnotations;
using System.Globalization;
using StockManagemant.Entities.Models;


namespace StockManagemant.Entities.DTO
{
    public class CategoryDto
    {
        public int? Id { get; set; } // Create işlemi için nullable

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public bool? IsDeleted { get; set; } // Create işlemi için null olabilir, Update işlemi için kullanılabilir

        public List<ProductDto>? Products { get; set; } // Get işlemlerinde kullanılır, Create ve Update için opsiyonel
    }
}
