using StockManagemant.Entities.Models;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace StockManagemant.DataAccess.Filters
{
    public class ReceiptFilter
    {
        public int? WarehouseId { get; set; } 
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ReceiptType receiptType{get;set;}
        public bool IncludeDeleted { get; set; } = false; 

        public Expression<Func<Receipt, bool>> GetFilterExpression()
        {
            return receipt =>
                (!IncludeDeleted ? !receipt.IsDeleted : true) && 
                (!StartDate.HasValue || receipt.Date >= StartDate.Value) && 
                (!EndDate.HasValue || receipt.Date <= EndDate.Value) && 
                (!WarehouseId.HasValue || receipt.WarehouseId == WarehouseId.Value)&&
                (receiptType == 0 || receipt.ReceiptType == receiptType);
                
        }
    }

}
