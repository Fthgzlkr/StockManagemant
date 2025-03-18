using StockManagemant.DataAccess.Repositories.Filters;
using StockManagemant.Entities.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockManagemant.DataAccess.Repositories.Interfaces
{
    public interface IWarehouseProductRepository : IRepository<WarehouseProduct>
    {
        /// <summary>
        /// Belirli bir ürünün toplam stok miktarını getir (Sadece aktif kayıtlar)
        /// </summary>
        Task<int> GetTotalStockByProductIdAsync(int productId);

        /// <summary>
        /// Belirli bir depodaki ürünleri getir (Sadece aktif kayıtlar)
        /// </summary>
        Task<IEnumerable<WarehouseProduct>> GetProductsByWarehouseIdAsync(int warehouseId);

        /// <summary>
        /// Filtreye ve depo ID'ye göre toplam depo ürünü sayısını getir
        /// </summary>
        Task<int> GetTotalCountAsync(WarehouseProductFilter filter, int warehouseId);

        /// <summary>
        /// Sayfalama ile belirli bir depodaki ürünleri getir
        /// </summary>
        Task<IEnumerable<WarehouseProduct>> GetPagedWarehouseProductsWithStockAsync(WarehouseProductFilter filter, int warehouseId, int page, int pageSize);

        /// <summary>
        /// ID'ye göre sadece aktif (silinmemiş) depo ürününü getir
        /// </summary>
        Task<WarehouseProduct> GetByIdAsync(int id);

        /// <summary>
        /// Belirli bir depoda belirli bir ürünün stok kaydını getir
        /// </summary>
        Task<WarehouseProduct> GetProductInWarehouseByIdAsync(int warehouseId, int productId);

        /// <summary>
        /// ID'ye göre depo ürününü getir (Silinmişler dahil)
        /// </summary>
        Task<WarehouseProduct> GetByIdWithDeletedAsync(int id);
    }
}
