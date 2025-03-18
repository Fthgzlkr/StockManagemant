using StockManagemant.Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.DataAccess.Repositories.Interfaces
{
    public interface IReceiptDetailRepository : IRepository<ReceiptDetail>
    {

        Task<List<ReceiptDetail>> GetByReceiptIdAsync(int receiptId);

        Task UpdateReceiptTotal(int receiptId);

        Task<decimal> GetTotalAmountByReceiptIdAsync(int receiptId);
    }
}
