using StockManagemant.Entities.DTO;
using StockManagemant.Entities.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.BusinessLogic.Managers.Interfaces
{
    public interface IWareHouseLocationManager
    {
        Task<List<WarehouseLocationDto>> GetAllAsync();
        Task<WarehouseLocationDto> GetByIdAsync(int id);
        Task AddAsync(WarehouseLocationDto locationDto);
        Task UpdateAsync(WarehouseLocationDto locationDto);
        Task<IEnumerable<WarehouseLocationDto>> GetLocationsByWarehouseIdAsync(int warehouseId);
        Task DeleteAsync(int id);
        Task RestoreAsync(int id);

        // Lokasyon hiyerarşisi
        Task<List<WarehouseLocationDto>> GetChildrenAsync(int parentId);
        Task<List<WarehouseLocationDto>> GetLocationsByStorageTypeAsync(int warehouseId, StorageType storageType);
        Task<List<WarehouseLocationDto>> GetLocationsByLevelAndStorageTypeAsync(int warehouseId, LocationLevel level, StorageType storageType);
    }
}