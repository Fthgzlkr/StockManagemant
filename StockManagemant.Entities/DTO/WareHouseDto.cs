using System.ComponentModel.DataAnnotations;
using System.Globalization;
using StockManagemant.Entities.Models;


namespace StockManagemant.Entities.DTO
{
     public class WareHouseDto
    {
        public int? Id { get; set; } // Create işlemi için nullable, Update için zorunlu

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string Location { get; set; }

        public bool? IsDeleted { get; set; } // Create için null olabilir, Update işlemi için kullanılabilir

        public int? TotalProducts { get; set; } // Create ve Update işlemlerinde gerekli değil, Get işlemleri için kullanılır
    }
}
