using StockManagemant.Entities.Models;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace StockManagemant.DataAccess.Filters
{
    public class ReceiptFilter
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IncludeDeleted { get; set; } = false; // Silinmiş fişleri dahil etme

        public Expression<Func<Receipt, bool>> GetFilterExpression()
        {
            return receipt =>
                (!IncludeDeleted ? !receipt.IsDeleted : true) && // Soft delete olmayanları getir
                (!StartDate.HasValue || receipt.Date >= StartDate.Value) && // Başlangıç tarihi
                (!EndDate.HasValue || receipt.Date <= EndDate.Value); // Bitiş tarihi
        }
    }
}
