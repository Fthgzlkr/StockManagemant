using System.ComponentModel.DataAnnotations;
using System.Globalization;
using StockManagemant.Entities.Models;


namespace StockManagemant.Entities.DTO
{
    public class CustomersDto
    {
        public int? Id { get; set; } // Create için null olabilir

        [Required(ErrorMessage = "Müşteri adı zorunludur.")]
        [StringLength(100, ErrorMessage = "Müşteri adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage = "Adres en fazla 200 karakter olabilir.")]
        public string? Address { get; set; }

        [StringLength(50, ErrorMessage = "Telefon numarası en fazla 50 karakter olabilir.")]
        public string? Phone { get; set; }

        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string? Email { get; set; }

        public bool? IsDeleted { get; set; } // Soft Delete için

        public DateTime? CreatedAt { get; set; } // Listeleme için eklenebilir
    }
}
