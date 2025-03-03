using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManagemant.Entities.Models
{
    public class Warehouse
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // Depo Adı

        [MaxLength(255)]
        public string Location { get; set; } // Adres veya Konum Bilgisi

        public ICollection<WarehouseProduct> WarehouseProducts { get; set; } = new List<WarehouseProduct>();

        public bool IsDeleted { get; set; } = false; // Soft Delete
    }

}
