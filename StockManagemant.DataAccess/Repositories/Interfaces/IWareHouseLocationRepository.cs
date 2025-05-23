using StockManagemant.Entities.Models;
using StockManagemant.Entities.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.DataAccess.Repositories.Interfaces
{
    public interface IWareHouseLocationRepository : IRepository<WarehouseLocation>
    {
        Task<List<WarehouseLocation>> GetLocationsByWarehouseIdAsync(int warehouseId);
        Task<List<WarehouseLocation>> GetRootLocationsAsync(int warehouseId);
        Task<List<WarehouseLocation>> GetChildrenAsync(int parentId);
        Task<bool> HasChildrenAsync(int id);
        Task<WarehouseLocation?> GetLocationByIdAsync(int locationId);
        Task<List<WarehouseLocation>> GetLocationsByStorageTypeAsync(int warehouseId, StorageType storageType);
        Task<List<WarehouseLocation>> GetLocationsByLevelAndStorageTypeAsync(int warehouseId, LocationLevel level, StorageType storageType);
        Task<int?> GetLocationIdByLevelPathAsync(int warehouseId, List<string> levelPath);
    }
}