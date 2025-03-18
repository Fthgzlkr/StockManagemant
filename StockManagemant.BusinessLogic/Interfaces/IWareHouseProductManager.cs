using StockManagemant.Entities.DTO;
using StockManagemant.DataAccess.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using StockManagemant.DataAccess.Repositories.Filters;

namespace StockManagemant.Business.Managers
{
    public interface IWarehouseProductManager
    {
        Task<List<WarehouseProductDto>> GetProductsByWarehouseIdAsync(int warehouseId);
        Task<int> GetTotalStockByProductIdAsync(int productId);
        Task IncreaseStockAsync(WarehouseProductDto dto);
        Task DecreaseStockAsync(WarehouseProductDto dto);
        Task AddProductToWarehouseAsync(WarehouseProductDto dto);
        Task RemoveProductFromWarehouseAsync(int warehouseProductId);
        Task RestoreWarehouseProductAsync(int warehouseProductId);
        Task<List<WarehouseProductDto>> GetPagedWarehouseProductsAsync(WarehouseProductFilter filter, int warehouseId, int page, int pageSize);
        Task<int> GetTotalWarehouseProductCountAsync(WarehouseProductFilter filter, int WarehouseId);
        Task<WarehouseProductDto> GetProductInWarehouseByIdAsync(int warehouseId, int productId);
    }
}
