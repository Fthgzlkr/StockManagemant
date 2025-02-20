using StockManagemant.Entities.DTO;
using StockManagemant.DataAccess.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.Business.Managers
{
    public interface IReceiptDetailManager
    {
        Task<List<ReceiptDetailDto>> GetAllReceiptDetailsAsync();

        Task<List<ReceiptDetailDto>> GetReceiptDetailsByReceiptIdAsync(int receiptId);
        Task AddProductToReceiptAsync(int receiptId, int productId, int quantity);
        Task RemoveProductFromReceiptAsync(int receiptDetailId);
        Task UpdateProductQuantityInReceiptAsync(int receiptDetailId, int newQuantity);
        Task DeleteDetailsByReceiptIdAsync(int receiptId);
    }
}
