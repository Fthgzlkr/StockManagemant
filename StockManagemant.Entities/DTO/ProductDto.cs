using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockManagemant.Entities.Models;

namespace StockManagemant.Entities.DTO
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } 
        public int Stock { get; set; }
        public CurrencyType Currency { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateProductDto
    {
        [Required]
        public string Name { get; set; }
        public decimal? Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public int Stock { get; set; }
        public CurrencyType Currency { get; set; }
    }

    public class UpdateProductDto
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public decimal? Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public int Stock { get; set; }
        public CurrencyType Currency { get; set; }
        public bool IsDeleted { get; set; }
    }

}

