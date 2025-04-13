using StockManagemant.DataAccess.Repositories.Filters;
using StockManagemant.Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.DataAccess.Repositories.Interfaces
{
    public interface IWarehouseProductRepository : IRepository<WarehouseProduct>
    {
        Task<int> GetTotalStockByProductIdAsync(int productId);
        Task<IEnumerable<WarehouseProduct>> GetProductsByWarehouseIdAsync(int warehouseId);
        Task<int> GetTotalCountAsync(WarehouseProductFilter filter, int warehouseId);
        Task<IEnumerable<WarehouseProduct>> GetPagedWarehouseProductsWithStockAsync(WarehouseProductFilter filter, int warehouseId, int page, int pageSize);
        Task<WarehouseProduct> GetByIdAsync(int id);   
        Task<WarehouseProduct> GetProductInWarehouseByIdAsync(int warehouseId, int productId);
        Task<WarehouseProduct> GetByIdWithDeletedAsync(int id);
    }
}
