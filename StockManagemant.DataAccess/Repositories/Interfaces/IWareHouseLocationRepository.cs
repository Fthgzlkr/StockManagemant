using StockManagemant.Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.DataAccess.Repositories.Interfaces
{
    public interface IWareHouseLocationRepository : IRepository<WarehouseLocation>
    {
        Task<IEnumerable<object>> GetCorridorsByWarehouseIdAsync(int warehouseId);
        Task<IEnumerable<object>> GetShelvesByWarehouseAsync(int warehouseId, string corridor);
        Task<IEnumerable<object>> GetBinsByWarehouseAsync(int warehouseId, string corridor, string shelf);
    }
}