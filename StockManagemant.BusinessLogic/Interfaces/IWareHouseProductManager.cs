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
        Task UpdateStockAsync(UpdateStockDto dto);
        Task AddProductToWarehouseAsync(WarehouseProductDto dto);
        Task RemoveProductFromWarehouseAsync(int warehouseProductId);
        Task RestoreWarehouseProductAsync(int warehouseProductId);
        Task<List<WarehouseProductDto>> GetPagedWarehouseProductsAsync(WarehouseProductFilter filter, int warehouseId, int page, int pageSize);
        Task<int> GetTotalWarehouseProductCountAsync(WarehouseProductFilter filter, int WarehouseId);
        Task<WarehouseProductDto> GetProductInWarehouseByBarcodeAsync(int warehouseId, string barcode);
    }
}
