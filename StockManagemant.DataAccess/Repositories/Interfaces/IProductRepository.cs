using StockManagemant.DataAccess.Filters;
using StockManagemant.Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.DataAccess.Repositories.Interfaces
{
    public interface IProductRepository : IRepository<Product>
    {

        Task<int> GetTotalCountAsync(ProductFilter filter);
        Task<IEnumerable<Product>> GetPagedProductsAsync(ProductFilter filter, int page, int pageSize);
        Task<Product> GetByIdAsync(int id);
        Task<Product> GetByIdWithDeletedAsync(int id);
        Task<bool> IsProductInWarehouseAsync(int productId, int warehouseId);
        Task<Product?> GetProductByBarcodeAsync(string barcode);
    }
}
