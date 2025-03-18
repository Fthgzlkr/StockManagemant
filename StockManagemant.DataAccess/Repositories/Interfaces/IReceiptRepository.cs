using StockManagemant.DataAccess.Filters;
using StockManagemant.Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.DataAccess.Repositories.Interfaces
{
    public interface IReceiptRepository : IRepository<Receipt>
    {

        Task<int> GetTotalCountAsync(ReceiptFilter filter);


        Task<IEnumerable<Receipt>> GetPagedReceiptsAsync(ReceiptFilter filter, int page, int pageSize);


        Task<int> AddReceiptAsync(Receipt receipt);

        Task UpdateReceiptAsync(Receipt receipt);


        Task EditAsync(Receipt receipt);


        Task DeleteReceiptAsync(int id);
    }
}
