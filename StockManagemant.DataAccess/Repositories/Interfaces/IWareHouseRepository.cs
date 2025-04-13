using StockManagemant.Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.DataAccess.Repositories.Interfaces
{
    public interface IWarehouseRepository : IRepository<Warehouse>
    {
        /// <summary>
        /// Tüm aktif depoları getir (Soft delete olmayanlar)
        /// </summary>
        Task<IEnumerable<Warehouse>> GetAllActiveWarehousesAsync();

        /// <summary>
        /// Depo adına göre depo getir (Soft delete olmayanlar)
        /// </summary>
        Task<Warehouse> GetByNameAsync(string name);

        /// <summary>
        /// Belirli bir deponun içindeki tüm ürünleri getir
        /// </summary>
        Task<Warehouse> GetWarehouseWithProductsAsync(int warehouseId);
    }
}