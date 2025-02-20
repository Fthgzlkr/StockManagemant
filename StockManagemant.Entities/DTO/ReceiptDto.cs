using System;
using System.Collections.Generic;
using System.Globalization;

namespace StockManagemant.Entities.DTO
{
    public class ReceiptDto
    {
        public int Id { get; set; }

        // Orijinal tarih bilgisi (DateTime olarak)
        public DateTime Date { get; set; }

        // Tarihi sadece gün, ay ve yıl olarak göster
        public string FormattedDate
        {
            get
            {
                return Date.ToString("dd MMMM yyyy", new CultureInfo("tr-TR"));
            }
        }

        public decimal TotalAmount { get; set; }
        public ICollection<ReceiptDetailDto> ReceiptDetails { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateReceiptDto
    {
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public ICollection<CreateReceiptDetailDto> ReceiptDetails { get; set; }
    }

    public class UpdateReceiptDto
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalAmount { get; set; }
        public ICollection<UpdateReceiptDetailDto> ReceiptDetails { get; set; }
        public bool IsDeleted { get; set; }
    }
}
