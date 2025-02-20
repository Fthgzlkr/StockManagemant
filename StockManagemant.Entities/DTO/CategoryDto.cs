using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManagemant.Entities.DTO
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
    }

    public class UpdateCategoryDto
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
    }

}

