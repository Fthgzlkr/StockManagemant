using StockManagemant.Entities.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StockManagemant.DataAccess.Context;

namespace StockManagemant.DataAccess.Repositories
{
    public class WarehouseRepository : Repository<Warehouse>
    {
        public WarehouseRepository(AppDbContext context) : base(context) { }

        // ✅ Tüm aktif depoları getir (Soft delete olmayanlar)
        public async Task<IEnumerable<Warehouse>> GetAllActiveWarehousesAsync()
        {
            return await _context.Warehouses
                .Where(w => !w.IsDeleted)
                .ToListAsync();
        }

        // ✅ Depo adına göre depo getir (Soft delete olmayanlar)
        public async Task<Warehouse> GetByNameAsync(string name)
        {
            return await _context.Warehouses
                .Where(w => w.Name == name && !w.IsDeleted)
                .FirstOrDefaultAsync();
        }

        // ✅ Belirli bir deponun içindeki tüm ürünleri getir
        public async Task<Warehouse> GetWarehouseWithProductsAsync(int warehouseId)
        {
            return await _context.Warehouses
                .Where(w => w.Id == warehouseId && !w.IsDeleted)
                .Include(w => w.WarehouseProducts)
                .ThenInclude(wp => wp.Product)
                .FirstOrDefaultAsync();
        }
    }
}
