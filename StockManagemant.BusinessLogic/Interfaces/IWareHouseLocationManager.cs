using StockManagemant.Entities.DTO;
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
        Task<IEnumerable<object>> GetCorridorsByWarehouseIdAsync(int warehouseId);
        Task<IEnumerable<object>> GetShelvesByWarehouseAsync(int warehouseId, string corridor);
        Task<IEnumerable<object>> GetBinsByWarehouseAsync(int warehouseId, string corridor, string shelf);
    }
}