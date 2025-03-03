using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockManagemant.Entities.DTO
{
    public class ReceiptDetailDto
    {
        public int Id { get; set; }
        public int ReceiptId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } 
        public int Quantity { get; set; }
        public decimal ProductPriceAtSale { get; set; }
        public decimal SubTotal => ProductPriceAtSale * Quantity;
        public bool IsDeleted { get; set; }
    }

    public class CreateReceiptDetailDto
    {

        [Required]
        public int ReceiptId { get; set; }

        [Required]
        public int ProductId { get; set; }

       


        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal ProductPriceAtSale { get; set; }

        [Required]

        public decimal SubTotal => ProductPriceAtSale * Quantity;
    }



    public class UpdateReceiptDetailDto
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal ProductPriceAtSale { get; set; }
        [Required]
        public decimal SubTotal => ProductPriceAtSale * Quantity;
        public bool IsDeleted { get; set; }
    }

}

