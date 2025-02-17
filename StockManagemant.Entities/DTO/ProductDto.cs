using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManagement.Entities.DTO
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } // Kategori adını da ekliyoruz
        public int Stock { get; set; }
        public string Currency { get; set; } // Enum yerine string
    }
}

